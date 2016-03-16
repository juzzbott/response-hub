responseHub.jobLog = (function () {

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

		var postData = {
			Body: jobNote,
			isWordback: isWordback
		};

		// Set the spinner
		$("#btnAddNote").find('i').removeClass('fa-comment-o').addClass('fa-refresh fa-spin');
		$("#btnAddNote").attr('disabled', 'disabled');

		var jobId = $('#Id').val();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/notes',
			type: 'POST',
			dataType: 'json',
			data: postData,
			success: function (data) {
		
				if (data == null) {
					// TODO: show front-end error.
				}
		
				var noteDate = moment(data.Date);

				var noteMarkup = buildJobNoteMarkup(data.Id, data.Body, noteDate.format('YYYY-MM-DD HH:mm:ss'), data.IsWordBack);
		
				$('#job-notes ul').prepend(noteMarkup);
				$('#job-notes').removeClass('hidden');
				$('#txtJobNote').val('');

				// Reset the button
				$("#btnAddNote").find('i').addClass('fa-comment-o').removeClass('fa-refresh fa-spin');

				// Clear any existing alerts
				$(".job-note-messages .alert").remove();
		
			},
			error: function (jqXHR, textStatus, errorThrown) {

				// Reset the button
				$("#btnAddNote").find('i').addClass('fa-comment-o').removeClass('fa-refresh fa-spin');
				$("#btnAddNote").removeAttr('disabled');

				// Clear any existing alerts
				$(".job-note-messages .alert").remove();

				// Show the error message
				$(".job-note-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding your note. Please try again shortly.</p>');

			}
		});

	}

	/**
	 * Builds the markup for the job note to be added to the page.
	 */
	function buildJobNoteMarkup(noteId, body, date, isWordback) {

		// Create the note list item
		var noteListItem = $('<li data-job-note-id="' + noteId + '"></li>');

		// Generate wordback markup if required
		var wordbackMarkup = isWordback ? ' <i class="fa fa-commenting-o wordback-icon"></i> wordback' : '';

		// Append the meta and note information to the list item
		noteListItem.append('<small class="text-muted"><i class="fa fa-clock-o"></i> ' + date + wordbackMarkup + '</small>');
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
		$(elem).append('<span class="btn-icon"><i class="fa fa-fw fa-clock-o"></i>' + date.format('YYYY-MM-DD HH:mm') + '</span><br />');
		$(elem).append('<span class="text-muted btn-icon"><i class="fa fa-fw fa-user"></i>' + userFullName + '</span>');

	}

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

	}

	// Bind the UI
	bindUI();

})();