using System;
using System.Collections.Generic;
using AHI.Broker.Function.Model.ImportModel;

namespace AHI.Broker.Function.Constant
{
    public static class EntityFileMapping
    {
        // mapping between model and the file type supported for file operation on that model
        private static readonly IDictionary<string, string> _entityFileMapping = new Dictionary<string, string>() {
            {IOEntityType.BROKER, MimeType.JSON}
        };

        // mapping between model and the model specific entity type, use when invoke generic method
        private static readonly IDictionary<string, Type> _entityTypeMapping = new Dictionary<string, Type>() {
            {IOEntityType.BROKER, typeof(BrokerModel)}
        };

        public static string GetMimeType(string entityType) => _entityFileMapping[entityType];
        public static Type GetEntityType(string entityType) => _entityTypeMapping[entityType];
    }
}
