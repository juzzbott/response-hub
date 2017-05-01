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
		hideSignOutForm: hideSignOutForm
	}

})();