responseHub.groups = (function () {

	function bindUI() {
		// Bind the use current location
		$('#btn-current-location').on('click', function () {
			responseHub.maps.getCurrentLocation('#Latitude', '#Longitude');
		});

		$('#confirm-delete.delete-user').on('show.bs.modal', function (e) {
			
			// Generate the confirm message
			var message = $(this).find('.modal-body p').text();
			message = message.replace('#USER#', $(e.relatedTarget).data('user-name'));

			// Set the confirm message
			$(this).find('.modal-body p').text(message);

		});

	}

	// Bind the ui
	bindUI();

})();