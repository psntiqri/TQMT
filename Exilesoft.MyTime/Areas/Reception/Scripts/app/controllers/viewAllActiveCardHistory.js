var activeCardHistoryController = angular.module("receptionApp");
activeCardHistoryController.controller('ActiveCardCtrl', ['$scope', '$rootScope', '$http', 'Employee', 'ReceptionActiveCards', function ($scope, $rootScope, $http, Employee, ReceptionActiveCards) {
	var url = 'Reception/api/ActiveCardHistory/';

	$scope.activeCardHistory = [];
	$scope.employeeSelectedRow = [];
	$scope.user = '';
	$scope.activeCardHistory = {
		data: 'activeCardHistory',
		multiSelect: false,
		enableColumnResize: true,
		enableColumnReordering: true,
		showSelectionCheckbox: false,
		selectedItems: $scope.employeeSelectedRow,
		columnDefs: [

                  
                   { field: "CardId", displayName: 'Card No' },
				   { field: "Name", displayName: 'Name' },
				   { field: "Type", displayName: 'Entry Type' }
                   ]
                  
	};


	$scope.getAllActiveCardsHistory = function () {
		$http.get('Reception/api/ActiveCardHistory').success(function (data) {			
			$scope.activeCardHistory = data;
		});
	};
	$scope.getAllActiveCardsHistory();



}]);