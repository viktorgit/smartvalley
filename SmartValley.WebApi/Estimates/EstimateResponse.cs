﻿using SmartValley.Domain;
using SmartValley.Domain.Entities;

namespace SmartValley.WebApi.Estimates
{
    public class EstimateResponse
    {
        public int Score { get; set; }

        public string Comment { get; set; }

        public static EstimateResponse Create(Estimate estimate)
        {
            return new EstimateResponse
                   {
                       Score = estimate.Score,
                       Comment = estimate.Comment
                   };
        }
    }
}