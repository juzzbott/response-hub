responseHub.signIn = (function () {

	// Sets the job number and the job id when selected.
	function setOperationJobNumber(jobNumber, jobId) {

		// Set the jbo number and id in the hidden field
		$('#OperationDescription').val(jobNumber);
		$('#OperationJobId').val(jobId);
	}

	// Shows the elements for operation details
	function setActivityDetails(activity) {
		$('#operation-task').addClass('hidden');
		$('#training-task').addClass('hidden');
		$('#other-task').addClass('hidden');

		// Clear any error messages
		$('.training-types-messages .field-validation-error').empty().addClass('field-validation-valid').removeClass('field-validation-error');
		$('#operation-task .input-validation-error, #training-task .input-validation-error, #other-task .input-validation-error').removeAttr('aria-invalid').removeClass('input-validation-error');
		$('.validation-summary-errors').addClass('validation-summary-valid').removeClass('validation-summary-errors');
		$('.validation-summary-valid ul').empty().append('<li style="display:none"></li>');

		switch (activity) {
			case 'operation':
				$('#operation-task').removeClass('hidden');
				break;

			case 'training':
				$('#training-task').removeClass('hidden');
				break;

			case 'other':
				$('#other-task').removeClass('hidden');
				break;
		}
	}

	// Shows the sign out form for the specific sign in entry
	function showSignOutForm(elem) {

		$(elem).addClass('hidden');
		$(elem).closest('.sign-out-row').find('form').removeClass('hidden');

	}

	function hideSignOutForm(elem)
	{
		$(elem).closest('.sign-out-row').find('form').addClass('hidden');
		$('.show-sign-out-form').removeClass('hidden');
	}

	/**
	 * Signs the current user into the job.
	 * @param {any} jobMessageId
	 * @param {any} description
	 */
	function signInToJob(jobMessageId, description) {

		// Disable and set spinner
		$('.member-sign-in button').attr('disabled', 'disabled').addClass('disabled');
		$('.member-sign-in button i').removeClass('fa-sign-in').addClass('fa-spin fa-spinner');

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/sign-in',
			type: 'POST',
			dataType: 'json',
			data: { JobMessageId: jobMessageId, Description: description, SignInType: 1 },
			success: function (data) {

				if (data != null) {

					// Show the message and remove the button
					$('.member-sign-in button').remove();
					$('.member-sign-in').append('<div class="alert alert-success alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>You have been successfully signed in.</div>')

					// remove the no members row
					$('.no-members-signed-in').remove();

					// add the information to the table
					var row = $('<tr></tr>');
					row.append('<td>' + data.FullName + '</td>');
					row.append('<td>' + data.MemberNumber + '</td>');

					var signInDate = moment(data.SignInTime).local();
					row.append('<td>' + signInDate.format('YYYY-MM-DD HH:mm') + '</td>');

					// Add the row to the table
					$('#signed-in-members tbody').append(row);

					// increment the user count
					var memberCount = parseInt($('#tab-header-members .member-count').text());
					$('#tab-header-members .member-count').text(memberCount + 1);


				}
				else {
					$('.member-sign-in').append('<p class="text-danger">There was an error signing you into the job.</p>');
					$('.member-sign-in button').removeAttr('disabled').removeClass('disabled');
					$('.member-sign-in button i').removeClass('fa-spin fa-spinner').addClass('fa-sign-in');
				}

			},
			error: function (jqXHR, errorThrown, textStatus) {
				$('.member-sign-in').append('<p class="text-danger">There was an error signing you into the job.</p>');
				$('.member-sign-in button').removeAttr('disabled').removeClass('disabled');
				$('.member-sign-in button i').removeClass('fa-spin fa-spinner').addClass('fa-sign-in');
			}
		});

	}

	function bindUI()
	{

		// Bind the other type select box to determine if the other option description should be shown
		$('#SignInTypeOther').change(function () {

			if ($(this).val() == "99")
			{
				$('.other-type-other').removeClass('hidden');
			} else {
				$('.other-type-other').addClass('hidden');
			}

		});

	}

	// Bind ui
	bindUI();

	return {
		setOperationJobNumber: setOperationJobNumber,
		setActivityDetails: setActivityDetails,
		showSignOutForm: showSignOutForm,
		hideSignOutForm: hideSignOutForm,
		signInToJob: signInToJob
	}

})();