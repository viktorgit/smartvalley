﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartValley.Domain.Contracts;
using SmartValley.Domain.Core;
using SmartValley.Domain.Entities;
using SmartValley.Domain.Exceptions;
using SmartValley.Domain.Interfaces;

namespace SmartValley.Domain.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ScoringService : IScoringService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IScoringExpertsManagerContractClient _scoringExpertsManagerContractClient;
        private readonly IScoringManagerContractClient _scoringManagerContractClient;
        private readonly IClock _clock;
        private readonly IScoringRepository _scoringRepository;
        private readonly IUserRepository _userRepository;

        public ScoringService(
            IProjectRepository projectRepository,
            IScoringExpertsManagerContractClient scoringExpertsManagerContractClient,
            IScoringManagerContractClient scoringManagerContractClient,
            IClock clock,
            IScoringRepository scoringRepository,
            IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _scoringExpertsManagerContractClient = scoringExpertsManagerContractClient;
            _scoringManagerContractClient = scoringManagerContractClient;
            _clock = clock;
            _scoringRepository = scoringRepository;
            _userRepository = userRepository;
        }

        public async Task<long> StartAsync(long projectId, IDictionary<AreaType, int> areas)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            var contractOffers = await _scoringExpertsManagerContractClient.GetOffersAsync(project.ExternalId);
            var pendingOffersDueDate = contractOffers.Max(i => i.ExpirationTimestamp);

            if (pendingOffersDueDate == null)
                throw new AppErrorException(ErrorCode.PendingOffersNotFound);

            var scoring = await CreateScoringAsync(project, pendingOffersDueDate.Value, areas, contractOffers);
            return scoring.Id;
        }

        public Task<Scoring> GetAsync(long scoringId)
            => _scoringRepository.GetAsync(scoringId);

        private async Task<Scoring> CreateScoringAsync(
            Project project,
            DateTimeOffset offersDueDate,
            IDictionary<AreaType, int> areas,
            IReadOnlyCollection<ScoringOfferInfo> contractOffers)
        {
            var contractAddress = await _scoringManagerContractClient.GetScoringAddressAsync(project.ExternalId);

            var areaScorings = areas.Select(x => new AreaScoring {AreaId = x.Key, ExpertsCount = x.Value}).ToList();
            var offers = await CreateOffersAsync(contractOffers);
            var scoring = new Scoring(project.Id, contractAddress, offersDueDate, _clock.UtcNow, areaScorings, offers);

            await _scoringRepository.AddAsync(scoring);
            return scoring;
        }

        private async Task<IReadOnlyCollection<ScoringOffer>> CreateOffersAsync(IReadOnlyCollection<ScoringOfferInfo> contractOffers)
        {
            var experts = await GetExpertsForOffersAsync(contractOffers);
            return contractOffers
                   .Select(o => new ScoringOffer
                                {
                                    AreaId = o.Area,
                                    ExpertId = experts.First(e => e.Address == o.ExpertAddress).Id,
                                    Status = o.Status,
                                    ExpirationTimestamp = o.ExpirationTimestamp
                                })
                   .ToArray();
        }

        private Task<IReadOnlyCollection<User>> GetExpertsForOffersAsync(IReadOnlyCollection<ScoringOfferInfo> contractOffers)
        {
            var expertAddresses = contractOffers
                                  .Select(o => (Address) o.ExpertAddress)
                                  .Distinct()
                                  .ToArray();

            return _userRepository.GetByAddressesAsync(expertAddresses);
        }
    }
}