
function DashBoard() {

    // Load dialog on click
    $('#specialEventNortification').click(function (e) {
       // $('#specilEventDetail').modal();
        $.colorbox({ html: $("#specilEventDetail").html() });
        return false;
    });

    this.UpdateEmployeeAvailability = function() {
        azyncPost("/Home/GetEmployeeAvailability", null,
            new DashBoard().EmployeeAvailabilitySuccess, ConnectionError);
    },
    this.HighlightWorkCoverage = function (workCoverage) {
          
          var coverageText = '';
          if (workCoverage >= 90) {

              $("#SPAN_My_Workcoverage").css('color', 'green');
          }
          else if (workCoverage >= 80) {
              
              $("#SPAN_My_Workcoverage").css('color', 'orange');
          }
          else {
              
              $("#SPAN_My_Workcoverage").css('color', 'red');
          }
         
    },

    this.HighlightCompanyWorkCoverage = function (workCoverage) {
       
        if (workCoverage >= 90) {

            $("#SPAN_Company_Workcoverage").css('color', 'green');
        }
        else if (workCoverage >= 80) {

            $("#SPAN_Company_Workcoverage").css('color', 'orange');
        }
        else {

            $("#SPAN_Company_Workcoverage").css('color', 'red');
        }

    },
    this.HighlightAchievedWorkCoverage = function (workCoverage) {

        if (workCoverage >= 90) {

            $("#SPAN_Achieved_Workcoverage").css('color', 'green');
        }
        else if (workCoverage >= 80) {

            $("#SPAN_Achieved_Workcoverage").css('color', 'orange');
        }
        else {

            $("#SPAN_Achieved_Workcoverage").css('color', 'red');
        }

    },

    this.EmployeeAvailabilitySuccess = function(result) {
        $("#DIV_EmployeeAvailabilityGraph").html("");
        $("#specilEventDetail").html("");
        $("#specialEventNortification").hide();
        if (result.specilEvent != "") {
            $("#specialEventNortification").show();
            $("#specilEventDetail").html(result.specilEvent);
        }

        g = new Dygraph(document.getElementById('DIV_EmployeeAvailabilityGraph'), result.GraphData,
            {
                strokeWidth: 1,
                animatedZooms: true,
                fillGraph: true,
                labelsDivStyles: { 'textAlign': 'right' },
                xValueFormatter: function(x, opts, series_name, dg) {
                    return new Date(x).strftime('%d/%m/%Y');
                }
            });

        if ((result.PeopleIn == 0) && (result.PeopleOut == 0)) {
            $("#DIV_InOutTimeGraph").html("<div class=\"noDataFoundClass\">No data available for today.</div>");
            $("#DIV_AtWorkAbsentOnSiteGraph").html("<div class=\"noDataFoundClass\">No data available for today.</div>");
        } else {
            var todayCount = result.PeopleIn + result.PeopleOut;
            $("#DIV_InOutTimeGraph").kendoChart({
                legend: {
                    
                },
                seriesDefaults: {
                    labels: {
                        visible: true,
                        format: "{0:0}%"
                    }
                },
                seriesColors: ["#81C200", "#1e90ff"],
                series: [{
                    type: "pie",
                    data: [{
                            category: "Inside",
                            value: (result.PeopleIn / todayCount) * 100.00,
                            explode: true
                        }, {
                            category: "Out",
                            value: (result.PeopleOut / todayCount) * 100.00
                        }]
                }]
            });

            // At Work-Absent-OnSite
            $("#DIV_AtWorkAbsentOnSiteGraph").kendoChart({
                legend: {
                    
                },
                seriesDefaults: {
                    labels: {
                        visible: true,
                        format: "{0:0}%"
                    }
                },
                seriesColors: ["#ffb400", "#fd483b", "#d70bd5"],
                series: [{
                    type: "pie",
                    data: [{
                            category: "At Work",
                            value: (todayCount / result.totalEmployeeCount) * 100.00,
                            explode: true
                        }, {
                            category: "Absent",
                            value: (result.absentEmployeeCount / result.totalEmployeeCount) * 100.00
                        }, {
                            category: "OnSite",
                            value: (result.onSiteCount / result.totalEmployeeCount) * 100.00
                        }]
                }]
            });
        }

        $('#SPAN_InsideCount').html(result.PeopleIn);
        $('#SPAN_OutsideCount').html(result.PeopleOut);
        $('#SPAN_AtWorkCount').html(todayCount);
        $('#SPAN_AbsentCount').html(result.absentEmployeeCount);
        $('#SPAN_OnSiteCount').html(result.onSiteCount);
        $("#SPAN_My_Workcoverage").html(result.workCoverage + " %");
        $("#SPAN_Company_Workcoverage").html(result.CompanyCoverage + " %");
        $("#SPAN_Achieved_Workcoverage").html(result.AchievedCoverage + " %");
        
        new DashBoard().HighlightWorkCoverage(result.workCoverage);
        new DashBoard().HighlightCompanyWorkCoverage(result.CompanyCoverage);
        new DashBoard().HighlightAchievedWorkCoverage(result.AchievedCoverage);
     
        window.setTimeout("new DashBoard().UpdateInOutEmployeeList()", 600);
    };

  

    this.UpdateInOutEmployeeList = function () {
        azyncPost("/Home/GetEmployeesInandOutOffice", null,
            new DashBoard().InOutEmployeeListSuccess, ConnectionError);
    };

    this.InOutEmployeeListSuccess = function(result) {
        document.getElementById('DIV_EmployeesInsideOffice_Loding').style.display = 'none';
        document.getElementById('DIV_EmployeesInsideOffice_TableData').style.display = 'block';

        $("#InsideOfficeEmployeeTable").html();
        $("#InsideOfficeEmployeeTable").dataTable().fnDestroy();

        oTable = $('#InsideOfficeEmployeeTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "200px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result.InsideEmployeeList,
            "aoColumns": [
                { "mData": "DisplayText" }
            ],
            fnDrawCallback: function() {
                $("#selector thead").remove()
            }
        });

        document.getElementById('DIV_EmployeesOutsideOffice_Loding').style.display = 'none';
        document.getElementById('DIV_EmployeesOutsideOffice_TableData').style.display = 'block';

        $("#OutsideOfficeEmployeeTable").html();
        $("#OutsideOfficeEmployeeTable").dataTable().fnDestroy();

        oTable = $('#OutsideOfficeEmployeeTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "200px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result.OutOfOfficeEmployeeList,
            "aoColumns": [
                { "mData": "DisplayText" }
            ],
            fnDrawCallback: function() {
                $("#selector thead").remove()
            }
        });

        document.getElementById('DIV_EmployeesOnSite_Loding').style.display = 'none';
        document.getElementById('DIV_EmployeesOnSite_TableData').style.display = 'block';

        $("#OnSiteEmployeeTable").html();
        $("#OnSiteEmployeeTable").dataTable().fnDestroy();

        oTable = $('#OnSiteEmployeeTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "200px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result.OnSiteEmployeeList,
            "aoColumns": [
                { "mData": "DisplayText" }
            ],
            fnDrawCallback: function() {
                $("#selector thead").remove()
            }
        });

        document.getElementById('DIV_EmployeesAbsent_Loding').style.display = 'none';
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'block';

        $("#AbsentEmployeeTable").html();
        $("#AbsentEmployeeTable").dataTable().fnDestroy();

        oTable = $('#AbsentEmployeeTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "200px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result.AbsentEmployeeList,
            "aoColumns": [
                { "mData": "DisplayText" }
            ],
            fnDrawCallback: function() {
                $("#selector thead").remove()
            }
        });

        document.getElementById('DIV_EmployeesAtWork_Loding').style.display = 'none';
        document.getElementById('DIV_EmployeesAtWork_TableData').style.display = 'block';

        $("#AtWorkEmployeeTable").html();
        $("#AtWorkEmployeeTable").dataTable().fnDestroy();

        oTable = $('#AtWorkEmployeeTable').dataTable({
            "bJQueryUI": true,
            "sPaginationType": "full_numbers",
            "bScrollInfinite": true,
            "sScrollY": "200px",
            "bFilter": false,
            "bCollapse": true,
            "aaData": result.AtWorkEmployeeList,
            "aoColumns": [
                { "mData": "DisplayText" }
            ],
            fnDrawCallback: function() {
                $("#selector thead").remove()
            }
        });
    };

    this.UpdateSelectedTime = function() {
        var dateStr = $('#pickeddate').val();

        var dashboardData = {
            selectedDate: $('#pickeddate').val()
        };
        this.ClearUILoading();
        $.colorbox.close();
        azyncPost("/Home/UpdateSelectedTime", dashboardData,
            new DashBoard().UpdateSelectedTimeSuccess, ConnectionError);
    };

    this.UpdateSelectedTimeSuccess = function(result) {
        if (result.Status == "Successful") {
            $('#TD_PictureAtMessage').html('Picture as at:' + $('#pickeddate').val());
            new DashBoard().UpdateEmployeeAvailability();
        }
    };

    this.ClearUILoading = function() {
        $('#DIV_EmployeeAvailabilityGraph').html($('#DIV_TempLoadingMessage').html());
        $('#DIV_InOutTimeGraph').html($('#DIV_TempLoadingMessage').html());
        $('#DIV_AtWorkAbsentOnSiteGraph').html($('#DIV_TempLoadingMessage').html());
        document.getElementById('DIV_EmployeesInsideOffice_Loding').style.display = 'block';
        document.getElementById('DIV_EmployeesInsideOffice_TableData').style.display = 'none';
        document.getElementById('DIV_EmployeesOutsideOffice_Loding').style.display = 'block';
        document.getElementById('DIV_EmployeesOutsideOffice_TableData').style.display = 'none';
        document.getElementById('DIV_EmployeesOnSite_Loding').style.display = 'block';
        document.getElementById('DIV_EmployeesOnSite_TableData').style.display = 'none';
        document.getElementById('DIV_EmployeesAbsent_Loding').style.display = 'block';
        document.getElementById('DIV_EmployeesAbsent_TableData').style.display = 'none';
        document.getElementById('DIV_EmployeesAtWork_Loding').style.display = 'block';
        document.getElementById('DIV_EmployeesAtWork_TableData').style.display = 'none';
    };
}