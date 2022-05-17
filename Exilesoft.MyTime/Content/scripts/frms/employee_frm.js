var _lastUploadedImageSucsess;

function EmployeeForm() {

    this.loadExilesoftEmployeePage = function() {
        hide("systemContentPage");
        show("Div_SystemLodingCntent");
        var t = setTimeout("new HomeLogin().LoadPage('Employee/Index')", 100);
    };

  
    this.loadChecklistPage = function() {
        window.open("/Checklist/Index", null,
            "height=500,width=1000,status=yes,toolbar=no,menubar=yes,scrollbars=yes,location=yes");
    };

    this.DeleteQualification = function(i) {
        azyncPost("/Employee/DeleteQualification", { index: i },
            new EmployeeForm().DeleteQualificationSuccess, ConnectionError);
    };

    this.DeleteQualificationSuccess = function(result) {
        if (result.status == "Success") {

            var rows = $("#EmployeeQualificationDataTable").dataTable().fnGetNodes();
            for (var i = 0; i < rows.length; i++) {
                // Get HTML of 3rd column (for example)
                if ($(rows[i]).find("td:eq(0)").html() == result.rowId)
                    oTable.fnDeleteRow(i);
            }
            $("#EmployeeQualificationDataTable").dataTable().fnDraw();
            AttachDeleteEvent();
        } else {
            alert(result.message);
        }
    };

    this.AddQualification = function() {
        var qualification = {
            InstituteId: $('#Dropdown_Institute').val(),
            Qualification: $('#QualificationName').val(),
            DateAwarded: $('#DateAwarded').val(),
            Notes: $('#QualificationNote').val()
        };

        if (qualification.InstituteId == "" || qualification.Qualification == "") {
            alert('Please select the institute and the qualification.');
            return;
        }

        azyncPost("/Employee/AddQualification", qualification,
            new EmployeeForm().AddQualificationSuccess, ConnectionError);
    };

    this.AddQualificationSuccess = function(result) {
        if (result.status == "Success") {
            oTable.fnAddData([
                result.nextRowId,
                "Delete",
                result.InstituteName,
                $('#QualificationName').val(),
                $('#DateAwarded').val(),
                $('#QualificationNote').val()]);

            AttachDeleteEvent();

            $('#Dropdown_Institute').val("");
            $('#QualificationName').val("");
            $('#DateAwarded').val("");
            $('#QualificationNote').val("");
        } else {
            alert(result.message);
        }
    };

    this.OpenUploader = function(begin) {
        var el = document.getElementById('ProfileImage_UploderFrame');
        _lastUploadedImageSucsess = this.ChangeImageComplete;
        getIframeWindow(el).OpenUploader(this.ChangeImageBegin);
    };

    this.ChangeImageBegin = function() {
        var img = document.getElementById("Image_Preview");
        //img.src = "../../Content/images/dynamic/" + fileName;
    };

    this.ChangeImageComplete = function(fileName) {
        var img = document.getElementById("Image_Preview");
        img.src = "../../Content/images/employee/" + fileName;
        new EmployeeForm().UpdateEmployeeFileName(fileName);
    };

    this.OnUpload = function(evt) {
        var files = $("#file1").get(0).files;
        if (files.length > 0) {

            var ext = $('#file1').val().split('.').pop().toLowerCase();
            if ($.inArray(ext, ['gif', 'png', 'jpg', 'jpeg']) == -1) {
                alert('Invalid image type. Please try again.');
                return;
            }

            if (window.FormData !== undefined) {
                var data = new FormData();
                for (i = 0; i < files.length; i++) {
                    data.append("file" + i, files[i]);
                }
                $.ajax({
                    type: "POST",
                    url: "/api/fileupload",
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function(results) {
                        for (i = 0; i < results.length; i++) {
                            $('#Image_Preview').attr("src", "../../Content/images/employee/" + results[i]);
                            new EmployeeForm().UpdateEmployeeFileName(results[i]);
                        }
                    }
                });
            } else {
                alert("This browser doesn't support HTML5 multiple file uploads!");
            }
        }
    };

    this.UpdateEmployeeFileName = function(name) {
        azyncPost("/Employee/UpdateEmployeeFileName", { fileName: name },
            new EmployeeForm().UpdateEmployeeFileNameSuccess, ConnectionError);
    };

    this.UpdateEmployeeFileNameSuccess = function(result) {
        if (result.status != "Success") {
            alert(result.message);
        }
    };

    this.AddEditEmployee = function(id) {
        var url = 'Employee/AddEditEmployee';
        if (id != "")
            url += '?id=' + id;
        var t = setTimeout("new HomeLogin().LoadPage('" + url + "')", 100);
    };

    this.EditEmployeProfile = function() {
        var t = setTimeout("new HomeLogin().LoadPage('Employee/MyProfileUpdate')", 100);
    };

    this.UpdateMyProfile = function() {
        var profile = {
            Id: $("#Id").val(),
            Title: $("#Title").val(),
            ImagePath: $("#ImagePath").val(),
            FirstName: $("#FirstName").val(),
            LastName: $("#LastName").val(),
            Name: $("#Name").val(),
            Gender: $("#Gender").val(),
            NICNumber: $("#NICNumber").val(),
            PassportNumber: $("#PassportNumber").val(),
            DateOfBirth: $("#DateOfBirth").val(),
            CivilStatus: $("#CivilStatus").val(),
            HomePhone: $("#HomePhone").val(),
            MobileNumber: $("#MobileNumber").val(),
            SkypeID: $("#SkypeID").val(),
            MSNID: $("#MSNID").val(),
            PrimaryEmailAddress: $("#PrimaryEmailAddress").val(),
            CurrentAddress: $("#CurrentAddress").val(),
            PermanentAddress: $("#PermanentAddress").val(),
            DateCareerStarted: $("#DateCareerStarted").val(),
            PreviousEmployer: $("#PreviousEmployer").val(),
            EmergencyContactName: $("#EmergencyContactName").val(),
            EmergencyContactAddress: $("#EmergencyContactAddress").val(),
            EmergencyContactRelationship: $("#EmergencyContactRelationship").val(),
            EmergencyContactNumber: $("#EmergencyContactNumber").val()
        };

        if (profile.Title.replace(/\s/g, "") == "" || profile.Gender.replace(/\s/g, "") == "" ||
            profile.FirstName.replace(/\s/g, "") == "" || profile.LastName.replace(/\s/g, "") == "" ||
            profile.Name.replace(/\s/g, "") == "" || profile.DateOfBirth.replace(/\s/g, "") == "" ||
            profile.MobileNumber.replace(/\s/g, "") == "" || profile.PrimaryEmailAddress.replace(/\s/g, "") == "") {
            alert("Please enter all the required fields.");
            return;
        }

        azyncPost("/Employee/UpdateMyProfile", profile, this.UpdateMyProfileSuccess, ConnectionError);
    };

    this.UpdateMyProfileSuccess = function(result) {
        if (result.status == "Success") {
            new HomeLogin().MyProfile();
        } else {
            alert(result.message);
        }
    };

    this.CancelAddEdit = function() {
        var t = setTimeout("new HomeLogin().LoadPage('Employee/Index')", 100);
    };

    this.SaveEmployeeDetails = function() {
        var profile = {
            Id: $("#Id").val(),
            Title: $("#Title").val(),
            ImagePath: $("#ImagePath").val(),
            FirstName: $("#FirstName").val(),
            LastName: $("#LastName").val(),
            Name: $("#Name").val(),
            Gender: $("#Gender").val(),
            NICNumber: $("#NICNumber").val(),
            PassportNumber: $("#PassportNumber").val(),
            DateOfBirth: $("#DateOfBirth").val(),
            CivilStatus: $("#CivilStatus").val(),
            HomePhone: $("#HomePhone").val(),
            MobileNumber: $("#MobileNumber").val(),
            SkypeID: $("#SkypeID").val(),
            MSNID: $("#MSNID").val(),
            PrimaryEmailAddress: $("#PrimaryEmailAddress").val(),
            CurrentAddress: $("#CurrentAddress").val(),
            PermanentAddress: $("#PermanentAddress").val(),
            DateCareerStarted: $("#DateCareerStarted").val(),
            PreviousEmployer: $("#PreviousEmployer").val(),
            EmergencyContactName: $("#EmergencyContactName").val(),
            EmergencyContactAddress: $("#EmergencyContactAddress").val(),
            EmergencyContactRelationship: $("#EmergencyContactRelationship").val(),
            EmergencyContactNumber: $("#EmergencyContactNumber").val(),

            Designation: $("#Designation").val(),
            DateJoined: $("#DateJoined").val(),
            CardNo: $("#CardNo").val(),
            Username: $("#Username").val(),
            EPFNumber: $("#EPFNumber").val(),
            EPFStatus: $("#EPFStatus").val(),
            DateConfirationDue: $("#DateConfirationDue").val(),
            DateResigned: $("#DateResigned").val(),
            Priv: $("#Priv").val(),
            Enable: $('#Enable').is(':checked')
        };

        if (profile.Title.replace(/\s/g, "") == "" || profile.Gender.replace(/\s/g, "") == "" ||
            profile.FirstName.replace(/\s/g, "") == "" || profile.LastName.replace(/\s/g, "") == "" ||
            profile.Name.replace(/\s/g, "") == "" || profile.DateOfBirth.replace(/\s/g, "") == "" ||
            profile.MobileNumber.replace(/\s/g, "") == "" || profile.PrimaryEmailAddress.replace(/\s/g, "") == "") {
            alert("Please enter all the required fields.");
            return;
        }

        azyncPost("/Employee/SaveEmployee", profile, this.SaveEmployeeDetailsSuccess, ConnectionError);
    };

    this.SaveEmployeeDetailsSuccess = function(result) {
        if (result.status == "Success") {
            alert('Save completed successfully.');
            var t = setTimeout("new HomeLogin().LoadPage('Employee/Index')", 100);
        } else {
            alert(result.message);
        }
    };


    this.EmployeeAttendanceReport = function() {
        window.open("/Employee/AttendanceReport", null,
            "height=200,width=500,status=yes,toolbar=no,menubar=no,location=no");
    };
    
}


