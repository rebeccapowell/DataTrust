using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTrust.Pod.Api.Messages;

namespace DataTrust.Pod.Api.EndPoints.TokenEndPoints
{
    public class CreateTokenResult : BaseResponse
    {
        public CreateTokenResult(Guid correlationId) : base(correlationId)
        {
        }

        public CreateTokenResult()
        {
        }

        public string Token { get; set; }
    }
}
