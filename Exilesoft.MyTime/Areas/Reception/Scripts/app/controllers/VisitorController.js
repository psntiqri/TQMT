var viewVisitors = angular.module("receptionApp");

viewVisitors.controller('VisitorCtrl', ['$scope','$rootScope', '$http', 'Employee', 'SharedVisitor', 'Visit', 'VisitorPass', 'ReceptionActiveCards', 'ReceptionPendingCards', function ($scope, $rootScope, $http, Employee, SharedVisitor, Visit, VisitorPass, ReceptionActiveCards, ReceptionPendingCards) {

    //$http.get('Reception/api/visitor').success(function (data) {
    //    $scope.visitors = data;
    //});

    //$scope.EMAIL_REGEXP = /^[a-z0-9!#$%&'*+/=?^_`{|}~.-]+@[a-z0-9-]+(\.[a-z0-9-]+)*$/i;
    $scope.Visitor = SharedVisitor;
    $scope.Employee = Employee;
    $scope.Visit = Visit;
    $scope.VisitorPass = VisitorPass;
    
    
    var isMobileNoSearch = false;
    $scope.SearchByMobileNo = function (event) {
        if (event.which == 13 || event.which == 9) {
            $scope.Visitor.VisitorId = '';
            var mobileNo = $scope.Visitor.mobileNo;
            if (mobileNo && (mobileNo.length < 1)) return;
            $http.get('./api/visitor/GetByMobileNo?mobileNo=' + mobileNo).success(function (data) {
                if (data == null) {
                    $scope.Visitor.VisitorIdNo = '';
                    $scope.Visitor.VisitorName = '';
                    $scope.Visitor.Company = '';
                    $scope.Visitor.Email = '';
                    $scope.Visitor.VisitorId = 0;
                    Employee.employeeId = '';
                    Employee.employeeName = '';

                    $scope.VisitorPass.CardId = '';
                    Visit.VisitId = 0;
                    Visit.Purpose = '';
                    Visit.Description = '';
                    Visit.Category = 'undefined';
                } else {
                    if ($scope.Visitor.VisitorIdNo && $scope.Visitor.mobileNo === data.MobileNo && $scope.Visitor.VisitorIdNo !== data.IdentificationNo) {
                        var replaceIdNo = confirm('Previously entered Id No is different from entered Id No. Is entered Id No correct?', 'Confirmation');
                        if (replaceIdNo) {
                            $scope.Visitor.VisitorIdNo = $scope.Visitor.VisitorIdNo;
                        } else {
                            $scope.Visitor.VisitorIdNo = data.IdentificationNo;
                        }
                    } else {
                        $scope.Visitor.VisitorIdNo = data.IdentificationNo;
                    }
                        
                    //$scope.Visitor.VisitorIdNo = data.IdentificationNo;
                  //  isMobileNoSearch = true;
                    $scope.Visitor.VisitorName = data.Name;
                    $scope.Visitor.Company = data.Company;
                    $scope.Visitor.Email = data.Email;
                    $scope.Visitor.VisitorId = data.Id;

                   

                }
            });
        }
    };
    var isIdSearch = false;
    $scope.SearchByIdentityNo = function (event) {
        if (event.which == 13 || event.which == 9) {
            $scope.Visitor.VisitorId = '';
            var visitorIdNo = $scope.Visitor.VisitorIdNo;
            if (visitorIdNo && (visitorIdNo.length < 1)) return;
            $http.get('./api/visitor/GetByIdNo?identityNo=' + visitorIdNo).success(function (data) {
                if (data == null) {
                    // $scope.Visitor.mobileNo = '';
                    $scope.Visitor.VisitorName = '';
                    $scope.Visitor.Company = '';
                    $scope.Visitor.Email = '';
                    $scope.Visitor.VisitorId = 0;
                    Employee.employeeId = '';
                    Employee.employeeName = '';


                    $scope.VisitorPass.CardId = '';
                    Visit.Purpose = '';
                    Visit.Description = '';
                    Visit.Category = 'undefined';
                    Visit.VisitId = 0;
                } else {
                    if ($scope.Visitor.mobileNo && $scope.Visitor.VisitorIdNo === data.IdentificationNo && $scope.Visitor.mobileNo !== data.MobileNo) {
                        var replaceMobileNo = confirm('Previously entered Mobile No is different from entered Mobile No. Is entered Mobile No correct?', 'Confirmation');
                        if (replaceMobileNo) {
                            $scope.Visitor.mobileNo = $scope.Visitor.mobileNo;
                        } else {
                            $scope.Visitor.mobileNo = data.MobileNo;
                        }
                    } else {
                        $scope.Visitor.mobileNo = data.MobileNo;
                    }
                  //  isIdSearch = true;
                  //  $scope.Visitor.mobileNo = data.MobileNo;
                    $scope.Visitor.VisitorName = data.Name;
                    $scope.Visitor.Company = data.Company;
                    $scope.Visitor.Email = data.Email;
                    $scope.Visitor.VisitorId = data.Id;

                  
                }
            });
        }
    };

    $scope.AddEditVisitorVisit = function (isVisitorView, isUpdate) {

        var insertData = {
            Name: $scope.Visitor.VisitorName,
            MobileNo: $scope.Visitor.mobileNo,
            Email: $scope.Visitor.Email,// 1
            IdentificationNo: $scope.Visitor.VisitorIdNo,
            Company: $scope.Visitor.Company, //1
            EmployeeId: Employee.employeeId,
            EmployeeName: Employee.employeeName,
           
            CardAccessLevelId: 1,//$scope.Visit.Category.Id, //1
            VisitorId: $scope.Visitor.VisitorId,
            VisitId: Visit.VisitId,
            IsUpdate: isUpdate

        };
        if ($scope.VisitorPass)
            insertData.CardId= $scope.VisitorPass.CardId;
        if ($scope.Visit) {
            insertData.VisitPurpose = $scope.Visit.Purpose;
            insertData.Description = $scope.Visit.Description;
        }

        var addVisitUrl = "Reception/api/visitor";
        if (isVisitorView) {
            addVisitUrl = "api/visitor";
            insertData.IsVisitorView = isVisitorView;
        }

        var btn = $("#btnVisitorSubmit");
        btn.button('loading');
        
        $http.post(addVisitUrl, insertData).success(function (data, status, headers) {
            
            btn.button('reset');
            if (!data || !data.Status || data.Status === false) {
              
                if (data.Message)
                    toastr.warning(data.Message);
                else
                    
                    toastr.error(data.Message);
            }
            else if (data.Status) {
                if (isVisitorView) {
                    toastr.success("Visit Registered Successfully");
                } else {
                    if (isUpdate) {
                        toastr.success("Visitor information changed successfully");
                    }
                    else {
                        //ReceptionPendingCards.removeRow(insertData);
                        //ReceptionActiveCards.addRow(insertData);
                        toastr.success("Card Assigned To the visitor Successfully");
                    }
                    $rootScope.$broadcast('refreshActiveCards');
                    $rootScope.$broadcast('refreshCards', insertData);
                }
                $scope.ClearFields();
            }
        }).error(function (data, status, headers) {
            toastr.warning(data.Message);
            btn.button('reset');
        });
    };
    


    //$http.get('Reception/api/CardAccessLevel').success(function (data) {
    //    $scope.GetCardAccessLevels = data;
    //});

    $scope.ClearFields = function () {
        $scope.Visitor.VisitorName = '';
        $scope.Visitor.mobileNo = '';
        $scope.Visitor.Email = '';
        $scope.Visitor.VisitorIdNo = '';
        $scope.Visitor.Company = '';
        Employee.employeeId = '';
        Employee.employeeName = '';
       
        $scope.Visitor.VisitorId = 0;
        $scope.Visitor.VisitId = 0;
        SharedVisitor.isAllocateButtonDisabled = false;
        
        if ($scope.VisitorPass)
            $scope.VisitorPass.CardId = '';
        if ($scope.Visit) {
            Visit.Purpose = '';
            Visit.Description = '';
            Visit.Category = 'undefined';
        }

    };


    $scope.ClearFields();
    //$scope.$watch(
    //               "Visitor.mobileNo",
    //    function (newValue, oldValue) {

    //        // Ignore initial setup.
    //        if (newValue === oldValue) {
    //            return;
    //        } else {
    //            if (isIdSearch && newValue != undefined && newValue != '' && oldValue != '') {
    //                var replaceMobileNo = confirm('Previously entered Mobile No is different from entered Mobile No. Is entered Mobile No correct?', 'Confirmation');
    //                if (!replaceMobileNo) {
    //                    $scope.Visitor.mobileNo = newValue;
    //                } else {
    //                    $scope.Visitor.mobileNo = oldValue;
    //                }
    //                //var dlg = $dialogs.confirm('Please Confirm', 'Previously ?');
    //                //dlg.result.then(function (btn) {
    //                //    $scope.Visitor.mobileNo = newValue;
    //                //}, function (btn) {
    //                //    $scope.Visitor.mobileNo = oldValue;
    //                //});
    //                isIdSearch = false;

    //            };


    //        }
    //    }

    //);

   // $scope.$watch(
   //               "Visitor.VisitorIdNo",
   //    function (newValue, oldValue) {

   //        // Ignore initial setup.
   //        if (newValue === oldValue) {
   //            return;
   //        } else {
   //            if (isMobileNoSearch && newValue != undefined && newValue != '' && oldValue != '') {
   //                var replaceIdNo = confirm('Previously entered Id No is different from entered Id No. Is entered Id No correct?', 'Confirmation');
   //                if (replaceIdNo) {
   //                    $scope.Visitor.VisitorIdNo = newValue;
   //                } else {
   //                    $scope.Visitor.VisitorIdNo = oldValue;
   //                }
   //                isMobileNoSearch = false;
   //            }
   //        }
   //    }
	//);

    $scope.checkRequiredFieldsForMobile = function ( Visitor) {
	   
	    if (isNaN(Visitor.mobileNo)) {
	    	toastr.error("Please Enter Your Mobile No Correctly");
	    }
	    else if (Visitor.VisitorIdNo.replace(/\s/g, "") == "") {
	    	toastr.error("Please Enter Your NIC or Passport No");
	    }
	    else if (Visitor.VisitorName.replace(/\s/g, "") == "") {
	    	toastr.error("Please Enter Your Name");
	    }
	    else {
	    	$scope.AddEditVisitorVisit(true, false);
	    }
	    
    };

    $scope.checkRequiredFieldsForDesktop = function (Visitor, VisitorPass, isUpdate) {
	   
	    if (isNaN(Visitor.mobileNo)) {
	    	toastr.error("Please Enter Your Mobile No Correctly");
		}
	    else if (Visitor.VisitorIdNo.replace(/\s/g, "") == "") {
	    	toastr.error("Please Enter Your NIC or Passport No");
		}
	    else if (Visitor.VisitorName.replace(/\s/g, "") == "") {
	        toastr.error("Please Enter Your Name");
	    }
	    else {
	        $scope.AddEditVisitorVisit(false, isUpdate);
	    }

    };
	
    $scope.telephoneNovalidation = /^-*[0-9,\.]+$/i; //^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$/;
}]);
