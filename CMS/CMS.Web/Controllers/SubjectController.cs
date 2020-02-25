using CMS.Common;
using CMS.Domain.Infrastructure;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using CMS.Domain.Storage.Services;
using CMS.Web.Helpers;
using CMS.Web.Logger;
using CMS.Web.Models;
using CMS.Web.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace CMS.Web.Controllers
{
    [Authorize(Roles = Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole)]
    public class SubjectController : BaseController
    {
        readonly IClassService _classService;
        readonly ILogger _logger;
        readonly IRepository _repository;
        readonly ISubjectService _subjectService;
        readonly IEmailService _emailService;
        readonly IAspNetRoles _aspNetRolesService;
        readonly IBranchAdminService _branchAdminService;

        public SubjectController(IClassService classService, ILogger logger, IRepository repository, ISubjectService subjectService, IEmailService emailService, IAspNetRoles aspNetRolesService, IBranchAdminService branchAdminService)
        {
            _classService = classService;
            _logger = logger;
            _repository = repository;
            _subjectService = subjectService;
            _emailService = emailService;
            _aspNetRolesService = aspNetRolesService;
            _branchAdminService = branchAdminService;
        }

        // GET: Subject
        public ActionResult Index(int? id)
        {
            ViewBag.ClassList = (from c in _classService.GetClasses()
                                 select new SelectListItem
                                 {
                                     Value = c.ClassId.ToString(),
                                     Text = c.Name
                                 }).ToList();
            ViewBag.ClassId = id;
            var subjects = id == null ? _subjectService.GetAllSubjects().ToList() : _subjectService.GetSubjects((int)id).ToList();
            var viewModelList = AutoMapper.Mapper.Map<List<SubjectProjection>, SubjectViewModel[]>(subjects);
            return View(viewModelList);
        }
        public ActionResult Create()
        {
            var classes = _classService.GetClasses().ToList();
            var viewModel = new SubjectViewModel();
            viewModel.Classes = new SelectList(classes, "ClassId", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubjectViewModel viewModel)
        {
            CMSResult result = new CMSResult();
            if (ModelState.IsValid)
            {
                var ClassIds = viewModel.SelectedClasses.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).Distinct();
                var classList = _classService.GetClasses().Where(x => ClassIds.Contains(x.ClassId)).ToList();

                result = _subjectService.Save(new Subject { Name = viewModel.Name }, classList);

                var className = classList.Select(x => x.Name).ToList();
                viewModel.ClassName = string.Join(",", className);

                var resultMessage = result.Results.Select(x => x.Message).ToList();
                var resultMessages = string.Join("<br />", resultMessage);

                var isResultSuccessful = result.Results.Where(x => x.IsSuccessful == true).Count();

                if (isResultSuccessful > 0)
                {
                    var resultSuccessList = result.Results.Where(x => x.IsSuccessful == true).Select(x => x.Message).ToList();
                    var resulSuccess = string.Join("<br />", resultSuccessList);

                    var roleUserId = User.Identity.GetUserId();
                    var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);
                    var bodySubject = "Subject Create";
                    var message = " created Successfully";
                    SendMailToAdmin(roles, roleUserId, message, viewModel.Name, viewModel.ClassName, bodySubject);
                    Success(resultMessages);
                    ModelState.Clear();
                    viewModel = new SubjectViewModel();
                }
                else
                {
                    _logger.Warn(resultMessages);
                    Warning(resultMessages, true);
                }
            }
            viewModel = new SubjectViewModel();
            var classes = _classService.GetClasses().ToList();
            viewModel.Classes = new SelectList(classes, "ClassId", "Name");
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.SelectedClass = from mt in _classService.GetClasses()
                                    select new SelectListItem
                                    {
                                        Value = mt.ClassId.ToString(),
                                        Text = mt.Name
                                    };

            var projection = _subjectService.GetSubjectById(id);

            if (projection == null)
            {
                _logger.Warn(string.Format("Subject does not Exists {0}.", id));
                Warning("Subject does not Exists.");
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = projection.ClassId;
            var viewModel = AutoMapper.Mapper.Map<SubjectProjection, SubjectViewModel>(projection);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubjectViewModel viewModel)
        {
            ViewBag.SelectedClass = from mt in _classService.GetClasses()
                                    select new SelectListItem
                                    {
                                        Value = mt.ClassId.ToString(),
                                        Text = mt.Name
                                    };

            ViewBag.ClassId = viewModel.ClassId;

            if (ModelState.IsValid)
            {
                var subject = _repository.Project<Subject, bool>(subjects => (from subj in subjects where subj.SubjectId == viewModel.SubjectId select subj).Any());
                if (!subject)
                {
                    _logger.Warn(string.Format("Subject not exists '{0}'.", viewModel.Name));
                    Danger(string.Format("Subject not exists '{0}'.", viewModel.Name));
                }

                var result = _subjectService.Update(new Subject { SubjectId = viewModel.SubjectId, Name = viewModel.Name, ClassId = viewModel.ClassId });
                if (result.Success)
                {
                    var roleUserId = User.Identity.GetUserId();
                    var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);
                    var message = " updated Successfully";
                    var bodySubject = "Subject updated";
                    SendMailToAdmin(roles, roleUserId, message, viewModel.Name, viewModel.ClassName, bodySubject);
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

        public ActionResult Delete(int id)
        {
            var projection = _subjectService.GetSubjectById(id);

            if (projection == null)
            {
                _logger.Warn(string.Format("Subject does not Exists {0}.", id));
                Warning("Subject does not Exists.");
                return RedirectToAction("Index");
            }

            var viewModel = AutoMapper.Mapper.Map<SubjectProjection, SubjectViewModel>(projection);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(SubjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = _subjectService.Delete(viewModel.SubjectId);
                if (result.Success)
                {
                    var roleUserId = User.Identity.GetUserId();
                    var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);
                    var bodySubject = "Subject delete";
                    var message = " deleted Successfully";
                    SendMailToAdmin(roles, roleUserId, message, viewModel.Name, viewModel.ClassName, bodySubject);
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

        public ActionResult GetSubjects(int classId)
        {
            var subjects = _subjectService.GetAllSubjects().Where(x => x.ClassId == classId).Select(x => new { x.SubjectId, x.Name });
            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        public ActionResult QGetSubjects(string[] depdrop_parents)
        {
            var v = string.IsNullOrEmpty(depdrop_parents[0]) ? "0" : depdrop_parents[0];
            var subjects = _subjectService.GetAllSubjects().Where(x => x.ClassId == Convert.ToInt32(v)).Select(x => new { id = x.SubjectId, name = x.Name });
            var obj = new { output = subjects, selected = "" };
            return Json(obj);
        }

        public void SendMailToAdmin(string roles, string roleUserId, string message, string Name, string ClassName, string bodySubject)
        {
            if (roles == "BranchAdmin")
            {
                var branchAdmin = _branchAdminService.GetBranchAdminById(roleUserId);
                var branchName = branchAdmin.BranchName;
                var branchAdminEmail = branchAdmin.Email;
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/MailDesign/CommonMailDesign.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{BranchName}", branchName);
                body = body.Replace("{ModuleName}", message);
                body = body.Replace("{BranchAdminEmail}", "( " + branchAdminEmail + " )");
                var emailMessage = new MailModel
                {
                    Body = body,
                    Subject = bodySubject,
                    IsBranchAdmin = true
                };
                _emailService.Send(emailMessage);
            }
        }
    }
}