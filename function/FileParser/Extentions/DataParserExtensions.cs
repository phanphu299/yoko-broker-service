using System;
using System.Collections.Generic;
using AHI.Broker.Function.Model.ImportModel;
using AHI.Broker.Function.Model.ImportModel.Validation;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.FileParser.BaseExcelParser;
using AHI.Broker.Function.FileParser.ErrorTracking;
using AHI.Broker.Function.FileParser.ErrorTracking.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using AHI.Broker.Function.Constant;

namespace AHI.Broker.Function.FileParser.Extension
{
    public static class DataParserExtensions
    {
        public static void AddDataParserServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IParserContext, ParserContext>();

            serviceCollection.AddScoped<JsonTrackingService>();
            serviceCollection.AddScoped<IExportTrackingService, ExportTrackingService>();
            serviceCollection.AddScoped<IJsonTrackingService>(service => service.GetRequiredService<JsonTrackingService>());
            serviceCollection.AddScoped<IDictionary<string, IImportTrackingService>>(service =>
            {
                return new Dictionary<string, IImportTrackingService> {
                    {MimeType.JSON, service.GetRequiredService<JsonTrackingService>()}
                };
            });

            serviceCollection.AddScoped<IValidator<BrokerModel>, BrokerValidation>();
            // serviceCollection.AddScoped<IValidator<EvenHub>, EventHubValidation>();
            serviceCollection.AddScoped<IDictionary<Type, IValidator>>(service =>
            {
                return new Dictionary<Type, IValidator> {
                    {typeof(BrokerModel), service.GetRequiredService<IValidator<BrokerModel>>()},
                    // {typeof(EvenHub), service.GetRequiredService<IValidator<EvenHub>>()}
                };
            });
        }
    }
}
