using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataTrust.Pod.Api.Messages
{
    public abstract class BaseResponse : BaseMessage
    {
        public BaseResponse(Guid correlationId) : base()
        {
            base._correlationId = correlationId;
        }

        public BaseResponse()
        {
        }
    }
}
