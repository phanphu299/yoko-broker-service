using System.Collections.Generic;
using System.Linq;

namespace Broker.Api.Model
{
    public class ValidationResultApiResponse
    {
        public bool IsSuccess { get; }

        /// <summary>
        /// Validation result message.
        /// </summary>
        public string Message { get; set; } = "VALIDATION_ERROR";
        public string ErrorCode { get; set; }

        /// <summary>
        /// Property to its validation failures.
        /// </summary>
        public ICollection<FieldFailureMessage> Fields { get; }

        public ValidationResultApiResponse(bool isSuccess, string errorCode)
        {
            Fields = new LinkedList<FieldFailureMessage>();
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
        }

        public ValidationResultApiResponse(bool isSuccess, string errorCode, IDictionary<string, string[]> failures) : this(isSuccess, errorCode)
        {
            if (failures == null || !failures.Any())
                return;

            Fields = failures.SelectMany(fieldFailures => fieldFailures.Value.Select(item => new FieldFailureMessage
            {
                Name = fieldFailures.Key,
                ErrorCode = item
            })).ToList();
        }

        public class FieldFailureMessage
        {
            public string Name { get; set; }
            public string ErrorCode { get; set; }
        }
    }
}
