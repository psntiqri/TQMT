
function OnSiteForm() {

    this.DeleteAttendance = function (id) {
        if(confirm('Are you sure you want to delete this record?'))
        {
            azyncPost("/OnSite/DeleteOnSiteAttendanceEntry", { Id: id },
                   new OnSiteForm().DeleteOnSiteAttendanceSuccess, ConnectionError);
        }
    }

    this.DeleteOnSiteAttendanceSuccess = function (result) {
        if (result.status == "Success") {
            new OnSiteForm().UpdateEmployeeOnSiteAttendaceList();
        }
        else {
            alert(result.message);
        }
    }
    
    this.UpdateEmployeeOnSiteAttendaceList = function () {
        var t = setTimeout(function () {
            azyncPost("/OnSite/GetEmployeeOnSiteAttendanceEntries", null,
                       new OnSiteForm().EmployeeOnSiteAttendanceSuccess, ConnectionError);
        }, 1000)
    }

    this.EmployeeOnSiteAttendanceSuccess = function(result) {

        document.getElementById('DIV_LoadingReportWaitMessage').style.display = 'none';
        document.getElementById('DIV_EmployeeOnSiteEntryListTable').style.display = 'block';
        $("#EmployeeOnSiteEntryListTable").html();
        $("#EmployeeOnSiteEntryListTable").dataTable().fnDestroy();

        oTable = $('#EmployeeOnSiteEntryListTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "300px",
            "bFilter": true,
            "bCollapse": true,
            "bAutoWidth": false,
            "iDisplayLength": 200,
            "aaData": result,
            "aoColumns": [
                { "mData": "EditLink" },
                { "mData": "DeleteLink" },
                { "mData": "Employee" },
                { "mData": "Date" },
                { "mData": "Time" },
                { "mData": "Status" },
                { "mData": "Location" }
            ],
            "aoColumnDefs": [
                { sClass: "gridIconColumn", "aTargets": [0], "bSortable": false },
                { sClass: "gridIconColumn", "aTargets": [1], "bSortable": false },
                { sClass: "filledWidthColumn", "aTargets": [2] },
                { sClass: "filledWidthColumn", "aTargets": [3] },
                { sClass: "filledWidthColumn", "aTargets": [4] },
                { sClass: "filledWidthColumn", "aTargets": [5] },
                { sClass: "filledWidthColumn", "aTargets": [6] }
            ]
        });

        $('#EmployeeOnSiteEntryListTable_filter').append("<a class='InnerGridButton iframe CNewOnSiteAtte' href='OnSite/OnSiteAttendance'>New On Site Attendance</a>");

        if ($('#UserPriviladge').val() > 0) {
            $('#EmployeeOnSiteEntryListTable_filter').append("<a class='InnerGridButton iframe CManageOnSite' href='OnSite/Index'>Manage On Site Employees</a>");
        }

        $("#EmployeeOnSiteEntryListTable_filter input").css('height', '20px');
        $("#EmployeeOnSiteEntryListTable_filter").css('width', '100%');
        $(".CNewOnSiteAtte").colorbox({ iframe: true, width: "700px", height: "85%", onClosed: function () { new OnSiteForm().UpdateEmployeeOnSiteAttendaceList(); } });
        $(".CManageOnSite").colorbox({ iframe: true, width: "80%", height: "85%" });
    }
}
