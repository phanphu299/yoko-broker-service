namespace Broker.Application.SharedKernel
{
    public class BaseCriteria
    {
        public int _pageSize { get; set; } = 10;
        public const int maxPageSize = 100;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }
        public OrderByBase OrderBy { get; set; }
    }
}
