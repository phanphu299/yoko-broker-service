using System;
using AHI.Infrastructure.Repository.Generic;

namespace Broker.Application.Repository.Abstraction
{
    public interface ISchemaRepository : ISearchRepository<Domain.Entity.Schema, Guid>
    {

    }
}
