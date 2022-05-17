
function EmployeeEnrollmentForm() {
    this.SaveEmployeeEnrollmentDetails = function () {
        var user = {
            EmployeeId: $("#employeeIdHidden").val(),
            CardNo: $("#enrollmentNoTextBox").val(),
            Username: $("#userNameTextBox").val(),
            Privillage: $("#privilegesDropdown").val(),
            IsEnable: $('#isEnableCheckBox').is(':checked')
        };

        azyncPost("/UserManagement/SaveUser", user, this.SaveEmployeeDetailsSuccess, ConnectionError);
    };

    this.SaveEmployeeDetailsSuccess = function(result) {
        if (result.status == "Success") {
            alert('Save completed successfully.');
            var t = setTimeout("new HomeLogin().LoadPage('UserManagement/Index')", 100);
        } else {
            alert(result.message);
        }
    };
}