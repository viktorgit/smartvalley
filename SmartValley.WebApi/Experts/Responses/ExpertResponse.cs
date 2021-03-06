﻿using System.Collections.Generic;
using System.Linq;
using SmartValley.Domain.Entities;

namespace SmartValley.WebApi.Experts.Responses
{
    public class ExpertResponse
    {
        public long Id { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string About { get; set; }

        public string Bitcointalk { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsInHouse { get; set; }

        public IReadOnlyCollection<AreaResponse> Areas { get; set; }

        public static ExpertResponse Create(Expert expert)
        {
            return new ExpertResponse
                   {
                       Id = expert.UserId,
                       Address = expert.User.Address,
                       Email = expert.User.Email,
                       About = expert.User.About,
                       Bitcointalk = expert.User.BitcointalkLink,
                       IsAvailable = expert.IsAvailable,
                       IsInHouse = expert.IsInHouse,
                       FirstName = expert.User.FirstName,
                       SecondName = expert.User.SecondName,
                       Areas = expert.ExpertAreas.Select(j => new AreaResponse {Id = j.Area.Id.FromDomain(), Name = j.Area.Name}).ToArray()
                   };
        }
    }
}
