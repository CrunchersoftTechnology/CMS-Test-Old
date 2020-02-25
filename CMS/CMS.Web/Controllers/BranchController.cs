using CMS.Common;
using CMS.Domain.Infrastructure;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using CMS.Domain.Storage.Services;
using CMS.Web.Logger;
using CMS.Web.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Web.Controllers
{
    [Authorize(Roles = Constants.AdminRole)]
    public class BranchController : BaseController
    {
        readonly IBranchService _branchService;
        readonly ILogger _logger;
        readonly IRepository _repository;

        public BranchController(IBranchService branchService, ILogger logger, IRepository repository)
        {
            _branchService = branchService;
            _logger = logger;
            _repository = repository;
        }

        // GET: Branch
        public ActionResult Index()
        {
            //var branches = _branchService.GetAllBranches().ToList();
            //var viewModelList = AutoMapper.Mapper.Map<List<BranchProjection>, BranchViewModel[]>(branches);
            //return View(viewModelList);
            return View();
        }

        // GET: Branch/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Branch/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BranchViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var branch = new Branch
                {
                    Address = viewModel.Address,
                    Name = viewModel.Name
                };

                var result = _branchService.Save(branch);
                if (result.Success)
                {

                    Success(result.Results.FirstOrDefault().Message);
                    ModelState.Clear();
                }
                else
                {
                    _logger.Warn(result.Results.FirstOrDefault().Message);
                    Warning(result.Results.FirstOrDefault().Message, true);
                }
            }

            viewModel = new BranchViewModel();

            return View(viewModel);
        }

        // GET: Branch/Edit/5
        public ActionResult Edit(int id)
        {
            var projection = _branchService.GetBoardById(id);
            if (projection == null)
            {
                _logger.Warn(string.Format("Branch does not Exists {0}.", id));
                Warning("Branch does not Exists.");
                return RedirectToAction("Index");
            }
            var viewModel = AutoMapper.Mapper.Map<BranchProjection, BranchViewModel>(projection);
            return View(viewModel);
        }

        // POST: Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BranchViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var board = _repository.Project<Branch, bool>(branches => (from b in branches where b.BranchId == viewModel.BranchId select b).Any());
                if (!board)
                {
                    _logger.Warn(string.Format("Branch not exists '{0}'.", viewModel.Name));
                    Danger(string.Format("Branch not exists '{0}'.", viewModel.Name));
                }
                var result = _branchService.Update(new Branch
                {
                    Name = viewModel.Name,
                    Address = viewModel.Address,
                    BranchId = viewModel.BranchId
                });
                if (result.Success)
                {
                    Success(result.Results.FirstOrDefault().Message);
                    ModelState.Clear();
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.Warn(result.Results.FirstOrDefault().Message);
                    Warning(result.Results.FirstOrDefault().Message, true);
                }
            }
            return View(viewModel);
        }

        // GET: Branch/Delete/5
        public ActionResult Delete(int id)
        {
            var projection = _branchService.GetBoardById(id);
            if (projection == null)
            {
                _logger.Warn(string.Format("Board does not Exists {0}.", id));
                Warning("Board does not Exists.");
                return RedirectToAction("Index");
            }
            var viewModel = AutoMapper.Mapper.Map<BranchProjection, BranchViewModel>(projection);
            return View(viewModel);
        }

        // POST: Branch/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(BranchViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = _branchService.Delete(viewModel.BranchId);
                if (result.Success)
                {
                    Success(result.Results.FirstOrDefault().Message);
                    ModelState.Clear();
                }
                else
                {
                    _logger.Warn(result.Results.FirstOrDefault().Message);
                    Warning(result.Results.FirstOrDefault().Message, true);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
