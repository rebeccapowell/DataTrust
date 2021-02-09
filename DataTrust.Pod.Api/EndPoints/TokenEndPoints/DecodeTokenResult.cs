using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTrust.Pod.Api.Messages;

namespace DataTrust.Pod.Api.EndPoints.TokenEndPoints
{
    public class DecodeTokenResult : BaseResponse
    {
        public DecodeTokenResult(Guid correlationId) : base(correlationId)
        {
        }

        public DecodeTokenResult()
        {
        }

        public Dictionary<string, string> Claims { get; set; }
    }
}
