function Attendence() {


    this.WorkingHome = function (lnk) {
        var url = "Home/WorkingHome";
        $(".syste_hedder_link_selected").removeClass("syste_hedder_link_selected").addClass("syste_hedder_link");
        $(lnk).addClass("syste_hedder_link_selected");
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

    };

    this.ConfirmAttendence = function (lnk) {
        var url = "Home/PendingAttendence";
        $(".syste_hedder_link_selected").removeClass("syste_hedder_link_selected").addClass("syste_hedder_link");
        $(lnk).addClass("syste_hedder_link_selected");
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };
    this.LoadGrid = function () {
        var employeeId = $("#EmployeeId").val()
        if ($("#Dropdown_AppType").val() == 0) {

            var url = "Home/PendingAttendence?pEmployeeId=" + employeeId ;
            var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 200);

        } else if ($("#Dropdown_AppType").val() == 1) {
           
            var url = "Home/ConfirmAttendence?pEmployeeId=" + employeeId;
            var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 200);
        }
        else if ($("#Dropdown_AppType").val() == 2) {
          
            var url = "Home/RejectedAttendence?pEmployeeId=" + employeeId;
            var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 200);
        }
    };
    this.LoadGridSuccess = function (result) {
        debugger

        if (result.status == "Success") {
            var url = "Home/ConfirmAttendence";
            var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

        } else {
            alert(result.status);
        }
    };
    this.ViewAttendence = function () {

        // var url = "Home/ShowEmployeeAttendence?EmployeeId=" + $("#EmployeeId").val();
        var url = "Home/WorkingHome?pEmployeeId=" + $("#EmployeeId").val() + "&updateRecordId=0";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 200);

    }

    this.ViewAttendenceSuccess = function (result) {
        debugger
        if (result.status == "Success") {


        } else {

            alert(result.status);

        }
    }
    this.doConfirmAttendence = function (id) {
        var user = { id: id }
        azyncPost("/Home/SaveConfirmAttendence", user, this.SaveConfirmAttendenceSuccess, ConnectionError);
    }
    this.doConfirmAttendenceBulk = function (id) {
        var values = new Array();

        $.each($("input[name='Prints']:checked"),
              function () {
                  if ($(this).val() != null && $(this).val() != "on") {
                      values.push($(this).val());
                  }
              });
        var users = { idList: values }
        azyncPost("/Home/SaveConfirmAttendenceBulk", users, this.SaveConfirmAttendenceSuccess, ConnectionError);
    }
    this.doRejectAttendence = function (id) {
        var user = { id: id }
        azyncPost("/Home/doRejectAttendence", user, this.doRejectAttendenceSuccess, ConnectionError);
    }

    this.doRejectAttendenceBulk = function () {

        var values = new Array();

        $.each($("input[name='Prints']:checked"),
              function () {
                  if ($(this).val() != null && $(this).val() != "on") {
                      values.push($(this).val());
                  }
              });



        var users = { idList: values }



        azyncPost("/Home/doRejectAttendenceBulk", users, this.doRejectAttendenceSuccess, ConnectionError);
    }

    this.doForwardAttendenceBulk = function () {

        var values = new Array();

        $.each($("input[name='Prints']:checked"),
            function () {
                if ($(this).val() != null && $(this).val() != "on") {
                    values.push($(this).val());
                }
            });

        var supervisorId = $("#SupervisorId").val()

        var users = { idList: values, supervisorId: supervisorId}



        azyncPost("/Home/ForwardEntry", users, this.doRejectAttendenceSuccess, ConnectionError);
    }
    this.SaveConfirmAttendenceSuccess = function (result) {

        if (result.status == "Success") {

            //alert('Attendance record Confirmed . ');

        } else {

            alert(result.status);

        }
    };

    this.doRejectAttendenceSuccess = function (result) {

        if (result.status == "Success") {

           // alert('Attendance record Rejected . ');

        } else {

            alert(result.status);

        }
    };

    this.SaveWorkingHome = function () {

        var user = {
            SupervisorId: $("#SupervisorId").val(),
            AttendanceDateIn: $("#AttendanceDateIn").val(),
            AttendanceDateOut: $("#AttendanceDateOut").val(),
            EmployeeId: $("#EmployeeId").val(),
            Description: $("#Description").val(),
            WorkingFromHomeTaskList: [],
            UpdateRecordId: 0
        };

        var table = $("#EmployeeTaskListTable");
        table.find('.wft-task').each(function (i, el) {
            var $inputs = $(this).find('input'),
                taskHours = $inputs.eq(0).val(),
                taskMinutes = $inputs.eq(1).val();
            var $textAreas = $(this).find('textarea'),
                taskDescription = $textAreas.eq(0).val();
            user.WorkingFromHomeTaskList.push({
                Description: taskDescription,
                Hours: taskHours,
                Minutes: taskMinutes
            });
        });

        azyncPost("/Home/SaveWorkingHome", user, this.SaveWorkingHomeSuccess, ConnectionError);
    };

    this.UpdateButtonPress = function (employeeId, recordId) {

        var url = "Home/WorkingHome?EmployeeId=" + employeeId + "&updateRecordId=" + recordId;
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 200);
    };

    this.UpdateWorkingHome = function () {

        var user = {            
            SupervisorId: $("#SupervisorId").val(),
            AttendanceDateIn: $("#AttendanceDateIn").val(),
            AttendanceDateOut: $("#AttendanceDateOut").val(),
            EmployeeId: $("#EmployeeId").val(),
            Description: $("#Description").val(),
            WorkingFromHomeTaskList: [],
            UpdateRecordId: $("#hdnUpdateRecordId").val()
        };

        var table = $("#EmployeeTaskListTable");
        table.find('.wft-task').each(function (i, el) {
            var $inputs = $(this).find('input'),
                taskHours = $inputs.eq(0).val(),
                taskMinutes = $inputs.eq(1).val();
            var $textAreas = $(this).find('textarea'),
                taskDescription = $textAreas.eq(0).val();
            user.WorkingFromHomeTaskList.push({
                Description: taskDescription,
                Hours: taskHours,
                Minutes: taskMinutes
            });
        });

        azyncPost("/Home/SaveWorkingHome", user, this.UpdateWorkingHomeSuccess, ConnectionError);
    };

    this.SaveWorkingHomeSuccess = function (result) {
        var url = "Home/WorkingHome";

        if (result.status == "Success") {
            if (!alert('Attendance record created successfully and once your supervisor confirm your attendence will take in to consideration as an attendence. ')) {
                var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
            }

        } else {
            alert(result.status);
        }
    };

    this.UpdateWorkingHomeSuccess = function (result) {
        var url = "Home/WorkingHome";

        if (result.status == "Success") {            
            if (!alert('Attendance updated successfully and once your supervisor confirm your attendence will take in to consideration as an attendence. ')) {
                var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
            }

        } else {
            alert(result.status);
        }
    };

    this.DeleteWorkingFromHome = function (employeeId, recordId) {
        var record = {
            EmployeeId: employeeId,
            RecordId: recordId
        };
        azyncPost("/Home/DeleteWorkingHome", record, this.DeleteWorkingHomeSuccess, ConnectionError);
    };

    this.DeleteWorkingHomeSuccess = function (result) {
        var url = "Home/WorkingHome";

        if (result.status == "Success") {
            if (!alert('Attendance deleted successfully')) {
                var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
            }
        } else {
            alert(result.status);
        }
    };
}