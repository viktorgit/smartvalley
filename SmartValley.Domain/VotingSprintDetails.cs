﻿using System;
using System.Collections.Generic;

namespace SmartValley.Domain
{
    public class VotingSprintDetails
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public long AcceptanceThreshold { get; set; }

        public long MaximumScore { get; set; }

        public List<Guid> ProjectExternalIds { get; set; }

        public string Address { get; set; }
    }
}
