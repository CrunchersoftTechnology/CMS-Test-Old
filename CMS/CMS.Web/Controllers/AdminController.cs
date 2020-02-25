using CMS.Common;
using CMS.Domain.Storage.Projections;
using CMS.Domain.Storage.Services;
using CMS.Web.Logger;
using CMS.Web.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Web.Controllers
{
    [Authorize(Roles = Common.Constants.AdminRole + "," + Common.Constants.BranchAdminRole + "," + Common.Constants.StudentRole)]
    public class AdminController : BaseController
    {
        readonly IBranchService _branchService;
        readonly IBatchService _batchService;
        readonly ITeacherService _teacherService;
        readonly IStudentService _studentService;
        readonly ILogger _logger;
        readonly IAspNetRoles _aspNetRolesService;
        readonly IApiService _apiService;

        public AdminController(IBranchService branchService,
            IBatchService batchService,
            ITeacherService teacherService,
            IStudentService studentService,
            ILogger logger, IAspNetRoles aspNetRolesService,IApiService apiService)
        {

            _branchService = branchService;
            _batchService = batchService;
            _teacherService = teacherService;
            _studentService = studentService;
            _logger = logger;
            _aspNetRolesService = aspNetRolesService;
            _apiService = apiService;
        }
        // GET: Admin
        //[Route("adminLogin")]
        public ActionResult Index()
        {
            var summaryModel = new AdminSummaryViewModel
            {
                BatchesCount = _batchService.GetBatchesCount(),
                BranchesCount = _branchService.GetBranchesCount(),
                StudentsCount = _studentService.GetStudentsCount(),
                TeachersCount = _teacherService.GetTeachersCount(),
                PendingAdmissionCount =_apiService.GetPendingAdmission(),
            };
            var roleUserId = User.Identity.GetUserId();
            var roles = _aspNetRolesService.GetCurrentUserRole(roleUserId);
            if (roles == "Student")
                return RedirectToAction("Index", "StudentDashBoard");
            else
                return View(summaryModel);
        }

        public ActionResult PendingAdmissionList()
        {
            var admissionList = _apiService.GetPendingAdmissionList();
            var admission = JsonConvert.DeserializeObject<List<PendingStudentAdmissionProjection>>(admissionList);
            var viewModelList = AutoMapper.Mapper.Map<List<PendingStudentAdmissionProjection>, PendingStudentAdmissionViewModel[]>(admission);
            return View(viewModelList);
        }
    }
}