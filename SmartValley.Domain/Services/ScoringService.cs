﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartValley.Domain.Contracts;
using SmartValley.Domain.Core;
using SmartValley.Domain.Entities;
using SmartValley.Domain.Exceptions;
using SmartValley.Domain.Interfaces;

namespace SmartValley.Domain.Services
{
    public class ScoringService : IScoringService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IScoringOffersManagerContractClient _scoringOffersManagerContractClient;
        private readonly IScoringsRegistryContractClient _scoringsRegistryContractClient;
        private readonly IClock _clock;
        private readonly IScoringRepository _scoringRepository;
        private readonly IUserRepository _userRepository;

        public ScoringService(
            IProjectRepository projectRepository,
            IScoringOffersManagerContractClient scoringOffersManagerContractClient,
            IScoringsRegistryContractClient scoringsRegistryContractClient,
            IClock clock,
            IScoringRepository scoringRepository,
            IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _scoringOffersManagerContractClient = scoringOffersManagerContractClient;
            _scoringsRegistryContractClient = scoringsRegistryContractClient;
            _clock = clock;
            _scoringRepository = scoringRepository;
            _userRepository = userRepository;
        }

        public async Task<long> StartAsync(long projectId, IDictionary<AreaType, int> areas)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            var offers = await _scoringOffersManagerContractClient.GetOffersAsync(project.ExternalId);

            if (!offers.Any())
                throw new AppErrorException(ErrorCode.PendingOffersNotFound);

            return await CreateScoringAsync(project, areas, offers);
        }

        public Task<Scoring> GetByIdAsync(long scoringId)
            => _scoringRepository.GetByIdAsync(scoringId);

        private async Task<long> CreateScoringAsync(
            Project project,
            IDictionary<AreaType, int> areas,
            IReadOnlyCollection<ScoringOfferInfo> contractOffers)
        {
            var contractAddress = await _scoringsRegistryContractClient.GetScoringAddressAsync(project.ExternalId);

            var areaScorings = areas.Select(x => new AreaScoring {AreaId = x.Key, ExpertsCount = x.Value}).ToList();
            var offers = await CreateOffersAsync(contractOffers);
            var scoring = new Scoring(project.Id, contractAddress, _clock.UtcNow, areaScorings, offers);

            _scoringRepository.Add(scoring);
            await _scoringRepository.SaveChangesAsync();
            return scoring.Id;
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