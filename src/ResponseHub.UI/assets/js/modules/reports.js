responseHub.reports = (function () {


	function displayTrainingReportGraph() {

		// If there is no element for chart data, just exit function
		if ($('#training-overview-chart-data').length == 0) {
			return;
		}
		var jsonData = $('#training-overview-chart-data').val().replace(/&quot;/g, '"');
		var chartData = JSON.parse(jsonData);
		var ctx = document.getElementById("training-report-overview-chart").getContext('2d');

		var chart = new Chart(ctx, {
			type: 'horizontalBar',
			data: chartData,
			options: {
				scales: {
					xAxes: [{
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
				},
				animation: {
					duration: 0
				}
			}
		});

		if ($('#change-canvas-to-image').val() == "1") {
			swapCanvasToImage();
		}
	}

	function bindUI() {

		if ($('#training-report').length > 0) {
			displayTrainingReportGraph();
		}
	}

	function swapCanvasToImage() {
		var canvas = document.getElementById("training-report-overview-chart");
		var img = canvas.toDataURL("image/png");
		$('<img src="' + img + '"/>').insertAfter('#change-canvas-to-image');
		$('<h1>testing</h1>').insertAfter('#change-canvas-to-image');
		$('#training-report-overview-chart').remove();
	};

	bindUI();

})();