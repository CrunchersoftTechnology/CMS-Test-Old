using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface IBranchService
    {
        CMSResult Save(Branch branch);
        CMSResult Update(Branch branch);
        CMSResult Delete(int branchId);
        IEnumerable<BranchProjection> GetAllBranches();
        BranchProjection GetBoardById(int id);
        int GetBranchesCount();
        IEnumerable<BranchGridModel> GetBranchData(out int totalRecords, string BranchName,
     int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        IEnumerable<BranchProjection> GetBranchByMultipleBranchId(string selectedBranch);
    }
}
