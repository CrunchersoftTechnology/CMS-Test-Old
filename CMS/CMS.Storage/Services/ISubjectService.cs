using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface ISubjectService
    {
        IEnumerable<SubjectProjection> GetAllSubjects();
        CMSResult Save(Subject subject, List<ClassProjection> classList);
        CMSResult Update(Subject subject);
        CMSResult Delete(int id);
        SubjectProjection GetSubjectById(int subjectId);
        IEnumerable<SubjectProjection> GetSubjects(int ClassId);
        IEnumerable<SubjectGridModel> GetSubjectData(out int totalRecords, string Name, int filterClassName,
        int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        IEnumerable<SubjectProjection> GetSubjectByClassId(int classId);
        IEnumerable<SubjectProjection> GetSubjectSubjectIds(string selectedSubject);
    }
}
