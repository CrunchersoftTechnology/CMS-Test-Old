using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface IClassService
    {
        IEnumerable<ClassProjection> GetClasses();
        CMSResult Save(Class newClass);
        CMSResult Update(Class oldClass);
        CMSResult Delete(int id);
        ClassProjection GetClassById(int classId);
        IEnumerable<ClassGridModel> GetClassData(out int totalRecords, string Name,
     int? limitOffset, int? limitRowCount, string orderBy, bool desc);
    }
}