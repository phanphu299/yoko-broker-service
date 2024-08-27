namespace AHI.Infrastructure.Exception.Helper
{
    public static class ValidationExceptionHelper
    {
        public static EntityValidationException GenerateDuplicateValidation(string fieldName)
        {
            return EntityValidationExceptionHelper.GenerateException(
                fieldName,
                ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED);
        }

        public static EntityValidationException GenerateInvalidValidation(string fieldName, string errorCode)
        {
            return EntityValidationExceptionHelper.GenerateException(
                fieldName,
                errorCode);
        }
    }
}
