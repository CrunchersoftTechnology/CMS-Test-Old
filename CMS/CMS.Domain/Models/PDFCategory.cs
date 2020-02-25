using CMS.Domain.Infrastructure;

namespace CMS.Domain.Models
{
    public class PDFCategory : AuditableEntity
    {
        public int PDFCategoryId { get; set; }

        public string Name { get; set; }
    }
}
