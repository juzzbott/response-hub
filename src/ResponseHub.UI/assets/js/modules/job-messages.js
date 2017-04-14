responseHub.jobMessages = (function () {

	/**
	 * Sets the job type of the job from the specifed jobType parameter.
	 */
	function setJobType(jobType) {

		var jobTypeCss = jobType.replace(/\s/g, '-').toLowerCase();

		$('.job-types').addClass('hidden');
		$('.cancel-new-job').addClass('hidden');

		$('#job-title-banner').removeClass('hidden');
		$('#job-title-banner div').addClass(jobTypeCss);
		$('#job-title-banner h2').text(jobType + ' job');

		$('#job-details-form').removeClass('hidden');

	}

	/**
	 * Adds the job note to the job and displays on the screen.
	 */
	function addJobNote() {

		var jobNote = $('#txtJobNote').val();
		var isWordback = $('#chkWordBack').is(':checked');
		var userDisplayName = $('#note-form').data('user-display-name');

		var postData = {
			Body: jobNote,
			isWordback: isWordback
		};

		// Set the spinner
		$("#btnAddNote").find('i').removeClass('fa-comment-o').addClass('fa-refresh fa-spin');
		$("#btnAddNote").attr('disabled', 'disabled');

		var jobId = $('#Id').val();

		// Clear any existing alerts
		$(".job-note-messages .alert").remove();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/notes',
			type: 'POST',
			dataType: 'json',
			data: postData,
			success: function (data) {

				if (data == null) {
					$(".job-note-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding your note. Please try again shortly.</p>');
					return;
				}

				var noteDate = moment(data.Date);

				var noteMarkup = buildJobNoteMarkup(data.Id, data.Body, noteDate.format('YYYY-MM-DD HH:mm:ss'), data.IsWordBack, userDisplayName);

				$('#job-notes ul').prepend(noteMarkup);
				$('#job-notes').removeClass('hidden');

			},
			error: function (jqXHR, textStatus, errorThrown) {

				// Show the error message
				$(".job-note-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding your note. Please try again shortly.</p>');

			},
			complete: function (jqXHR, textStatus) {

				// Reset the button
				$("#btnAddNote").find('i').addClass('fa-comment-o').removeClass('fa-refresh fa-spin');
				$('#txtJobNote').val('');

			}
		});

	}

	/**
	 * Builds the markup for the job note to be added to the page.
	 */
	function buildJobNoteMarkup(noteId, body, date, isWordback, userDisplayName) {

		// Create the note list item
		var noteListItem = $('<li data-job-note-id="' + noteId + '"></li>');

		// Generate wordback markup if required
		var wordbackMarkup = isWordback ? ' <i class="fa fa-commenting-o wordback-icon"></i> wordback' : '';

		var userDisplayNameMarkup = ' <i class="fa fa-user user-icon"></i> ' + userDisplayName;

		// Append the meta and note information to the list item
		noteListItem.append('<small class="text-muted"><i class="fa fa-clock-o"></i> ' + date + wordbackMarkup + userDisplayNameMarkup + '</small>');
		noteListItem.append('<p class="text-info">' + body + '</p>');

		// return the node list item
		return noteListItem;

	}

	/**
	 * Sets the job status when the button is clicked.
	 */
	function setJobStatusTime(statusType, progressDateTime, sender) {

		var intStatusType;

		switch (statusType) {

			case "on_route":
				intStatusType = 1;
				break;

			case "on_scene":
				intStatusType = 2;
				break;

			case "job_clear":
				intStatusType = 3;
				break;

		}

		// Set the spinner
		if (sender != null) {
			$(sender).find('i').removeClass('fa-check-square-o').addClass('fa-refresh fa-spin');
			$(sender).attr('disabled', 'disabled');
		}

		var jobId = $('#Id').val();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/progress',
			type: 'POST',
			dataType: 'json',
			data: { ProgressType: intStatusType, ProgressDateTime: progressDateTime, Version: parseInt($('#Version').val()) },
			success: function (data) {

				// If there is a failed result, display that
				if (data.Success == true) {

					var progressDate = moment(data.Timestamp).local();

					switch (statusType) {

						case "on_route":
							addProgressMarkup($('.progress-on-route'), "On route", progressDate, data.UserFullName);
							break;

						case "on_scene":
							addProgressMarkup($('.progress-on-scene'), "On scene", progressDate, data.UserFullName);
							break;

						case "job_clear":
							addProgressMarkup($('.progress-job-clear'), "Job clear", progressDate, data.UserFullName);
							break;

					}

					// Update the version
					$('#Version').val(data.NewVersion);

					if (sender != null) {
						$(sender).remove();
					}
					else {
						// Sender is null, so sender is actually the edit form, so we want to close the form
						closeEditProgressForm();
					}

				} else {

					// Reset the button
					if (sender != null) {
						$(sender).find('i').addClass('fa-check-square-o').removeClass('fa-refresh fa-spin');
						$(sender).removeAttr('disabled');
					}
					else 
					{
						// Sender is null, so sender is actually the edit form, so we want to close the form
						closeEditProgressForm();
					}

					// Clear any existing alerts
					$(".progess-messages .alert").remove();

					// Display the error message
					$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.ErrorMessage + '</p>');

				}

			},
			error: function (jqXHR, textStatus, errorThrown) {
				// Reset the button

				if (sender != null) {
					$(sender).find('i').addClass('fa-check-square-o').removeClass('fa-refresh fa-spin');
					$(sender).removeAttr('disabled');
				}
				else {
					// Sender is null, so sender is actually the edit form, so we want to close the form
					closeEditProgressForm();
				}

				// Clear any existing alerts
				$(".progess-messages .alert").remove();

				// SHow the error message
				$(".progess-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error saving the job message progress.</p>');
			}
		});

	}

	/**
	 * Adds the progress markup to the dom.
	 */
	function addProgressMarkup(elem, progressType, date, userFullName) {

		// empty the current markup to prevent duplicate markup entries
		$(elem).empty();

		$(elem).append("<h4>" + progressType + "</h4>");
		$(elem).append('<span class="btn-icon"><i class="fa fa-fw fa-clock-o"></i>' + date.format('YYYY-MM-DD HH:mm:ss') + '</span><br />');
		$(elem).append('<span class="text-muted btn-icon"><i class="fa fa-fw fa-user"></i>' + userFullName + '</span>');
		$(elem).append('<div><a class="btn btn-link btn-icon action-edit"><i class="fa fa-fw fa-pencil-square-o"></i>Edit</a><a class="btn btn-link btn-icon action-undo"><i class="fa fa-fw fa-undo"></i>Undo</a></div>');

		// Rebind the edit and undo options
		bindJobProgressEditUndo();

	}

	/**
	 * Sends the updated progress type to the server.
	 */
	function submitEditProgressTime()
	{

		// Get the specified time
		var dateVal = $('#EditProgressDate').val() + " " + $('#EditProgressTime').val();

		// Get the progress type
		var progressType = $('#ProgressType').val();

		// Set the button sending status
		$('#edit-progress-update button').addClass('disabled');
		$('#edit-progress-update button').attr('disabled', 'disabled');
		$('#edit-progress-update button i').removeClass('fa-check').addClass('fa-spinner fa-spin');


		// Submit the details of the update
		setJobStatusTime(progressType, dateVal, null);


	}

	function closeEditProgressForm()
	{
		$('#edit-progress-update').addClass('hidden');
		$('#edit-progress-update button').removeClass('disabled');
		$('#edit-progress-update button').removeAttr('disabled');
		$('#edit-progress-update button i').removeClass('fa-spinner').removeClass('fa-spin').addClass('fa-check');
	}

	/**
	 * Gets the next set of pager messages to display.
	 */
	function getNextJobMessages(messageType) {

		// Get the skip count
		var skipCount = $(".job-list li").length;

		$('#jobs-load-more .loading').removeClass('hidden');
		$('#jobs-load-more button').addClass('hidden');

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/?skip=' + skipCount + '&msg_type=' + messageType,
			dataType: 'json',
			success: function (data) {

				for (var i = 0; i < data.length; i++) {

					addMessageToList(data[i], true);

				}

				// If there is zero results, hide the show more button
				if (data.length == 0) {
					$("#jobs-load-more").remove();
				} else {
					$('#jobs-load-more .loading').addClass('hidden');
					$('#jobs-load-more button').removeClass('hidden');
				}

			}
		});

	}

	/**
	 * Adds the job message to the list of existing job messages.
	 * @param {any} pagerMessage
	 * @param {any} append
	 */
	function addMessageToList(jobMessage, append) {

		var listItem = $('<li data-message-id="' + jobMessage.Id + '"></li>');

		// Get the job date
		var jobDate = moment(jobMessage.Timestamp);
		var localDateString = jobDate.local().format('DD/MM/YYYY HH:mm:ss');

		// Create the header
		var header = $('<h3></h3>');

		// Set the priority
		// Add the priority icon
		switch (jobMessage.Priority) {
			case 1:
				header.append('<i class="fa fa-fw fa-exclamation-triangle p-message-emergency"></i>');
				break;

			case 2:
				header.append('<i class="fa fa-fw fa-exclamation-circle p-message-non-emergency"></i>');
				break;

			default:
				header.append('<i class="fa fa-fw fa-info-circle p-message-admin"></i>');
				break;
		}

		// If there is job number, then show it here
		if (jobMessage.JobNumber != "") {
			header.append('<a href="/jobs/' + jobMessage.Id + '">' + jobMessage.JobNumber + '</a><span class="text-muted message-date btn-icon">' + localDateString + ' </span>');
		}
		else {
			header.append('<a href="/jobs/' + jobMessage.Id + '">' + localDateString + '</a>');
		}

		// Add the header
		listItem.append(header);

		// Add the job meta data
		var metaContainer = $('<p class="job-message-meta text-muted"></p>');
		var statusSpan = $('<span class="job-status"></span>');

		// Set the job status
		if (jobMessage.Cancelled != null)
		{
			statusSpan.append('<i class="fa fa-ban"></i>');
		}
		else if (jobMessage.JobClear != null)
		{
			statusSpan.append('<i class="fa fa-check-circle-o"></i>');
		}
		else if (jobMessage.OnScene != null)
		{
			statusSpan.append('<i class="fa fa-hourglass-half"></i>');
		}
		else if (jobMessage.OnRoute != null)
		{
			statusSpan.append('<i class="fa fa-arrow-circle-o-right"></i>');
		}
		else
		{
			statusSpan.append('<i class="fa fa-asterisk"></i>');
		}

		// Add the job status span to the metadata container
		metaContainer.append(statusSpan);

		// Add the group capcode name
		metaContainer.append('<span class="capcode-group-name">' + jobMessage.CapcodeGroupName + '</span>');

		// Append the job message meta
		listItem.append(metaContainer);

		// Append the pager message content
		listItem.append($('<p>' + jobMessage.MessageBody + '</p>'));

		// Append the list item to list of jobs
		if (append) {
			$(".job-list").append(listItem);
		} else {
			$(".job-list").prepend(listItem);
		}

	}

	/**
	 * Gets the distance between two jobs, and displays to the user.
	 */
	function getDistanceBetweenJobs()
	{

		// Ensure the form is valid
		if (!$('#dist-between-jobs form').valid())
		{
			return;
		}
		
		// Disable the button to start with
		$('#dist-between-jobs button').attr('disabled', 'disabled');
		$('#dist-between-jobs button').addClass('disabled');
		$('#dist-between-jobs button i').removeClass('fa-search').addClass('fa-spin fa-spinner');

		// Get the current job id and referenced job number
		var currentJobId = $('#Id').val();
		var referencedJobNumber = $('#DistanceFromJobNumber').val();

		// Clear any previous results
		$('#dist-results').empty();


		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + currentJobId + '/distance-from-job/' + referencedJobNumber,
			dataType: 'json',
			success: function (data) {

				if (data != null)
				{

					// Is not successull, then display error message
					if (!data.Success)
					{
						$('#dist-results').append('<div class="alert alert-danger alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.Error + '</div>');
					}
					else
					{
						$('#dist-results').append('<h3>Distance results:</h4>');
						$('#dist-results').append('<p>Distance to job <strong><a href="/jobs/' + data.ReferencedJobId + '">' + data.ReferencedJobNumber + '</a></strong>: ' + (Math.round((data.Distance / 1000) * 10) / 10) + ' km');
					}

				}

			}, 
			error: function (jqXHR, textStatus, errorThrown) {
				$('#dist-results').append('<div class="alert alert-danger alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>There was an error determining the distance between jobs.</div>');
			},
			complete: function () {
				// Re-enable the button to start with
				$('#dist-between-jobs button').removeAttr('disabled');
				$('#dist-between-jobs button').removeClass('disabled');
				$('#dist-between-jobs button i').removeClass('fa-spin fa-spinner').addClass('fa-search');
			}
		});

	}

	/**
	 * Binds the job progress actions
	 */
	function bindJobProgressEditUndo()
	{

		// Unbind all click events
		$('.progress-action .action-edit').off('click');
		$('.progress-action .action-undo').off('click');

		// Set the click event handler for the progress action
		$('.progress-action .action-edit').click(function () {
			
			// From the undo link, get the progress action container
			var progressAction = $(this).closest('.progress-action');

			// Get the progress type from the data attribute
			var progressType = progressAction.data('progress-type');

			// Set the progress type hidden value and the h3 title
			$('#ProgressType').val(progressType);

			headerText = "Update progress";
			if (progressType == "on_route")
			{
				headerText = "Update on route time";
			}
			else if (progressType == "on_scene")
			{
				headerText = "Update on scene time";
			}
			else if (progressType == "job_clear")
			{
				headerText = "Update job clear time";
			}

			// Show the update form.
			var currentDate = moment().local();
			$('#EditProgressDate').val(currentDate.format('YYYY-MM-DD'));
			$('#EditProgressTime').val(currentDate.format('HH:mm:ss'));
			$('#edit-progress-update h4').text(headerText);
			$('#edit-progress-update').removeClass('hidden');

			// If we are on mobile, then scroll to the form
			if (responseHub.isMobile()) {
				$(window).scrollTop($("#edit-progress-update").offset().top - 50);
			}

		});

		// Bind the 'undo' event
		$('.progress-action .action-undo').click(function () {

			var intStatusType;
			
			// From the undo link, get the progress action container
			var progressAction = $(this).closest('.progress-action');

			// Get the progress type from the data attribute
			var progressType = progressAction.data('progress-type');

			switch (progressType) {

				case "on_route":
					intStatusType = 1;
					break;

				case "on_scene":
					intStatusType = 2;
					break;

				case "job_clear":
					intStatusType = 3;
					break;

			}

			// Get the job id
			var jobId = $('#Id').val();

			// Get the element from the curret click event
			var elem = $(this);

			// Disable and set spinner
			$(elem).attr('disabled', 'disabled');
			$(elem).addClass('disabled');
			$(elem).find('i').removeClass('fa-undo').addClass('fa-spinner fa-spin');

			// Close the edit form just in case it's for the same type
			closeEditProgressForm();

			// Create the ajax request
			$.ajax({
				url: responseHub.apiPrefix + '/job-messages/' + jobId + '/progress/delete',
				type: 'POST',
				dataType: 'json',
				data: { ProgressType: intStatusType, Version: parseInt($('#Version').val()) },
				success: function (data) {

					// If there is a failed result, display that
					if (data.Success == true) {

						// Remove the progress details
						progressAction.find('.progress-time').empty();

						// Get the button label
						var buttonLabel = '';
						var buttonClass = '';
						if (progressType == "on_route") {
							buttonLabel = 'On route';
							buttonClass = 'btn-on-route';
						}
						else if (progressType == "on_scene") {
							buttonLabel = 'On scene';
							buttonClass = 'btn-on-scene';
						}
						else if (progressType == "job_clear") {
							buttonLabel = 'Job clear';
							buttonClass = 'btn-job-clear';
						}

						// Add the progress button back in
						progressAction.append('<button class="btn btn-primary btn-icon btn-block btn-lg ' + buttonClass + '"><i class="fa fa-fw fa-check-square-o"></i> ' + buttonLabel + '</button>');

						// Rebind the progress actions
						bindJobProgressActions();

						// Update the version
						$('#Version').val(data.NewVersion);

					}
					else
					{

						// Reset the button
						$(elem).removeAttr('disabled');
						$(elem).removeClass('disabled');
						$(elem).find('i').removeClass('fa-spinner fa-spin').addClass('fa-undo');

						// Clear any existing alerts
						$(".progess-messages .alert").remove();

						// Display the error message
						$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.ErrorMessage + '</p>');

					}

				}, 
				error: function (jqXHR, textStatus, errorThrown)
				{
					// Re-enable spinner
					$(progressAction).find('.action-undo').removeAttr('disabled');
					$(progressAction).find('.action-undo').removeClass('disabled');
					$(progressAction).find('.action-undo i').removeClass('fa-spinner').removeClass('fa-spin').addClass('fa-undo');
					
					// Clear any existing alerts
					$(".progess-messages .alert").remove();

					// Display the error message
					$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error clearing job progress.</p>');

				}
			});

		});

	}

	/**
	 * Bind the job progress button events.
	 */
	function bindJobProgressActions()
	{

		$(".btn-on-route").off('click');
		$(".btn-on-route").click(function () {
			setJobStatusTime('on_route', "", this);
		});

		$(".btn-on-scene").off('click');
		$(".btn-on-scene").click(function () {
			setJobStatusTime('on_scene', "", this);
		});

		$(".btn-job-clear").off('click');
		$(".btn-job-clear").click(function () {
			setJobStatusTime('job_clear', "", this);
		});

	}

	/**
	 * Binds the UI controls
	 */
	function bindUI() {

		// Bind the progress update actions
		bindJobProgressActions();
		bindJobProgressEditUndo();

		$('#btnAddNote').click(function () {
			addJobNote();
		});

		$('#txtJobNote').on('keyup', function () {

			if ($(this).val() == '') {
				$('#btnAddNote').attr('disabled', 'disabled');
			} else {
				$('#btnAddNote').removeAttr('disabled');
			}

		});

		$('#confirm-delete.delete-attachment').on('show.bs.modal', function (e) {

			// Generate the confirm message
			var message = $(this).find('.modal-body p').text();
			message = message.replace('#FILE#', $(e.relatedTarget).data('filename'));

			// Set the confirm message
			$(this).find('.modal-body p').text(message);

		});

		// If we are on the job list page, then load the next jobs
		if ($('#jobs-list-container').length > 0)
		{
			getNextJobMessages('job');
		}
		else if ($('#message-list-container').length > 0) {
			// If we are on the job list page, then load the next jobs
			getNextJobMessages('message');
		}
	}

	// Bind the UI
	bindUI();

	return {
		getNextJobMessages: getNextJobMessages,
		submitEditProgressTime: submitEditProgressTime,
		closeEditProgressForm: closeEditProgressForm,
		getDistanceBetweenJobs: getDistanceBetweenJobs
	};

})();