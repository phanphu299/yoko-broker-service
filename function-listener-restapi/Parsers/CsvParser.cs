using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace AHI.Broker.Function.Parser
{
    public static class CsvParser
    {
        public static IEnumerable<IDictionary<string, object>> ParseCsvData(Stream contentStream)
        {
            using (var parser = new TextFieldParser(contentStream))
            {
                parser.SetDelimiters(new[] { "," });
                parser.HasFieldsEnclosedInQuotes = true;

                // read header line
                string[] headers = null;
                if (parser.EndOfData || (headers = parser.ReadFields()) is null)
                    return Array.Empty<IDictionary<string, object>>();

                // read data lines
                var result = new List<IDictionary<string, object>>();
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields() ?? Array.Empty<string>();
                    var data = new Dictionary<string, object>();
                    for (int i = 0; i < headers.Length && i < fields.Length; i++)
                    {
                        data[headers[i]] = fields[i];
                    }
                    result.Add(data);
                }
                return result;
            }
        }
    }
}