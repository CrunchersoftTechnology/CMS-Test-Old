using CMS.Common;
using CMS.Common.GridModels;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;

namespace CMS.Domain.Storage.Services
{
    public interface IBoardService
    {
        IEnumerable<BoardProjection> GetBoards();
        BoardProjection GetBoardById(int boardId);
        CMSResult Save(Board newBoard);
        CMSResult Update(Board oldBoard);
        CMSResult Delete(int id);
        IEnumerable<BoardGridModel> GetBoardData(out int totalRecords, string Name,
        int? limitOffset, int? limitRowCount, string orderBy, bool desc);
    }
}
