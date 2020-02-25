using System.ComponentModel.DataAnnotations;

namespace CMS.Web.ViewModels
{
    public class ClassViewModel
    {
        public int ClassId { get; set; }

        //[RegularExpression("^[a-zA-Z0-9]+[0-9]*[a-zA-Z\\- ]+$", ErrorMessage = "Class Name should contain 0-9, a-z, -, A-Z.")]
        [Required]
        [MaxLength(50, ErrorMessage = "The field Class Name must be a minimum length of '1' and maximum length of '50'.")]
        [MinLength(1, ErrorMessage = "The field Class Name must be a minimum length of '1' and maximum length of '50'.")]
        [Display(Name = "Class Name")]
        public string Name { get; set; }
    }
}