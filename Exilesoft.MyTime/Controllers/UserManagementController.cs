using System;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Mappings;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Controllers
{
    public class UserManagementController : BaseController
    {
        private const string UserNameExistMessage = "Username is exist, try with different username";
        //
        // GET: /UserManagement/

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            
            ViewBag.Privileges = new SelectList(PrivilegesRepository.GetAllPrivilegeList(), "Id", "Name", -2);
            return View();
        }

        [HttpGet]
        public ActionResult GetEmployees(string query = "")
        {
            var searchEmployeeList = EmployeeRepository.EmployeeSearchByName(query);
           // var employees = (new EmployeeViewModelMapper()).Map(searchEmployeeList);
            return Json(searchEmployeeList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet] 
        public ActionResult GetEmployeesEnrollDetails(int employeeId = 0)
        {
            var employeeEnrollment = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employeeId);
            var employeeEnrollmentViewModel = (new EmployeeEnrollmentMapper()).Map(employeeEnrollment);
            return Json(employeeEnrollmentViewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet] 
        public ActionResult CheckUserName(string userName)
        {
            var isUserNameExist = EmployeeEnrollmentRepository.GetEmployeeEnrollmentByUsername(userName);
            return Json(isUserNameExist != null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveUser(EmployeeEnrollment user)
        {
            EmployeeEnrollment enrollment = null;

            if (user.EmployeeId != 0)
            {
                enrollment = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(user.EmployeeId);
            }

            var isUserNameExist = EmployeeEnrollmentRepository.GetEmployeeEnrollmentByUsername(user.UserName);

            if(enrollment != null)
            {
                if(enrollment.UserName != user.UserName && isUserNameExist != null)
                {
                    return Json(new { status = "Failed", message = UserNameExistMessage });
                }
            }
            else
            {
                if(isUserNameExist != null)
                {
                    return Json(new { status = "Failed", message = UserNameExistMessage });
                }
            }

            if (enrollment == null)
            {
                enrollment = new EmployeeEnrollment {EmployeeId = user.EmployeeId};
            }

            enrollment.CardNo = user.CardNo;
            enrollment.UserName = user.UserName;
            enrollment.Privillage = user.Privillage;
            enrollment.IsEnable = user.IsEnable;

            try
            {
                EmployeeEnrollmentRepository.SaveUser(enrollment);
                return Json(new { status = "Success" });
            }
            catch (Exception Ex)
            {
                return Json(new { status = "Failed", message = Ex.Message });
            }
        }

	   
    }
}
