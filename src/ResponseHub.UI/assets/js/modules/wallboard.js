responseHub.wallboard = (function () {

	var jobListPollingInterval = 30000;

	var jobListPollingEnabled = true;

	var jobListIntervalId = null;

	var selectedJobIndex = 0;

	/**
	 * Gets the height of the containers for the screen size.
	 */
	function getContainerHeight(width) {

		// If the width is < 768 then just set the heights to auto
		if (width < 768) {
			$('.wallboard-sidebar, .wallboard-main, .wallboard-warnings').css('height', 'auto');
			return;
		}

		// Set the heights of the main containers
		var headerHeight = $('.page-navbar').height();
		var containerHeight = ($(window).height() - headerHeight);

		// return the height
		return containerHeight;

	}

	/**
	 * Sets the container height for the columns. Only for devices above mobile.
	 */
	function setContainerHeights(width) {

		var containerHeight = getContainerHeight(width);

		// Set the height of the columns.
		$('.wallboard-sidebar, .wallboard-main, .wallboard-warnings').height(containerHeight);

	}

	/**
	 * Method that is called when a job is selected from the left list for the deatils to be displayed in the center column.
	 */
	function selectMessage(elemIndex) {

		// Set the index
		selectedJobIndex = elemIndex;

		// Get the element at the index
		var elem = $('.wallboard-layout .message-list li').get(elemIndex);

		var isFirstSelected = $('.wallboard-layout .message-list').first().hasClass('selected');

		// If the message details markup does not exist, we need to build it
		if ($('.wallboard-layout .message-details h2').length == 0) {
			buildMessageDetailsMarkup();
		} else if (isFirstSelected) {
			// If the current element is already selected, just return
			return;
		}

		// Get the job id
		var jobId = $(elem).data('id');

		// Get the progress information from the data attribute. It's a JSON object.
		var latestProgress = null;
		$(".wallboard-main .job-status").html('');
		if ($(elem).data('progress') != "") {
			var progressData = $(elem).data('progress');

			// Format the progress dae
			var progressTimestamp = moment(progressData.Timestamp).format('DD-MM-YYYY HH:mm:ss');

			if (progressData.ProgressType == 4) {
				$(".wallboard-main .job-status").html('<span class="job-cancelled"><i class="fa fa-fw fa-ban"></i>Job cancelled by ' + progressData.UserFullName + ' on ' + progressTimestamp + '</span>');
			} else if (progressData.ProgressType == 3) {
				$(".wallboard-main .job-status").html('<span class="job-clear"><i class="fa fa-fw fa-check-circle-o"></i>Job cleared by ' + progressData.UserFullName + ' on ' + progressTimestamp + '</span>');
			} else if (progressData.ProgressType == 2) {
				$(".wallboard-main .job-status").html('<span class="on-scene"><i class="fa fa-fw fa-hourglass-half"></i>On scene by ' + progressData.UserFullName + ' on ' + progressTimestamp + '</span>');
			} else if (progressData.ProgressType == 1) {
				$(".wallboard-main .job-status").html('<span class="on-route"><i class="fa fa-fw fa-arrow-circle-o-right"></i>On route by ' + progressData.UserFullName + ' on ' + progressTimestamp + '</span>');
			}

		}

		// Set the message text, date and job number
		var jobNumber = $(elem).data('job-number');
		if (jobNumber == "") {
			jobNumber = "<em>No job number</em>";
		}
		$('.wallboard-main .job-number').html(jobNumber);
		$('.wallboard-main .job-message-body').text($(elem).data('message'));
		$('.wallboard-main .job-date').text($(elem).data('date'));

		$('#message-type').attr('class', '');
		if ($(elem).data('priority') == '1') {
			$('#message-type').attr('class', 'fa fa-exclamation-triangle p-message-emergency');
		} else if ($(elem).data('priority') == '2') {
			$('#message-type').attr('class', 'fa fa-exclamation-circle p-message-non-emergency');
		} else {
			$('#message-type').attr('class', 'fa fa-info-circle p-message-admin');
		}

		// Set the map reference
		var mapRef = $(elem).data('map-ref');
		var address = $(elem).data('address');

		if (mapRef != "") {
			if (address != "" && address != "undefined") {
				$('.wallboard-main .map-reference').text(address);
				$('.wallboard-main .map-reference').append('<span class="small show">' + mapRef + '</span>');
			} else {
				$('.wallboard-main .map-reference').text(mapRef);
			}
			$('.wallboard-main .job-location').removeClass('hidden');
		} else {
			$('.wallboard-main .job-location').addClass('hidden');
		}

		var lat = parseFloat($(elem).data('lat'));
		var lon = parseFloat($(elem).data('lon'));

		if (lat != 0 && lon != 0) {

			// Set the height of the map canvas
			$('#map-canvas').css('height', '300px');

			if (!responseHub.maps.mapExists()) 
			{
				var mapConfig = {
					lat: lat,
					lon: lon,
					zoom: 15,
					minZoom: 4,
					scrollWheel: true,
					mapContainer: 'map-canvas',
					loadCallback: null
				};
				responseHub.maps.displayMap(mapConfig);
			}
			else
			{
				responseHub.maps.setMapCenter(lat, lon);
			}

			responseHub.maps.clearMarkers();
			responseHub.maps.addMarkerToMap(lat, lon);

		} else {
			$('#map-canvas').css('height', '0px');
		}

		// Load the notes
		loadJobNotes(jobId);

		// Set the active class on the list item
		$('.wallboard-layout .message-list li').removeClass('selected');
		$(elem).addClass('selected');

	}

	/**
	 * Load job notes
	 */
	function loadJobNotes(jobId) {

		// Show the loading notes
		$(".notes-loading").removeClass("hidden");

		// Clear the loading notes
		$("ul.job-notes").empty();
		$('.no-notes-msg').remove();

		// Hide the loading
		$(".notes-loading").addClass("hidden");

		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/notes',
			dataType: 'json',
			success: function (data) {
				
				if (data == null || data.length == 0) {

					$(".job-notes-container").append('<p class="no-notes-msg">No notes available.</p>');

				} else {

					for (var i = 0; i < data.length; i++) {

						// Add the notes to the list
						var listItem = $("<li></li>");

						// Create the note details
						var noteInfo = $('<small class="text-muted"></small>');

						// Add the note time
						var noteDate = moment(data[i].Timestamp);
						var localDateString = noteDate.format('YYYY-MM-DD HH:mm:ss');
						noteInfo.append('<i class="fa fa-clock-o"></i> ' + localDateString);

						// Add the wordback details
						if (data[i].IsWordBack) {
							noteInfo.append('<i class="fa fa-commenting-o wordback-icon"></i> wordback');
						}

						// Add the user
						noteInfo.append('<i class="fa fa-user user-icon"></i> ' + data[i].UserDisplayName);

						// append the note details
						listItem.append(noteInfo);

						// Add the message body
						listItem.append('<p class="text-info">' + data[i].Body + '</p>');
						
						// Add the item to the start of the list, so newest notes are first
						$("ul.job-notes").prepend(listItem);

					}
				}

			}
		});

	}

	/**
	 * Builds the markup required for the message details to be displayed.
	 */
	function buildMessageDetailsMarkup() {

		// Build the header row
		var messageHeader = $('<div class="row"></div></div>')

		// Build the h2 elements
		var h2 = $('<div class="col-sm-5"><h2><i id="message-type"></i><span class="job-number"></span></h2><p class="job-status"></p></div>');
		var h2Date = $('<div class="col-sm-7"><p class="text-right job-date"></p></div>');

		messageHeader.append(h2);
		messageHeader.append(h2Date);

		// Build the location element
		var location = $('<div class="job-location"><h3>Location</h3><p class="lead map-reference"></p><div id="map-canvas" style="height: 0px;"></div></div>');

		// Clear the message detail of any other elements
		$('.wallboard-layout .message-details').empty();
		$('.wallboard-layout .message-details').append(messageHeader);
		$('.wallboard-layout .message-details').append($('<p class="lead job-message-body"></p>'));
		$('.wallboard-layout .message-details').append(location);
		$('.wallboard-layout .message-details').append($('<h3>Notes</h3><p class="notes-loading">Loading notes... <i class="fa fa-spinner fa-pulse fa-fw"></i></p><div class="job-notes-container"><ul class="job-notes list-unstyled"></ul></div>'))

		$('.job-notes-container').scrollator();
	}

	/**
	 * Binds the UI events
	 */
	function bindUI() {

		// Get the container height
		$('.wallboard-sidebar').scrollator();

		// If the window is resize, reset container heights (i.e. moving to fullscreen).
		$(window).resize(function (e) {
			setContainerHeights(e.target.innerWidth);
			$('.wallboard-sidebar').scrollator();
		});

		$('.wallboard-layout .message-list-options input').click(function () {

			// The is in this case is 'being check' not 'is checked'. So for 'is being checked' we load the job list, not 'is not being checked' we clear the interval.
			if ($(this).is(":checked")) {
				loadJobList();
				setJobListPolling();
			} else {
				clearInterval(jobListIntervalId);
			}
		});

		$('.wallboard-layout .message-list-options select').on('change', function () {
			// reset the interval to the new time
			clearInterval(jobListIntervalId);
			var delay = parseInt($(this).val()) * 1000;
			jobListPollingInterval = delay;
			setJobListPolling();
		});

	}

	/**
	 * Loads the UI of the wallboard. Sets the column heights dynamically and loads the jobs and radar images.
	 */
	function loadUI() {

		// If the body class does not contain the 'wallboard-layout' class, exit as we aren't in wallboard view.

		if (!$('body').hasClass('wallboard-layout')) {
			return;
		}

		// Initially set the container heights
		setContainerHeights($(window).width());

		// Load the jobs list
		loadJobList();

	}

	/**
	 * Calls the jobs api to load the list of most recent jobs. 
	 */
	function loadJobList() {

		$.ajax({
			url: responseHub.apiPrefix + '/job-messages',
			dataType: 'json',
			success: function (data) {

				// Populate the list of jobs
				$('.wallboard-layout .message-list').empty();
				$('.wallboard-layout .message-list-options .checkbox i').remove();

				// If there is no elements, show the no jobs messages
				if (data == null || data.length == 0) {

					$('.wallboard-layout .message-list').append($('<li><p class="lead no-jobs">No jobs are currently available.</p></li>'));
					$('.wallboard-layout .message-details').empty();
					$('.wallboard-layout .message-details').append($('<p class="lead top-20">No jobs are currently available.</p>'));

				} else {

					// loop through each job message and add to the list
					for (var i = 0; i < data.length; i++) {
						var jobMessage = data[i];
						buildJobListItem(jobMessage, i);
					}

					// Select the first list item.
					selectMessage(selectedJobIndex);
				}

			},
			error: function () {

				$('.wallboard-layout .message-list').empty();
				$('.wallboard-layout .message-list').append($('<li><p class="lead no-jobs text-danger">Error loading jobs.</p></li>'));
				$('.wallboard-layout .message-details').empty();
				$('.wallboard-layout .message-details').append($('<div class="alert alert-danger top-20" role="alert"><p>Error loading jobs.</p></div>'));

			},
			complete: function () {

				// Set the job list update interval to poll for new jobs only if it's not already set.
				// This is used to ensure once page has loaded and the inital call is called, then we do so at the interval, 
				// but only create a single interval object
				if (jobListIntervalId == null)
				{
					setJobListPolling();
				}

				// Set the list item click event for message list
				$('.wallboard-layout .message-list li').each(function (index) {
					$(this).click(function () {
						selectMessage(index);
					});
				});

				$('.wallboard-layout .message-list-options .checkbox i').remove();
			}
		});

	}

	/**
	 * Builds the markup to display the job message in the list of messages left in the left column.
	 */
	function buildJobListItem(jobMessage, index) {

		// Get the job date
		var jobDate = moment(jobMessage.Timestamp);
		var localDateString = jobDate.format('DD-MM-YYYY HH:mm:ss');

		var mapReference = "";
		var address = "";
		var lat = 0;
		var lon = 0;

		if (jobMessage.Location != null)
		{

			// If there is an address, then set that.
			if (jobMessage.Location.Address != null && jobMessage.Location.Address.FormattedAddress != "" && jobMessage.Location.Address.FormattedAddress != "undefined")
			{
				address = jobMessage.Location.Address.FormattedAddress;
			}

			// Set the map reference
			mapReference = jobMessage.Location.MapReference;

			if (jobMessage.Location.Coordinates != null)
			{
				lat = jobMessage.Location.Coordinates.Latitude;
				lon = jobMessage.Location.Coordinates.Longitude;
			}
		}

		// Get the latest priority json
		var latestProgress = "";

		if (jobMessage.Cancelled != null) {
			latestProgress = getJobProgressJson(jobMessage.Cancelled);
		} else if (jobMessage.JobClear != null) {
			latestProgress = getJobProgressJson(jobMessage.JobClear);
		} else if (jobMessage.OnScene != null) {
			latestProgress = getJobProgressJson(jobMessage.OnScene);
		} else if (jobMessage.OnRoute != null) {
			latestProgress = getJobProgressJson(jobMessage.OnRoute);
		}

		// Determine if the job has notes
		var hasNotes = (jobMessage.Notes != null && jobMessage.Notes.length > 0);

		// Creat the list item
		var listItem = $('<li class="' + (selectedJobIndex == index ? "selected" : "") + '" data-message="' + jobMessage.MessageBody + '" data-job-number="' + jobMessage.JobNumber +
			'" data-date="' + localDateString + '" data-priority="' + jobMessage.Priority + '" data-map-ref="' + mapReference + '" data-address="' + address + '" data-lat="' + lat + '" data-lon="' + lon +
			'" data-id="' + jobMessage.Id + '" data-progress="' + latestProgress + '" data-has-notes="' + (hasNotes ? 'true' : 'false') + '">');

		// Build the h3 tag
		var h3 = $('<h3></h3>');

		// Add the priority icon
		switch (jobMessage.Priority) {
			case 1:
				h3.append('<i class="fa fa-exclamation-triangle p-message-emergency"></i>');
				break;

			case 2:
				h3.append('<i class="fa fa-exclamation-circle p-message-non-emergency"></i>');
				break;

			default:
				h3.append('<i class="fa fa-info-circle p-message-admin"></i>');
				break;
		}

		// Add the job number
		h3.append('<span class="job-number">' + (jobMessage.JobNumber == "" ? "No job number" : jobMessage.JobNumber) + '</span>');

		// Add the progress indicator
		if (jobMessage.Cancelled != null) {
			h3.append('<span class="job-status job-cancelled"><i class="fa fa-ban"></i></span>');
		} else if (jobMessage.JobClear != null) {
			h3.append('<span class="job-status job-clear"><i class="fa fa-check-circle-o"></i></span>');
		} else if (jobMessage.OnScene != null) {
			h3.append('<span class="job-status on-scene"><i class="fa fa-hourglass-half"></i></span>');
		} else if (jobMessage.OnRoute != null) {
			h3.append('<span class="job-status on-route"><i class="fa fa-arrow-circle-o-right"></i></span>');
		} else {
			h3.append('<span class="job-status"><i class="fa fa-asterisk"></i></span>');
		}

		// Add the comments indication
		if (hasNotes) {
			h3.append('<span class="has-notes text-info"><i class="fa fa-fw fa-comments"></i></span>');
		} else {
			h3.append('<span class="has-notes text-muted"><i class="fa fa-fw fa-comments-o"></i></span>');
		}

		// Create the message element
		var messageElem = $('<div class="message"></div>');
		messageElem.append(h3);

		// Add the job date
		messageElem.append('<div class="message-meta"><p class="text-info message-date">' + localDateString + '</p></div>');

		// Add the message body
		messageElem.append('<small class="text-muted">' + jobMessage.MessageBodyTruncated + '</small>');
		
		listItem.append(messageElem);

		// Add the list item to the list object
		$('.wallboard-layout .message-list').append(listItem);

	}

	/**
	 * Gets the Json for passing the job progress details through to the display message method.
	 */
	function getJobProgressJson(jobProgress) {
		return '{ &quot;ProgressType&quot;: ' + jobProgress.ProgressType + ', &quot;Timestamp&quot;: &quot;' + jobProgress.Timestamp + '&quot;, &quot;UserFullName&quot;: &quot;' + jobProgress.UserFullName + '&quot;, &quot;UserId&quot;: &quot;' + jobProgress.UserId + '&quot; }';
	}

	/**
	 * Enables the polling for new jobs by setting the interval for the jobs api to be called.
	 */
	function setJobListPolling()
	{

		// Set the interval
		jobListIntervalId = setInterval(function () {

			// Display the loading animation
			$('.wallboard-layout .message-list-options .checkbox').append('<i class="fa fa-spinner fa-pulse fa-fw"></i>');

			// Call the jobs list
			loadJobList();

		}, jobListPollingInterval);


	}
	
	// Bind and load the UI
	bindUI();
	loadUI();

})();