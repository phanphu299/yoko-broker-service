using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Validators;
using AHI.Broker.Function.Service.Abstraction;

namespace AHI.Broker.Function.Model.ImportModel.Validation
{
    public class MatchRegex : AsyncValidatorBase
    {
        private string _regexKey;
        private string _regexMessage;
        private readonly ISystemContext _systemContext;
        private bool _acceptNullEmpty;
        private string _modifyNameProperty;

        public MatchRegex(string regexKey, string regexMessage, ISystemContext systemContext, bool acceptNullEmpty = false, string modifyNameProperty = null) : base("{Message}")
        {
            _regexKey = regexKey;
            _systemContext = systemContext;
            _regexMessage = regexMessage;
            _acceptNullEmpty = acceptNullEmpty;
            _modifyNameProperty = modifyNameProperty;
        }

        protected async override Task<bool> IsValidAsync(PropertyValidatorContext context, CancellationToken cancellation)
        {
            var value = context.PropertyValue as string;
            if (_acceptNullEmpty && string.IsNullOrEmpty(value))
                return true;
            var propertyNameValid = context.PropertyName;
            if (!string.IsNullOrEmpty(_modifyNameProperty))
                propertyNameValid = _modifyNameProperty;
            if (string.IsNullOrEmpty(value))
            {
                context.MessageFormatter.AppendArgument("Message", $"{propertyNameValid} is null or empty");
                return false;
            }
            try
            {
                var szExpression = await _systemContext.GetValueAsync(_regexKey, null);
                //var szExpression = @"^[\w*\s*]{0,255}$";
                var regularExpression = new Regex(szExpression, RegexOptions.IgnoreCase);
                var result = regularExpression.IsMatch(value);
                if (!result)
                {
                    _regexMessage = await _systemContext.GetValueAsync(_regexMessage, null);
                    context.MessageFormatter.AppendArgument("Message", $"{propertyNameValid} {_regexMessage}");
                }

                return result;
            }
            catch
            {
                // for cant load regex from system
                return false;
            }


        }


    }
}
