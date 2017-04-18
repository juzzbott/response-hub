responseHub.resources = (function () {

	/** 
	 * Adds a resource to the system for the specified unit. 
	 */
	function addResource() {

		var buttonCtl = $("#add-resource");

		// Set the spinner
		buttonCtl.find('i').removeClass('fa-plus').addClass('fa-refresh fa-spin');
		buttonCtl.attr('disabled', 'disabled');

		// Get the unit id
		var eventId = $("#EventId").val();

		// Create the post data object
		var postData = {
			'Name': $("#Name").val(),
			'AgencyId': $("#AgencyId").val(),
			'UserId': null,
			'Type': 2 // Additional resource
		};

		// Clear any existing alerts
		$(".add-resource-messages .alert").remove();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId + '/resources',
			type: 'POST',
			dataType: 'json',
			data: postData,
			success: function (data) {

				if (data == null) {
					// Show the error message
					$(".add-resource-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding the resource. Please try again shortly.</p>');
					return;
				}

			},
			error: function (jqXHR, textStatus, errorThrown) {
				
				// Show the error message
				$(".add-resource-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding the resource. Please try again shortly.</p>');

			},
			complete: function () {

				// Reset the button
				buttonCtl.find('i').addClass('fa-plus').removeClass('fa-refresh fa-spin');
				buttonCtl.removeAttr('disabled');

				// Reset fields
				$("#AdditionalResourceModel_Name").val('');
				$("#AdditionalResourceModel_AgencyId").val('');

			}
		});
	}

	return {
		addResource: addResource
	}

})();