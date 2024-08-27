using System.Collections.Generic;

namespace Device.Domain.Entity
{
    public class WaylayQueryResultDataDto
    {
        public IEnumerable<WaylayQuerySeries> Series { get; set; }
    }
    public class WaylayQuerySeries : List<double?>
    {

    }
}