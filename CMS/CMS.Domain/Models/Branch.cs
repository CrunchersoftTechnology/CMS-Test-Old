using CMS.Domain.Infrastructure;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Domain.Models
{
    public class Branch : AuditableEntity
    {
        public Branch()
        {
            IsChangeDetected = false;
        }
        public int BranchId { get; set; }

        //[Required]
        //[MaxLength(100)]
        //public string Name { get; set; }

        private string name;

        [Required]
        [MaxLength(100)]
        public string Name
        {
            get { return name; }
            set
            {
                if (name != null &&  value != name )
                    IsChangeDetected = true;
                name = value;
            }
        }


        //[Required]
        //[MaxLength(150)]
        //public string Address { get; set; }

        private string address;

        [Required]
        [MaxLength(150)]
        public string Address
        {
            get { return address; }
            set {
                if (address != null && value != address)
                    IsChangeDetected = true;
                address = value;
            }
        }

        [NotMapped]
        public bool IsChangeDetected { get; set; }
    }
}
