using Microsoft.Azure.WebJobs;
using AHI.Broker.Function.FileParser.Constant;

namespace AHI.Broker.Function.FileParser.Abstraction
{
    public interface IParserContext
    {
        void SetExecutionContext(ExecutionContext context, ParseAction action);
        string GetTemplatePath(string templateName);
        IImportTrackingService GetErrorTracking(string mimeType);
        void SetContextFormat(string key, string format);
        string GetContextFormat(string key);
    }
}
