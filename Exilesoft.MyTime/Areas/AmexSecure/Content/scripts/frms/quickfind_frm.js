
function QuickFind() {

	this.QuickFindLocation = function () {
        $("#DIV_EmployeeQuickFindList").html($("#DIV_BusyState").html());
        $("#DIV_AttendanceHistoryDetail").html($("#DIV_BusyState").html());

        var _searchData = {
            employeeID: $("#SelectedEmployeeID").val(),
            employeeName: GetSearchTextBoxText("TextBox_Employee_GlobalSearch"),
            selectedDate: $("#pickeddate").val(),
            userType: $("#SearchUserType").val()
        };

        azyncGet("/Amex/quickfindlocation", _searchData,
               new QuickFind().UpdateQuickFindLocationOnParaChangeSuccess, ConnectionError);
    }

    this.UpdateQuickFindLocationOnParaChangeSuccess = function (result) {
        if (result.SearchResult.EmployeeLocationContent == "0") {
            document.getElementById("DIV_EmployeeQuickFindList").innerHTML = $("#DIV_EmptyState").html();
            document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = $("#DIV_EmptyState").html();
        }
        else {
            if (result.SearchResult.EmployeeLocationContent != "") {
                document.getElementById("DIV_EmployeeQuickFindList").innerHTML = result.SearchResult.EmployeeLocationContent;
                document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = result.SearchResult.AttendanceListEntryContent;
            }
            else {
                document.getElementById("DIV_EmployeeQuickFindList").innerHTML = "<div class='page_hedder_mid'>Please enter the keywords to search.</div>";
                document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = "<div class='page_hedder_mid'>Please enter the keywords to search.</div>";
            }
        }
    }

    this.OnSearchTextChange = function () {
        $("#SelectedEmployeeID").val("");
        var _searchData = {
            employeeID: $("#SelectedEmployeeID").val(),
            employeeName: GetSearchTextBoxText("TextBox_Employee_GlobalSearch"),
            selectedDate: $("#pickeddate").val(),
            userType: $("#SearchUserType").val()
        };
        if (_searchData.employeeName != "" && _searchData.employeeName != "*" && _searchData.employeeName.length >2) {
		    initLoadObjectCall("/Amex/QuickFindLocation", _searchData, this.UpdateQuickFindLocationSuccess,
			    ConnectionError, this.OnSearchStart);
        } else if (_searchData.employeeName != "" && _searchData.employeeName == "*") {
		    _searchData.employeeName = "";
		    initLoadObjectCall("/Amex/QuickFindLocation", _searchData, this.UpdateQuickFindLocationSuccess,
			    ConnectionError, this.OnSearchStart);
	    }
	    else {
		    document.getElementById("DIV_EmployeeQuickFindList").innerHTML = $("#DIV_EmptyIntialState").html();
		    document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = $("#DIV_EmptyIntialState").html();
	    }
    }

    this.OnSearchStart = function () {
        document.getElementById("TextBox_Employee_GlobalSearch").className = "globalSearchTextBox_busy";
        document.getElementById("TextBox_Employee_GlobalSearch").blur();
        $("#DIV_EmployeeQuickFindList").html($("#DIV_BusyState").html());
        $("#DIV_AttendanceHistoryDetail").html($("#DIV_BusyState").html());
    }

    this.UpdateQuickFindLocationSuccess = function (result) {
        if (result.SearchResult.EmployeeLocationContent == "0") {
            document.getElementById("DIV_EmployeeQuickFindList").innerHTML = $("#DIV_EmptyState").html();
            document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = $("#DIV_EmptyState").html();
        }
        else {
            if (result.SearchResult.EmployeeLocationContent != "") {
                document.getElementById("DIV_EmployeeQuickFindList").innerHTML = result.SearchResult.EmployeeLocationContent;
                document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = result.SearchResult.AttendanceListEntryContent;                
            }
            else {
                document.getElementById("DIV_EmployeeQuickFindList").innerHTML = "<div class='page_hedder_mid'>Please enter the keywords to search.</div>";
                document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = "<div class='page_hedder_mid'>Please enter the keywords to search.</div>";
            }
        }

        document.getElementById("TextBox_Employee_GlobalSearch").className = "globalSearchTextBox";
        //if (GetSearchTextBoxText("TextBox_Employee_GlobalSearch") != "")
        //document.getElementById("TextBox_Employee_GlobalSearch").select();

        if (GetSearchTextBoxText("TextBox_Employee_GlobalSearch") != "")
            document.getElementById("TextBox_Employee_GlobalSearch").focus();


    }


    // On Selection click of an employee - Following two
    this.ShowEmployeeAttendnace = function (employeeID)
    {
        $("#SelectedEmployeeID").val(employeeID);
        $("#DIV_AttendanceHistoryDetail").html($("#DIV_BusyState").html());

        var _searchData = {
            employeeID: $("#SelectedEmployeeID").val(),
            employeeName: GetSearchTextBoxText("TextBox_Employee_GlobalSearch"),
            selectedDate: $("#pickeddate").val(),
            userType: $("#SearchUserType").val()
        };

        azyncGet("/Amex/QuickFindLocation", _searchData,
               new QuickFind().ShowEmployeeAttendnaceSuccess, ConnectionError);
    }

    this.ShowEmployeeAttendnaceSuccess = function (result) {
        if (result.SearchResult.EmployeeLocationContent == "0") {
            document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = $("#DIV_EmptyState").html();
        }
        else {
            if (result.SearchResult.EmployeeLocationContent != "") {
                document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = result.SearchResult.AttendanceListEntryContent;
            }
            else {
                document.getElementById("DIV_AttendanceHistoryDetail").innerHTML = "<div class='page_hedder_mid'>Please enter the keywords to search.</div>";
            }
        }
    }

    //this.OnSelectedDateChanged = function () {
    //	$("#SelectedEmployeeID").val("");
	//    var employeeName = GetSearchTextBoxText("TextBox_Employee_GlobalSearch");
	//    if (employeeName != "")
	//    	this.QuickFindLocation();
    //}

    this.OnSelectedDateChanged = function () {
       
            this.QuickFindLocation();
    }

    this.SwitchUserType = function (userType) {
        if ($("#SearchUserType").val() == userType)
            return;

        $("#TD_EmployeeAttendanceOption").removeClass("optionTableSelected");
        $("#TD_VisitorAttendanceOption").removeClass("optionTableSelected");

        if (userType == "Employee") {
            $("#TD_EmployeeAttendanceOption").addClass("optionTableSelected");
        }
        else {
            $("#TD_VisitorAttendanceOption").addClass("optionTableSelected");
        }

        $("#SearchUserType").val(userType);
        $("#SelectedEmployeeID").val("");
        $("#TextBox_Employee_GlobalSearch").val("");
        AddWatermark("TextBox_Employee_GlobalSearch", "Who do you want to find ?");
        this.QuickFindLocation();        
    }


    this.ShowVisitorAttendnace = function (visitorID) {
        $("#TextBox_Employee_GlobalSearch").val(visitorID);
        $("#TextBox_Employee_GlobalSearch").removeClass('watermark');

        $("#DIV_AttendanceHistoryDetail").html($("#DIV_BusyState").html());

        var _searchData = {
            employeeID: $("#SelectedEmployeeID").val(),
            employeeName: GetSearchTextBoxText("TextBox_Employee_GlobalSearch"),
            selectedDate: $("#pickeddate").val(),
            userType: $("#SearchUserType").val()
        };

        azyncGet("/Amex/QuickFindLocation", _searchData,
               new QuickFind().ShowEmployeeAttendnaceSuccess, ConnectionError);
    }

}
