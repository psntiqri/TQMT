(function (angular) {
	var receptionApp = angular.module("receptionApp",
	[
		'ngRoute',
		'ngResource',
		'ui.router',
		'ngGrid'
	]);

	receptionApp.config(function ($stateProvider, $urlRouterProvider) {

        // For any unmatched url, send to /dashboard
	    $urlRouterProvider.otherwise("/dashboard");

        $stateProvider
            .state('dashboard', {
                url: "/dashboard",
                templateUrl: "Reception/ReceptionHome/Dashboard"
            })
            .state('dashboard.visitor', {
                url: "/visitor",
                templateUrl: "Reception/ReceptionHome/VisitorDetails?deviceType=desktop"
            })
            .state('dashboard.employee', {
                url: "/employee",
                templateUrl: "Reception/ReceptionHome/EmployeeDetails"
            })
            .state('cardhistory', {
                url: "/cardhistory",
                templateUrl: "Reception/ReceptionHome/ViewAllCardHistory"
            })
            .state('history', {
                url: "/history",
                templateUrl: "Reception/ReceptionHome/History",
                controller: "historyDataController"
            })
            .state('mobile', {
                url: "/mobile",
                templateUrl: "/Reception/Mobile",
            })
            .state('visitor', {
                url: "/visitor",
                templateUrl: "/Reception/Mobile/Visitor",
            })
            .state('employee', {
                url: "/employee",
                templateUrl: "/Reception/Mobile/Employee",
            });


        //    var mobile = {
        //        name: 'mobile',  //mandatory
        //        url: '/mobile',
        //        templateUrl: '/Reception/Mobile'
        //    }, mobileVisitor = {
        //        name: 'visitor', //mandatory. This counter-intuitive requirement addressed in issue #368
        //        //parent: mobile,  //mandatory
        //        url: '/visitor',
        //        templateUrl: '/Reception/Mobile/Visitor'
        //    }, mobileEmployee = {
        //        name: 'employee', //mandatory. This counter-intuitive requirement addressed in issue #368
        //        //parent: mobile,  //mandatory
        //        url: '/employee',
        //        templateUrl: '/Reception/Mobile/Employee'
        //    };

        //    $stateProvider
        //        .state(mobile)
        //        .state(mobileVisitor)
        //        .state(mobileEmployee);
    });

    receptionApp.factory('Employee', function () {
        return {};
    });
    receptionApp.factory('SharedVisitor', function () {
        return {};
    });
    
    receptionApp.factory('Visit', function () {
        return {};
    });
    
    receptionApp.factory('VisitorPass', function () {
        return {};
    });
    
    receptionApp.factory('ReceptionActiveCards', function () {
        return {};
    });
    
    receptionApp.factory('ReceptionPendingCards', function () {
        return {};
    });
    

})(angular);
