﻿using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SmartValley.Application;
using SmartValley.Application.Contracts.Scorings;
using SmartValley.Domain;
using SmartValley.Domain.Entities;
using SmartValley.Domain.Interfaces;
using SmartValley.WebApi.Estimates.Requests;

namespace SmartValley.WebApi.Estimates
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class EstimationService : IEstimationService
    {
        private readonly IEstimateCommentRepository _estimateCommentRepository;
        private readonly EthereumClient _ethereumClient;
        private readonly IScoringContractClient _scoringContractClient;
        private readonly IScoringRepository _scoringRepository;

        public EstimationService(
            IEstimateCommentRepository estimateCommentRepository,
            EthereumClient ethereumClient,
            IScoringContractClient scoringContractClient,
            IScoringRepository scoringRepository)
        {
            _estimateCommentRepository = estimateCommentRepository;
            _ethereumClient = ethereumClient;
            _scoringContractClient = scoringContractClient;
            _scoringRepository = scoringRepository;
        }

        public async Task SubmitEstimatesAsync(SubmitEstimatesRequest request)
        {
            await _ethereumClient.WaitForConfirmationAsync(request.TransactionHash);

            await AddEstimateCommentsAsync(request);

            var scoring = await _scoringRepository.GetByProjectIdAsync(request.ProjectId);
            var scoringStatistics = await _scoringContractClient.GetScoringStatisticsAsync(scoring.ContractAddress);

            await UpdateProjectScoringAsync(scoring, scoringStatistics);
        }

        public async Task<ScoringStatisticsInArea> GetScoringStatisticsInAreaAsync(long projectId, Domain.Entities.AreaType areaType)
        {
            var scoring = await _scoringRepository.GetByProjectIdAsync(projectId);
            var scores = await _scoringContractClient.GetEstimatesAsync(scoring.ContractAddress);
            var comments = await _estimateCommentRepository.GetAsync(projectId, areaType);

            var estimates = (from comment in comments
                             join score in scores
                                 on new {comment.QuestionId, ExpertAddress = comment.ExpertAddress.ToUpper(CultureInfo.InvariantCulture)}
                                 equals new {score.QuestionId, ExpertAddress = score.ExpertAddress.ToUpper(CultureInfo.InvariantCulture)}
                             select CreateEstimate(score, comment)).ToArray();

            var requiredSubmissionsInArea = (double) await _scoringContractClient.GetRequiredSubmissionsInAreaCountAsync(scoring.ContractAddress);
            var averageScore = scoring.IsCompletedInArea(areaType)
                                   ? estimates.Sum(i => i.Score) / requiredSubmissionsInArea
                                   : (double?) null;

            return new ScoringStatisticsInArea(averageScore, estimates);
        }

        private static Estimate CreateEstimate(EstimateScore score, EstimateComment comment)
            => new Estimate(comment.ProjectId, score.ExpertAddress, score.QuestionId, score.Score, comment.Comment);

        private Task UpdateProjectScoringAsync(Domain.Entities.Scoring scoring, ProjectScoringStatistics scoringStatistics)
        {
            scoring.Score = scoringStatistics.Score;
            scoring.IsScoredByHr = scoringStatistics.IsScoredByHr;
            scoring.IsScoredByAnalyst = scoringStatistics.IsScoredByAnalyst;
            scoring.IsScoredByTechnical = scoringStatistics.IsScoredByTech;
            scoring.IsScoredByLawyer = scoringStatistics.IsScoredByLawyer;

            return _scoringRepository.UpdateWholeAsync(scoring);
        }

        private Task AddEstimateCommentsAsync(SubmitEstimatesRequest request)
        {
            var estimates = request
                            .EstimateComments
                            .Select(e => CreateEstimateComment(e, request.ProjectId, request.ExpertAddress))
                            .ToArray();

            return _estimateCommentRepository.AddRangeAsync(estimates);
        }

        private static EstimateComment CreateEstimateComment(
            EstimateCommentRequest estimateComment,
            long projectId,
            string expertAddress)
        {
            return new EstimateComment
                   {
                       ProjectId = projectId,
                       ExpertAddress = expertAddress,
                       QuestionId = estimateComment.QuestionId,
                       Comment = estimateComment.Comment
                   };
        }
    }
}