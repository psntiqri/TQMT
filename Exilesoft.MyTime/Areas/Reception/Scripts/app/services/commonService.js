(function (angular) {
    var common = angular.module("delphiCommon", ['ngCookies']);

    common.service("commonService", ["$resource", function () {
        var serviceResult = {};

        serviceResult.logOut = function ($window, $cookies) {
            delete $cookies['LoginCookie'];
            $window.location.href = "";
        };

        return serviceResult;
    }
    ]);
})(angular);

