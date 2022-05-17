
function DailyAttendance() {

    this.SearchEmployee = function (txt) {
        initFunctionDelayedCall(this.SearchEmployeeProcessCall);
    }

    this.SearchSharedEmployeeProcessCall = function () {
        var _shareEmployeeSearchText = $('#TextBox_User_GlobalSearchForShare').val();

        var _employeeSearchData1 = {
            searchText: _shareEmployeeSearchText
        };
        if (_shareEmployeeSearchText == "") {
            show("DIV_employeeFindAutoFilterListShare");
            hide("DIV_FilterdEmployeeListShare");
            return;
        }

        if ($("#teamSelect").val() != "0") {
            show("DIV_employeeFindAutoFilterListShare");
            hide("DIV_FilterdEmployeeListShare");

            // if (__sharedEmployeeList.length == 0) {
            hide("DIV_TeamAlreadySharedWithLabel");
            // }
            azyncPost("/DailyAttendance/SearchSharedEmployees", _employeeSearchData1,
                new DailyAttendance().EmployeeShareSearchSuccessful, ConnectionError);

        }

    },
        this.EmployeeSearchSuccessful = function (result) {
            document.getElementById("DIV_FilterdEmployeeList").innerHTML = result.SearchResult;
            document.getElementById("TextBox_User_GlobalSearch").className = "globalSearchTextBox";
            document.getElementById("TextBox_User_GlobalSearch").focus();

            show("DIV_employeeFindAutoFilterList");
            show("DIV_FilterdEmployeeList");

            $(".employeelistdiv").die("click");
            $(".employeelistdiv").live("click", function () {
                new DailyAttendance().AddEmployeeToReport(this.id, $(this).html());
            });
        },

        this.AddEmployeeToReport = function (tempID, employeeName) {
            var _employeeID = tempID.replace("DIV_", "").replace("_FilterEmployeeItem", "");
            for (var i = 0; i < _selectedEmployeeList.length; i++) {
                if (_selectedEmployeeList[i].Id == _employeeID) {
                    alert('Employee already added to the report.');
                    return;
                }
            }

            var _selectedEmployeeObj = {
                Id: _employeeID,
                Name: employeeName
            };
            _selectedEmployeeList.push(_selectedEmployeeObj)

            $("#DIV_SelectedEmployeeList").append('<div id="DIV_' + _employeeID + '_SelectedEmployeeItem" class="selectedemployeelistdiv">' + employeeName + '</div>');

            //user groups process: show edit button, reset drop down
            show("DIV_selectedEmplyeeLabel");
            new DailyAttendance().ShowCreateGroupButton();
            // $("#teamSelect").val(0);


            $(".selectedemployeelistdiv").die("click");
            $(".selectedemployeelistdiv").live("click", function () {
                new DailyAttendance().RemoveEmployeeToReport(this.id);
            });

            show("DIV_SelectedEmployeeList");
            hide("DIV_PleaseAddToListMessage");

            hide("DIV_PleaseAddToViewReport");
            hide("DIV_PleaseAddToViewSummeryReport");
            show("DIV_ReportViewerSummery");

            if ($('#Radio_ReportType_Analysis').is(':checked')) {
                show("DIV_ReportViewer", function () {
                    initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);
                });
            } else {
                show("DIV_SummeryReportViewer", function () {
                    initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);
                });
            }
        },
        this.RemoveEmployeeToReport = function (tempID) {
            debugger
            var _employeeID = tempID.replace("DIV_", "").replace("_SelectedEmployeeItem", "");
            $('#DIV_' + _employeeID + '_SelectedEmployeeItem').remove();
            //reset user group drop down
            // $("#teamSelect").val(0);

            for (var i = 0; i < _selectedEmployeeList.length; i++) {
                if (_selectedEmployeeList[i].Id == _employeeID) {
                    _selectedEmployeeList.splice(i, 1);
                }
            }

            if (_selectedEmployeeList.length == 0) {
                hide("DIV_SelectedEmployeeList");
                show("DIV_PleaseAddToListMessage");
                show("DIV_PleaseAddToViewReport");
                show("DIV_PleaseAddToViewSummeryReport");
                hide("DIV_ReportViewer");
                hide("DIV_ReportViewerSummery");
                hide("DIV_SummeryReportViewer");
                hide("DIV_AttendanceHighlight");
                hide("DIV_TaskSummeryReportViewer");
                $("#DIV_ReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());
                $("#DIV_ReportViewerSummery").html("");
                new DailyAttendance().HideCreateGroupButton();
            } else {
                initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);
                new DailyAttendance().ShowCreateGroupButton();
            }
        },
        this.ShowCreateGroupButton = function () {
            hide("DIV_SaveTeamButton");
            hide("DIV_SaveTeamNameText");
            show("DIV_CreateTeamButton");
            hide("DIV_shareEmployee");
            hide("DIV_SharedEmployeeList");
        },
        this.HideCreateGroupButton = function () {
            hide("DIV_SaveTeamButton");
            hide("DIV_SaveTeamNameText");
            hide("DIV_CreateTeamButton");
        },
        this.AddTeamToDb = function (teamName) {
            var teamName = teamName;

            var _teamObject = {
                TeamMembers: _selectedEmployeeList,
                TeamName: teamName
            };
            azyncPost("/DailyAttendance/AddTeamToDb", _teamObject,
                new DailyAttendance().TeamCreationSuccess, ConnectionError);
        },


        this.TeamCreationSuccess = function (result) {

            new DailyAttendance().HideCreateGroupButton();
            alert(result.Msg);
            __sharedEmployeeList.length = [];
            //$("#DIV_CreateTeamResultMsg").html("<p>" + result.Msg + "<p>");
            //show("DIV_CreateTeamResultMsg");
            hide("ErrorMsgTeamNameEmpty");
            $("#DIV_SaveTeamNameTextBox").val("");
            new DailyAttendance().RefreshTeamDropDown();
            show("sharedGlobalSearchTr");
            show("sharedMessageTr");
        },
        this.TeamUpdate = function (teamId, MemberString) {
            var _teamObject = {
                TeamId: teamId,
                MemberString: MemberString
            };
            azyncPost("/DailyAttendance/TeamUpdate", _teamObject,
                new DailyAttendance().TeamUpdateSuccess, ConnectionError);
        },
        this.TeamUpdateSuccess = function (result) {
            alert(result.Msg);
        },
        this.TeamDelete = function (teamId) {
            var _teamObject = {
                TeamId: teamId
            };
            azyncPost("/DailyAttendance/TeamDelete", _teamObject,
                new DailyAttendance().TeamDeleteSuccess, ConnectionError);
        },
        this.TeamDeleteSuccess = function (result) {


            new DailyAttendance().RefreshTeamDropDown();
            __sharedEmployeeList = [];
            _selectedEmployeeList = [];

            hide("DIV_ReportViewer");
            hide("DIV_ReportViewerSummery");
            hide("DIV_selectedEmplyeeLabel");
            hide("DIV_employeeFindAutoFilterList");

            hide("DIV_shareEmployee");
            show("DIV_PleaseAddToViewReport");

            $("#DIV_SelectedEmployeeList").html("");
            $("#DIV_SharedEmployeeList").html("");

            //reset user group drop down
            $("#teamSelect").val(0);
            alert(result.Msg);

        },

        this.RefreshTeamDropDown = function () {
            setTimeout(function () {
                azyncPost("/DailyAttendance/GetTeamDropdownHtml", null,
                    new DailyAttendance().RefreshTeamDropDownSuccess, ConnectionError);
            }, 500);

        },
        this.RefreshTeamDropDownSuccess = function (result) {
            $("#teamSelect").html(result.Html);
            var groupName = $("#teamSelectValue").val();
            if (groupName != "") {
                var ct = $("option:contains('" + groupName + "')").val();
                $("#teamSelect").val(ct);
                $('#TextBox_User_GlobalSearch').val("");
                new DailyAttendance().SearchEmployee("");
            }
            if ($("#teamSelect").val() == "0" || typeof $("#teamSelect").val() === "undefined") {
                hide("DIV_selectedEmplyeeLabel");
                hide("DIV_employeeFindAutoFilterList");
                hide("DIV_shareEmployee");
                hide("DIV_TeamAlreadySharedWithLabel");
            }
            else {
                show("DIV_selectedEmplyeeLabel");
                show("DIV_employeeFindAutoFilterList");
                show("DIV_shareEmployee");
                if (__sharedEmployeeList.length == 0) {
                    hide("DIV_TeamAlreadySharedWithLabel");
                    hide("shareTeambutton");
                }
            }

        },
        this.SetDropDownBoxValue = function () {
            setTimeout(function () {
                var id = $('#teamSelect').find('option[text="p"]').id;
                alert(id);
            }, 2000);
        },
        this.ChangeSelectedTeam = function () {
            var teamId = { TeamId: $("#teamSelect").val() };
            if ($("#teamSelect").val() == "0") {
                __sharedEmployeeList = [];
                _selectedEmployeeList = [];
                hide("DIV_ReportViewer");
                hide("DIV_ReportViewerSummery");
                hide("DIV_selectedEmplyeeLabel");
                hide("DIV_employeeFindAutoFilterList");

                hide("DIV_shareEmployee");
                show("DIV_PleaseAddToViewReport");

                $("#DIV_SelectedEmployeeList").html("");
                $("#DIV_SharedEmployeeList").html("");
                return;
            }
            else {
                show("DIV_selectedEmplyeeLabel");
                show("DIV_employeeFindAutoFilterList");
                show("DIV_shareEmployee");
            }

            setTimeout(function () {
                azyncPost("/DailyAttendance/GetTeamMembersDetails", teamId,
                    new DailyAttendance().ChangeSelectedTeamSuccess, ConnectionError);
            }, 500);

            //azyncPost("/DailyAttendance/GetSharedMemberDetails", teamId,
            //    new DailyAttendance().ChangeSharedMemberSuccess, ConnectionError);


        },

        this.GetTeamMembersDetailsForDashboard = function (teamId) {
            alert(teamId);
            setTimeout(function () {
                azyncPost("/DailyAttendance/GetTeamMembersDetails", teamId,
                    new DailyAttendance().ChangeSelectedTeamSuccess, ConnectionError);
            }, 2000);
        },

        this.ChangeSelectedTeamSuccess = function (result) {
            _selectedEmployeeList = [];
            var teamMembers = result.TeamMembers.TeamMembers;
            var sharedMembers = result.TeamMembers.SharedEmployeeList;
            //--------------------
            __sharedEmployeeList = [];
            show("sharedGlobalSearchTr");
            show("sharedMessageTr");


            $("#DIV_SharedEmployeeList").html("");
            if (sharedMembers != null) {
                for (var i = 0; i < sharedMembers.length; i++) {
                    var member = sharedMembers[i];
                    var _selectedSharedEmployeeObj = {
                        Id: member.Id,
                        Name: member.Name
                    };
                    __sharedEmployeeList.push(_selectedSharedEmployeeObj)
                    $("#DIV_SharedEmployeeList").append('<div id="DIV_' + member.Id + '_SharedEmployeeItem" class="selectedsharedemployeelistdiv">' + member.Name + '</div>');



                }
            }
            //if (sharedMembers.length > 0)
            //    hide("DIV_PleaseAddToViewReport");
            //else
            //    show("DIV_PleaseAddToViewReport");

            $(".selectedsharedemployeelistdiv").die("click");
            $(".selectedsharedemployeelistdiv").live("click", function () {
                new DailyAttendance().RemoveSharedEmployeeToReport(this.id);
            });

            if (__sharedEmployeeList.length > 0) {
                show("DIV_TeamAlreadySharedWithLabel");
                show("DIV_SharedEmployeeList");
                hide("DIV_SharedPleaseAddToListMessage");
            } else {
                hide("DIV_TeamAlreadySharedWithLabel");
                hide("DIV_SharedEmployeeList");
                hide("DIV_SharedTeamButton");
                show("DIV_SharedPleaseAddToListMessage");

            }
            //--------------
            $("#DIV_SelectedEmployeeList").html("");
            for (var i = 0; i < teamMembers.length; i++) {
                var member = teamMembers[i];
                var _selectedEmployeeObj = {
                    Id: member.Id,
                    Name: member.Name
                };
                _selectedEmployeeList.push(_selectedEmployeeObj)
                $("#DIV_SelectedEmployeeList").append('<div id="DIV_' + member.Id + '_SelectedEmployeeItem" class="selectedemployeelistdiv">' + member.Name + '</div>');
            }

            if (teamMembers.length > 0)
                hide("DIV_PleaseAddToViewReport");
            else
                show("DIV_PleaseAddToViewReport");

            $(".selectedemployeelistdiv").die("click");
            $(".selectedemployeelistdiv").live("click", function () {
                new DailyAttendance().RemoveEmployeeToReport(this.id);
            });

            if ($('#Radio_ReportType_Analysis').is(':checked')) {
                show("DIV_ReportViewer", function () {
                    initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);
                });
            } else {
                show("DIV_SummeryReportViewer", function () {
                    initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);
                });
            }

            new DailyAttendance().HideCreateGroupButton();
            if (_selectedEmployeeList.length > 0) {
                show("DIV_SelectedEmployeeList");
                hide("DIV_PleaseAddToListMessage");
            } else {
                hide("DIV_SelectedEmployeeList");
                show("DIV_PleaseAddToListMessage");
            }
        },


        this.UpdateGraphProcess = function () {
            if (_selectedEmployeeList.length == 0)
                return;

            var _analysisModelData = {
                FromDate: $("#Textbox_FromDate").val(),
                ToDate: $("#Textbox_ToDate").val(),
                SelectedEmployeeList: _selectedEmployeeList
            };

            if ($('#Radio_ReportType_Analysis').is(':checked')) {
                $("#DIV_ReportViewerSummery").html("");
                show("DIV_ReportViewerSummery");
                $("#DIV_ReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());
                azyncPost("/DailyAttendance/GetEmployeesHorsGraphData", _analysisModelData,
                    new DailyAttendance().UpdateGraphSucuss, ConnectionError);
            }

        if ($('#Radio_ReportType_Summery').is(':checked')) {
            hide("DIV_TaskSummeryReportViewer");
            hide("DIV_AttendanceHighlight");
                $("#DIV_SummeryReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());
                azyncPost("/DailyAttendance/GetEmployeesHorsSummeryData", _analysisModelData,
                    new DailyAttendance().UpdateSummerySucuss, ConnectionError);
            }
        },
        this.UpdateTaskSummary = function () {
            if ($('#Radio_ReportType_Summery').is(':checked')) {
                $("#DIV_TaskSummeryReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());
                var _analysisModelData = {
                    FromDate: $("#Textbox_FromDate").val(),
                    ToDate: $("#Textbox_ToDate").val(),
                    SelectedEmployeeList: _selectedEmployeeList
                };
                azyncPost("/DailyAttendance/GetEmployeesTaskHoursSummeryData", _analysisModelData,
                    new DailyAttendance().UpdateTaskSummerySucuss, ConnectionError);
            }
        },
        this.UpdateLateEmployeesTableProcess = function () {
            var empty = "";
            $("#TblLateEmployee").html(empty);
            $("#DIV_GrapfUpdateWaitMessage").show()
            initFunctionDelayedCall(new DailyAttendance().UpdateLateEmployeesTableProcessComplete);
        },
        this.UpdateLateEmployeesTableProcessComplete = function () {
            var _lateEmployeeModel = {
                Date: $("#Textbox_Date").val(),
            };
            azyncPost("/DailyAttendance/GetSelectedEmployeesHorsGraphData2", _lateEmployeeModel,
                new DailyAttendance().UpdateLateEmployeesTableSuccess, ConnectionError);
        },
        this.UpdateLateEmployeesTableSuccess = function (result) {
            $("#DIV_GrapfUpdateWaitMessage").hide()
            $("#TblLateEmployee").html(result.LateEmployeesTable);
        },
        this.UpdateGraphSucuss = function (result) {
            var newData = result.AttendanceStructure.ResultGraphData;
            g = new Dygraph(document.getElementById('DIV_ReportViewer'), newData,
                {
                    strokeWidth: 2,
                    animatedZooms: true,
                    xValueFormatter: function (x, opts, series_name, dg) {
                        return new Date(x).strftime('%d/%m/%Y');
                    }
                });

            var _summeryDiv = '<table class="clearTable"><tr><td colspan="2" style="padding-bottom: 15px;"><strong>Employee Attendance Covered Detail</strong></td></tr>';
            var _temCount = 0;
            for (var i = 0; i < result.AttendanceStructure.EmployeeCoverageList.length; i++) {
                if (_temCount == 0)
                    _summeryDiv += '<tr>'

                if (result.AttendanceStructure.EmployeeCoverageList[i].Precentage >= 90)
                    _summeryDiv += '<td style="width:50%; color:green;">';
                else
                    _summeryDiv += '<td style="width:50%; color:red;">';
                _summeryDiv += result.AttendanceStructure.EmployeeCoverageList[i].EmployeeName + ' ';
                _summeryDiv += new DailyAttendance().DecimalToHours(result.AttendanceStructure.EmployeeCoverageList[i].ActualHours) + ' Hours (';
                _summeryDiv += result.AttendanceStructure.EmployeeCoverageList[i].Precentage + '% Covered)';

                _temCount++;
                if (_temCount == 2) {
                    _summeryDiv += '</tr>'
                    _temCount = 0;
                }
            }

            var totalTeamCoverageColor = 'green';
            if (result.AttendanceStructure.TotalTeamWorkCoverage < 90)
                totalTeamCoverageColor = 'red';

            _summeryDiv += '<tr><td colspan="2" style=" padding-top: 15px; color: ' + totalTeamCoverageColor + ';font-size: 16px !important;"><strong>Total Number of leave: ';
            _summeryDiv += result.AttendanceStructure.TotalLeaveCount + '</strong></td>';

            _summeryDiv += '<tr><td colspan="2" style=" color: ' + totalTeamCoverageColor + ';font-size: 16px !important;"><strong>Total Number of work days: ';
            _summeryDiv += result.AttendanceStructure.WorkingDays + '</strong></td>';

            _summeryDiv += '<tr><td colspan="2" style=" color: ' + totalTeamCoverageColor + ';font-size: 16px !important;"><strong>Total Team Coverage: ';
            _summeryDiv += result.AttendanceStructure.TotalTeamWorkCoverage + '%</strong></td>';

            _summeryDiv += '</table>';

            $("#DIV_ReportViewerSummery").html(_summeryDiv);
        },
        this.UpdateSummerySucuss = function (result) {
            $("#DIV_SummeryReportViewer").html(result.AttendanceReport);
            new DailyAttendance().UpdateTaskSummary();
        },
        this.UpdateTaskSummerySucuss = function (result) {
        const reportArr = result.AttendanceReport.split("<seperator>", 2)
        
        show("DIV_AttendanceHighlight");
        $("#DIV_AttendanceHighlight").html(reportArr[0]);

        show("DIV_TaskSummeryReportViewer");
        $("#DIV_TaskSummeryReportViewer").html(reportArr[1]);
        },
        this.ReportRangeChange = function () {

            if (!new DailyAttendance().ValidateReportDates())
                return;

            if (_selectedEmployeeList.length > 0) {
                new DailyAttendance().UpdateGraphProcess();
            }
        },
        this.ChangeReportType = function () {
            if ($('#Radio_ReportType_Analysis').is(':checked')) {
                show("DIV_ReportGraphWrapper");
                hide("DIV_ReportSummeryWrapper");
            } else {
                hide("DIV_ReportGraphWrapper");
                show("DIV_ReportSummeryWrapper");
                $('#DIV_SummeryReportViewer').css("width", $("#DIV_ReportSummeryWrapper").width() + "px");
                $('#DIV_SummeryReportViewer').css("overflow", "auto");
                $('#DIV_TaskSummeryReportViewer').css("width", $("#DIV_ReportSummeryWrapper").width() + "px");
                $('#DIV_TaskSummeryReportViewer').css("overflow", "auto");
            }

            if (_selectedEmployeeList.length == 0) {
                hide("DIV_SelectedEmployeeList");
                show("DIV_PleaseAddToListMessage");
                show("DIV_PleaseAddToViewReport");
                show("DIV_PleaseAddToViewSummeryReport");
                hide("DIV_ReportViewer");
                hide("DIV_ReportViewerSummery");
                hide("DIV_SummeryReportViewer");
                hide("DIV_AttendanceHighlight");
                hide("DIV_TaskSummeryReportViewer");
                return;
            }

            if ($('#Radio_ReportType_Analysis').is(':checked')) {
                hide("DIV_PleaseAddToViewReport");
                hide("DIV_ReportViewerSummery");
                show("DIV_ReportViewer", function () {
                    initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);

                });
            } else {
                hide("DIV_PleaseAddToViewSummeryReport");
                show("DIV_SummeryReportViewer", function () {
                    initFunctionDelayedCall(new DailyAttendance().UpdateGraphProcess);

                });
            }
        },
        this.ValidateReportDates = function () {

            var startDate = GetDateFromText('Textbox_FromDate');
            var endDate = GetDateFromText('Textbox_ToDate');

            if (startDate >= endDate) {
                alert("Invalid date range. Please try again.");
                return false;
            }

            if (MonthDiff(startDate, endDate) >= 12) {
                alert("Invalid date range. Please select a range below 6 months.");
                return false;
            }

            return true;
        },
        // --------------- In Out Time Analysis Report ---------------//

        this.ChangeReportFormat = function () {
            if ($("#Dropdown_GraphType").val() == "Line") {
                show("DIV_LineGraphOptionList");
                hide("DIV_CandleGraphOptionList");
            } else {
                hide("DIV_LineGraphOptionList");
                show("DIV_CandleGraphOptionList");
            }

            new DailyAttendance().UpdateInOutTimeAnalysisReport();
        },
        this.UpdateInOutTimeAnalysisReport = function () {
            if ($("#SelectedEmployeeID").val() == "") {
                show("DIV_PleaseAddToViewReport");
                show("DIV_PlannedActualPleaseAddToViewReport");
                hide("DIV_ReportViewer");
                hide("DIV_PlannedActualReportViewer");
                hide("DIV_ReportSummery");
                return;
            }

            if ($("#Dropdown_GraphType").val() == "Line" &&
                $('#CheckBox_ViewInTime').prop('checked') == false && $('#CheckBox_ViewOutTime').prop('checked') == false) {
                show("DIV_PleaseAddToViewReport");
                hide("DIV_ReportViewer");
                return;
            }

            var _analysisModelData = {
                FromDate: $("#Textbox_FromDate").val(),
                ToDate: $("#Textbox_ToDate").val(),
                ViewInTime: $('#CheckBox_ViewInTime').prop('checked'),
                ViewOutTime: $('#CheckBox_ViewOutTime').prop('checked'),
                SelectedEmployeeID: $("#SelectedEmployeeID").val(),
                ReportType: $("#SelectedEmployeeID").val()
            };

            if (!new DailyAttendance().ValidateReportDates())
                return;

            $("#DIV_ReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());
            $("#DIV_PlannedActualReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());

            if ($("#Dropdown_GraphType").val() == "Line") {
                azyncPost("/Analysis/GetEmployeeTimeTrendGraphData", _analysisModelData,
                    new DailyAttendance().UpdateTimeTrendGraphSucuss, ConnectionError);
            }

            if ($("#Dropdown_GraphType").val() == "Candle") {
                azyncPost("/Analysis/GetEmployeeTimeTrendGraphData", _analysisModelData,
                    new DailyAttendance().UpdateTimeTrendCandleSucuss, ConnectionError);
            }
        },
        this.UpdateTimeTrendGraphSucuss = function (result) {
            hide("DIV_PleaseAddToViewReport");
            show("DIV_ReportViewer", function () {

                var reportViewr = $("#DIV_ReportViewer");
                var position = reportViewr.position();
                var newData = result.AttendanceStructure.ResultGraphData;
                var outOfOfficeData = result.AttendanceStructure.EmployeeOutOfOfficeList;
                g = new Dygraph(document.getElementById('DIV_ReportViewer'),
                    newData,
                    {
                        strokeWidth: 2,
                        valueRange: [24, 0],
                        animatedZooms: true,
                        colors: ['#284785', '#EE1111'],
                        xValueFormatter: function (x, opts, series_name, dg) {
                            return new Date(x).strftime('%d/%m/%Y');
                        },
                        highlightCallback: function (e, x, pts, row) {

                            if ($("#Dropdown_GraphType").val() == "Line" &&
                                $('#CheckBox_ViewInTime').prop('checked') == true && $('#CheckBox_ViewOutTime').prop('checked') == true) {


                                for (var i = 0; i < pts.length; i++) {
                                    if (i == 0) {
                                        xline.style.left = (position.left + pts[i].canvasx - 1) + "px";
                                        xline.style.top = (position.top + pts[0].canvasy) + "px";
                                        xline.style.height = pts[1].canvasy - pts[0].canvasy + "px";

                                        var fromTimeStrArray = pts[0].yval.toString().split('.');
                                        var toTimeStrArray = pts[1].yval.toString().split('.');

                                        var _reportDate = g.getValue(pts[0].xval, pts[1].yval);

                                        if (fromTimeStrArray.length == 1)
                                            fromTimeStrArray[1] = "00";
                                        if (toTimeStrArray.length == 1)
                                            toTimeStrArray[1] = "00";

                                        if (fromTimeStrArray[1].length == 1)
                                            fromTimeStrArray[1] = fromTimeStrArray[1] + "0";
                                        if (toTimeStrArray[1].length == 1)
                                            toTimeStrArray[1] = toTimeStrArray[1] + "0";

                                        var fromTime = new Date(2000, 0, 1, fromTimeStrArray[0], fromTimeStrArray[1]);
                                        var toTime = new Date(2000, 0, 1, toTimeStrArray[0], toTimeStrArray[1]);
                                        var minutesDifference = Math.floor((toTime - fromTime) / 1000 / 60);

                                        var _workhrs = Math.floor(minutesDifference / 60);
                                        var _displayWorkHours = _workhrs + ":" + (minutesDifference % 60);

                                        xdetail.innerHTML = _displayWorkHours;// + " hrs";

                                        if (_workhrs >= 9) {
                                            xdetail.style.backgroundColor = "#CAFFCA";
                                            xdetail.style.border = "1px solid #009900";
                                            xline.style.backgroundColor = "green";
                                        } else {
                                            xdetail.style.backgroundColor = "#FFD5D5";
                                            xdetail.style.border = "1px solid #CC0000";
                                            xline.style.backgroundColor = "red";
                                        }

                                        xdetail.style.left = (position.left + pts[i].canvasx + 10) + "px";
                                        xdetail.style.top = (position.top + pts[0].canvasy + ((pts[1].canvasy - pts[0].canvasy)) / 2) + "px";
                                    }
                                }
                                xline.style.visibility = "visible";
                                xdetail.style.visibility = "visible";
                            }
                        },
                        unhighlightCallback: function (e) {
                            xline.style.visibility = "hidden";
                            xdetail.style.visibility = "hidden";
                        },
                        underlayCallback: function (canvas, area, g) {

                            canvas.fillStyle = "rgba(255, 0, 0, 0.8)";

                            for (var i = 0; i < outOfOfficeData.length; i++) {
                                var x_topcor = g.toDomXCoord(new Date(outOfOfficeData[i]["OutDate"]));
                                var y_topCor = g.toDomYCoord(outOfOfficeData[i]["FromTime"]);
                                var y_bottomCor = g.toDomYCoord(outOfOfficeData[i]["ToTime"]);
                                canvas.fillRect(x_topcor, y_topCor, 5, (y_bottomCor - y_topCor));
                            }
                        }
                    });

                xline = document.createElement("div");
                xline.className = "line xline";
                xline.style.top = "0px";
                document.getElementById("DIV_ReportViewer").appendChild(xline);

                xdetail = document.createElement("div");
                xdetail.className = "xdatadetail";
                xdetail.style.top = "0px";
                xdetail.style.color = "#000";
                xdetail.style.paddingLeft = "4px";
                xdetail.style.paddingRight = "4px";
                xdetail.style.paddingTop = "2px";
                xdetail.style.paddingBottom = "2px";
                xdetail.style.fontSize = "10px";
                document.getElementById("DIV_ReportViewer").appendChild(xdetail);

                new DailyAttendance().UpdateTimeTrendReportSummery(result);

                // To reiniate for the first time only
                if (_finalReportDataSource == null) {
                    _finalReportDataSource = result;
                    initFunctionDelayedCall(new DailyAttendance().RegenerateGraph);
                } else {
                    new DailyAttendance().UpdateDashBordPlannedVsActualAnalysisReport();
                }
            });
        },
        this.RegenerateGraph = function () {
            new DailyAttendance().UpdateTimeTrendGraphSucuss(_finalReportDataSource);
        },
        this.UpdateTimeTrendCandleSucuss = function (result) {
            hide("DIV_PleaseAddToViewReport");
            show("DIV_ReportViewer", function () {
                var newData = result.AttendanceStructure.ResultGraphData;
                g2 = new Dygraph(
                    document.getElementById("DIV_ReportViewer"),
                    newData,
                    {
                        plotter: candlePlotter,
                        animatedZooms: true,
                        valueRange: [24, 0]
                    });
                new DailyAttendance().UpdateTimeTrendReportSummery(result);
                new DailyAttendance().UpdateDashBordPlannedVsActualAnalysisReport();
            });
        },
        this.UpdateTimeTrendReportSummery = function (result) {
            show("DIV_ReportSummery");
            $("#SPAN_ReportSummery_Duration").html(result.AttendanceStructure.Duration);
            $("#SPAN_ReportSummery_WorkingDays").html(result.AttendanceStructure.WorkingDays + " Days");
            $("#SPAN_ReportSummery_LoggedDays").html(result.AttendanceStructure.LoggedDays + " Days");
            $("#SPAN_ReportSummery_PlannedLeave").html(result.AttendanceStructure.TotalPlannedLeave + " Days");
            $("#SPAN_ReportSummery_TotalPlanned").html(new DailyAttendance().DecimalToHours(result.AttendanceStructure.TotalPlanned) + " Hrs");
            $("#SPAN_ReportSummery_TotalActual").html(new DailyAttendance().DecimalToHours(result.AttendanceStructure.TotalActual) + " Hrs");
            $("#SPAN_ReportSummery_TotalOutOfOffice").html(new DailyAttendance().DecimalToHours(result.AttendanceStructure.TotalOutOfOffice) + " Hrs");
            $("#SPAN_ReportSummery_Workcoverage").html(result.AttendanceStructure.WorkCoverage + " %");
            $("#SPAN_ReportSummery_AverageIn").html(result.AttendanceStructure.AverageInTime);
            $("#SPAN_ReportSummery_AverageOut").html(result.AttendanceStructure.AverageOutTime);
            new DailyAttendance().HighlightWorkCoverage(result.AttendanceStructure.WorkCoverage, result.AttendanceStructure.TotalPlanned, result.AttendanceStructure.TotalActual);
        },

        this.HighlightWorkCoverage = function (workCoverage, planned, actual) {
            var hoursNeeded = (0.9 * planned) - actual;
            // var hoursNeeded = planned - actual;
            var coverageText = '';
            if (workCoverage >= 90) {
                // $("#TR_Work_Coverage td").css('background-color', 'green');
                $("#SPAN_ReportSummery_Workcoverage").css('color', 'green');
                $("#coverageText").css('color', 'green');
                // $("#coverageText").css('color', '#FFF');
                coverageText = "Wow! You are in green zone";
            }
            else if (workCoverage >= 80) {
                //#FF9900-orange
                // $("#TR_Work_Coverage td").css('background-color', '#FF9900');
                $("#SPAN_ReportSummery_Workcoverage").css('color', 'orange');
                $("#coverageText").css('color', '#FF9900');
                // $("#coverageText").css('color', '#333');
                coverageText = "You need more " + new DailyAttendance().DecimalToHours(hoursNeeded) + " hours to be in green zone.";
            }
            else {
                // $("#TR_Work_Coverage td").css('background-color', 'red');
                $("#SPAN_ReportSummery_Workcoverage").css('color', 'red');
                $("#coverageText").css('color', 'red');
                // $("#coverageText").css('color', '#FFF');
                coverageText = "You need more " + new DailyAttendance().DecimalToHours(hoursNeeded) + " hours to be in green zone.";
            }
            $("#coverageText").text(coverageText);
        };

    this.UpdateDashBordPlannedVsActualAnalysisReport = function () {

        var _selectedEmployeeObj = {
            Id: $("#SelectedEmployeeID").val()
        };

        var _selectedEmployee = new Array();
        _selectedEmployee.push(_selectedEmployeeObj)

        var _analysisModelData = {
            FromDate: $("#Textbox_FromDate").val(),
            ToDate: $("#Textbox_ToDate").val(),
            SelectedEmployeeList: _selectedEmployee
        };

        if (!new DailyAttendance().ValidateReportDates())
            return;

        $("#DIV_PlannedActualReportViewer").html($("#DIV_GrapfUpdateWaitMessage").html());
        azyncPost("/DailyAttendance/GetSelectedEmployeesHorsGraphData", _analysisModelData,
            new DailyAttendance().UpdateDashBordPlannedVsActualGraphSucuss, ConnectionError);

    },
        this.UpdateDashBordPlannedVsActualGraphSucuss = function (result) {
            $("#DIV_PlannedActualReportViewer").html();
            hide("DIV_PlannedActualPleaseAddToViewReport");
            document.getElementById("DIV_PlannedActualReportViewer").style.display = "block";
            var newData = result.AttendanceStructure.ResultGraphData;
            var holidayData = result.AttendanceStructure.HolidayDateList;
            g = new Dygraph(document.getElementById('DIV_PlannedActualReportViewer'), newData,
                {
                    strokeWidth: 2,
                    animatedZooms: true,
                    xValueFormatter: function (x, opts, series_name, dg) {
                        return new Date(x).strftime('%d/%m/%Y');
                    },
                    underlayCallback: function (canvas, area, g) {

                        canvas.fillStyle = "rgba(255, 255, 102, 0.3)";
                        var yrange = g.yAxisRange();

                        for (var i = 0; i < holidayData.length; i++) {
                            var x_topcor = g.toDomXCoord(new Date(holidayData[i]["FromDate"]));
                            var x_endcor = g.toDomXCoord(new Date(holidayData[i]["ToDate"]));
                            var y_topCor = g.toDomYCoord(yrange[1]);
                            var y_bottomCor = g.toDomYCoord(0);
                            canvas.fillRect(x_topcor, y_topCor, (x_endcor - x_topcor), (y_bottomCor - y_topCor));
                        }
                    }
                });
        },
        this.DecimalToHours = function (val) {

            var _coms = val.toString().split(".");
            var _hours = _coms[0];
            if (_coms.length == 1) {
                _hours += ":00";
            } else {
                if (_coms[1].length == 1)
                    //  _hours += ":0" + _coms[1];
                    _hours += ":" + (_coms[1] * 6);
                else {
                    // _hours += ":" + _coms[1].slice(0, 2);
                    var min = (_coms[1].slice(0, 2) * .6).toString().split(".")[0];
                    if (min.length == 1) {
                        _hours += ":0" + min;
                    } else {
                        _hours += ":" + min;
                    }
                }

            }
            return _hours;
        },

        //Share teams among employee
        //Region start
        this.SearchSharedEmployee = function (txt) {
            initFunctionDelayedCall(this.SearchSharedEmployeeProcessCall);
        },

        this.SearchEmployeeProcessCall = function () {

            var _searchText = $('#TextBox_User_GlobalSearch').val();

            var teamId = $("#teamSelect").val();

            if (_searchText == "") {
                show("DIV_employeeFindAutoFilterList");
                hide("DIV_FilterdEmployeeList");
                return;
            } else {
                var _employeeSearchData = {
                    searchText: _searchText
                };
                setTimeout(function () {
                    azyncPost("/DailyAttendance/SearchEmployees", _employeeSearchData,
                        new DailyAttendance().EmployeeSearchSuccessful, ConnectionError);
                }, 500);
            }


        },

        this.EmployeeShareSearchSuccessful = function (result) {
            document.getElementById("DIV_FilterdEmployeeListShare").innerHTML = result.SearchResult;
            document.getElementById("TextBox_User_GlobalSearchForShare").className = "globalSearchTextBox";
            document.getElementById("TextBox_User_GlobalSearchForShare").focus();
            hide("DIV_employeeFindAutoFilterListShare");
            show("DIV_FilterdEmployeeListShare");

            $(".employeeSharedlistdiv").die("click");
            $(".employeeSharedlistdiv").live("click", function () {
                new DailyAttendance().AddSharedEmployeeToReport(this.id, $(this).html());
            });
        },

        this.AddSharedEmployeeToReport = function (tempID, employeeName) {

            var _employeeID = tempID.replace("DIV_", "").replace("_FilterSharedEmployeeItem", "");

            if (typeof __sharedEmployeeList === "undefined" || __sharedEmployeeList.length == 0) {
                __sharedEmployeeList = [];
            }

            for (var i = 0; i < __sharedEmployeeList.length; i++) {
                if (__sharedEmployeeList[i].Id == _employeeID) {
                    alert('This team  already shared on this employee.');
                    return;
                }
            }

            var _selectedEmployeeObj = {
                Id: _employeeID,
                Name: employeeName
            };
            __sharedEmployeeList.push(_selectedEmployeeObj);

            $("#DIV_SharedEmployeeList").append('<div id="DIV_' + _employeeID + '_SharedEmployeeItem" class="selectedsharedemployeelistdiv">' + employeeName + '</div>');
            $("#shareTeambutton").val("Share Team");
            if (__sharedEmployeeList.length != 0) {
                show('DIV_TeamAlreadySharedWithLabel');
                show("shareTeambutton");
            }
            show("DIV_SharedTeamButton");

            //user groups process: show edit button, reset drop down
            // new DailyAttendance().ShowCreateGroupButton();

            $(".selectedsharedemployeelistdiv").die("click");
            $(".selectedsharedemployeelistdiv").live("click", function () {
                new DailyAttendance().RemoveSharedEmployeeToReport(this.id);
            });

            show("DIV_SharedEmployeeList");
        },

        this.ShareTeam = function (teamId, teamName) {
            var _teamObject = {
                SharedEmployeeList: __sharedEmployeeList,
                TeamId: teamId,
                TeamName: teamName
            };
            $('#TextBox_User_GlobalSearchForShare').val('');
            azyncPost("/DailyAttendance/ShareTeam", _teamObject,
                new DailyAttendance().SharedTeamSuccess, ConnectionError);
        },

        this.SharedTeamSuccess = function (result) {
            hide("DIV_FilterdEmployeeListShare");

            alert(result.Msg);
        };

    this.RemoveSharedEmployeeToReport = function (tempID) {
        var _employeeID = tempID.replace("DIV_", "").replace("_SharedEmployeeItem", "");
        $('#DIV_' + _employeeID + '_SharedEmployeeItem').remove();

        for (var i = 0; i < __sharedEmployeeList.length; i++) {
            if (__sharedEmployeeList[i].Id == _employeeID) {
                __sharedEmployeeList.splice(i, 1);
            }
        }

        if (__sharedEmployeeList.length == 0) {
            hide("DIV_SharedEmployeeList");
            show("DIV_SharedPleaseAddToListMessage");

        }
        show("DIV_SharedTeamButton");
        $("#shareTeambutton").val("Update Share Team");
    },
        this.ShowShareEmployeeButton = function () {
            show("DIV_SharedTeamButton");
        };

    //Share teams among employee
    //Region end


}