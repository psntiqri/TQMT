function PastAttendance() {

	this.UpdatePastAttendance = function () {
		if (IsStartNEndDatesAreNotValid("Textbox_FromDate", "Textbox_ToDate"))
			return;
		
        var fromToDates = this.GetFromToDateString();
        var url = "/Home/GetPastAttendance" + fromToDates;
        azyncPost(url, null,
            new PastAttendance().EmployeeAvailabilitySuccess, ConnectionError);
    };

    this.EmployeeAvailabilitySuccess = function (result) {
        hide("Div_SystemLodingCntent");
        show("systemContentPage");
        new PastAttendance().EmployeeAvailabilityPieChart(result);
        new PastAttendance().EmployeeAvailabilityBarChart(result);
    };

    // Pie Chart
    this.EmployeeAvailabilityPieChart = function (result) {
      
            // At Work-Absent-OnSite
            var allCount = result.InOfficeCount + result.OnsiteCount + result.AbsentCount;
            var peopleIn = (result.InOfficeCount / allCount * 100);
            var absent = (result.AbsentCount / allCount * 100);
            var onsite = (result.OnsiteCount / allCount * 100);
        
          

            $("#DIV_AtWorkAbsentOnSiteGraph").kendoChart({
                legend: {
                    visible:false,
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
                        value: peopleIn,
                        explode: true
                    }, {
                        category: "Absent",
                        value: absent,
                    }, {
                        category: "OnSite",
                        value: onsite
                    }]
                }]
            });
         
        
        $('#SPAN_AtWorkCount').html(peopleIn.toFixed(2) + '%');
        $('#SPAN_AbsentCount').html(absent.toFixed(2) + '%');
        $('#SPAN_OnSiteCount').html(onsite.toFixed(2) + '%');

    };

    // Bar Chart
    this.EmployeeAvailabilityBarChart = function (result) {
        var type = $('input[name=chartType]:checked', '#DIV_Past_Attendance').val();
        
        $("#DIV_Bar_Chart_Past_Attendance").kendoChart({

            legend: {
                
                position: "top"
            },
            seriesDefaults: {
                type: type
            },
            series: [{
                name: "Total Employees",
                data: result.ActEmp,
                color: "#0090ff"
            }, {
                name: "In Office",
                data: result.InOffice,
                color: "#FFB400"
            }, {
                name: "On Site",
                data: result.OnSite,
                color: "#D70BD5"
            }, {
                name: "Absent",
                data: result.Absent,
                color: "#FD483B "
            }],
            valueAxis: {
                labels: {
                    format: "{0}"
                },
                line: {
                    visible: false
                },
                title: {
                    text: "Count"
                },
               
            },
            categoryAxis: {
                categories: result.Dates,
                line: {
                    visible: false
                },
                labels: {
                    padding: { top: 5 }
                },
                title: {
                    text: "Date"
                }
            },
            tooltip: {
                visible: true,
                format: "{0}",
                template: "#= series.name #: #= value #"
            }
        });
    };

    this.GetFromToDateString = function () {
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
    };

    this.ChangeChartType = function (chartType) {
        var chart = $("#DIV_Bar_Chart_Past_Attendance").data("kendoChart");
        for (var i = 0; i < chart.options.series.length; i++) {
            chart.options.series[i].type = chartType;
        }

        chart.refresh();
    };

}