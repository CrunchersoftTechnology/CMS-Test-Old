using CMS.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace CMS.Domain.Models
{
    public class School : AuditableEntity
    {
        public int SchoolId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string CenterNumber { get; set; }
    }
}
