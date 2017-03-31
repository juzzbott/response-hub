responseHub.reports = (function () {


	function displayTrainingReportGraph() {

		var jsonData = $('#training-overview-chart-data').val().replace(/&quot;/g, '"');
		var chartData = JSON.parse(jsonData);
		console.log(jsonData);

		new Chartist.Bar('#training-overview-chart', chartData, {
			distributeSeries: true,
			showGridBackground: true,
			axisY: {
				onlyInteger: true
			},
			axisX: {
				scaleMinSpace: 5
			}
		});

	}

	function bindUI() {

		if ($('#training-report').length > 0) {
			displayTrainingReportGraph();
		}
	}

	bindUI();

})();