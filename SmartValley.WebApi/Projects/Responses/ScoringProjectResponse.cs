﻿using System;
using System.Collections.Generic;
using System.Linq;
using SmartValley.WebApi.Scoring;

namespace SmartValley.WebApi.Projects.Responses
{
    public class ScoringProjectResponse
    {
        public string ProjectId { get; set; }

        public string Address { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ScoringProjectStatus Status { get; set; }

        public IEnumerable<AreaExpertResponse> AreasExperts { get; set; }

        public static ScoringProjectResponse Create(ScoringProjectDetailsWithCounts details)
        {
            return new ScoringProjectResponse
                   {
                       Address = details.Address,
                       Name = details.Name,
                       ProjectId = details.ProjectId.ToString(),
                       StartDate = details.CreationDate.Date,
                       EndDate = details.OffersEndDate.Date,
                       Status = details.Status,
                       AreasExperts = details.AreaCounts.Select(AreaExpertResponse.Create).ToArray()
                   };
        }
    }
}