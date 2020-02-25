using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface IStudentTimetableService
    {
        CMSResult Save(StudentTimetable studentTimetable);
        IEnumerable<StudentTimetableGridModel> GetStudentExamTimetable(out int totalRecords, int userId,
       int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        StudentTimetableProjection GetStudentTimetableById(int id);
        CMSResult Delete(int StudentTimetableId);
        IEnumerable<StudentTimetableGridModel> GetStudentClassTimetable(out int totalRecords, int userId,
        int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        CMSResult Update(StudentTimetable oldTimeTable);
    }
}
