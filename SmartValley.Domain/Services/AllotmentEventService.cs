﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SmartValley.Domain.Contracts;
using SmartValley.Domain.Core;
using SmartValley.Domain.Entities;
using SmartValley.Domain.Interfaces;

namespace SmartValley.Domain.Services
{
    public class AllotmentEventService : IAllotmentEventService
    {
        private readonly IAllotmentEventRepository _allotmentEventRepository;
        private readonly IClock _clock;
        private readonly IEthereumTransactionService _ethereumTransactionService;
        private readonly IAllotmentEventsManagerContractClient _allotmentEventsManagerContractClient;

        public AllotmentEventService(IAllotmentEventRepository allotmentEventRepository,
                                     IEthereumTransactionService ethereumTransactionService,
                                     IClock clock,
                                     IAllotmentEventsManagerContractClient allotmentEventsManagerContractClient)
        {
            _allotmentEventRepository = allotmentEventRepository;
            _ethereumTransactionService = ethereumTransactionService;
            _clock = clock;
            _allotmentEventsManagerContractClient = allotmentEventsManagerContractClient;
        }

        public async Task<PagingCollection<AllotmentEvent>> QueryAsync(AllotmentEventsQuery queryAllotmentEventsRequest)
        {
            return await _allotmentEventRepository.QueryAsync(queryAllotmentEventsRequest);
        }

        public async Task StartPublishingAsync(long id, long userId, string hash)
        {
            var allotmentEvent = await _allotmentEventRepository.GetByIdAsync(id);
            if (allotmentEvent.Status != AllotmentEventStatus.Created && allotmentEvent.Status != AllotmentEventStatus.Publishing)
            {
                return;
            }

            var transaction = new EthereumTransaction(userId, hash, EthereumTransactionType.PublishAllotmentEvent, EthereumTransactionStatus.InProgress, _clock.UtcNow, id);
            await _ethereumTransactionService.AddAsync(transaction);
            allotmentEvent.Status = AllotmentEventStatus.Publishing;
            await _allotmentEventRepository.SaveChangesAsync();
        }

        public async Task FinishPublishingAsync(long id)
        {
            var allotmentEvent = await _allotmentEventRepository.GetByIdAsync(id);

            if (allotmentEvent.Status != AllotmentEventStatus.Created && allotmentEvent.Status != AllotmentEventStatus.Publishing)
                return;

            allotmentEvent.Status = AllotmentEventStatus.Published;
            allotmentEvent.TokenContractAddress = await _allotmentEventsManagerContractClient.GetAllotmentEventContractAddressAsync(id);

            await _allotmentEventRepository.SaveChangesAsync();
        }

        public async Task StopPublishingAsync(long id)
        {
            var allotmentEvent = await _allotmentEventRepository.GetByIdAsync(id);
            var transactions = await _ethereumTransactionService.GetByAllotmentEventIdAsync(id);

            if (allotmentEvent.Status != AllotmentEventStatus.Publishing || transactions.Any(x => x.Status == EthereumTransactionStatus.InProgress))
                return;

            allotmentEvent.Status = AllotmentEventStatus.Created;
            await _allotmentEventRepository.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(string name, string tokenContractAddress, int totalDecimals, string tokenTicker, long projectId, DateTimeOffset? finishDate)
        {
            var entity = new AllotmentEvent
                         {
                             Name = name,
                             ProjectId = projectId,
                             TokenContractAddress = tokenContractAddress,
                             TokenTicker = tokenTicker,
                             TokenDecimals = totalDecimals,
                             Status = AllotmentEventStatus.Created,
                             FinishDate = finishDate
                         };
            _allotmentEventRepository.Add(entity);
            await _allotmentEventRepository.SaveChangesAsync();
            return entity.Id;
        }
    }
}