using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Broker.Application.Service.Abstractions;
using FluentValidation.Results;

namespace Configuration.Application.Service
{
    public class RemoteValidateService : IRemoteValidateService
    {
        #region Properties

        private readonly ISystemContext _systemContext;

        #endregion

        #region Constructors

        public RemoteValidateService(ISystemContext systemContext)
        {
            _systemContext = systemContext;
        }

        #endregion

        #region Methods

        public async Task<ValidationFailure[]> ValidateByKeyAsync<T>(string propertyName, T value, string keyPrefix, bool useCache = true)
        {
            var descriptionKey = $"{keyPrefix}.description";
            var ruleKey = $"{keyPrefix}.rule";


            var validationFailures = new LinkedList<ValidationFailure>();
            var loadRegexTask =  _systemContext.GetValueAsync(ruleKey, null);
            var loadDescriptionTask = _systemContext.GetValueAsync(descriptionKey, $"{propertyName} is invalid");

            // Wait for all tasks to be complete.
            await Task.WhenAll(loadRegexTask, loadDescriptionTask);
            var szRegex = loadRegexTask.Result;
            var description = loadDescriptionTask.Result;

            if (string.IsNullOrEmpty(szRegex))
                return new ValidationFailure[0];

            if (value is string actualValue && !string.IsNullOrWhiteSpace(actualValue))
            {
                var doesRegexMatch = Regex.Match(actualValue, szRegex);
                if (!doesRegexMatch.Success)
                    validationFailures.AddLast(new ValidationFailure(propertyName, description ?? $"This validator key {ruleKey} does not have any description.", actualValue));
            }

            return validationFailures.ToArray();
        }

        #endregion
    }
}
