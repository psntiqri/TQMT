using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.Models;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class EmployeeController : ApiController
    {
        private IVisitorPassAllocationRepository _visitorPassAllocationRepository;
        private AttendanceRepository _attendanceRepository;
        public EmployeeController()
        {
            _visitorPassAllocationRepository= new VisitorPassAllocationRepository();
            _attendanceRepository= new AttendanceRepository();
        }
        // GET api/employee
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/employee/5
        public List<EmployeeViewModel> Get(string name)
        {
            List<EmployeeData> employeeList =EmployeeRepository.EmployeeSearchByNameInCacheAndEmployeeService(name).Where(e=>e.IsEnable==true).ToList();
            List<EmployeeViewModel> employeeViewModelList= new List<EmployeeViewModel>();

            foreach (EmployeeData employee in employeeList)
            {
                EmployeeViewModel employeeViewModel = new EmployeeViewModel();
                employeeViewModel.Name = employee.Name;
                employeeViewModel.Id = employee.Id;
                employeeViewModel.ImagePath = employee.ImagePath;
                employeeViewModelList.Add(employeeViewModel);
            }
            if (employeeViewModelList.Count == 1)
            {
                EmployeeViewModel employeeViewModel = employeeViewModelList[0];
                EmployeeEnrollment employeeEnrollmentById = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employeeViewModel.Id);
                Card card = null;
                if (employeeEnrollmentById != null)
                    employeeViewModel.EmployeeCardAccessLevel = CardRepository.GetCardAccessLevel(employeeEnrollmentById.CardNo);
                
                string cardList = "";
                foreach (
                    VisitorPassAllocation visitorPassAllocation in
                        VisitorPassAllocationRepository.GetActiveVisitorPassAllocationForEmployee(employeeViewModel.Id))
                {
                    cardList = cardList + "," + visitorPassAllocation.CardNo;
                }
                employeeViewModel.VisitorCardsNotReturned = cardList.TrimStart(',');
            }

            return employeeViewModelList;
        }

        public EmployeeViewModel Get(int id)
        {
            
            EmployeeViewModel employeeViewModel = new EmployeeViewModel();
            employeeViewModel.Name = EmployeeRepository.GetEmployee(id).Name;
            employeeViewModel.Id = id;
            EmployeeEnrollment employeeEnrollmentById = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(id);
            Card card = null;
            if (employeeEnrollmentById != null)
                employeeViewModel.EmployeeCardAccessLevel = CardRepository.GetCardAccessLevel(employeeEnrollmentById.CardNo);
            
            string cardList = "";
            foreach (
                VisitorPassAllocation visitorPassAllocation in
                    VisitorPassAllocationRepository.GetActiveVisitorPassAllocationForEmployee(id))
            {
                cardList = cardList + "," + visitorPassAllocation.CardNo;
            }
            employeeViewModel.VisitorCardsNotReturned = cardList.TrimStart(',');
            

            return employeeViewModel;
        }

        // POST api/employee
        [HttpPost]
        public ReceptionActionResult Post([FromBody]EmployeeViewModel employeeViewModel)
        {
            var result = new ReceptionActionResult();
            var message = string.Empty;
            result.Status = _visitorPassAllocationRepository.IsValidCard(employeeViewModel.NewVisitorCard, employeeViewModel.Id,ref message);
            result.Message = message;

            if (!result.Status)
                return result;

            if (employeeViewModel.IsUpdate)
            {
                UpdateEmployeeVisitorPass(employeeViewModel);
            }

            else
            {

                var visitorPassAllocation = new VisitorPassAllocation();
                visitorPassAllocation.AssignDate = Utility.GetDateTimeNow().Date;
                visitorPassAllocation.EmployeeId = employeeViewModel.Id;
                visitorPassAllocation.CardNo = employeeViewModel.NewVisitorCard;
                visitorPassAllocation.IsActive = true;
                visitorPassAllocation.IsCardReturned = false;
                visitorPassAllocation.CardIssuedBy = 1;
                _visitorPassAllocationRepository.AssignEmployeeCard(visitorPassAllocation);
                _attendanceRepository.AllocatedHistoryAttendance(employeeViewModel.NewVisitorCard,
                    visitorPassAllocation.Id, employeeViewModel.Id, Utility.GetDateTimeNow().Date);
            }
            return result;
        }

        private void UpdateEmployeeVisitorPass(EmployeeViewModel employeeViewModel)
        {
            var visitorPassAllocation = _visitorPassAllocationRepository.GetById(employeeViewModel.VisitorPassAllocationId);

            _attendanceRepository.ModifyCardNumber(employeeViewModel.NewVisitorCard, visitorPassAllocation.Id,
                                                   visitorPassAllocation.EmployeeId.Value, Utility.GetDateTimeNow().Date);
        }

        // PUT api/employee/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/employee/5
        public void Delete(int id)
        {
        }
    }
}
