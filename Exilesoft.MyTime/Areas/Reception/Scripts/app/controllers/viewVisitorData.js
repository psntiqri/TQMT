var viewVisitors = angular.module("MyTime");
viewEmployees.controller("viewVisitorDataController", ['$scope', '$http', 'receptionService', '$window', '$location', '$cookies', '$stateParams', function ($scope, $http, receptionService, $window, $location, $cookies, $stateParams) {
    SetCookies($http, $cookies);

    $scope.loadEmployee = function () {
        var employeeId = $stateParams.employeeId;
        if (employeeId) {
            receptionService.getEmployeeById($http, $scope, $window, employeeId);
        } else {
            receptionService.getCurrentLoginEmployee($http, $scope, $window);
        }
    };
    
    $scope.doSearch = function () {

        $scope.currentPage = 1;
       /// getPlaces();

    };

   

}]);