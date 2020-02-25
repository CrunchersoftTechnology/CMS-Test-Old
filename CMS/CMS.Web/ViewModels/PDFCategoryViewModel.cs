using System.ComponentModel.DataAnnotations;

namespace CMS.Web.ViewModels
{
    public class PDFCategoryViewModel
    {
        public int PDFCategoryId { get; set; }

        [RegularExpression("^[a-zA-Z0-9]+[a-zA-Z0-9\\- ]+$", ErrorMessage = "PDF Category should contain A-Z, a-z,0-9, -.")]
        [Required]
        [MaxLength(50, ErrorMessage = "The field PDF Category Name must be a minimum length of '2' and maximum length of '50'.")]
        [MinLength(2, ErrorMessage = "The field PDF Category Name must be a minimum length of '2' and maximum length of '50'.")]
        [Display(Name = "PDF Category Name")]
        public string Name { get; set; }
    }
}