using CMS.Common;
using CMS.Domain.Infrastructure;
using CMS.Domain.Storage.Services;
using CMS.Web.Helpers;
using CMS.Web.Logger;
using CMS.Web.ViewModels;
using System.Web.Mvc;
using System.Linq;
using CMS.Domain.Models;
using System;
using CMS.Domain.Storage.Projections;
using System.Collections.Generic;
using CMS.Web.Models;
using System.Configuration;
using System.IO;
using Microsoft.AspNet.Identity;
using System.Threading;

namespace CMS.Web.Controllers
{
    [Authorize(Roles = Common.Constants.AdminRole)]
    public class BranchAdminController : BaseController
    {
        readonly IBranchService _branchService;
        readonly IBranchAdminService _branchAdminService;
        readonly ILogger _logger;
        readonly IRepository _repository;
        readonly IApplicationUserService _applicationUserService;
        readonly IEmailService _emailService;
        readonly IAspNetRoles _aspNetRolesService;
        readonly ILocalDateTimeService _localDateTimeService;

        public BranchAdminController(ILogger logger, IRepository repository, 
            IApplicationUserService applicationUserService, IBranchAdminService branchAdminService,
            IEmailService emailService, IBranchService branchService, IAspNetRoles aspNetRolesService,
            ILocalDateTimeService localDateTimeService)
        {
            _logger = logger;
            _repository = repository;
            _applicationUserService = applicationUserService;
            _branchAdminService = branchAdminService;
            _emailService = emailService;
            _branchService = branchService;
            _aspNetRolesService = aspNetRolesService;
            _localDateTimeService = localDateTimeService;
        }

        public ActionResult Index()
        {
            //var branchAdmins = _branchAdminService.GetBranches().ToList();
            //var viewModelList = AutoMapper.Mapper.Map<List<BranchAdminProjection>, BranchAdminEditViewModel[]>(branchAdmins);
            return View();
        }

        [Authorize(Roles = Common.Constants.AdminRole)]
        public ActionResult Create()
        {
            var branchList = (from b in _branchService.GetAllBranches()
                              select new SelectListItem
                              {
                                  Value = b.BranchId.ToString(),
                                  Text = b.Name
                              }).ToList();
            return View(new BranchAdminViewModel
            {
                Branches = branchList
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Common.Constants.AdminRole)]
        public ActionResult Create(BranchAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var localTime = (_localDateTimeService.GetDateTime());
                var user = new ApplicationUser();
                user.UserName = viewModel.Email;
                user.Email = viewModel.Email.Trim();
                user.CreatedBy = User.Identity.Name;
                user.CreatedOn = localTime;
                user.PhoneNumber = viewModel.ContactNo.Trim();
                user.BranchAdmin = new BranchAdmin
                {
                    CreatedBy = User.Identity.Name,
                    CreatedOn = localTime,
                    BranchId = viewModel.BranchId,
                    Name = viewModel.Name.Trim(),
                    Active = viewModel.Active,
                    ContactNo = viewModel.ContactNo
                };

                string userPassword = PasswordHelper.GeneratePassword();

                var result = _applicationUserService.SaveBranchAdmin(user, userPassword);

                if (result.Success)
                {
                    string message ="User Name :"+ viewModel.Email + "<br/>Password : " + userPassword + "<br/>Branch Admin created successfully";
                    string email = viewModel.Email;
                    SendMailToAdmin(message, email);
                    Success(result.Results.FirstOrDefault().Message);
                    ModelState.Clear();
                    viewModel = new BranchAdminViewModel();
                }
                else
                {
                    var messages = "";
                    foreach (var message in result.Results)
                    {
                        messages += message.Message + "<br />";
                    }
                    _logger.Warn(messages);
                    Warning(messages, true);
                }
            }
            var branchList = (from b in _branchService.GetAllBranches()
                              select new SelectListItem
                              {
                                  Value = b.BranchId.ToString(),
                                  Text = b.Name
                              }).ToList();
            viewModel.Branches = branchList;
            return View(viewModel);
        }

        public ActionResult Edit(string id)
        {
            ViewBag.branchList = (from b in _branchService.GetAllBranches()
                                  select new SelectListItem
                                  {
                                      Value = b.BranchId.ToString(),
                                      Text = b.Name
                                  }).ToList();

            var projection = _branchAdminService.GetBranchAdminById(id);

            if (projection == null)
            {
                _logger.Warn(string.Format("Branch Admin not Exists {0}.", projection.Name));
                Warning("Branch Admin does not Exists.");
                return RedirectToAction("Index");
            }

            ViewBag.BranchId = projection.BranchId;

            var viewModel = AutoMapper.Mapper.Map<BranchAdminProjection, BranchAdminEditViewModel>(projection);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Common.Constants.AdminRole)]
        public ActionResult Edit(BranchAdminEditViewModel viewModel)
        {
            ViewBag.branchList = (from b in _branchService.GetAllBranches()
                                  select new SelectListItem
                                  {
                                      Value = b.BranchId.ToString(),
                                      Text = b.Name
                                  }).ToList();

            ViewBag.BranchId = viewModel.BranchId;

            if (ModelState.IsValid)
            {
                var student = _repository.Project<BranchAdmin, bool>(users => (from u in users where u.UserId == viewModel.UserId select u).Any());

                if (!student)
                {
                    _logger.Warn(string.Format("Branch Admin does not exists '{0}'.", viewModel.Name));
                    Danger(string.Format("Branch Admin does not exists '{0}'.", viewModel.Name));
                    return RedirectToAction("Index");
                }

                var result = _branchAdminService.Update(new BranchAdmin
                {
                    BranchId = viewModel.BranchId,
                    ContactNo = viewModel.ContactNo,
                    Name = viewModel.Name,
                    Active = viewModel.Active,
                    UserId = viewModel.UserId
                });
                if (result.Success)
                {
                    Success(result.Results.FirstOrDefault().Message);
                    ModelState.Clear();
                    return RedirectToAction("Index");
                }
                else
                {
                    var messages = "";
                    foreach (var message in result.Results)
                    {
                        messages += message.Message + "<br />";
                    }
                    _logger.Warn(messages);
                    Warning(messages, true);
                }
            }
            return View(viewModel);
        }

        public ActionResult Delete(string id)
        {
            var projection = _branchAdminService.GetBranchAdminById(id);
            if (projection == null)
            {
                _logger.Warn(string.Format("Branch Admin does not Exists {0}.", id));
                Warning("Branch Admin does not Exists.");
                return RedirectToAction("Index");
            }
            var viewModel = AutoMapper.Mapper.Map<BranchAdminProjection, BranchAdminDeleteViewModel>(projection);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Common.Constants.AdminRole)]
        public ActionResult Delete(BranchAdminDeleteViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                var result = _branchAdminService.Delete(viewModel.UserId);
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

        [Authorize(Roles = Common.Constants.AdminRole)]
        public ActionResult Details(string id)
        {
            var branchAdmins = _branchAdminService.GetBranchAdminById(id);
            var viewModel = AutoMapper.Mapper.Map<BranchAdminProjection, BranchAdminEditViewModel>(branchAdmins);
            return View(viewModel);
        }

        public void SendMailToAdmin(string message, string email)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/MailDesign/TeacherAndBranchAdminMailDesign.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{BranchName}", User.Identity.GetUserName() + "(" + "Master Admin" + ")");
            body = body.Replace("{UserName}", message);
            var emailMessage = new MailModel
            {
                Body = body,
                Subject = "Branch Admin Registration",
                To = email
            };
            _emailService.Send(emailMessage);
        }
    }
}