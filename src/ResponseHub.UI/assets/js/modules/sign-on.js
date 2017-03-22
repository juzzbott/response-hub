responseHub.signOn = (function () {

	// Sets the job number and the job id when selected.
	function setOperationJobNumber(jobNumber, jobId) {

		// Set the jbo number and id in the hidden field
		$('#OperationDescription').val(jobNumber);
		$('#OperationJobId').val(jobId);
	}

	// Shows the elements for operation details
	function showOperationDetails() {
		$('#operation-task').removeClass('hidden');
		$('#training-task').addClass('hidden');
	}

	// Shows the elements for training details
	function showTrainingDetails() {
		$('#training-task').removeClass('hidden');
		$('#operation-task').addClass('hidden');
	}

	// Binds the UI elements.
	function bindUI() {

		// Set the StartTime time picker
		if ($('body.sign-on').length > 0) {	
			console.log('sign-on');
			$('#StartTime').datetimepicker({
				format: 'HH:mm',
				icons: {
					time: 'fa fa-fw fa-clock-o',
					date: 'fa fa-fw fa-calendar',
					up: 'fa fa-fw fa-chevron-up',
					down: 'fa fa-fw fa-chevron-down',
					previous: 'fa fa-fw fa-chevron-left',
					next: 'fa fa-fw fa-chevron-right',
					today: 'fa fa-fw fa-bullseye',
					clear: 'fa fa-fw fa-trash-o',
					close: 'fa fa-fw fa-times'
				}
			});
		}

	}

	// Bind the UI
	bindUI();

	return {
		setOperationJobNumber: setOperationJobNumber,
		showOperationDetails: showOperationDetails,
		showTrainingDetails: showTrainingDetails
	}

})();