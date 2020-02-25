using CMS.Domain.Infrastructure;

namespace CMS.Domain.Models
{
    public class Board : AuditableEntity
    {
        public int BoardId { get; set; }
        public string Name { get; set; }
    }
}
