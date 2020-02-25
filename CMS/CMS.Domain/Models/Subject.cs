using CMS.Domain.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Domain.Models
{
    public class Subject : AuditableEntity
    {
        public int SubjectId { get; set; }

        public string Name { get; set; }

        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        public virtual ICollection<Chapter> Chapters { get; set; }

        public virtual ICollection<MasterFee> MasterFees { get; set; }

        public virtual ICollection<Student> Students { get; set; }

        public Subject()
        {
            Students = new HashSet<Student>();
        }
    }
}
