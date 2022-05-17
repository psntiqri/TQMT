
var _selectedSessionLogID = null;
var _selectedMachineLogID = null;

function SynServiceBoard() {

    this.UpdateSessionLogs = function () {
        azyncGet("/SyncService/GetSessionLogEntries", null,
               new SynServiceBoard().UpdateSessionLogDataTable, ConnectionError);
    }

    this.UpdateSessionLogDataTable = function (result) {
        $("#SessionLogTable").html();
        $("#SessionLogTable").dataTable().fnDestroy();

        oTable = $('#SessionLogTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "150px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result.SessionLogViewModelList,
            "aoColumns": [
                { "mData": "SessionID" },
                { "mData": "StatusIcon" },
                { "mData": "StartTime" },
                { "mData": "EndTime" },
                { "mData": "Status" },
                { "mData": "NoOfEmployeeEntries" },
                { "mData": "NoOfVisitorEntrie" }
            ],
            "aoColumnDefs": [
                { sClass: "gridHiddenColumn", "aTargets": [0], "bSortable": false },
                { sClass: "gridIconColumn", "aTargets": [1], "bSortable": false }
            ]
        });

        oTable.fnSort([[2, 'desc']]);

        $('#SessionLogTable tr').css('cursor', 'pointer');
        $('#SessionLogTable tr').die("click");
        $('#SessionLogTable tr').live('click', function () {
            var objectID;
            var nTds = $('td', $(this).closest('tr'));
            var objectID = $(nTds[0]).text();
            _selectedSessionLogID = objectID;
            _selectedMachineLogID = null;
            new SynServiceBoard().UpdateMachineLogs();
        });


        $("#DIV_ReportViewerProcessingTime").html("");
        var newData = result.ResultGraphData;
        g = new Dygraph(document.getElementById('DIV_ReportViewerProcessingTime'), newData,
        {
            strokeWidth: 2,
            animatedZooms: true
        });

        new SynServiceBoard().UpdateMachineLogs();
    }

    this.UpdateMachineLogs = function() {
        var _searchData = {
            sessionLogID: _selectedSessionLogID
        };
        azyncGet("/SyncService/GetMachineLogEntries", _searchData,
                new SynServiceBoard().UpdateMachineLogDataTable, ConnectionError);
    }

    this.UpdateMachineLogDataTable = function(result) {

        $("#MachineLogTable").html();
        $("#MachineLogTable").dataTable().fnDestroy();
        oTable = $('#MachineLogTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "208px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result,
            "aoColumns": [
                { "mData": "MachineLogID" },
                { "mData": "StatusIcon" },
                { "mData": "StartTime" },
                { "mData": "EndTime" },
                { "mData": "Status" },
                { "mData": "Description" },
                { "mData": "NoOfEmployeeEntries" },
                { "mData": "NoOfVisitorEntrie" }
            ],
            "aoColumnDefs": [
                { sClass: "gridHiddenColumn", "aTargets": [0], "bSortable": false },
                { sClass: "gridIconColumn", "aTargets": [1], "bSortable": false }
            ]
        });

        oTable.fnSort([[2, 'desc']]);

        $('#MachineLogTable tr').css('cursor', 'pointer');
        $('#MachineLogTable tr').die("click");
        $('#MachineLogTable tr').live('click', function () {
            var locationID;
            var nTds = $('td', $(this).closest('tr'));
            var objectID = $(nTds[0]).text();
            _selectedMachineLogID = objectID;
            new SynServiceBoard().UpdateSyncLogs();
        });

        new SynServiceBoard().UpdateSyncLogs();
    }

    this.UpdateSyncLogs = function() {
        var _searchData = {
            sessionLogID: _selectedSessionLogID,
            machineLogID: _selectedMachineLogID
        };
        azyncGet("/SyncService/GetSyncLogEntries", _searchData,
                new SynServiceBoard().UpdateSyncLogDataTable, ConnectionError);
    }

    this.UpdateSyncLogDataTable = function(result) {

        $("#SyncLogTable").html();
        $("#SyncLogTable").dataTable().fnDestroy();
        oTable = $('#SyncLogTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "200px",
            "bFilter": false,
            "aaData": result,
            "aoColumns": [
                { "mData": "StatusIcon" },
                { "mData": "StartTime" },
                { "mData": "Location" },
                { "mData": "Description" },
                { "mData": "Status" }
            ],
            "aoColumnDefs": [
                { sClass: "gridIconColumn", "aTargets": [0], "bSortable": false }
            ]
        });
    }
}