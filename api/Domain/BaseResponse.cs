using System.Net;

namespace StargateAPI.Domain
{
    public class BaseResponse
    {
        public bool Success { get; set; } = true;
        public int ResponseCode { get; set; } = (int)HttpStatusCode.OK;
        public ErrorDetail? Error { get; set; }
    }
}
