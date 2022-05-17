
function HomeLogin() {

    this.UserName = "";
    this.Password = "";

    this.ValidateLogin = function(e) {
        if (e.keyCode == 13) {
            new HomeLogin().LoginUser();
        }
    };

    this.LoginUser = function() {
        // Creating the Client side view model mapper to sent to the server.
        //var _homeLogin = new HomeLogin();
        //_homeLogin.UserName = $('#UserName').val();
        //_homeLogin.Password = $('#Password').val();

        //if (_homeLogin.UserName.replace(/\s/g, "") == "" || _homeLogin.Password.replace(/\s/g, "") == "") {
        //    ShowAlert("Please enter User Name and Password.");
        //    return;
        //}
        azyncPost("/Home/LoginUser", {}, this.LoginComplete, this.ConnectionError);
    };

    this.LoginComplete = function(result) {
        if (result.Success == "True") {
            $("#login_wave").slideUp(200);
            $("#LogginSlogen_p1").slideUp(200);
            $("#LogginSlogen_p2").slideUp(200);
            $("#LoginPageLogo").slideUp(200);
            $("#mytime_banerlogo").slideUp(200);
            $("#DIV_SystemBody").slideUp(500);
            hide("DIV_PageContent");
            var landingUrl = "new HomeLogin().LoadUserHome(\"/Home/Landing\")";
            var t = setTimeout(landingUrl, 500);
        } else {
            ShowAlert(result.Message);
        }
    };

    this.ConnectionError = function(err) {      
        //window.location.href = "https://portal.tiqri-qa.com/Home";
        window.location.href = "https://localhost:44320/";
    };

    this.LoadUserHome = function(loadUrl) {
        $('#DIV_PageContent').html("");
        $('#DIV_PageContent').load(loadUrl, new function() {
            show("DIV_PageContent")
        });
    };

    this.LoadPage = function(loadUrl) {
        $('#systemContentPage').html("");
        $('#systemContentPage').load(loadUrl, new function() {
        });
    };

    this.UpdateLinkCSS = function(lnk) {
        hide("systemContentPage");
        show("Div_SystemLodingCntent");
        $(".syste_hedder_link_selected").removeClass("syste_hedder_link_selected").addClass("syste_hedder_link");
        $(lnk).addClass("syste_hedder_link_selected");
    };

    this.ShowHome = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Home/Dashboard')", 500);
    };

    this.ShowAnalysis = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Analysis/Index')", 500);
    };

    this.WorkingHome = function(lnk) {
        
        var url = "Home/WorkingHome";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

    };

    this.ShowCoverageList = function (lnk) {

        var url = "Home/ShowCoverage";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

    }; 

    this.ShowCoverageList90Less = function (lnk) {

        var url = "Home/ShowCoverageList90Less";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

    }

    this.ShowCoverageListMore90Less100 = function (lnk) {

        var url = "Home/ShowCoverageListMore90Less100";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

    }

    this.ShowCoverageListMoreThan100 = function (lnk) {

        var url = "Home/ShowCoverageListMoreThan100";
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);

    }
    this.SaveWorkingHome = function () {
        
        var user = {
            SupervisorId: $("#AttendanceDateIn").val(),
            AttendanceDateIn: $("#AttendanceDateIn").val(),
            AttendanceDateOut: $("#AttendanceDateOut").val()
        };

        azyncPost("/Home/SaveWorkingHome", user, this.SaveWorkingHomeSuccess, ConnectionError);
    };

    this.SaveWorkingHomeSuccess = function (result) {
        var url = "Home/WorkingHome";
        
        if (result.status == "Success") {
            alert('Attendance record create successfully.');
            //var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
        } else {
           
            alert(result.status);
            
        }
    };
    this.QuickFind = function (lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Attendance/Index')", 500);
    };

    this.QuickFindWithEmployee = function (employeeId, pickDate) {
        this.UpdateLinkCSS(document.getElementById('EmployeeQuickFindTD'));
        var url = "Attendance/Index";
        if (employeeId != "")
            url += "?EmployeeId=" + employeeId;
        if (pickDate != "")
            url += "&pickeddate=" + pickDate;
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 500);
    };

    this.MyProfile = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Employee/MyProfileView')", 500);
    };

    this.ShowEmployees = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Employee/Index')", 500);
    };

    this.ShowTeamForCustomer = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Customer/Index')", 500);
    };


    this.Administration = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('Administration/Index')", 500);
    };

    this.ShowOnSite = function(lnk) {
        this.UpdateLinkCSS(lnk);
        var t = setTimeout("new HomeLogin().LoadPage('OnSite/OnSiteAttendanceList')", 500);
    };
}
