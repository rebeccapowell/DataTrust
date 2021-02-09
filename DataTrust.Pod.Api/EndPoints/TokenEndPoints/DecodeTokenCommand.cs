using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataTrust.Pod.Api.Messages;

namespace DataTrust.Pod.Api.EndPoints.TokenEndPoints
{
    public class DecodeTokenCommand : BaseRequest
    {
        // localize this
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
    }
}
