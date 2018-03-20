﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartValley.Application;
using SmartValley.Domain.Entities;
using SmartValley.WebApi.Experts;
using SmartValley.WebApi.Extensions;
using SmartValley.WebApi.Scoring.Requests;
using SmartValley.WebApi.Scoring.Responses;
using SmartValley.WebApi.WebApi;

namespace SmartValley.WebApi.Scoring
{
    [Route("api/scoring/offers")]
    [Authorize]
    public class ScoringOffersController : Controller
    {
        private readonly IScoringService _scoringService;
        private readonly EthereumClient _ethereumClient;
        private readonly IClock _clock;

        public ScoringOffersController(
            IScoringService scoringService,
            EthereumClient ethereumClient,
            IClock clock)
        {
            _scoringService = scoringService;
            _ethereumClient = ethereumClient;
            _clock = clock;
        }

        [HttpGet("pending")]
        public async Task<CollectionResponse<ScoringOfferResponse>> GetAllPendingAsync()
        {
            var offers = await _scoringService.GetPendingOfferDetailsAsync(User.GetUserId());
            var now = _clock.UtcNow;
            return new CollectionResponse<ScoringOfferResponse>
                   {
                       Items = offers.Select(o => ScoringOfferResponse.Create(o, now)).ToArray()
                   };
        }

        [HttpGet("accepted")]
        public async Task<CollectionResponse<ScoringOfferResponse>> GetAllAcceptedAsync()
        {
            var offers = await _scoringService.GetAcceptedOfferDetailsAsync(User.GetUserId());
            var now = _clock.UtcNow;
            return new CollectionResponse<ScoringOfferResponse>
                   {
                       Items = offers.Select(o => ScoringOfferResponse.Create(o, now)).ToArray()
                   };
        }

        [HttpGet("history")]
        public async Task<CollectionResponse<ScoringOfferResponse>> GetHistoryAsync()
        {
            var now = _clock.UtcNow;
            var offers = await _scoringService.GetExpertOffersHistoryAsync(User.GetUserId(), now);
            return new CollectionResponse<ScoringOfferResponse>
                   {
                       Items = offers.Select(o => ScoringOfferResponse.Create(o, now)).ToArray()
                   };
        }

        [HttpPut("accept")]
        public async Task<EmptyResponse> AcceptAsync([FromBody] AcceptRejectOfferRequest request)
        {
            await _ethereumClient.WaitForConfirmationAsync(request.TransactionHash);
            await _scoringService.AcceptOfferAsync(request.ScoringId, request.AreaId, User.GetUserId());
            return new EmptyResponse();
        }

        [HttpPut("reject")]
        public async Task<EmptyResponse> RejectAsync([FromBody] AcceptRejectOfferRequest request)
        {
            await _ethereumClient.WaitForConfirmationAsync(request.TransactionHash);
            await _scoringService.RejectOfferAsync(request.ScoringId, request.AreaId, User.GetUserId());
            return new EmptyResponse();
        }

        [HttpGet("status")]
        public async Task<ScoringOfferStatusResponse> GetOfferStatusAsync(GetScoringOfferStatusRequest request)
        {
            var offer = await _scoringService.GetOfferAsync(request.ProjectId, request.AreaType.ToDomain(), User.GetUserId());
            return new ScoringOfferStatusResponse {Status = GetOfferStatus(offer, _clock.UtcNow)};
        }

        [HttpPut]
        public async Task<EmptyResponse> UpdateOffersAsync([FromBody] UpdateOffersRequest request)
        {
            await _ethereumClient.WaitForConfirmationAsync(request.TransactionHash);
            await _scoringService.UpdateOffersAsync(request.ProjectExternalId);
            return new EmptyResponse();
        }

        private static OfferStatus GetOfferStatus(ScoringOffer offer, DateTimeOffset now)
        {
            if (offer == null)
                return OfferStatus.None;

            switch (offer.Status)
            {
                case ScoringOfferStatus.Pending:
                    return offer.ExpirationTimestamp < now ? OfferStatus.Timeout : OfferStatus.Pending;
                case ScoringOfferStatus.Accepted:
                    return OfferStatus.Accepted;
                case ScoringOfferStatus.Rejected:
                    return OfferStatus.Rejected;
                case ScoringOfferStatus.Finished:
                    return OfferStatus.Finished;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}