﻿using System;
using System.Collections.Generic;
using SmartValley.Domain;
using SmartValley.Domain.Core;
using SmartValley.WebApi.Projects;

namespace SmartValley.WebApi.Scoring
{
    public class ScoringProjectDetailsWithCounts : ScoringProjectDetails
    {
        public ScoringProjectDetailsWithCounts(
            ScoringProjectStatus status,
            IReadOnlyCollection<AreaCount> areaCounts,
            long projectId,
            long scoringId,
            Address address,
            string name,
            DateTimeOffset creationDate,
            DateTimeOffset offersEndDate)
            : base(projectId, scoringId, address, name, creationDate, offersEndDate)
        {
            Status = status;
            AreaCounts = areaCounts;
        }

        public ScoringProjectStatus Status { get; }

        public IReadOnlyCollection<AreaCount> AreaCounts { get; }
    }
}