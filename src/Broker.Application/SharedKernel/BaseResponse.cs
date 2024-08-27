namespace Broker.Application.SharedKernel
{
    public class BaseResponse : AHI.Infrastructure.SharedKernel.Model.BaseResponse
    {
        public int ErrorCode { get; set; }
        public BaseResponse(bool isSuccess, string message) : base(isSuccess, message)
        {
        }
        public BaseResponse(bool isSuccess, string message, int errorCode) : base(isSuccess, message)
        {
            ErrorCode = errorCode;
        }
        public static new BaseResponse Success
        {
            get
            {
                return new BaseResponse(true, null);
            }
        }
        public static new BaseResponse Failed
        {
            get
            {
                return new BaseResponse(false, null);
            }
        }
    }
}
