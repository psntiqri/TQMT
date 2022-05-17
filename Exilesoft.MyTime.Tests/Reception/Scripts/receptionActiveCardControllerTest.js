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
/// <reference path="../../../exilesoft.mytime/areas/reception/scripts/app/controllers/receptionactivecardscontroller.js" />

describe("receptionActiveCardsController Tests->", function () {

    var httpBackend;
    var scope;
    var mainController;
    var rootScope;

    beforeEach(module("receptionApp"));
    
    beforeEach(inject(function ($rootScope, $controller, $httpBackend) {
        
        httpBackend = $httpBackend;
        scope = $rootScope.$new();
        mainController = $controller("ReceptionActiveCardsCtrl", { $scope: scope });
    }));
        
    it("has controllers registered ", function () {
        expect(mainController).not.toBeNull();
        expect(mainController).toBeDefined();
       
    });
    
    it("Get employeeActiveCards", function () {
        httpBackend.expectGET("Reception/api/reception/GetActiveCards").respond(
            [
                { Id: '1', Name: 'Asanka', NewVisitorCard: 111 },
                { Id: '2', Name: 'Kishan', NewVisitorCard: 112 },
                { Id: '3', Name: 'Tharaka', NewVisitorCard: 113 }
            ]
        );

        scope.fetchActiveCardsOfEmployee();
        httpBackend.flush();
      
        expect(scope.employeeActiveCards.length).toEqual(3);
        
    });

    //it("Can_Load_Directive", inject(function (ReceptionActiveCards) {
    //    expect(ReceptionActiveCards).toBeDefined();
    //}));
    
    //it("Remove employee from list when complete button click", function () {
    //    httpBackend.expectPOST("Reception/api/reception/DeAllocateEmployeePass?visitorPassAllocationId=' + 1").respond(
    //        [
    //            { Id: '1', Name: 'Asanka', NewVisitorCard: 111 },
    //            { Id: '2', Name: 'Kishan', NewVisitorCard: 112 },
    //            { Id: '3', Name: 'Tharaka', NewVisitorCard: 113 }
    //        ]
    //    );
    //    scope.deallocateEmployeePass();
    //    httpBackend.flush();
    //    expect(scope.employeeActiveCards.count()).toEqual(2);
    //});

    //it("Should get user name from server", function () {
    //    httpBackend.expectGET("Reception/api/reception/GetActiveCards").respond("Horana");
    //    scope.saveUser();
    //    httpBackend.flush();
    //    expect(scope.user).toBeUndefined();
    //});


});