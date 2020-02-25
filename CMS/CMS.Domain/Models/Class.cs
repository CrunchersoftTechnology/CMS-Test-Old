using CMS.Domain.Infrastructure;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    public class Class : AuditableEntity
    {
        public int ClassId { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }

        public virtual ICollection<PDFUpload> PDFUploads { get; set; }
    }
}
