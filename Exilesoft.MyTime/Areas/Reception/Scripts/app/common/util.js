function SetCookies($http, $cookies) {
    $http.defaults.headers.common = { 'Authorization': $cookies.LoginCookie };
};

angular.module("receptionApp", []).directive('autoComplete', function($timeout) {
    return function(scope, iElement, iAttrs) {
        iElement.autocomplete({
            source: scope[iAttrs.uiItems],
            select: function() {
                $timeout(function() {
                    iElement.trigger('input');
                }, 0);
            }
        });
    };
});

//function httpErrorHandler(result, $window) {
//    switch (result.status) {
//        case 401:
//            //Unauthorized
//            $window.location.href = mainLoginUrl;
//        case 403:
//            //Forbidden
//            $window.location.href = mainLoginUrl;
//        case 406:
//            //Forbidden
//            toastr.error(result.data);
//        default:
//            toastr.error(result.data.ExceptionMessage);
//    }
//}

