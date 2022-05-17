var datePickerModule = angular.module("receptionApp");

datePickerModule.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function ($scope, $element, $attrs, $ngModel) {
            $element.datepicker({
                dateFormat: 'dd/mm/yy',
                onSelect: function (date) {
                    $scope.$apply(function() {
                        $ngModel.$setViewValue(date);
                    });
                }
            });
        }
    };
});