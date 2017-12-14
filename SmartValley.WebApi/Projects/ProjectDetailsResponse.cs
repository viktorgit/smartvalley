﻿using System.Collections.Generic;
using SmartValley.WebApi.Applications;

namespace SmartValley.WebApi.Projects
{
    public class ProjectDetailsResponse
    {
        public string Name { get; set; }

        public string AuthorAddress { get; set; }

        public string Area { get; set; }

        public string ProjectAddress { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public string WhitePaperLink { get; set; }

        public string BlockChainType { get; set; }

        public string Country { get; set; }

        public string FinanceModelLink { get; set; }

        public string MvpLink { get; set; }

        public string SoftCap { get; set; }

        public string HardCap { get; set; }

        public bool AttractedInvestments { get; set; }

        public double? Score { get; set; }

        public IReadOnlyCollection<TeamMemberResponse> TeamMembers { get; set; }
    }
}