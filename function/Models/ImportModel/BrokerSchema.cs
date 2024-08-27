using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using ValidationMessage = AHI.Broker.Function.Constant.MessageConstants.FluentValidation;

namespace AHI.Broker.Function.Model.ImportModel
{
    public enum DetailDataType
    {
        @null,
        text,
        number,
        @bool,
        select
    }

    public class BrokerSchema
    {
        public IEnumerable<SchemaDetail> Details { get; private set; }

        public BrokerSchema(IEnumerable<SchemaDetail> details)
        {
            Details = details;
        }

        public bool Validate(BrokerModel broker, Action<Exception> errorHandler)
        {
            StandardizeSetting(broker);
            var isValid = true;
            foreach (var detail in Details)
            {
                try
                {
                    detail.Validate(broker);
                }
                catch (Exception e)
                {
                    errorHandler.Invoke(e);
                    isValid &= false;
                }
            }
            return isValid;
        }

        private void StandardizeSetting(BrokerModel broker)
        {
            // remove redundant data
            var currentKeys = Details.Select(detail => detail.Key);
            var keysToRemove = broker.Settings.Keys.Where(key => !currentKeys.Contains(key.ToLowerInvariant())).ToArray();
            foreach (var key in keysToRemove)
            {
                broker.Settings.Remove(key);
            }

            // lowercase keys
            var keysToLower = broker.Settings.Keys.Where(key => key != key.ToLowerInvariant()).ToArray();
            foreach (var key in keysToLower)
            {
                broker.Settings[key.ToLowerInvariant()] = broker.Settings[key];
                broker.Settings.Remove(key);
            }
        }
    }

    public class SchemaDetail
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public DetailDataType DataType { get; set; }
        public string Regex { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public ICollection<SchemaOption> Options { get; } = new List<SchemaOption>();

        public void Validate(BrokerModel broker)
        {
            if (!broker.Settings.TryGetValue(Key, out var currentValue) || currentValue == null)
            {
                var error = new ValidationFailure(Name, ValidationMessage.REQUIRED)
                {
                    FormattedMessagePlaceholderValues = new Dictionary<string, object>
                    {
                        { "propertyName", Name }
                    }
                };
                throw new ValidationException(new List<ValidationFailure>{ error });
            }

            if (!TryParseByDetailDataType(currentValue, out var value))
            {
                // $"{propertyName} has invalid value for type {DataType}"
                var error = new ValidationFailure(Name, ValidationMessage.INVALID_VALUE_TYPE, currentValue)
                {
                    FormattedMessagePlaceholderValues = new Dictionary<string, object>
                    {
                        { "propertyName", Name },
                        { "propertyValue", currentValue },
                        { "propertyType", currentValue }
                    }
                };
                throw new ValidationException(new List<ValidationFailure>{ error });
            }

            if (!ValidateOption(ref value))
            {
                // $"{propertyName} must be one of these values: [{propertyOption}]"
                var error = new ValidationFailure(Name, ValidationMessage.INVALID_OPTION, value)
                {
                    FormattedMessagePlaceholderValues = new Dictionary<string, object>
                    {
                        { "propertyName", Name },
                        { "propertyOption", string.Join(", ", Options.Select(x => x.Name)) }
                    }
                };
                throw new ValidationException(new List<ValidationFailure>{ error });
            }

            if (!ValidateRegex(value))
            {
                var error = new ValidationFailure(Name, ValidationMessage.GENERAL_INVALID, value)
                {
                    FormattedMessagePlaceholderValues = new Dictionary<string, object>
                    {
                        { "propertyName", Name },
                        { "propertyValue", value }
                    }
                };
                throw new ValidationException(new List<ValidationFailure>{ error });
            }

            if (!ValidateRange(value))
            {
                var error = new ValidationFailure(Name, ValidationMessage.OUT_OF_RANGE, value)
                {
                    FormattedMessagePlaceholderValues = new Dictionary<string, object>
                    {
                        { "propertyName", Name },
                        { "from", MinValue },
                        { "to", MaxValue }
                    }
                };
                throw new ValidationException(new List<ValidationFailure>{ error });
            }

            broker.Settings[Key] = value;
        }

        private bool TryParseByDetailDataType(object value, out object result)
        {
            try
            {
                result = DataType switch
                {
                    DetailDataType.text => value.ToString(),
                    DetailDataType.select => value.ToString(),
                    DetailDataType.@bool => Boolean.Parse(value.ToString()),
                    DetailDataType.number => Convert.ToDecimal(value),
                    _ => value.ToString()
                };
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private bool ValidateOption(ref object value)
        {
            if (DataType != DetailDataType.select)
                return true;

            if (value is string)
            {
                var optionName = (string)value;
                var validOption = Options.FirstOrDefault(option => option.Name.Equals(optionName, StringComparison.InvariantCultureIgnoreCase));
                if (validOption != null)
                {
                    value = validOption.Id;
                    return true;
                }
            }
            return false;
        }

        private bool ValidateRegex(object value)
        {
            if (DataType != DetailDataType.text || Regex is null)
                return true;

            if (value is string)
                return System.Text.RegularExpressions.Regex.IsMatch(value as string, Regex, RegexOptions.IgnoreCase);

            return false;
        }

        private bool ValidateRange(object value)
        {
            if (DataType != DetailDataType.number || (MinValue == null && MaxValue == null))
                return true;

            if (value is decimal)
            {
                var number = (decimal)value;
                return (MinValue == null || number >= MinValue.Value) && (MaxValue == null || number <= MaxValue.Value);
            }

            return false;
        }
    }

    public class SchemaOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}