using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;

namespace Broker.Application.SharedKernel
{
    public class ResponsePaginateList<T> : BaseSearchResponse<T>
    {
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public new IEnumerable<T> Data { get; set; } = new List<T>();
    }
}
