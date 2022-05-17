var employeeControllers = angular.module("receptionApp");

employeeControllers.controller('EmployeeCtrl', ['$scope','$rootScope', '$http', 'Employee', 'ReceptionActiveCards',  function ($scope, $rootScope, $http, Employee, ReceptionActiveCards) {
    var url = '/Reception/api/employee/';
    var searchUrlByName = url + '?name=';
    var searchUrlById = url + '?id=';
    
   
    //$scope.showDetails = false;
    
    $scope.init = function(showEmployeeDetails) {
        $scope.showDetails = showEmployeeDetails;
        $scope.showEmployeeTitle = showEmployeeDetails;
    };

    for (var key in Employee) {
        Employee[key] = "";
    }
    
    $scope.employee = Employee;
   
    $scope.searchEmployeeOnTextChange = function () {
        
        $scope.employee.employeeId = "";
        $scope.employee.visitorCardsNotReturned = "";
        $scope.employee.employeeCardAccessLevel = "";
        $scope.employee.newVisitorCard = "";
        $scope.employee.employeeSearchStatus = "";
        
        if ($scope.employee.employeeName.length < 2) return;
        
        $http.get(searchUrlByName + $scope.employee.employeeName).success(function (data) {
            var empNames = [];
            $.each(data, function (i, d) {
                //empNames.push(d.Name);
                //empNames.push({ label: d.Name, value: d.Name, Idd: d.Id, Name: d.Name, VisitorCardsNotReturned: d.VisitorCardsNotReturned, EmployeeCardAccessLevel: d.EmployeeCardAccessLevel });
                empNames.push({ label: d.Name, value: d.Name, Idd: d.Id, Name: d.Name, ImagePath: d.ImagePath });
            });
            //Object {Id: 112, Name: "Ravi", CurrentVisitorCardNo: ""}
            $scope.employee.empNames = empNames;
            //$scope.names = empNames;
            var isSet = false;
            if ($('#txtEmployeeName').autocomplete().data("ui-autocomplete")) {
                $('#txtEmployeeName').autocomplete({
                    source: empNames,
                    select: function(event, ui) {

                        $http.get(searchUrlById + ui.item.Idd).success(function(idSarchData) {
                            getEmployeeDetails(idSarchData);
                        });
                    }
                }).data("ui-autocomplete")._renderItem = function (ul, item) {
                    var inner_html = 
                         '<a><div class="list_item_container"><div class="image"><img src="../../Content/images/employee/' +
                         item.ImagePath + '"></div><div class="label">' + item.label +
                         '</div></div></a>';
                    return $("<li></li>")
                        .data("item.autocomplete", item)
                        .append(inner_html)
                        .appendTo(ul);
                };
            }


        });
    };
    function getEmployeeDetails(data) {
        $scope.employee.employeeId = "";
        $scope.employee.visitorCardsNotReturned = "";
        $scope.employee.employeeCardAccessLevel = "";
        $scope.employee.newVisitorCard = $rootScope.$broadcast('viewCard');
        $scope.employee.employeeSearchStatus = "";
        
        //$('#txtEmployeeName').val('');
        //$('#txtEmployeeId').val('');
        //$('#txtVisitorCardsNotReturned').val('');
        //$('#txtEmployeeCardAccessLevel').val('');

        if (data) {
            $scope.employee.employeeName = data.Name;
            $scope.employee.employeeId = data.Id;
            $scope.employee.visitorCardsNotReturned = data.VisitorCardsNotReturned;
            $scope.employee.employeeCardAccessLevel = data.EmployeeCardAccessLevel;
            $scope.employee.employeeSearchStatus = "Selected";
            
            //$('#txtEmployeeName').val(data.Name);
            //$('#txtEmployeeId').val(data.Id);
            //$('#txtVisitorCardsNotReturned').val(data.VisitorCardsNotReturned);
            //$('#txtEmployeeCardAccessLevel').val(data.EmployeeCardAccessLevel);
            
            $('#txtEmployeeName').autocomplete("close");
            //alert($scope.employee.employeeCardAccessLevel + "--" + $('#txtEmployeeCardAccessLevel').val());
        }
        
    };
    
    $scope.searchEmployeeOnKeyPress = function ($event) {
        if ($event.keyCode === 13 || $event.keyCode === 9) {
            $http.get(searchUrlByName + $scope.employee.employeeName).success(function (nameSarchData) {
                if (nameSarchData && nameSarchData.length === 1) {
                    getEmployeeDetails(nameSarchData[0]);
                } else if (nameSarchData && nameSarchData.length > 1) {
                   // $scope.searchEmployeeOnTextChange();
                } else {
                    
                }
            });
        }
    };
    
    $scope.searchEmployeeOnSearchButtonPress = function () {
        if ($scope.employee.employeeName === undefined || $scope.employee.employeeName === '') return;
        $http.get(searchUrlByName + $scope.employee.employeeName).success(function (nameSarchData) {
            if (nameSarchData && nameSarchData.length === 1) {
                getEmployeeDetails(nameSarchData[0]);
            } else if (nameSarchData && nameSarchData.length > 1) {
               // $scope.searchEmployeeOnTextChange();
            } else {

            }
        });
    };

    $scope.assignCardToEmployee = function (isUpdate) {
        if ($scope.employee.employeeId === undefined || $scope.employee.employeeId === '') return;
        var assignVisitorCardJSON = { Id: $scope.employee.employeeId, NewVisitorCard: $scope.employee.newVisitorCard, IsUpdate: isUpdate,VisitorPassAllocationId : $scope.employee.visitorPassAllocationId };
        $http.post(url, assignVisitorCardJSON).success(function (data) {
            if (!data || !data.Status || data.Status === false) {
                if (data.Message)
                    toastr.warning(data.Message);
                else
                    toastr.error("Error assigning card");
            }
            else if (data.Status) {
                if (!$scope.employee.visitorCardsNotReturned || $scope.employee.visitorCardsNotReturned === "") {
                    $scope.employee.visitorCardsNotReturned = assignVisitorCardJSON.NewVisitorCard;
                } else {
                    if (isUpdate) {
                        $scope.employee.visitorCardsNotReturned =  assignVisitorCardJSON.NewVisitorCard;
                    }
                    else {
                      $scope.employee.visitorCardsNotReturned = $scope.employee.visitorCardsNotReturned + ',' + assignVisitorCardJSON.NewVisitorCard;  
                    }
                    
                }
                $scope.employee.newVisitorCard = "";
                if (isUpdate) {
                	toastr.success("Card changed successfully");
	                $scope.clearFields();
                }
                else {
                	toastr.success("Card assigned successfully");
	                $scope.clearFields();
                }
                
                $rootScope.$broadcast('refreshEmployeeList');
            }
        });
    };
    
    $scope.clearFields = function () {
        $scope.employee.employeeName = "";
        $scope.employee.employeeId = "";
        $scope.employee.visitorCardsNotReturned = "";
        $scope.employee.employeeCardAccessLevel = "";
        $scope.employee.newVisitorCard = "";
        $scope.employee.employeeSearchStatus = "";
        Employee.isAllocateButtonDisabled = false;
    };

    $scope.CardNoValidation = /^\d{4,4}$/i;

    $scope.$on('viewCard', function () {
		$http.get("./api/card/").success(function(data) {
			if (data != null)
				$scope.employee.newVisitorCard = data;
			else
				$scope.employee.newVisitorCard = "";
		});
    });
	
    
}]);
