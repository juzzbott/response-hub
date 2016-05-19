responseHub.logs = (function () {

	function bindUI() {

		if ($('.log-file-select').length > 0) {

			$('.log-file-select').on('change', function () {

				// Get the current location without querystrings
				var url = window.location.href.split('?')[0];

				// Get the log file
				var logFile = $(this).val();

				// redirect to the selected log file
				window.location.href = url + "?file=" + logFile;

			});

		}

	}

	// Bind the UI
	bindUI();

})();