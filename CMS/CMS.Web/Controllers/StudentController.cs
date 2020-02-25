using CMS.Common;
using CMS.Domain.Infrastructure;
using CMS.Domain.Models;
using CMS.Domain.Storage.Projections;
using CMS.Domain.Storage.Services;
using CMS.Web.CustomAttributes;
using CMS.Web.Helpers;
using CMS.Web.Logger;
using CMS.Web.Models;
using CMS.Web.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Web.Controllers
{
    [Roles(Common.Constants.AdminRole, Common.Constants.StudentRole, Common.Constants.BranchAdminRole)]
    //[RequireHttpsAttribute]
    public class StudentController : BaseController
    {
        readonly IClassService _classService;
        readonly ILogger _logger;
        readonly IRepository _repository;
        readonly IBoardService _boardService;
        readonly IStudentService _studentService;
        readonly IApplicationUserService _applicationUserService;
        readonly ISubjectService _subjectService;
        readonly IInstallmentService _installmentService;
        readonly IBatchService _batchService;
        readonly IEmailService _emailService;
        readonly ISchoolService _schoolService;
        readonly IBranchService _branchService;
        readonly IAspNetRoles _aspNetRolesService;
        readonly IBranchAdminService _branchAdminService;
        readonly ITeacherService _teacherService;
        readonly IApiService _apiService;
        readonly ILocalDateTimeService _localDateTimeService;
        readonly ISmsService _smsService;

        public StudentController(IClassService classService, ILogger logger, IRepository repository,
            IBoardService boardService, IStudentService studentService,
            IApplicationUserService applicationUserService, ISubjectService subjectService,
            IInstallmentService installmentService, IBatchService batchService, IEmailService emailService,
            ISchoolService schoolService, IBranchService branchService, IAspNetRoles aspNetRolesService,
            IBranchAdminService branchAdminService, ITeacherService teacherService,
            IApiService apiService, ILocalDateTimeService localDateTimeService, ISmsService smsService)
        {
            _classService = classService;
            _logger = logger;
            _repository = repository;
            _boardService = boardService;
            _studentService = studentService;
            _applicationUserService = applicationUserService;
            _subjectService = subjectService;
            _installmentService = installmentService;
            _batchService = batchService;
            _emailService = emailService;
            _schoolService = schoolService;
            _branchService = branchService;
            _aspNetRolesService = aspNetRolesService;
            _branchAdminService = branchAdminService;
            _teacherService = teacherService;
            _apiService = apiService;
            _localDateTimeService = localDateTimeService;
            _smsService = smsService;
        }


        [Roles(Common.Constants.AdminRole, Common.Constants.BranchAdminRole)]
        public ActionResult Index(int? id)
        {
            var roleUserId = User.Identity.GetUserId();
            var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);
            var projection = roles == "BranchAdmin" ? _branchAdminService.GetBranchAdminById(roleUserId) : null;
            ViewBag.ClassList = (from c in _classService.GetClasses()
                                 select new SelectListItem
                                 {
                                     Value = c.ClassId.ToString(),
                                     Text = c.Name
                                 }).ToList();

            //ViewBag.ClassId = id;
            //var students = (roles == "Admin" && id == null) ? _studentService.GetAllStudents().ToList()
            //                    : (roles == "Admin" && id != null) ? _studentService.GetStudentsByClassId((int)id).ToList()
            //                        : (roles == "BranchAdmin" && id == null) ? _studentService.GetStudentsByBranchId(projection.BranchId).ToList()
            //                      : (roles == "BranchAdmin" && id != null) ? _studentService.GetStudentsByBranchAndClassId((int)id, projection.BranchId).ToList() : null;

            //var viewModelList = AutoMapper.Mapper.Map<List<StudentProjection>, StudentViewModel[]>(students);
            // return View(viewModelList);
            if (roles == "Admin")
            {
                ViewBag.userId = 0;
            }
            else
            {
                ViewBag.userId = projection.BranchId;
            }
            return View();
        }

        [Authorize(Roles = Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole)]
        public ActionResult Create(int? id)
        {
            var boardList = (from c in _boardService.GetBoards()
                             select new SelectListItem
                             {
                                 Value = c.BoardId.ToString(),
                                 Text = c.Name
                             }).ToList();

            var classList = (from c in _classService.GetClasses()
                             select new SelectListItem
                             {
                                 Value = c.ClassId.ToString(),
                                 Text = c.Name
                             }).ToList();

            var schoolList = (from s in _schoolService.GetAllSchools()
                              select new SelectListItem
                              {
                                  Value = s.SchoolId.ToString(),
                                  Text = s.Name
                              }).ToList();

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
                if (id != null)
                {
                    var admissionResult = _apiService.GetAdmission(id);
                    var admission = JsonConvert.DeserializeObject<AdmissionProjection>(admissionResult);

                    var result = _studentService.IsExistAdmission(admission.Email);
                    if (result)
                    {
                        _logger.Warn(admission.FirstName + " " + admission.LastName + "(" + admission.Email + ")" + " student is already added.");
                        Warning(admission.FirstName + " " + admission.LastName + "(" + admission.Email + ")" + " student is already added.", true);
                        ViewBag.BranchId = null;
                        return View(new StudentViewModel
                        {
                            Branches = branchList,
                            CurrentUserRole = roles,
                            Boards = boardList,
                            Classes = classList,
                            Schools = schoolList,
                        });
                    }
                    else
                    {
                        ViewBag.SelectedSubjects = admission.SelectedSubject;
                        ViewBag.BatchId = admission.BatchId;
                        return View(new StudentViewModel
                        {
                            Branches = branchList,
                            CurrentUserRole = roles,
                            Boards = boardList,
                            Classes = classList,
                            Schools = schoolList,
                            BoardId = admission.BoardId,
                            ClassId = admission.ClassId,
                            FirstName = admission.FirstName.Trim(),
                            MiddleName = admission.MiddleName == null ? "" : admission.MiddleName.Trim(),
                            LastName = admission.LastName.Trim(),
                            Gender = admission.Gender,
                            Address = admission.Address.Trim(),
                            Pin = admission.Pin,
                            DOB = admission.DOB.Date,
                            BloodGroup = admission.BloodGroup,
                            StudentContact = admission.StudentContact == null ? "" : admission.StudentContact.Trim(),
                            ParentContact = admission.ParentContact.Trim(),
                            PickAndDrop = admission.PickAndDrop,
                            DOJ = admission.DOJ.Date,
                            SchoolId = admission.SchoolId,
                            SelectedSubject = admission.SelectedSubject,
                            IsWhatsApp = admission.IsWhatsApp,
                            MotherName = admission.MotherName == null ? "" : admission.MotherName,
                            SeatNumber = admission.SeatNumber == null ? "" : admission.SeatNumber,
                            BranchId = admission.BranchId,
                            Email = admission.Email,
                            ConfirmEmail = admission.Email,
                            IsIdExits = id,
                            BatchId = admission.BatchId,
                            EmergencyContact = admission.EmergencyContact == null ? "" : admission.EmergencyContact.Trim(),
                            ParentEmailId = admission.ParentEmailId == null ? "" : admission.ParentEmailId.Trim(),
                            PaymentLists = admission.PaymentLists == null ? "" : admission.PaymentLists
                        });
                    }
                }
                else
                {
                    ViewBag.BranchId = null;
                    return View(new StudentViewModel
                    {
                        Branches = branchList,
                        CurrentUserRole = roles,
                        Boards = boardList,
                        Classes = classList,
                        Schools = schoolList,
                    });
                }
            }
            else if (roles == "BranchAdmin")
            {
                var projection = _branchAdminService.GetBranchAdminById(roleUserId);
                ViewBag.BranchId = projection.BranchId;

                return View(new StudentViewModel
                {
                    CurrentUserRole = roles,
                    BranchId = projection.BranchId,
                    BranchName = projection.BranchName,
                    Boards = boardList,
                    Classes = classList,
                    Schools = schoolList,
                });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole)]
        public ActionResult Create(StudentViewModel viewModel)
        {
            try
            {
                var roles = viewModel.CurrentUserRole;
                var branchId = viewModel.BranchId;
                var branchName = viewModel.BranchName;
                var boardList = (from b in _boardService.GetBoards()
                                 select new SelectListItem
                                 {
                                     Value = b.BoardId.ToString(),
                                     Text = b.Name
                                 }).ToList();

                var classList = (from c in _classService.GetClasses()
                                 select new SelectListItem
                                 {
                                     Value = c.ClassId.ToString(),
                                     Text = c.Name
                                 }).ToList();

                var schoolList = (from s in _schoolService.GetAllSchools()
                                  select new SelectListItem
                                  {
                                      Value = s.SchoolId.ToString(),
                                      Text = s.Name
                                  }).ToList();

                if (ModelState.IsValid)
                {
                    if (viewModel.PaidFee != 0)
                    {
                        var errorMessage = "";
                        if (viewModel.FinalFees < viewModel.PaidFee)
                        {
                            errorMessage = "Payment amount is exceded!";
                            _logger.Warn(errorMessage);
                            Warning(errorMessage, true);
                            if (errorMessage != "")
                            {
                                ViewBag.BatchId = viewModel.BatchId;
                                ViewBag.SelectedSubjects = viewModel.SelectedSubject;
                                ReturnViewModel(roles, viewModel, branchId, branchName, boardList, schoolList, classList);
                                return View(viewModel);
                            }
                        }
                        else if (viewModel.ReceiptBookNumber == null || viewModel.ReceiptNumber == null)
                        {
                            errorMessage += "  Receipt Book Number and Receipt Number is required!";
                            _logger.Warn(errorMessage);
                            Warning(errorMessage, true);
                            if (errorMessage != "")
                            {
                                ViewBag.BatchId = viewModel.BatchId;
                                ViewBag.SelectedSubjects = viewModel.SelectedSubject;
                                ReturnViewModel(roles, viewModel, branchId, branchName, boardList, schoolList, classList);
                                return View(viewModel);
                            }
                        }
                    }
                   
                        if (viewModel.PaymentErrorMessage != "" && viewModel.PaymentErrorMessage != null)
                        {
                            _logger.Warn(viewModel.PaymentErrorMessage);
                            Warning(viewModel.PaymentErrorMessage, true);
                            ViewBag.SelectedSubjects = viewModel.SelectedSubject;
                            ViewBag.BatchId = viewModel.BatchId;
                            ReturnViewModel(roles, viewModel, branchId, branchName, boardList, schoolList, classList);
                        }
                  
                    else
                    {
                        string base64 = Request.Form["imgCropped"];
                        var localTime = (_localDateTimeService.GetDateTime());
                        string filename = Server.MapPath(ConfigurationManager.AppSettings["brochureFile"].ToString());
                        if (System.IO.File.Exists(filename))
                        {
                            var user = new ApplicationUser();
                            string photoPath = "";
                            if (viewModel.PhotoFilePath != null)
                            {
                                photoPath = string.Format(@"~/Images/{0}/{1}.jpg", Common.Constants.StudentImageFolder, user.Id);
                                if (!Common.Constants.ImageTypes.Contains(viewModel.PhotoFilePath.ContentType))
                                {
                                    _logger.Warn("Please choose either a JPEG, JPG or PNG image.");
                                    Warning("Please choose either a JPEG, JPG or PNG image..", true);
                                    viewModel.Schools = schoolList;
                                    viewModel.Classes = classList;
                                    viewModel.Boards = boardList;
                                    return View(viewModel);
                                }
                            }
                            else if (viewModel.ImageData != null)
                            {
                                photoPath = string.Format(@"~/Images/{0}/{1}.jpg", Common.Constants.StudentImageFolder, user.Id);
                            }
                            else { photoPath = null; }
                            user.UserName = viewModel.Email;
                            user.Email = viewModel.Email.Trim();
                            user.CreatedBy = User.Identity.Name;
                            user.CreatedOn = localTime;
                            user.PhoneNumber = viewModel.StudentContact == null ? "" : viewModel.StudentContact.Trim();
                            user.Student = new Student
                            {
                                CreatedBy = User.Identity.Name,
                                CreatedOn = localTime,
                                BoardId = viewModel.BoardId,
                                ClassId = viewModel.ClassId,
                                FirstName = viewModel.FirstName.Trim(),
                                MiddleName = viewModel.MiddleName == null ? "" : viewModel.MiddleName.Trim(),
                                LastName = viewModel.LastName.Trim(),
                                Gender = viewModel.Gender,
                                Address = viewModel.Address.Trim(),
                                Pin = viewModel.Pin,
                                DOB = viewModel.DOB,
                                BloodGroup = viewModel.BloodGroup,
                                StudentContact = viewModel.StudentContact == null ? "" : viewModel.StudentContact.Trim(),
                                ParentContact = viewModel.ParentContact.Trim(),
                                PickAndDrop = viewModel.PickAndDrop,
                                DOJ = viewModel.DOJ,
                                SchoolId = viewModel.SchoolId,
                                TotalFees = viewModel.TotalFees,
                                Discount = viewModel.Discount,
                                FinalFees = viewModel.FinalFees,
                                SelectedSubject = viewModel.SelectedSubject,
                                PhotoPath = photoPath,
                                IsActive = viewModel.IsActive,
                                IsWhatsApp = viewModel.IsWhatsApp,
                                PunchId = viewModel.PunchId,
                                MotherName = viewModel.MotherName,
                                VANArea = viewModel.VANArea,
                                SeatNumber = viewModel.SeatNumber,
                                VANFee = viewModel.VANFee,
                                BranchId = viewModel.BranchId,
                                EmergencyContact = viewModel.EmergencyContact == null ? "" : viewModel.EmergencyContact.Trim(),
                                ParentEmailId = viewModel.ParentEmailId == null ? "" : viewModel.ParentEmailId.Trim(),
                                BatchId = viewModel.BatchId,
                                PaymentLists = viewModel.PaymentLists
                            };
                            string userPassword = PasswordHelper.GeneratePassword();

                            var result = _applicationUserService.Save(user, userPassword);

                            if (result.Success)
                            {
                                viewModel.UserId = user.Student.UserId;
                                // ViewBag.userId = viewModel.UserId;
                                if (viewModel.IsIdExits != null)
                                {
                                    var admissionResult = _apiService.UpdateAdmission(viewModel.Email);
                                }
                                if (viewModel.PhotoFilePath != null)
                                {
                                    byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);
                                    MemoryStream myMemStream = new MemoryStream(bytes);
                                    Image fullsizeImage = Image.FromStream(myMemStream);
                                    Image newImage = fullsizeImage.GetThumbnailImage(240, 240, null, IntPtr.Zero);
                                    MemoryStream myResult = new MemoryStream();
                                    newImage.Save(myResult, ImageFormat.Png);

                                    string StudentImagePath = Server.MapPath(string.Concat("~/Images/", Common.Constants.StudentImageFolder));
                                    var pathToSave = Path.Combine(StudentImagePath, user.Student.UserId + ".jpg");
                                    // viewModel.PhotoFilePath.SaveAs(pathToSave);
                                    System.IO.File.WriteAllBytes(pathToSave, myResult.ToArray());
                                }
                                else if (viewModel.ImageData != null)
                                {
                                    string StudentImagePath = Server.MapPath(string.Concat("~/Images/", Common.Constants.StudentImageFolder));
                                    var pathToSave = Path.Combine(StudentImagePath, user.Student.UserId + ".jpg");
                                    System.IO.File.WriteAllBytes(pathToSave, Convert.FromBase64String(viewModel.ImageData));
                                }
                                if (viewModel.StudentContact != null)
                                {
                                    SendSMS(viewModel);
                                }
                                SendEmail(viewModel, userPassword, filename);
                            }
                            if (viewModel.PaidFee != 0 || result.Results.FirstOrDefault().Message == "Email already exists!")
                            {
                                if (viewModel.UserId == null)
                                {
                                    var userId = _studentService.GetStudentUserIdInstallment(viewModel.PunchId, viewModel.Email);
                                    viewModel.UserId = userId.UserId.ToString();
                                }
                                var remainingPayment = viewModel.FinalFees - viewModel.PaidFee;
                                var resultInstallment = _installmentService.Save(new Installment
                                {
                                    ClassId = viewModel.ClassId,
                                    UserId = viewModel.UserId,
                                    Payment = viewModel.PaidFee,
                                    RemainingFee = remainingPayment,
                                    ReceiptBookNumber = viewModel.ReceiptBookNumber,
                                    ReceiptNumber = viewModel.ReceiptNumber,
                                    ReceivedFee = viewModel.PaidFee,
                                });
                                if (resultInstallment.Success)
                                {
                                    Success(result.Results.FirstOrDefault().Message + " " + resultInstallment.Results.FirstOrDefault().Message);
                                    ModelState.Clear();
                                    viewModel = new StudentViewModel();
                                }
                                else
                                {
                                    _logger.Warn(result.Results.FirstOrDefault().Message + " " + resultInstallment.Results.FirstOrDefault().Message);
                                    Warning(result.Results.FirstOrDefault().Message + " " + resultInstallment.Results.FirstOrDefault().Message, true);

                                }
                            }
                            else
                            {
                                if (result.Success)
                                {
                                    Success(result.Results.FirstOrDefault().Message);
                                    ModelState.Clear();
                                    viewModel = new StudentViewModel();
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
                            //else
                            //{
                            //    var messages = "";
                            //    foreach (var message in result.Results)
                            //    {
                            //        messages += message.Message + "<br />";
                            //    }
                            //    _logger.Warn(messages);
                            //    Warning(messages, true);
                            //}
                        }
                        else
                        {
                            _logger.Warn("Please add Brochure.");
                            Warning("Please add Brochure.", true);
                        }
                    }
                }
                ViewBag.SelectedSubjects = viewModel.SelectedSubject;
                ViewBag.BatchId = viewModel.BatchId;
                ReturnViewModel(roles, viewModel, branchId, branchName, boardList, schoolList, classList);
                // viewModel.BatchId = ViewBag.BatchId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " student create");
                throw ex;
            }
        }

        public void ReturnViewModel(string roles, StudentViewModel viewModel, int branchId, string branchName, List<SelectListItem> boardList,
                List<SelectListItem> schoolList, List<SelectListItem> classList)
        {
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
            }
            else if (roles == "BranchAdmin")
            {
                viewModel.BranchId = branchId;
                viewModel.BranchName = branchName;
            }
            viewModel.Schools = schoolList;
            viewModel.Classes = classList;
            viewModel.Boards = boardList;
            viewModel.CurrentUserRole = roles;
        }

        public CMSResult SendSMS(StudentViewModel viewModel)
        {
            CMSResult cmsresult = new CMSResult();

            if (viewModel.ParentContact != "")
            {
                var smsModel = new SmsModel
                {
                    Message = string.Format("Your Student {0} {1} has been joined  arpita Family in {2} \n Click Here for downloding Parent app \n {3} \n to check student class performance.",
                        viewModel.FirstName, viewModel.LastName, viewModel.BranchName, ConfigurationManager.AppSettings[Common.Constants.parentAppLink]),
                    SendTo = viewModel.ParentContact
                };


                var sendparentsms = _smsService.SendMessage(smsModel);
                cmsresult.Results.Add(new Result { Message = sendparentsms.Results[0].Message, IsSuccessful = sendparentsms.Results[0].IsSuccessful });
            }

            if (viewModel.StudentContact != "")
            {
                var smsModel = new SmsModel
                {
                    Message = string.Format("Thanks for Joining Arpita’s Family in  {0} {1} has been joined  arpita’s Family in {2} \n Click Here for downloding Student app \n {3}.\n Grow Your Konwlage",
                        viewModel.FirstName, viewModel.LastName, viewModel.BranchName, ConfigurationManager.AppSettings[Common.Constants.studentAppLink]),
                    SendTo = viewModel.StudentContact
                };
                var sendparentsms = _smsService.SendMessage(smsModel);
                cmsresult.Results.Add(new Result { Message = sendparentsms.Results[0].Message, IsSuccessful = sendparentsms.Results[0].IsSuccessful });

            }
            return cmsresult;

        }

        public bool SendEmail(StudentViewModel viewModel, string userPassword, string filename)
        {
            string userRole = "";
            bool isBranchAdmin = false;
            if (viewModel.CurrentUserRole == "BranchAdmin")
            {
                userRole = viewModel.BranchName + " ( " + User.Identity.GetUserName() + ")";
                isBranchAdmin = true;
            }
            else
            {
                userRole = User.Identity.GetUserName() + "(" + "Master Admin" + ")";
            }

            var batchWithSubject = _batchService.GetBatcheById(viewModel.BatchId);
            var Subject = _subjectService.GetSubjectSubjectIds(viewModel.SelectedSubject).Select(x => x.Name).ToList();

            var paidFee = _installmentService.GetCountInstallment(viewModel.UserId);
            paidFee = viewModel.PaidFee == 0 ? 0 : viewModel.PaidFee;
            var remainFee = viewModel.FinalFees - paidFee;
            var className = _classService.GetClassById(viewModel.ClassId);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/MailDesign/StudentMailDesign.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{BranchName}", userRole);
            body = body.Replace("{UserName}", viewModel.FirstName + " " + viewModel.LastName);
            body = body.Replace("{Password}", userPassword + "<br/>is successfully register with us");
            body = body.Replace("{UserId}", viewModel.Email);
            body = body.Replace("{ClassName}", className.Name);
            body = body.Replace("{BatchWithSubjectName}", batchWithSubject.BatchName + "( " + string.Join(",", Subject) + " )");
            body = body.Replace("{TotalFees}", viewModel.TotalFees.ToString());
            body = body.Replace("{Discount}", viewModel.Discount.ToString());
            body = body.Replace("{FinalFees}", viewModel.FinalFees.ToString());
            body = body.Replace("{PaidFees}", paidFee.ToString());
            body = body.Replace("{RemainingFees}", remainFee.ToString());

            var emailMessage = new MailModel
            {
                Body = body,
                Subject = "Web portal - Student Create",
                To = viewModel.Email,
             
                IsBranchAdmin = isBranchAdmin
            };

            emailMessage.AttachmentPaths.Add(filename);
            return _emailService.Send(emailMessage);
        }

        [Authorize(Roles = Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole)]
        public ActionResult Edit(string id)
        {
            ViewBag.boardList = from b in _boardService.GetBoards()
                                select new SelectListItem
                                {
                                    Value = b.BoardId.ToString(),
                                    Text = b.Name
                                };

            ViewBag.classList = from c in _classService.GetClasses()
                                select new SelectListItem
                                {
                                    Value = c.ClassId.ToString(),
                                    Text = c.Name
                                };

            ViewBag.schoolList = (from s in _schoolService.GetAllSchools()
                                  select new SelectListItem
                                  {
                                      Value = s.SchoolId.ToString(),
                                      Text = s.Name
                                  }).ToList();

            var projection = _studentService.GetStudentById(id);

            ViewBag.subjectList = from s in _subjectService.GetSubjectByClassId(projection.ClassId)
                                  select new SelectListItem
                                  {
                                      Value = s.SubjectId.ToString(),
                                      Text = s.Name
                                  };
            ViewBag.BatchList = from s in _batchService.GetAllBatchClsId(projection.ClassId)
                                select new SelectListItem
                                {
                                    Value = s.BatchId.ToString(),
                                    Text = s.BatchName
                                };

            ViewBag.SelectedSubjects = projection.SelectedSubjects;

            if (projection == null)
            {
                _logger.Warn(string.Format("Student does not Exists {0}.", id));
                Warning("Student does not Exists.");
                return RedirectToAction("Index");
            }

            ViewBag.BranchId = projection.BranchId;
            ViewBag.ClassId = projection.ClassId;
            ViewBag.BoardId = projection.BoardId;
            ViewBag.SubjectId = projection.SubjectId;
            ViewBag.SchoolId = projection.SchoolId;
            ViewBag.BatchId = projection.BatchId;
            ViewBag.UrlPhoto = projection.PhotoPath;

            var viewModel = AutoMapper.Mapper.Map<StudentProjection, StudentEditViewModel>(projection);
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
        [Authorize(Roles = Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole)]
        public ActionResult Edit(StudentEditViewModel viewModel)
        {
            string base64 = Request.Form["imgCropped"];
            var roles = viewModel.CurrentUserRole;
            ViewBag.boardList = from b in _boardService.GetBoards()
                                select new SelectListItem
                                {
                                    Value = b.BoardId.ToString(),
                                    Text = b.Name
                                };

            ViewBag.classList = from c in _classService.GetClasses()
                                select new SelectListItem
                                {
                                    Value = c.ClassId.ToString(),
                                    Text = c.Name
                                };

            ViewBag.subjectList = from s in _subjectService.GetSubjectByClassId(viewModel.ClassId)
                                  select new SelectListItem
                                  {
                                      Value = s.SubjectId.ToString(),
                                      Text = s.Name
                                  };
            ViewBag.BatchList = from s in _batchService.GetAllBatchClsId(viewModel.ClassId)
                                select new SelectListItem
                                {
                                    Value = s.BatchId.ToString(),
                                    Text = s.BatchName
                                };

            ViewBag.schoolList = (from s in _schoolService.GetAllSchools()
                                  select new SelectListItem
                                  {
                                      Value = s.SchoolId.ToString(),
                                      Text = s.Name
                                  }).ToList();

            ViewBag.BranchId = viewModel.BranchId;
            ViewBag.ClassId = viewModel.ClassId;
            ViewBag.BoardId = viewModel.BoardId;
            ViewBag.SubjectId = viewModel.SubjectId;
            ViewBag.SchoolId = viewModel.SchoolId;
            ViewBag.BatchId = viewModel.BatchId;

            if (ModelState.IsValid)
            {
                if (viewModel.PaymentErrorMessage != "" && viewModel.PaymentErrorMessage != null)
                {
                    _logger.Warn(viewModel.PaymentErrorMessage);
                    Warning(viewModel.PaymentErrorMessage, true);
                }
                else
                {
                    string filename = Server.MapPath(ConfigurationManager.AppSettings["brochureFile"].ToString());

                    if (System.IO.File.Exists(filename))
                    {
                        var user = new ApplicationUser();

                        string photoPath = "";
                        if (viewModel.PhotoFilePath != null)
                        {
                            photoPath = string.Format(@"~/Images/{0}/{1}.jpg", Common.Constants.StudentImageFolder, viewModel.UserId);
                            if (!Common.Constants.ImageTypes.Contains(viewModel.PhotoFilePath.ContentType))
                            {
                                _logger.Warn("Please choose either a JPEG, JPG or PNG image.");
                                Warning("Please choose either a JPEG, JPG or PNG image..", true);
                                return View(viewModel);
                            }
                        }
                        else if (viewModel.ImageData != null)
                        {
                            photoPath = string.Format(@"~/Images/{0}/{1}.jpg", Common.Constants.StudentImageFolder, viewModel.UserId);
                        }
                        else
                        {
                            photoPath = null;
                        }

                        var student = _repository.Project<ApplicationUser, bool>(users => (from u in users where u.Id == viewModel.UserId select u).Any());

                        if (!student)
                        {
                            _logger.Warn(string.Format("Student does not exists '{0} {1} {2}'.", viewModel.FirstName, viewModel.MiddleName, viewModel.LastName));
                            Danger(string.Format("Student does not exists '{0} {1} {2}'.", viewModel.FirstName, viewModel.MiddleName, viewModel.LastName));
                            return RedirectToAction("Index");
                        }

                        var result = _studentService.Update(new Student
                        {
                            BoardId = viewModel.BoardId,
                            ClassId = viewModel.ClassId,
                            FirstName = viewModel.FirstName.Trim(),
                            MiddleName = viewModel.MiddleName == null ? "" : viewModel.MiddleName.Trim(),
                            LastName = viewModel.LastName.Trim(),
                            Gender = viewModel.Gender,
                            Address = viewModel.Address.Trim(),
                            Pin = viewModel.Pin,
                            DOB = viewModel.DOB,
                            BloodGroup = viewModel.BloodGroup,
                            StudentContact = viewModel.StudentContact == null ? "" : viewModel.StudentContact.Trim(),
                            ParentContact = viewModel.ParentContact,
                            PickAndDrop = viewModel.PickAndDrop,
                            DOJ = viewModel.DOJ,
                            SchoolId = viewModel.SchoolId,
                            TotalFees = viewModel.TotalFees,
                            Discount = viewModel.Discount,
                            FinalFees = viewModel.FinalFees,
                            UserId = viewModel.UserId,
                            SelectedSubject = viewModel.SelectedSubject,
                            BatchId = viewModel.BatchId,
                            IsWhatsApp = viewModel.IsWhatsApp,
                            IsActive = viewModel.IsActive,
                            PunchId = viewModel.PunchId,
                            MotherName = viewModel.MotherName,
                            VANArea = viewModel.VANArea,
                            SeatNumber = viewModel.SeatNumber,
                            VANFee = viewModel.VANFee,
                            BranchId = viewModel.BranchId,
                            PhotoPath = photoPath == null ? viewModel.PhotoPath : photoPath,
                            EmergencyContact = viewModel.EmergencyContact == null ? "" : viewModel.EmergencyContact.Trim(),
                            ParentEmailId = viewModel.ParentEmailId == null ? "" : viewModel.ParentEmailId.Trim(),
                            PaymentLists = viewModel.PaymentLists
                        });


                        if (result.Success)
                        {
                            if (viewModel.PhotoFilePath != null)
                            {
                                byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);
                                MemoryStream myMemStream = new MemoryStream(bytes);
                                Image fullsizeImage = Image.FromStream(myMemStream);
                                Image newImage = fullsizeImage.GetThumbnailImage(240, 240, null, IntPtr.Zero);
                                MemoryStream myResult = new MemoryStream();
                                newImage.Save(myResult, ImageFormat.Png);

                                string StudentImagePath = Server.MapPath(string.Concat("~/Images/", Common.Constants.StudentImageFolder));
                                var pathToSave = Path.Combine(StudentImagePath, viewModel.UserId + ".jpg");
                                System.IO.File.WriteAllBytes(pathToSave, myResult.ToArray());
                                //  viewModel.PhotoFilePath.SaveAs(pathToSave);
                            }
                            else if (viewModel.ImageData != null)
                            {
                                string StudentImagePath = Server.MapPath(string.Concat("~/Images/", Common.Constants.StudentImageFolder));
                                var pathToSave = Path.Combine(StudentImagePath, viewModel.UserId + ".jpg");
                                System.IO.File.WriteAllBytes(pathToSave, Convert.FromBase64String(viewModel.ImageData));
                            }
                            if (viewModel.CurrentUserRole == "BranchAdmin")
                            {
                                string createdBranchName = viewModel.BranchName + " ( " + User.Identity.GetUserName() + " )";
                                var batchWithSubject = _batchService.GetBatcheById(viewModel.BatchId);
                                var Subject = _subjectService.GetSubjectSubjectIds(viewModel.SelectedSubject).Select(x => x.Name).ToList();
                                var paidFee = _installmentService.GetCountInstallment(viewModel.UserId);
                                var remainFee = viewModel.FinalFees - paidFee;
                                var className = _classService.GetClassById(viewModel.ClassId);
                                string body = string.Empty;
                                using (StreamReader reader = new StreamReader(Server.MapPath("~/MailDesign/StudentMailDesign.html")))
                                {
                                    body = reader.ReadToEnd();
                                }
                                body = body.Replace("{BranchName}", createdBranchName);
                                body = body.Replace("{UserName}", viewModel.FirstName + " " + viewModel.MiddleName + " " + viewModel.LastName);
                                body = body.Replace("{ClassName}", className.Name);
                                body = body.Replace("{BatchWithSubjectName}", batchWithSubject.BatchName + "( " + string.Join(",", Subject) + " )");
                                body = body.Replace("{TotalFees}", viewModel.TotalFees.ToString() + "</br>Discount : " + viewModel.Discount
                                    + "<br/>Fee after Discount : " + viewModel.FinalFees);
                                body = body.Replace("{PaidFees}", paidFee.ToString());
                                body = body.Replace("{RemainingFees}", remainFee.ToString());

                                var emailMessage = new MailModel
                                {
                                    Body = body,
                                    Subject = "Web portal changes student",
                                    IsBranchAdmin = true,
                                };
                                emailMessage.AttachmentPaths.Add(filename);
                                _emailService.Send(emailMessage);
                            }
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

        [Roles(Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole)]
        public ActionResult Details(string id)
        {
            var students = _studentService.GetStudentById(id);
            var viewModel = AutoMapper.Mapper.Map<StudentProjection, StudentEditViewModel>(students);

            var commaseperatedList = students.SelectedSubjects ?? string.Empty;
            var subjectIds = commaseperatedList.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse);

            var subjects = _repository.LoadList<Subject>(x => subjectIds.Contains(x.SubjectId)).ToList();
            string subject = "";
            foreach (var s in subjects)
            {
                subject += string.Format("{0},", s.Name);
            }

            viewModel.SelectedSubject = subject.TrimEnd(',');

            return View(viewModel);
        }

        [Roles(Common.Constants.StudentRole)]
        public ActionResult Dashboard(string userId)
        {
            var students = _studentService.GetStudentById(userId);
            var viewModel = AutoMapper.Mapper.Map<StudentProjection, StudentViewModel>(students);
            return View(viewModel);
        }

        [Roles(Common.Constants.StudentRole)]
        public ActionResult FeesDetails(string userId)
        {
            var installment = _installmentService.GetStudInstallments(userId).ToList();
            var viewModelList = AutoMapper.Mapper.Map<List<InstallmentProjection>, InstallmentViewModel[]>(installment);
            return View(viewModelList);
        }

        public ActionResult GetBatch(int classId)
        {
            var batch = _batchService.GetAllBatchesByClassId(classId).ToList();
            return Json(batch, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubjectFees(string selectedSubject, string selectedYear)
        {
            var totalFees = _studentService.GetTotalFees(selectedSubject, selectedYear);
            return Json(totalFees, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetClassesByBranchId(int branchId)
        {
            var subjects = _studentService.GetStudentsByBranchId(branchId).Select(x => new { x.ClassId, x.ClassName }).Distinct();
            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetClassesFromStudent(int? branchId)
        {
            var roleUserId = User.Identity.GetUserId();
            var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);

            if (roles == "BranchAdmin")
            {
                var classes = _studentService.GetClassesByBranchId((int)branchId).Select(x => new { x.ClassId, x.ClassName });
                int studentCount = classes.Count();
                classes = classes.Distinct();
                int teacherCount = _teacherService.GetTeacherContactListBrbranchId((int)branchId).Count();
                int branchAdminCount = _branchAdminService.GetBranchAdminContactListBrbranchId((int)branchId).Count();

                var result = new
                {
                    classes = classes,
                    studentParentCount = studentCount,
                    teacherCount = teacherCount,
                    branchAdminCount = branchAdminCount
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var classes = _studentService.GetClasses().Select(x => new { x.ClassId, x.ClassName });
                int studentCount = classes.Count();
                classes = classes.Distinct();
                int teacherCount = _teacherService.GetTeacherContactList().Count();
                int branchAdminCount = _branchAdminService.GetBranchAdminContactList().Count();
                var result = new
                {
                    classes = classes,
                    studentParentCount = studentCount,
                    teacherCount = teacherCount,
                    branchAdminCount = branchAdminCount
                };

                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GetClassesByMultipleBranches(string selectedBranch)
        {
            var classes = _studentService.GetClassesByMultipleBranchId(selectedBranch).Select(x => new { x.ClassId, x.ClassName });
            var studentCount = classes.Count();
            classes = classes.Distinct();
            var branchIds = selectedBranch.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse);
            var teacherCount = _teacherService.GetTeacherContactList().Where(x => branchIds.Contains(x.BranchId)).Count();
            var branchAdminCount = _branchAdminService.GetBranchAdminContactList().Where(x => branchIds.Contains(x.BranchId)).Count();
            var result = new
            {
                classes = classes,
                studentParentCount = studentCount,
                teacherCount = teacherCount,
                branchAdminCount = branchAdminCount
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult List()
        {
            return View();
        }

        public ActionResult GetSubjects(int classId)
        {
            var subjects = _subjectService.GetSubjectByClassId(classId).ToList();
            return Json(subjects, JsonRequestBehavior.AllowGet);
        }
    }
}