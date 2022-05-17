const { data } = require("jquery");

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

    this.SaveTemplate = function () {


        var template = {
            SupervisorId: $("#SupervisorId").val(),
            AttendanceDateIn: $("#AttendanceDateIn").val(),
            AttendanceDateOut: $("#AttendanceDateOut").val(),
            EmployeeId: $("#EmployeeId").val(),
            Description: $("#Description").val(),
            WorkingFromHomeTaskList: [],
            TemplateName: $("#templateName").val(),
            UpdateRecordId: 0
        };

        var table = $("#EmployeeTaskListTable");
        table.find('.wft-task').each(function (i, el) {
            var $textAreas = $(this).find('textarea'),
                taskDescription = $textAreas.eq(0).val();
            template.WorkingFromHomeTaskList.push({
                Description: taskDescription
            });
        });

        azyncPost("/Home/SaveTemplate", template, this.SaveWorkingHomeSuccess, ConnectionError);
    }

    this.UpdateTemplate = function (tempId) {
        var template = {
            SupervisorId: $("#SupervisorId").val(),
            AttendanceDateIn: $("#AttendanceDateIn").val(),
            AttendanceDateOut: $("#AttendanceDateOut").val(),
            EmployeeId: $("#EmployeeId").val(),
            Description: $("#Description").val(),
            WorkingFromHomeTaskList: [],
            TemplateName: $("#templateName").val(),
            UpdateRecordId: tempId
        };

        var table = $("#EmployeeTaskListTable");
        table.find('.wft-task').each(function (i, el) {
            var $textAreas = $(this).find('textarea'),
                taskDescription = $textAreas.eq(0).val();
            template.WorkingFromHomeTaskList.push({
                Description: taskDescription
            });
        });

        azyncPost("/Home/SaveTemplate", template, this.SaveWorkingHomeSuccess, ConnectionError);
    }

    this.RemoveTemplate = function (tempId) {
        var templateObj = {
            TemplateId: tempId
        };

        azyncPost("/Home/RemoveTemplate", templateObj, this.SaveWorkingHomeSuccess, ConnectionError);
    }


    this.LoadTemplateDetails = function () {
        var templateObj = {
            EmployeeId: $("#EmployeeId").val(),
        };

        $.ajax({
            type: "POST",
            url: "/Home/LoadTemplateDetails",
            data: JSON.stringify(templateObj),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var tblRws;
                tblRws += '<tr> <th id="id" style="Display: none;">Template Id</th> <th>Template Name</th> <th>Supervisor Name</th> <th>Reason</th> <th>Action</th> </tr>';
                for (var i = 0; i < data.length; i++) {
                    tblRws += '<tr id="' + i + '"> <td class="nr"  style="Display: none;">' + data[i].Id + '</td> <td>' + data[i].TemplateName + '</td> <td>' + data[i].SupervisorName + '</td>  <td>' + data[i].Description + '</td> <td> <button type="button" class="ButtonForm" onclick="loadTemplate()" >Use Template</button> <input type="button" value="" title="Remove Template" data-toggle="tooltip" class="wft-remove-btn" onclick="removeTemplate()"> </td> </tr>';
                }
                $("#tblTemplate").html(tblRws);
            }
        });
    }

    this.ClearTemplate = function () {
        $("textarea#Description").val("");
        $("#AttendanceDateIn").val("");
        $("#AttendanceDateOut").val("");
        $("#templateName").val("");
        $("#hdnTemplateId").val(0);
        $('#EmployeeTaskListTable').empty();
        addNewTask();
    }

    this.LoadTemplate = function (tempId) {

        var templateObj = {
            TemplateId: tempId
        };

        $.ajax({
            type: "POST",
            url: "/Home/LoadTemplate",
            data: JSON.stringify(templateObj),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                $("textarea#Description").val(data.Description);
                $("#EmployeeId").val(data.EmployeeId);
                $("#SupervisorId").val(data.SupervisorId);
                //$("#AttendanceDateIn").val(data.StartTime);
                //$("#AttendanceDateOut").val(data.EndTime);
                $("#templateName").val(data.TemplateName);
                $("#hdnTemplateId").val(data.Id);
                $('#EmployeeTaskListTable').empty();
                $('#dataEntryTable').empty();
                $.TaskCount = 0;
                for (var i = 0; i < data.TaskList.length; i++) {
                    var table = $('#EmployeeTaskListTable')
                    table.append('<tr class="wft-task">' +
                        '<td class="wft-task-td">' +
                        '<textarea id="desc-' + ($.TaskCount + 1) + '" name="taskDescription" class="wft-task-desc" value="' + data.TaskList[i] + '" placeholder="Add Task Description" rows="2" cols="80" required />' +
                        '</td>' +
                        '<td class="wft-task-td">' +
                        '<input type="number" id="hours-' + ($.TaskCount + 1) + '" name="taskHours" class="wft-task-hours" value="" min="0" max="24" size="2" maxlength="2" placeholder="HH" onchange="validateHh(this);" />' +
                        '</td>' +
                        '<td class="wft-task-td">' +
                        '<input type="number" id="minutes-' + ($.TaskCount + 1) + '" name="taskMinutes" class="wft-task-minutes" value="0" min="0" max="60" size="2" maxlength="2" placeholder="MM" onchange="validateMm(this);" />' +
                        '</td>' +
                        '<td class="wft-task-td">' +
                        '<input type="button" value="" title="Remove" data-toggle="tooltip" class="wft-remove-btn" onclick="DeleteTaskRow(this, ' + $.TaskCount + ')">' +
                        '</td>' +
                        '</tr>');
                    $.TaskCount++;
                }


                for (var i = 0; i < data.TaskList.length; i++) {
                    var id = "textarea#desc-" + (i + 1 );
                    $(id).val(data.TaskList[i]);
                }

            }
        });

        
    }

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