using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface IPDFCategoryService
    {
        CMSResult Save(PDFCategory newPDFCategory);
        CMSResult Update(PDFCategory oldPDFCategory);
        CMSResult Delete(int id);
        IEnumerable<PDFCategoryProjection> GetPDFCategories();
        PDFCategoryProjection GetPDFCategoryById(int pdfCategoryId);
     

        IEnumerable<PDFCategoryGridModel> GetPDFCategoryData(out int totalRecords,
            int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        object GetPDFCategoryById(List<int> pdfIds);
    }
}
