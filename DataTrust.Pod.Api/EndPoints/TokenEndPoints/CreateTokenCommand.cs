using System.ComponentModel.DataAnnotations;
using DataTrust.Pod.Api.Messages;

namespace DataTrust.Pod.Api.EndPoints.TokenEndPoints
{
    public class CreateTokenCommand : BaseRequest
    {
        // consider splitting this up into family and given name
        [Required(ErrorMessage = "Fullname is required")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Telephone is required")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }
}
