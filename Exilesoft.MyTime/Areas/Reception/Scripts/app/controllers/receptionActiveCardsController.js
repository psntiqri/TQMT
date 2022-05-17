var viewActiveCardDetails = angular.module("receptionApp");

viewActiveCardDetails.controller('ReceptionActiveCardsCtrl', ['$scope','$rootScope', '$http', 'SharedVisitor', 'Employee', 'Visit', 'VisitorPass', 'ReceptionActiveCards', '$state',
                function ($scope,$rootScope, $http, SharedVisitor, Employee, Visit, VisitorPass, ReceptionActiveCards, $state) {
    $scope.activeCards = '';
    $scope.selectedRow = [];
    $scope.gridOptions = {
        data: 'activeCards',
        multiSelect: false,
        enableColumnResize: true,
        enableColumnReordering: true,
        showSelectionCheckbox: false,
        selectedItems: $scope.selectedRow,
        columnDefs: [{ field: "VisitorId", visible: false },
                   { field: "VisitId", visible: false },
                   { field: "EmployeeId", visible: false },
                   { field: "Name", displayName: 'Visitor' },
                   { field: "EmployeeName", displayName: 'Visitor Of' },
                   { field: "CardId", displayName: 'Card No', width: 90 },
                   { field: "MobileNo", displayName: 'Mobile No', visible: false },
                   { field: "IdentificationNo", displayName: 'Identification No', visible: false },
                   { field: "VisitPurpose", displayName: 'Visit Purpose', visible: false },
                   { field: "Description", displayName: 'Description', visible: false },
                   { field: "Email", displayName: 'Email', visible: false },
                   { field: "Company", displayName: 'Company', visible: false },
                   { field: "Location", displayName: 'Location', width: 90 },
                   { field: '', displayName: '', width: 90, cellTemplate: '<button ng-click="deAllocateVisitorPass($event, row)">Complete</button>' }],

        afterSelectionChange: function (theRow, evt) {
        	$state.transitionTo('dashboard.visitor');
        	if (theRow.selected) {
        			SharedVisitor.VisitorName = (theRow.entity.Name == '') ? '' : theRow.entity.Name;
        			SharedVisitor.mobileNo = (theRow.entity.MobileNo == '') ? '' : theRow.entity.MobileNo;
        			SharedVisitor.VisitorIdNo = (theRow.entity.IdentificationNo == '') ? '' : theRow.entity.IdentificationNo;
        			Employee.employeeName = theRow.entity.EmployeeName;
        			SharedVisitor.Company = theRow.entity.Company;
        			SharedVisitor.Email = theRow.entity.Email;
        			Employee.employeeId = theRow.entity.EmployeeId;
        			SharedVisitor.VisitorId = theRow.entity.VisitorId;
        			Visit.VisitId = theRow.entity.VisitId;
        			VisitorPass.CardId = theRow.entity.CardId;
        			Visit.Purpose = (theRow.entity.VisitPurpose == '') ? '' : theRow.entity.VisitPurpose;
        			Visit.Description = theRow.entity.Description;
        			SharedVisitor.isAllocateButtonDisabled = true;
        	
            }
            
        }
        
    };
                    

    $scope.showEmployeeTab = function () {
                        $state.transitionTo('dashboard.employee');
                    };

    $scope.showVisitorTab = function() {
        $state.transitionTo('dashboard.visitor');
    };
   
 $scope.showVisitorTab();
    $scope.fetchActiveCards = function () {
        setTimeout(function () {
            $http({
                url: 'Reception/api/reception/GetPendingVisits?type=Active',
                type: 'GET'
            })
                .success(function (data) {
                    $scope.activeCards = data;
                    if (!$scope.$$phase) {
                        $scope.$apply();
                    }
                });
        }, 1000);
    };
    $scope.fetchActiveCards();
                    
    var unbindActiveCards = $scope.$on('refreshActiveCards', function () {
        $scope.fetchActiveCards();
    });

    $scope.$on('$destroy', unbindActiveCards);

    ReceptionActiveCards.addRow = function (row) {
        $scope.activeCards.push({
            VisitorId: row.VisitorId, VisitId: row.VisitId, EmployeeId: row.EmployeeId, Name: row.Name, EmployeeName: row.EmployeeName, CardId: row.CardId, MobileNo: row.MobileNo, IdentificationNo: row.IdentificationNo, VisitPurpose: row.VisitPurpose, Description: row.Description, Email: row.Email, Company: row.Company
        });
    };
    
    $scope.deAllocateVisitorPass = function (event, row) {
        event.stopPropagation();

        $http.post('./Reception/api/reception/DeAllocateVisitorPass?visitId=' + row.entity.VisitId).success(function (data) {
            var index = row.rowIndex;
            $scope.gridOptions.selectItem(index, false);
            $scope.activeCards.splice(index, 1);
        });
    };
    
    //Employee Active card details
    $scope.employee = Employee;
    $scope.employeeActiveCards = [];
    $scope.employeeSelectedRow = [];
    $scope.user = '';
    $scope.employeeGridOptions = {
        data: 'employeeActiveCards',
        multiSelect: false,
        enableColumnResize: true,
        enableColumnReordering: true,
        showSelectionCheckbox: false,
        selectedItems: $scope.employeeSelectedRow,
        columnDefs: [
                  
                   { field: "Id", displayName: 'Id', visible: false },
                   { field: "Name", displayName: 'Employee' },
                   { field: "NewVisitorCard", displayName: 'Card No' },
                   { field: "VisitorPassAllocationId", displayName: 'Visitor Pass Allocation Id', visible: false },
                   { field: '', displayName: '', cellTemplate: '<button ng-click="deallocateEmployeePass($scope.employeeSelectedRow)">Complete</button>' }],

        afterSelectionChange: function (theRow, evt) {
        	$state.transitionTo('dashboard.employee');
        	if (theRow.selected) {
        			$scope.employee.employeeName = theRow.entity.Name;
        			$scope.employee.employeeId = theRow.entity.Id;
        			$scope.employee.visitorCardsNotReturned = theRow.entity.NewVisitorCard;
        			$scope.employee.visitorPassAllocationId = theRow.entity.VisitorPassAllocationId;
        			$scope.employee.NewVisitorCard = $rootScope.$broadcast('viewCard');
        			Employee.isAllocateButtonDisabled = true;
        		
               
            }
           
			//$scope.showEmployeeTabWithData(theRow, evt);
         }

    };
   
    $scope.fetchActiveCardsOfEmployee = function () {
        $http.get("Reception/api/reception/GetActiveCards").success(function (data) {
            $scope.employeeActiveCards = data;
        });
    };
    
    var unbind = $scope.$on('refreshEmployeeList', function () {
        $scope.fetchActiveCardsOfEmployee();
    });

    $scope.$on('$destroy', unbind);

    $scope.deallocateEmployeePass = function () {
		var index = this.row.rowIndex;
        $scope.employeeGridOptions.selectItem(index, false);
        $scope.employeeActiveCards.splice(index, 1);
        $http.post('./Reception/api/reception/DeAllocateEmployeePass?visitorPassAllocationId=' + this.row.entity.Id);
    };


}]);


