using System;
using System.ComponentModel.DataAnnotations;

namespace CMS.Web.ViewModels
{
    public class BranchViewModel
    {
        public int BranchId { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9]+[a-zA-Z0-9\\-., ]*$", ErrorMessage = "Name should contain A-Z, a-z,0-9, dash, comma.")]
        [MinLength(5, ErrorMessage = "The field Name must be a minimum length of '5' and maximum length of '100'.")]
        [MaxLength(100, ErrorMessage = "The field Name must be a minimum length of '5' and maximum length of '100'.")]
        public string Name { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "The field Address must be a minimum length of '5' and maximum length of '150'.")]
        [MaxLength(150, ErrorMessage = "The field Address must be a minimum length of '5' and maximum length of '150'.")]
        public string Address { get; set; }        
    }
}