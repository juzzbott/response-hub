var responseHub = (function () {

	// The API prefix url.
	var apiPrefix = '/api';

	/**
	 * Determines if the site is a mobile site or not.
	 */
	function isMobile() {

		// Determine if the site is mobile or not
		if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|Windows Phone/i.test(navigator.userAgent)) {
			return true;
		} else {
			return false;
		}

	}

	function executeFunctionByName(functionName, context /*, args */) {
		var args = [].slice.call(arguments).splice(2);
		var namespaces = functionName.split(".");
		var func = namespaces.pop();
		for (var i = 0; i < namespaces.length; i++) {
			context = context[namespaces[i]];
		}
		return context[func].apply(this, args);
	}

	// return the response hub object
	return {
		apiPrefix: apiPrefix,
		isMobile: isMobile,
		executeFunctionByName: executeFunctionByName
	}

})();
;
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

		// Create the initial job
		createJob(jobType);

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

		var jobId = $('#hdnJobId').val();

		// Create the ajax request
		$.ajax({
			url: jobCard.apiPrefix + '/jobs/' + jobId + '/notes',
			type: 'POST',
			dataType: 'json',
			data: postData,
			success: function (data) {

				if (data == null) {
					// TODO: show front-end error.
				}

				var noteDate = moment(data.Date);

				var noteMarkup = buildJobNoteMarkup(data.Id, data.Body, noteDate.format('YYYY-MM-DD HH:mm:ss'), data.isWordback);

				$('#job-notes ul').prepend(noteMarkup);
				$('#job-notes').removeClass('hidden');

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
	 * Calls the API to create the job with the specified type.
	 * Once the job has been created, the jobId field is updated.
	 */
	function createJob(jobType) {

		// Submit the POST api call.
		$.ajax({
			url: jobCard.apiPrefix + '/jobs',
			type: 'POST',
			dataType: 'json',
			data: { '': jobType },
			success: function (data) {

				if (data != null) {
					$('#hdnJobType').val(data.Type);
					$('#hdnJobId').val(data.Id);
				}

			}
		});

	}

	function updateJobDetails() {

		var updateDetails = {
			Name: $('#txtJobName').val(),
			OnRoute: '',
			OnScene: '',
			JobClear: ''
		}

	}

	function bindUI() {

		$('.btn.start-new-job').click(function () {
			$('.new-job').addClass('hidden');
			$('.job-types').removeClass('hidden');
			$('.cancel-new-job').removeClass('hidden');
		});

		$('.cancel-new-job .btn').click(function () {
			$('.new-job').removeClass('hidden');
			$('.job-types').addClass('hidden');
			$('.cancel-new-job').addClass('hidden');
		});

		$('.job-types button').click(function () {
			
			var jobType = $(this).data('job-type');
			setJobType(jobType);

		});

		$('#btnAddNote').click(function () {
			addJobNote();
		});

	}

	// Bind the UI
	bindUI();

})();