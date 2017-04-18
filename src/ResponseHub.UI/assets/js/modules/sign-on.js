responseHub.signIn = (function () {

	// Sets the job number and the job id when selected.
	function setOperationJobNumber(jobNumber, jobId) {

		// Set the jbo number and id in the hidden field
		$('#OperationDescription').val(jobNumber);
		$('#OperationJobId').val(jobId);
	}

	// Shows the elements for operation details
	function showOperationDetails() {
		$('#operation-task').removeClass('hidden');
	}

	// Hides the elements for operation details
	function hideOperationDetails() {
		$('#operation-task').addClass('hidden');
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

	return {
		setOperationJobNumber: setOperationJobNumber,
		showOperationDetails: showOperationDetails,
		hideOperationDetails: hideOperationDetails,
		showSignOutForm: showSignOutForm,
		hideSignOutForm: hideSignOutForm
	}

})();