﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SmartValley.Domain.Entities;

namespace SmartValley.Domain.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IReadOnlyCollection<Question>> GetAllAsync();
    }
}