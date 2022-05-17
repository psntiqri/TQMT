(function (angular) {
    var viewVisitors = angular.module("MyTime");
    viewEmployees.service('receptionService', ['$resource', function ($resource) {

        var serviceResult = {};
        serviceResult.getVisitors = function ($http, $scope, $window) {
            $http.get("Areas/Reception/api/Visitor").then(
                    function (result) {

                        $scope.data = result.data;

                    },
                    function (result) {
                      //  httpErrorHandler(result, $window);
                    });
        };









        return serviceResult;

    }]);
})(angular);


