using System;

namespace CMS.Common.GridModels
{
    public class PDFCategoryGridModel
    {
        public int PDFCategoryId { get; set; }

        public string Name { get; set; }

        [Exclude]
        public string Action { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
