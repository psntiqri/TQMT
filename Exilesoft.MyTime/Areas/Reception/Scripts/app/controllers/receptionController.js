var viewCardDetails = angular.module("receptionApp");

viewCardDetails.controller('ReceptionCtrl', ['$scope', '$http', 'SharedVisitor', 'Employee', 'Visit', 'VisitorPass', 'ReceptionPendingCards', function ($scope, $http, SharedVisitor, Employee, Visit, VisitorPass, ReceptionPendingCards) {
    //$(function() {
    //    if (window.innerWidth <= 1350 && window.innerHeight <= 600) {
    //        $scope.isMobileDevice = true;
    //        return;
    //    }
    //});
    $scope.pendingCards = '';
    $scope.selectedRow = [];
    $scope.gridOptions = {
        data: 'pendingCards',
        multiSelect: false,
        showSelectionCheckbox: false,
        enableColumnResize: true,
        enableColumnReordering: true,
        selectedItems: $scope.selectedRow,
        columnDefs: [{ field: "VisitorId", visible: false },
                   { field: "VisitId", visible: false },
                   { field: "EmployeeId", visible: false },
                   { field: "Name", displayName: 'Visitor' },
                   { field: "EmployeeName", displayName: 'Visitor Of' },
                   { field: "AppointmentTime", displayName: 'Appointment Time', cellFilter: 'date:\'yyyy/MM/dd HH:MM:ss \'', visible: false },
				   { field: '', displayName: '', cellTemplate: '<button ng-click="closeAppointment($scope.selectedRow)">Close</button>' }],

        afterSelectionChange: function (theRow, evt) {
            if (theRow.selected) {
                SharedVisitor.VisitorName = theRow.entity.Name;
                SharedVisitor.mobileNo = theRow.entity.MobileNo;
                SharedVisitor.VisitorIdNo = theRow.entity.IdentificationNo;
                Employee.employeeName = theRow.entity.EmployeeName;
                Employee.employeeId = theRow.entity.EmployeeId;
                SharedVisitor.VisitorId = theRow.entity.VisitorId;
                Visit.VisitId = theRow.entity.VisitId;
                Visit.Purpose = '';
                SharedVisitor.Company = theRow.entity.Company;
                SharedVisitor.Email = '';
                VisitorPass.CardId = '';
                Visit.Description = '';
                SharedVisitor.isAllocateButtonDisabled = false;
            }

        }


    };

    ReceptionPendingCards.fetchPendingDetails = function () {
        setTimeout(function () {
            $http({
                url: 'Reception/api/reception/GetPendingVisits?type=Pending',
                type: 'GET'
            })
                .success(function (data) {
                    $scope.pendingCards = data;
                    if (!$scope.$$phase) {
                        $scope.$apply();
                    }
                });
        }, 3000);
    };

    ReceptionPendingCards.fetchPendingDetails();

    //ReceptionPendingCards.removeRow = function (row) {
    //    $scope.pendingCards.splice($scope.pendingCards.indexOf(row), 1);
    //};
    
    var unbindActiveCards = $scope.$on('refreshCards', function (event, data) {
        //$scope.pendingCards.splice($scope.pendingCards.indexOf(row), 1);
        
        for (var i = 0, l = $scope.pendingCards.length; i < l; i++) {
            // check the obj has the property before comparing it
            if (!$scope.pendingCards[i]['VisitId']) continue;

            // if the obj property equals our test value, return the obj
            if ($scope.pendingCards[i]['VisitId'] === data.VisitId) {
                $scope.pendingCards.splice($scope.pendingCards.indexOf($scope.pendingCards[i]), 1);
            }
        }
    });

    $scope.$on('$destroy', unbindActiveCards);
    
    $scope.addRow = function (row) {
        $scope.pendingCards.unshift({
            VisitorId: row.VisitorId, VisitId: row.VisitId, EmployeeId: row.EmployeeId, Name: row.Name, EmployeeName: row.EmployeeName, AppointmentTime: row.AppointmentTime, MobileNo: row.MobileNo, IdentificationNo: row.IdentificationNo
        });
        $scope.$digest();
    };
	
    $scope.closeAppointment = function () {
		var index = this.row.rowIndex;
    	$scope.gridOptions.selectItem(index, false);
    	$http.put('./Reception/api/reception/CloseAppoinment?appoinmentId=' + this.row.entity.VisitId).success(function () {
    		$scope.pendingCards.splice(index, 1);
    	});
    	
    };

}]);


