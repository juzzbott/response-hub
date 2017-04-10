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
	function setJobStatusTime(statusType, sender) {

		var intStatusType;

		switch (statusType) {

			case "on-route":
				intStatusType = 1;
				break;

			case "on-scene":
				intStatusType = 2;
				break;

			case "job-clear":
				intStatusType = 3;
				break;

		}

		// Set the spinner
		$(sender).find('i').removeClass('fa-check-square-o').addClass('fa-refresh fa-spin');
		$(sender).attr('disabled', 'disabled');

		var jobId = $('#Id').val();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/progress',
			type: 'POST',
			dataType: 'json',
			data: { '': intStatusType },
			success: function (data) {

				// If there is a failed result, display that
				if (data.Success == true) {

					var progressDate = moment(data.Timestamp).local();

					switch (statusType) {

						case "on-route":
							addProgressMarkup($('.progress-on-route'), "On route", progressDate, data.UserFullName);
							break;

						case "on-scene":
							addProgressMarkup($('.progress-on-scene'), "On scene", progressDate, data.UserFullName);
							break;

						case "job-clear":
							addProgressMarkup($('.progress-job-clear'), "Job clear", progressDate, data.UserFullName);
							break;

					}

					$(sender).remove();

				} else {

					// Reset the button
					$(sender).find('i').addClass('fa-check-square-o').removeClass('fa-refresh fa-spin');
					$(sender).removeAttr('disabled');

					// Clear any existing alerts
					$(".progess-messages .alert").remove();

					// Display the error message
					$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.ErrorMessage + '</p>');

				}

			},
			error: function (jqXHR, textStatus, errorThrown) {
				// Reset the button
				$(sender).find('i').addClass('fa-check-square-o').removeClass('fa-refresh fa-spin');
				$(sender).removeAttr('disabled');

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

		$(elem).append("<h4>" + progressType + "</h4>");
		$(elem).append('<span class="btn-icon"><i class="fa fa-fw fa-clock-o"></i>' + date.format('YYYY-MM-DD HH:mm:ss') + '</span><br />');
		$(elem).append('<span class="text-muted btn-icon"><i class="fa fa-fw fa-user"></i>' + userFullName + '</span>');

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
	 * Binds the UI controls
	 */
	function bindUI() {

		$(".btn-on-route").click(function () {
			setJobStatusTime('on-route', this);
		});

		$(".btn-on-scene").click(function () {
			setJobStatusTime('on-scene', this);
		});

		$(".btn-job-clear").click(function () {
			setJobStatusTime('job-clear', this);
		});

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
		getNextJobMessages: getNextJobMessages
	};

})();