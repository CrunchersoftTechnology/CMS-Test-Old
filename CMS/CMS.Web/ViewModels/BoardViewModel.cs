using System.ComponentModel.DataAnnotations;

namespace CMS.Web.ViewModels
{
    public class BoardViewModel
    {
        public int BoardId { get; set; }

        [RegularExpression("^[a-zA-Z&]+[a-zA-Z&\\- ]+$", ErrorMessage = "Board Name should contain A-Z, a-z,&, -.")]
        [Required]
        [MaxLength(50, ErrorMessage = "The field Board Name must be  a minimum length of '3' and maximum length of '50'.")]
        [MinLength(3, ErrorMessage = "The field Board Name must be  a minimum length of '3' and maximum length of '50'.")]
        [Display(Name = "Board Name")]
        public string Name { get; set; }
    }
}