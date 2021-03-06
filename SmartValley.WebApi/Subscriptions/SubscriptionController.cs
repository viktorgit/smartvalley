﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartValley.Data.SQL.Core;
using SmartValley.Data.SQL.Extensions;
using SmartValley.Domain.Entities;
using SmartValley.WebApi.Extensions;
using SmartValley.WebApi.Subscriptions.Requests;
using SmartValley.WebApi.Subscriptions.Responses;
using SmartValley.WebApi.WebApi;

namespace SmartValley.WebApi.Subscriptions
{
    [Route("api/subscriptions")]
    public class SubscriptionController : Controller
    {
        private readonly IReadOnlyDataContext _readContext;
        private readonly IEditableDataContext _editContext;

        public SubscriptionController(
            IReadOnlyDataContext readContext,
            IEditableDataContext editContext)
        {
            _readContext = readContext;
            _editContext = editContext;
        }

        [HttpPost("{projectId}")]
        public async Task<IActionResult> Post(long projectId, [FromBody] CreateSubscriptionRequest request)
        {
            _editContext.Subscriptions.Add(new Subscription(projectId, request.Name, request.Phone, request.Email, request.Sum));
            await _editContext.SaveAsync();

            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = nameof(RoleType.Admin))]
        public async Task<PartialCollectionResponse<SubscriptionResponse>> Get(CollectionPageRequest request)
        {
            var subscriptions = await (from subscriber in _readContext.Subscriptions
                                       join project in _readContext.Projects on subscriber.ProjectId equals project.Id
                                       select SubscriptionResponse.Create(subscriber, project))
                                    .GetPageAsync(request.Offset, request.Count);

            return subscriptions.ToPartialCollectionResponse();
        }
    }
}