/// <reference path="../../scripts/jasmine.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/angular.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/angular-route.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/angular-ui-router.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/angular-resource.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/angular-mocks.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/jquery/jquery-1.9.1.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/jqueryui/jquery-ui-1.10.4.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/toastr/toastr.min.js" />
/// <reference path="../../../exilesoft.mytime/areas/reception/scripts/app/app.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/directives/ng-grid-2.0.11.min.js" />
/// <reference path="../../../exilesoft.mytime/areas/reception/scripts/app/controllers/receptioncontroller.js" />

describe("receptionController Tests->", function () {

	var httpBackend;
	var scope;
	var mainController;
	var rootScope;

	beforeEach(module("receptionApp"));

	beforeEach(inject(function ($rootScope, $controller, $httpBackend) {
		httpBackend = $httpBackend;
		scope = $rootScope.$new();
		mainController = $controller("ReceptionCtrl", { $scope: scope });
	}));

	it("has controllers registered ", function () {
		expect(mainController).not.toBeNull();
		expect(mainController).toBeDefined();

	});

	it("Get pending visits", function () {
		httpBackend.expectGET("Reception/api/reception/GetPendingVisits?type=Pending").respond(
            [
                { Id: '1', Company: "Exile", IdentificationNo: "555555555v", Name: 'Asanka', MobileNo: 111 },
                { Id: '2', Company: "Exile", IdentificationNo: "555555555v", Name: 'Kishan', MobileNo: 112 },
                { Id: '3', Company: "Exile", IdentificationNo: "555555555v", Name: 'Tharaka', MobileNo: 113 }
            ]
        );

		scope.fetchActiveCardsOfEmployee();
		httpBackend.flush();

		expect(scope.pendingCards.length).toEqual(3);

	});
	
});