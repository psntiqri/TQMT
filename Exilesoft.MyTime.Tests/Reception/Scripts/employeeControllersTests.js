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
/// <reference path="../../../exilesoft.mytime/areas/reception/scripts/app/controllers/employeecontroller.js" />
/// <reference path="../../../exilesoft.mytime/scripts/libs/angular/directives/ng-grid-2.0.11.min.js" />

describe("employeeController Tests->", function () {

    beforeEach(function () {
        module("receptionApp");
    });

    var $httpBackend;
    var url = 'Reception/api/employee/';
    var searchUrlByName = './api/employee?name=';
    var searchUrlById = './api/employee?id=';
    var rootScope, employee, scope, createController;
   

    beforeEach(inject(function ($injector) {
        $httpBackend = $injector.get("$httpBackend");
        rootScope = $injector.get("$rootScope");
        employee = $injector.get("Employee");
        scope = rootScope.$new();
        var controller = $injector.get("$controller");
       
        createController = function () {
            return controller('EmployeeCtrl', {
                '$scope': scope,
                'Employee': employee
            });
        };
       
        
        $httpBackend.when("GET", searchUrlByName + 'Ra').respond(
            [
            {
                Id: 111,
                Name: "Ravi",
                ImagePath: "Ravi_1.jpg"
            },
            {
                Id: 112,
                Name: "Rani",
                ImagePath: "Rani_1.jpg"
            },
            {
                Id: 113,
                Name: "Ramesh",
                ImagePath: "Ramesh_1.jpg"
            }]
        );
	    
        $httpBackend.when("GET", "./api/address").respond(1096);
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    describe("Employee->", function () {
        it("has EmployeeObject", inject(function (Employee) {
            expect(Employee).toEqual({});
        }));
    });

    
    describe("EmployeeCtrl->", function () {
        it("has searchEmployeeOnTextChange", inject(function ($rootScope,  Employee, $controller) {
            //expect().toEqual([]);
           
            //console.log(searchUrlByName + 'Ra');
            var theScope = $rootScope.$new();
           
            // $httpBackend.expect('GET',searchUrlByName + 'Ra').respond(JSON_Names);
           
            var ctrl = $controller("EmployeeCtrl", {
                '$scope': theScope,
                'Employee': Employee
            });
            
            expect(ctrl).not.toBeNull();
            
            theScope.employee.employeeName = 'Ra';
            
            theScope.searchEmployeeOnTextChange();
           
            $httpBackend.flush();

            var expectedResult = [{ label: 'Ravi', value: 'Ravi', Idd: 111, Name: 'Ravi', ImagePath: 'Ravi_1.jpg' },
                               { label: 'Rani', value: 'Rani', Idd: 112, Name: 'Rani', ImagePath: 'Rani_1.jpg' },
                               { label: 'Ramesh', value: 'Ramesh', Idd: 113, Name: 'Ramesh', ImagePath: 'Ramesh_1.jpg' }];
            expect(theScope.employee.empNames).toEqual(expectedResult);

        }));
    });

    describe("EmployeeCtrl->", function () {
        it("has searchEmployeeOnKeyPress", inject(function ($compile) {
            var ctrl = createController();
            expect(ctrl).not.toBeNull();
            
            scope.employee.employeeName = 'Ra';
           
            var el = $compile('<input type="text" class="search_button" id="txtEmployeeName" data-ng-model="employee.employeeName" data-ng-change="searchEmployeeOnTextChange()" ng-keydown="searchEmployeeOnKeyPress($event)" />')(scope);
            var inputEl = el.find('input');
            var e = $.Event('keydown');
            e.which = 13;
            e.keyCode = 13;
            //angular.element(inputEl).triggerHandler(e);  //$(inputEl).trigger(e); 
            //$httpBackend.expect('GET', searchUrlByName + scope.employee.employeeName).respond(JSON_Names);
            scope.searchEmployeeOnKeyPress(e);
            $httpBackend.flush();
            expect(scope.employee.employeeId).toEqual(undefined);
            
            scope.employee.employeeName = 'Rani';
            $httpBackend.expect('GET', searchUrlByName + 'Rani').respond(
                [{
                Id: 112,
                Name: "Rani",
                ImagePath: "Rani_1.jpg"
            }]);
            scope.searchEmployeeOnKeyPress(e);
            $httpBackend.flush();
            expect(scope.employee.employeeId).toEqual(112);
           
        }));
    });

    describe("EmployeeCtrl->", function () {
        it("has searchEmployeeOnSearchButtonPress", function() {
            var ctrl = createController();
            expect(ctrl).not.toBeNull();
            
            scope.employee.employeeName = '';
            scope.searchEmployeeOnSearchButtonPress();
            expect(scope.employee.employeeId).toEqual(undefined);
            
            scope.employee.employeeName = 'Ra';
            scope.searchEmployeeOnSearchButtonPress();
            $httpBackend.flush();
            expect(scope.employee.employeeId).toEqual(undefined);
            
            scope.employee.employeeName = 'Rani';
            $httpBackend.expect('GET', searchUrlByName + 'Rani').respond(
                [{
                    Id: 113,
                    Name: "Ramesh",
                    ImagePath: "Ramesh_1.jpg"
                }]);
            scope.searchEmployeeOnSearchButtonPress();
            $httpBackend.flush();
            expect(scope.employee.employeeId).toEqual(113);
            
        });
    });

    describe("EmployeeCtrl->", function () {
        it("has assignCardToEmployee", function() {
            var ctrl = createController();
            expect(ctrl).not.toBeNull();

            scope.assignCardToEmployee();
            expect(scope.employee.visitorCardsNotReturned).toEqual(undefined);
            
            scope.employee.employeeId = 111;
            scope.employee.newVisitorCard = 5555;
            
            $httpBackend.expect('POST', url).respond(
               {
                   Status:true
               });
            scope.assignCardToEmployee();
            $httpBackend.flush();
            expect(scope.employee.visitorCardsNotReturned).toEqual(5555);

            //assign a another card and operation success
            $httpBackend.expect('POST', url).respond(
              {
                  Status: true
              });
            
            scope.employee.employeeId = 111;
            scope.employee.newVisitorCard = 6666;
            scope.assignCardToEmployee();
            $httpBackend.flush();
            expect(scope.employee.visitorCardsNotReturned).toEqual('5555,6666');
            
            // assign same card and operation fails
            $httpBackend.expect('POST', url).respond(
             {
                 Status: false
             });
            scope.employee.employeeId = 111;
            scope.employee.newVisitorCard = 6666;
            scope.assignCardToEmployee();
            $httpBackend.flush();
            expect(scope.employee.visitorCardsNotReturned).toEqual('5555,6666');
        });
    });

	describe("EmployeeCtrl->", function() {
		it("has viewCard", function() {
			var ctrl = createController();
			expect(ctrl).not.toBeNull();
			
			scope.$broadcast('viewCard');
			$httpBackend.flush();
			expect($scope.employee.newVisitorCard).not.toBeNull();
			expect($scope.employee.newVisitorCard).toEqual(1096);
		});
	});	
});
