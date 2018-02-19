﻿using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace SmartValley.Application.Contracts.Scorings.Dto
{
    [FunctionOutput]
    public class ScoringStatisticsDto
    {
        [Parameter("bool", "_isScored", 1)]
        public bool IsScored { get; set; }

        [Parameter("uint256", "_score", 2)]
        public int Score { get; set; }

        [Parameter("uint256[]", "_areas", 3)]
        public List<int> Areas { get; set; }

        [Parameter("bool[]", "_areaResults", 4)]
        public List<bool> AreaResults { get; set; }
    }
}