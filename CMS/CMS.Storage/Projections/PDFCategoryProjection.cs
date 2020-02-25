namespace CMS.Domain.Storage.Projections
{
    public class PDFCategoryProjection
    {
        public int PDFCategoryId { get; set; }
        
        public string Name { get; set; }

        public string DelimitedPdfIds { get; set; }
    }
}
