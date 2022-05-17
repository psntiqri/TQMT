var historyData = angular.module("receptionApp");
var isLoaded = false;

historyData.controller('historyDataController', ['$scope', '$http', function ($scope, $http) {
    
    var defaultDate = new Date();

    function fromDate() {
        var today = new Date();
        today.setDate(today.getDate() - 14);
        return today;
    }
	
    function formatDefaultDate(date) {
        var year = date.getFullYear();
        var month = date.getMonth() + 1;

        if (month < 10) {
            month = "0" + month;
        };

        var day = date.getDate();

        if (day < 10) {
            day = "0" + day;
        };

        return day + '/' + month + '/' + year;
    };

    $scope.historyList = [];
    $scope.fromDate = formatDefaultDate(fromDate());
    $scope.toDate = formatDefaultDate(defaultDate);

    $scope.historyGrid = {
        data: 'historyList',
        multiSelect: false,
        enableColumnResize: true,
        enableColumnReordering: false,
        selectedItems: $scope.selectedRow,
        columnDefs: [{ field: "VisitorName", displayName: 'Visitor/Employee Name' },
                   { field: "VisitorType", displayName: 'Type' },
                   { field: "Description", displayName: 'Description'},
                   { field: "VisitPurpose", displayName: 'Purpose'},
                   { field: "VisitorOfEmployee", displayName: 'Visitor Of' },
                   { field: "Location", displayName: 'Access Level' },
                   { field: "IdentificationNumber", displayName: 'NIC/Passport Number' },
				   { field: "MobileNo", displayName: 'Mobile No'},
                   { field: "Company", displayName: 'Company' },
                   { field: "DateAssigned", displayName: 'Date Assigned', cellFilter: 'date:\'dd-MM-yyyy HH:MM:ss\'' },
                   { field: "DeallocateDate", displayName: 'Date Returned', cellFilter: 'date:\'dd-MM-yyyy HH:MM:ss\'' },
                   { field: "IsActive", displayName: 'Is Active', cellTemplate: '<div style="text-align:center;vertical-align:text-bottom"><input type="checkbox" style="vertical-align: -7px;" ng-model="row.entity.IsActive" disabled></div>' },
                   { field: "CardId", displayName: 'Card Id' }],
        
    };
    
    $scope.searchHistory = function () {

        var dateRange = { fromDate: $scope.fromDate, toDate: $scope.toDate };

        $http.post("Reception/api/visithistory/GetHistory", dateRange).success(function (data) {
            $scope.historyList = data;
        });
    };

    if (isLoaded === false) {
        $scope.searchHistory();
        isLoaded = true;
    }
}]);