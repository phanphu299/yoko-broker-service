using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.FileParser.Constant;

namespace AHI.Broker.Function.FileParser.BaseExcelParser
{
    public class ParserContext : IParserContext
    {
        private readonly IDictionary<string, IImportTrackingService> _errorHandlers;
        private readonly IDictionary<ParseAction, string> _templateSubDirectoryMapping;
        private string _templateExecutionDirectory;
        private string _templateDirectory;
        private IDictionary<string, string> _formats;

        public ParserContext(IDictionary<string, IImportTrackingService> errorHandlers)
        {
            _errorHandlers = errorHandlers;
            _templateSubDirectoryMapping = new Dictionary<ParseAction, string>
            {
                {ParseAction.IMPORT, "ImportTemplate"},
                {ParseAction.EXPORT, "ExportTemplate"}
            };
            _formats = new Dictionary<string, string>();
        }

        public void SetExecutionContext(ExecutionContext context, ParseAction action)
        {
            _templateExecutionDirectory = context.FunctionAppDirectory;
            _templateDirectory = _templateSubDirectoryMapping[action];
        }
        public string GetTemplatePath(string templateName)
        {
            return System.IO.Path.Combine(_templateExecutionDirectory, "AppData", _templateDirectory, templateName);
        }

        public IImportTrackingService GetErrorTracking(string mimeType) => _errorHandlers[mimeType];

        public void SetContextFormat(string key, string format)
        {
            _formats[key] = format;
        }

        public string GetContextFormat(string key)
        {
            return _formats.TryGetValue(key, out var result) ? result : null;
        }
    }
}
