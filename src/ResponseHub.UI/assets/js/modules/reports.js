responseHub.reports = (function () {


	function displayTrainingReportGraph() {

		var jsonData = $('#training-overview-chart-data').val().replace(/&quot;/g, '"');
		var chartData = JSON.parse(jsonData);
		var ctx = document.getElementById("training-overview-chart").getContext('2d');

		var chart = new Chart(ctx, {
			type: 'bar',
			data: chartData,
			options: {
				scales: {
					yAxes: [{
						ticks: {
							beginAtZero: true,
							stepSize: 1,
							userCallback: function (label, index, labels) {
								// when the floored value is the same as the value we have a whole number
								if (Math.floor(label) === label) {
									return label;
								}
							},
						}
					}]
				},
				legend: {
					display: false,
				},
				tooltips: {
					callbacks: {
						label: tooltipItem => `${tooltipItem.yLabel}: ${tooltipItem.xLabel}`,
						title: () => null,
					}
				}
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