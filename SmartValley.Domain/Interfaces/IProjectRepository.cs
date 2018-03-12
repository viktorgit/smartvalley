﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartValley.Domain.Core;
using SmartValley.Domain.Entities;

namespace SmartValley.Domain.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project> GetByIdAsync(long id);

        Task<IReadOnlyCollection<ProjectScoring>> GetAllScoredAsync();

        Task<int> AddAsync(Project project);

        Task<IReadOnlyCollection<ProjectScoring>> GetByAuthorAsync(Address authorAddress);

        Task<IReadOnlyCollection<ProjectScoring>> GetForScoringAsync(Address expertAddress, AreaType areaType);

        Task<Project> GetByExternalIdAsync(Guid externalId);

        Task<IReadOnlyCollection<Project>> GetByExternalIdsAsync(IReadOnlyCollection<Guid> externalIds);
    }
}