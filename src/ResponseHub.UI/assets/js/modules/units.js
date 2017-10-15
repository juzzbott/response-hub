responseHub.units = (function () {

	function bindUI() {
		// Bind the use current location
		$('#btn-current-location').on('click', function () {
			responseHub.maps.getCurrentLocation('#Latitude', '#Longitude');
		});

		$('#confirm-delete.delete-user').on('show.bs.modal', function (e) {
			
			// Generate the confirm message
			$(this).find('.modal-body p span').text($(e.relatedTarget).data('user-name'));
			
		});

	}

	// Bind the ui
	bindUI();

})();