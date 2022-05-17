function AdministrationForm() {
   
    this.loadSpecialEventPage = function () {
        hide("systemContentPage");
        show("Div_SystemLodingCntent");
        var t = setTimeout("new HomeLogin().LoadPage('Administration/Index')", 100);
    };
    
    this.loadSpecialEventListPage = function () {
        hide("systemContentPage");
        show("Div_SystemLodingCntent");
        var t = setTimeout("new HomeLogin().LoadPage('Administration/SpecialEventList')", 100);
    };

    this.AddEditSpecialEvent = function (id) {
       
        var url = 'Administration/AddEditSpecialEvent';
        //var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
        if (id != "")
            url += '?id=' + id;
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };


    this.SaveEvent = function () {
        if (IsStartDateEndDateValidator("EventFromDate", "EventToDate"))
            return;
        
        var model = {
            EventName: $("#EventName").val(),
            Description: $("#Description").val(),
            EventFromDate: $("#EventFromDate").val(),
            EventToDate: $("#EventToDate").val(),
            Id: $("#Id").val()
        };

        if (model.EventName.replace(/\s/g, "") == "" || model.EventFromDate.replace(/\s/g, "") == "" ||
            model.EventToDate.replace(/\s/g, "") == "") {
            alert("Please enter all the required fields.");
            return;
        }

        azyncPost("/Administration/SaveSpecialEvent", model, this.SaveEmployeeDetailsSuccess, ConnectionError);
    };

    

   

    this.SaveEmployeeDetailsSuccess = function(result) {
        if (result.status == "Success") {
            alert('Save completed successfully.');
            var t = setTimeout("new AdministrationForm().loadSpecialEventListPage();", 100);
        } else {
            alert(result.message);
        }
    };
   
    this.UnPlannedLeaves = function() {

        var _url = "/Administration/UnplannedLeaves" + GetFromToDateString();
        var t = setTimeout("new HomeLogin().LoadPage('" + _url + "')", 100);
        
    };
    
    this.GetIncompleteAttendences = function () {

        var _url = "/Administration/GetIncompleteAttendences" + GetFromToDateString();
        var t = setTimeout("new HomeLogin().LoadPage('" + _url + "')", 100);

    };

    
    this.UserManagement = function () {
       var url = "/UserManagement";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };
    
   //---Visitor Pass----//
    this.VisitorPass = function () {
        var url = "Administration/VisitorPass";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };
    

    this.SaveVisitorPassDetails = function () {
        var pass = {
            //EmployeeId: $("#EmployeeId").val(),
            //CardNo: $("#CardNo").val(),
            //pickeddate: $("#pickeddate").val(),
        };

        //azyncPost("/Administration/SaveVisitorPass", pass, this.SaveVisitorPassDetailsSuccess, ConnectionError);
        azyncPost("/Administration/SaveVisitorPass?EmployeeId=" + $("#EmployeeId").val() +"&CardNo=" + $("#CardNo").val() +"&pickeddate=" + $("#pickeddate").val(), pass , this.SaveVisitorPassDetailsSuccess, ConnectionError);
    };
    
    this.SaveVisitorPassDetailsSuccess = function (result) {
        var url = "Administration/VisitorPass";
       if (result.status == "Success") {
            alert('Visitor pass allocated successfully.');
            var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
       } else {
            alert(result.message);
        }
    };


    //--- End Visitor Pass----//



    //---Floor Information---//

    this.FloorInformation = function () {
        
        var url = "Administration/FloorInformation";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };

    this.FindFloorInformation = function () {
       
        var user = {
            LocationId: $("#LocationId").val(),
            SelectedDate: $("#pickeddate").val(),
            userType: $("#SearchUserType").val(),
        };

        azyncPost("/Administration/FindFloorInformation", user,new AdministrationForm().FindFloorInformationSuccess, ConnectionError);
    };

    this.FindFloorInformationSuccess = function (result) {
       // alert('Hi');
       // alert(result.SearchResult.EmployeeLocationContent);
     
        document.getElementById("DIV_EmployeeQuickFindList").innerHTML = result.SearchResult.EmployeeLocationContent;            
          
    }

    this.ShowDetailInOut = function (empId,empName) {
            var user = {
            LocationId: $("#LocationId").val(),
            SelectedDate: $("#pickeddate").val(),
            UserId: empId,
            UserName:empName,
            userType: $("#SearchUserType").val(),
        };
        
        azyncPost("/Administration/ShowDetailInOut", user, new AdministrationForm().ShowDetailInOutSuccess, ConnectionError);
    }

    this.ShowDetailInOutSuccess = function (result) {
       
        document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = result.SearchResult.EmployeeLocationContent;

    }
    //--- End Floor Information----//



    //--Manual Attendence--// 

    this.ManualAttendance = function () {

        var url = "Administration/ManualAttendance";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };

    this.ManualAttendanceBulk = function () {
        var url = "Administration/ManualAttendanceBulk";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };
    
    this.SaveManualAttendenceDetails = function () {
        var user = {
            EmployeeId: $("#EmployeeId").val(),
            LocationId: $("#LocationId").val(),
            InOutMode: $("#InOutMode").val(),
            AttendanceDate: $("#AttendanceDate").val(),
        };

        azyncPost("/Administration/SaveManualAttendance", user, this.SaveManualAttendenceSuccess, ConnectionError);
    };
    
    this.SaveManualAttendenceSuccess = function (result) {
        var url = "Administration/ManualAttendance";
        if (result.status == "Success") {
            alert('Attendance record create successfully.');
            //var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
        } else {
            alert(result.message);
        }
    };
    
    this.SaveManualAttendenceBulkDetails = function () {

        var values = new Array();

         $.each($("input[name='Prints']:checked"),
               function () {
                   if ($(this).val()!=null && $(this).val()!="on" ) {
                  values.push($(this).val());
               }
               });

        var user = {
            Ids: values,
            LocationId: $("#LocationId").val(),
            InOutMode: $("#InOutMode").val(),
            AttendanceDate: $("#AttendanceDate").val(),
        };

        azyncPost("/Administration/SaveManualAttendanceBulk", user, this.SaveManualAttendenceBulkSuccess, ConnectionError);
    };

    this.SaveManualAttendenceBulkSuccess = function (result) {
        var url = "Administration/ManualAttendanceBulk";
        if (result.status == "Success") {
            alert('Attendance record create successfully.');
          //  var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
        } else {
            alert(result.message);
        }
    };
    this.loadBulkInsertion = function () {

        var t = setTimeout("new HomeLogin().LoadPage('Administration/ManualAttendanceBulk')", 100);
    };
    //--End Manual Attendence--//
    

    //--Employee Attendance Report--//
    this.EmployeeAttendanceReport = function() {
        window.open("/Administration/AttendanceReport", null,
            "height=200,width=500,status=yes,toolbar=no,menubar=no,location=no");
    };
    //--End Employee Attendance Report--//
    //PhysicallyNotAvailable Employees
    this.UpdatePhysicallyNotAvailableEmployeesTableProcess = function () {
        if (IsStartDateEndDateValidator("Textbox_DateFrom", "Textbox_DateTo"))
            return;
        var empty = "";
        $("#TblLateEmployee").html(empty);
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'none';

        $("#DIV_GrapfUpdateWaitMessage").show();

        initFunctionDelayedCall(new AdministrationForm().UpdatePhysicallyNotAvailableEmployeesTableProcessComplete);
    };
    this.UpdatePhysicallyNotAvailableEmployeesTableProcessComplete = function () {

        var _physicallyNotAvailableEmployeeModel = {
            dateFrom: $('#Textbox_DateFrom').val(),
            DateTo: $('#Textbox_DateTo').val()
        };
        azyncPost("/Home/GetEmployeesAbsentList", _physicallyNotAvailableEmployeeModel,
           new AdministrationForm().UpdatePhysicallyNotAvailableEmployeesTableSuccess, ConnectionError);
    };

    this.UpdateHalfDayEmployeesTableProcess = function () {
        if (IsStartDateEndDateValidator("Textbox_DateFrom", "Textbox_DateTo"))
            return;
        var empty = "";
        $("#TblLateEmployee").html(empty);
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'none';

        $("#DIV_GrapfUpdateWaitMessage").show();

        initFunctionDelayedCall(new AdministrationForm().UpdateHalfDayEmployeesTableProcessComplete);
    };

    this.UpdateWprkFromHomeEmployeesTableProcess = function () {
        if (IsStartDateEndDateValidator("Textbox_DateFrom", "Textbox_DateTo"))
            return;
        var empty = "";
        $("#TblLateEmployee").html(empty);
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'none';

        $("#DIV_GrapfUpdateWaitMessage").show();

        initFunctionDelayedCall(new AdministrationForm().UpdateWorkFromHomeEmployeesTableProcessComplete);
    };

    this.UpdateHalfDayEmployeesTableProcessComplete = function () {

        var _physicallyNotAvailableEmployeeModel = {
            dateFrom: $('#Textbox_DateFrom').val(),
            DateTo: $('#Textbox_DateTo').val()
        };
        azyncPost("/Home/GetEmployeesHalfDayList", _physicallyNotAvailableEmployeeModel,
           new AdministrationForm().UpdatePhysicallyNotAvailableEmployeesTableSuccess, ConnectionError);
    };


    this.UpdateWorkFromHomeEmployeesTableProcessComplete = function () {

        var _physicallyNotAvailableEmployeeModel = {
            dateFrom: $('#Textbox_DateFrom').val(),
            DateTo: $('#Textbox_DateTo').val()
        };
        azyncPost("/Home/GetEmployeesWorkingFromHomeList", _physicallyNotAvailableEmployeeModel,
           new AdministrationForm().UpdateWorkingFromHomeEmployeesTableSuccess, ConnectionError);
    };

    //Incomplete Attendence report
  
    this.UpdateIncompleteAttendenceTableProcess = function () {
        if (IsStartDateEndDateValidator("Textbox_DateFrom", "Textbox_DateTo"))
            return;
        var empty = "";
        $("#TblLateEmployee").html(empty);
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'none';

        $("#DIV_GrapfUpdateWaitMessage").show();

        initFunctionDelayedCall(new AdministrationForm().UpdateIncompleteAttendenceTableProcessComplete);
    };
 
    this.UpdateIncompleteAttendenceTableProcessComplete = function () {

        var _physicallyNotAvailableEmployeeModel = {
            dateFrom: $('#Textbox_DateFrom').val(),
            DateTo: $('#Textbox_DateTo').val()
        };

        azyncPost("/Home/SearchIncompleteAttendences", _physicallyNotAvailableEmployeeModel,
           new AdministrationForm().UpdatePhysicallyNotAvailableEmployeesTableSuccess, ConnectionError);
    };

    this.UpdatePhysicallyNotAvailableEmployeesTableSuccess = function (result) {
       $("#TblUnplannedLeaves").html();
        $("#TblUnplannedLeaves").dataTable().fnDestroy();
        document.getElementById('DIV_GrapfUpdateWaitMessage').style.display = 'none';
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'block';
        $("#TblUnplannedLeaves").html();
       
        var oTable = $('#TblUnplannedLeaves').dataTable({

            "bJQueryUI": true,
            "bFilter": true,
            "bAutoWidth": false,
            "iDisplayLength": 200,
            "bPaginate": false,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": false,
            "sScrollY": "300px",

            "bCollapse": true,
            "aaData": result.AbsentEmployeeList,
            "aoColumns": [
                { "mData": "EmployeeName" },
                { "mData": "AbsentDate" }
            ],
            "aoColumnDefs": [
              { sClass: "filledWidthColumn", "aTargets": [0] },
               { sClass: "filledWidthColumn", "aTargets": [1] }
            ],
            fnDrawCallback: function () {
                $("#selector thead").remove();
            }
        });
        $("#TblUnplannedLeaves_filter input").css('height', '20px');
        $("#TblUnplannedLeaves_filter").css('width', '100%');
        $(".iframe").colorbox({ iframe: true, width: "700px", height: "85%" });
        $(".iframeManageCheckList").colorbox({ iframe: true, width: "80%", height: "85%" });

    };


    this.UpdateWorkingFromHomeEmployeesTableSuccess = function (result) {
        $("#TblUnplannedLeaves").html();
        $("#TblUnplannedLeaves").dataTable().fnDestroy();
        document.getElementById('DIV_GrapfUpdateWaitMessage').style.display = 'none';
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'block';
        $("#TblUnplannedLeaves").html();

        var oTable = $('#TblUnplannedLeaves').dataTable({

            "bJQueryUI": true,
            "bFilter": true,
            "bAutoWidth": false,
            "iDisplayLength": 200,
            "bPaginate": false,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": false,
            "sScrollY": "300px",

            "bCollapse": true,
            "aaData": result.WokingFromHomeEmployeeList,
            "aoColumns": [
                { "mData": "EmployeeName" },
                { "mData": "Status" },
                { "mData": "AbsentDate" }
            ],
            "aoColumnDefs": [
              { sClass: "filledWidthColumn", "aTargets": [0] },
               { sClass: "filledWidthColumn", "aTargets": [1] },
               { sClass: "filledWidthColumn", "aTargets": [2] }
            ],
            fnDrawCallback: function () {
                $("#selector thead").remove();
            }
        });
        $("#TblUnplannedLeaves_filter input").css('height', '20px');
        $("#TblUnplannedLeaves_filter").css('width', '100%');
        $(".iframe").colorbox({ iframe: true, width: "700px", height: "85%" });
        $(".iframeManageCheckList").colorbox({ iframe: true, width: "80%", height: "85%" });

    };
    //End PhysicallyNotAvailable Employees

}

function GetFromToDateString() {
    
   var _paramStr = "";
    if ($("#Textbox_FromDate").val() != "")
        _paramStr = "fromDate=" + $("#Textbox_FromDate").val();
    if ($("#Textbox_ToDate").val() != "") {
        if (_paramStr != "")
            _paramStr += "&";
        _paramStr += "toDate=" + $("#Textbox_ToDate").val();
    }
    if (_paramStr != "")
        _paramStr = "?" + _paramStr;

    return _paramStr;
}

