using System;
using AHI.Infrastructure.SharedKernel.Model;

namespace Broker.Application.Models
{
    public class ActivityResponse : BaseResponse
    {
        public Guid ActivityId { get; set; }

        public ActivityResponse(Guid activityId)
            : base(true, null)
        {
            ActivityId = activityId;
        }
    }
}
