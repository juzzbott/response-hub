responseHub.reports = (function () {


	var PIXEL_RATIO = (function () {
		var ctx = document.createElement("canvas").getContext("2d"),
			dpr = window.devicePixelRatio || 1,
			bsr = ctx.webkitBackingStorePixelRatio ||
				ctx.mozBackingStorePixelRatio ||
				ctx.msBackingStorePixelRatio ||
				ctx.oBackingStorePixelRatio ||
				ctx.backingStorePixelRatio || 1;

		return dpr / bsr;
	})();

	function createHiDPICanvas (w, h, ratio) {
		if (!ratio) { ratio = PIXEL_RATIO; }
		var can = document.createElement("canvas");
		can.width = w * ratio;
		can.height = h * ratio;
		can.style.width = w + "px";
		can.style.height = h + "px";
		can.getContext("2d").setTransform(ratio, 0, 0, ratio, 0, 0);
		return can;
	}

	function displayTrainingReportGraph() {

		var chartCanvas = createHiDPICanvas(1200, 400);
		
		document.getElementById("chart-container").appendChild(chartCanvas);
		var jsonData = $('#chart-data').val().replace(/&quot;/g, '"');
		var chartData = JSON.parse(jsonData);
		var itemLength = chartData.datasets[0].data.length;
		chartData.datasets[0].backgroundColor = palette('tol', itemLength).map(function(hex) {{return '#' + hex;}})

		var myPieChart = new Chart(chartCanvas, {
			type: 'bar',
			data: chartData,
			options: {
				scales: {
					yAxes: [{
						ticks: {
							stepSize: 1
						}
					}]
				},
				title: {
					display: true,
					text: 'Unit training sessions',
					fontSize: 16
				},
				legend: {
					display: false
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