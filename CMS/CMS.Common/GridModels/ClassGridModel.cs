using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Common.GridModels
{
   public class ClassGridModel
    {
        public int ClassId{ get; set; }

        public string ClassName{ get; set; }
        [Exclude]
        public string Action { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
