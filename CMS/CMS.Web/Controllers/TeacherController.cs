using CMS.Domain.Infrastructure;
using CMS.Domain.Models;
using CMS.Domain.Storage;
using CMS.Domain.Storage.Projections;
using CMS.Domain.Storage.Services;
using CMS.Web.CustomAttributes;
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
    [Roles(Common.Constants.AdminRole, Common.Constants.BranchAdminRole, Common.Constants.StudentRole)]
    public class TeacherController : BaseController
    {
        readonly ITeacherService _teacherService;
        readonly ILogger _logger;
        readonly IRepository _repository;
        readonly IApplicationUserService _applicationUserService;
        readonly IEmailService _emailService;
        readonly IBranchService _branchService;
        readonly IBranchAdminService _branchAdminService;
        readonly IAspNetRoles _aspNetRolesService;
        readonly ILocalDateTimeService _localDateTimeService;

        public TeacherController(ILogger logger, IRepository repository,
            IApplicationUserService applicationUserService, ITeacherService teacherService,
            IEmailService emailService, IBranchService branchService,
            IBranchAdminService branchAdminService, IAspNetRoles aspNetRolesService,
            ILocalDateTimeService localDateTimeService)
        {
            _logger = logger;
            _repository = repository;
            _applicationUserService = applicationUserService;
            _teacherService = teacherService;
            _emailService = emailService;
            _branchService = branchService;
            _branchAdminService = branchAdminService;
            _aspNetRolesService = aspNetRolesService;
            _localDateTimeService = localDateTimeService;
        }
        // GET: Teacher
        public ActionResult Index()
        {
            var roleUserId = User.Identity.GetUserId();
            var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);
            var projection = roles == "BranchAdmin" ? _branchAdminService.GetBranchAdminById(roleUserId) : null;
            var teachers = roles == "Admin" ? _teacherService.GetTeachers().ToList() : roles == "BranchAdmin" ? _teacherService.GetTeachers(projection.BranchId).ToList() : null;
            var viewModelList = AutoMapper.Mapper.Map<List<TeacherProjection>, TeacherViewModel[]>(teachers);
            if (roles == "Admin")
            {
                ViewBag.userId = 0;
            }
            else
            {
                ViewBag.userId = projection.BranchId;
            }
            return View(viewModelList);
        }

        public ActionResult Create()
        {
            var roleUserId = User.Identity.GetUserId();
            var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);

            if (roles == "Admin")
            {
                var branchList = (from b in _branchService.GetAllBranches()
                                  select new SelectListItem
                                  {
                                      Value = b.BranchId.ToString(),
                                      Text = b.Name
                                  }).ToList();

                ViewBag.BranchId = null;

                return View(new TeacherViewModel
                {
                    Branches = branchList,
                    CurrentUserRole = roles
                });
            }
            else if (roles == "BranchAdmin")
            {
                var projection = _branchAdminService.GetBranchAdminById(roleUserId);

                ViewBag.BranchId = projection.BranchId;
                ViewBag.CurrentUserRole = roles;
                return View(new TeacherViewModel
                {
                    CurrentUserRole = roles,
                    BranchId = projection.BranchId,
                    BranchName = projection.BranchName
                });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TeacherViewModel viewModel)
        {
            var localTime = (_localDateTimeService.GetDateTime());
            var roles = viewModel.CurrentUserRole;
            var branchId = viewModel.BranchId;
            var branchName = viewModel.BranchName;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser();
                user.UserName = viewModel.Email.Trim();
                user.Email = viewModel.Email.Trim();
                user.CreatedBy = User.Identity.Name;
                user.CreatedOn = localTime;
                user.PhoneNumber = viewModel.ContactNo.Trim();
                user.Teacher = new Teacher
                {
                    CreatedBy = User.Identity.Name,
                    CreatedOn = localTime,
                    FirstName = viewModel.FirstName.Trim(),
                    MiddleName = viewModel.MiddleName == null ? "" : viewModel.MiddleName.Trim(),
                    LastName = viewModel.LastName.Trim(),
                    ContactNo = viewModel.ContactNo.Trim(),
                    UserId = viewModel.UserId,
                    Description = viewModel.Description.Trim(),
                    BranchId = viewModel.BranchId,
                    IsActive = viewModel.IsActive,
                    Qualification =viewModel.Qualification
                };

                string userPassword = PasswordHelper.GeneratePassword();

                var result = _applicationUserService.SaveTeacher(user, userPassword);

                if (result.Success)
                {
                    string bodySubject = "Teacher Registration";
                    string message = viewModel.FirstName + " " + viewModel.MiddleName + " " + viewModel.LastName + "<br/>Teacher created successfully";
                    SendMailToAdmin(roles, message, viewModel.Email, viewModel.BranchName, bodySubject);
                    Success(result.Results.FirstOrDefault().Message);
                    ModelState.Clear();
                    viewModel = new TeacherViewModel();
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

            if (roles == "Admin")
            {
                var branchList = (from b in _branchService.GetAllBranches()
                                  select new SelectListItem
                                  {
                                      Value = b.BranchId.ToString(),
                                      Text = b.Name
                                  }).ToList();

                viewModel.Branches = branchList;
                ViewBag.BranchId = null;

                return View(new TeacherViewModel
                {
                    Branches = branchList,
                    CurrentUserRole = roles
                });
            }
            else if (roles == "BranchAdmin")
            {
                return View(new TeacherViewModel
                {
                    CurrentUserRole = roles,
                    BranchId = branchId,
                    BranchName = branchName
                });
            }

            return View(viewModel);
        }

        public ActionResult Edit(string id)
        {
            var projection = _teacherService.GetTeacherById(id);
            if (projection == null)
            {
                _logger.Warn(string.Format("Teacher does not Exists {0}.", id));
                Warning("Teacher does not Exists.");
                return RedirectToAction("Index");
            }

            var viewModel = AutoMapper.Mapper.Map<TeacherProjection, TeacherEditViewModel>(projection);

            var roleUserId = User.Identity.GetUserId();
            var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);

            if (roles == "Admin")
            {
                viewModel.CurrentUserRole = roles;
                ViewBag.branchList = (from b in _branchService.GetAllBranches()
                                      select new SelectListItem
                                      {
                                          Value = b.BranchId.ToString(),
                                          Text = b.Name
                                      }).ToList();
                ViewBag.BranchId = projection.BranchId;
                return View(viewModel);
            }
            else if (roles == "BranchAdmin")
            {
                viewModel.CurrentUserRole = roles;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TeacherEditViewModel viewModel)
        {
            var roles = viewModel.CurrentUserRole;
            ViewBag.BranchId = viewModel.BranchId;

            if (ModelState.IsValid)
            {
                var teacher = _repository.Project<ApplicationUser, bool>(users => (from u in users where u.Id == viewModel.UserId select u).Any());

                if (!teacher)
                {
                    _logger.Warn(string.Format("Teacher does not exists '{0} {1} {2}'.", viewModel.FirstName, viewModel.MiddleName, viewModel.LastName));
                    Danger(string.Format("Teacher does not exists '{0} {1} {2}'.", viewModel.FirstName, viewModel.MiddleName, viewModel.LastName));
                    return RedirectToAction("Index");
                }
                var result = _teacherService.Update(new Teacher
                {
                    FirstName = viewModel.FirstName,
                    MiddleName = viewModel.MiddleName,
                    LastName = viewModel.LastName,
                    ContactNo = viewModel.ContactNo,
                    Description = viewModel.Description,
                    UserId = viewModel.UserId,
                    BranchId = viewModel.BranchId,
                    IsActive = viewModel.IsActive,
                    Qualification=viewModel.Qualification
                });
                if (result.Success)
                {
                    string message = viewModel.FirstName + " " + viewModel.MiddleName + " " + viewModel.LastName + "<br/>Teacher updated successfully";
                    string bodySubject = "Web portal changes in teacher";
                    if (viewModel.CurrentUserRole == "BranchAdmin")
                        SendMailToAdmin(roles, message, null, viewModel.BranchName, bodySubject);

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

            if (viewModel.CurrentUserRole == "Admin")
            {
                ViewBag.branchList = (from b in _branchService.GetAllBranches()
                                      select new SelectListItem
                                      {
                                          Value = b.BranchId.ToString(),
                                          Text = b.Name
                                      }).ToList();
            }
            else if (viewModel.CurrentUserRole == "BranchAdmin")
            {
            }

            return View(viewModel);
        }

        public ActionResult Details(string id)
        {
            var teachers = _teacherService.GetTeacherById(id);
            var viewModel = AutoMapper.Mapper.Map<TeacherProjection, TeacherViewModel>(teachers);
            return View(viewModel);
        }

        public ActionResult GetTeachers(int branchId)
        {
            var teachers = _teacherService.GetTeachers(branchId).Select(x => new { x.UserId, x.FirstName, x.MiddleName, x.LastName });
            return Json(teachers, JsonRequestBehavior.AllowGet);
        }

        public ActionResult List()
        {
            return View();
        }

        public void SendMailToAdmin(string roles, string message, string email, string branchName, string bodySubject)
        {
            string userRole = "";
            bool isBranchAdmin = false;

            if (roles == "BranchAdmin")
            {
                userRole = branchName + " ( " + User.Identity.GetUserName() + " ) ";
                isBranchAdmin = true;
            }
            else
            {
                userRole = User.Identity.GetUserName() + "(" + "Master Admin" + ")";
            }

            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/MailDesign/TeacherAndBranchAdminMailDesign.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{BranchName}", userRole);
            body = body.Replace("{UserName}", message);

            var emailMessage = new MailModel
            {
                Body = body,
                Subject = bodySubject,
                To = email,
                IsBranchAdmin = isBranchAdmin
            };
            _emailService.Send(emailMessage);
        }
    }
}