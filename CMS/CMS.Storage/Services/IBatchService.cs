using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface IBatchService
    {
        IEnumerable<BatchProjection> GetBatches(int classId);
        IEnumerable<BatchProjection> GetAllBatches();
        CMSResult Save(Batch batch);
        CMSResult Update(Batch batch);
        CMSResult Delete(int id);
        BatchProjection GetBatcheById(int batchId);
        IEnumerable<BatchProjection> GetAllBatchesByClassId(int classId);
        IEnumerable<BatchProjection> GetAllBatchClsId(int classId);
        IEnumerable<BatchProjection> GetBatchesByClassId(int classId);
        IEnumerable<BatchProjection> GetBatchesByClassIds(string selectedClasses);
        int GetBatchesCount();
        IEnumerable<BatchGridModel> GetBatchData(out int totalRecords, int filterClassName,
          int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        IEnumerable<BatchProjection> GetBatchesByBatchIds(string SelectedBatches);
        IEnumerable<BatchProjection> GetBatchesBySubjectId(int sujectId);
    }
}
