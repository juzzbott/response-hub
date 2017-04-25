responseHub.events = (function () {

	/*
	 * The object to store the interval handler for reloading event data when viewing an event.
	 */
	viewEventReloadInterval = null;

	/*
	 * The array containing the job markers on the map
	 */
	jobMapMarkers = [];
	
	// Adds the user to the specified list, as either a trainer or a member.
	function addUserToList(userId, listId, selectedId, tableId, removeCallbackMethodName) {

		// Get the user
		var user = findUser(userId);

		// If the user id already exists in the selected users element, just return
		if ($('#' + selectedId).val().indexOf(userId) != -1) {
			$('#' + listId).selectpicker('val', '');
			return;
		}

		// If the first table row in the body is nothing selecting, then remove it
		if ($('#' + tableId + ' td.none-selected').length > 0) {
			$('#' + tableId + ' tbody tr').remove();
		}

		// Build the markup 
		var row = $('<tr data-user-id="' + user.id + '"></tr>');
		row.append('<td>' + user.name + '</td>');
		row.append('<td>' + user.memberNumber + '</td>');
		row.append('<td><div class="radio graphic-radio"><label><input type="radio" name="CrewLeaderId" id="CrewLeaderId_' + user.id + '" value="' + user.id + '" ' + ($('#' + tableId + ' tbody tr').length == 0 ? 'checked="checked"' : '') + ' /></label></div>');
		row.append('<td><a href="#" onclick="responseHub.events.removeCrewMember(this); return false;" title="Remove member" class="text-danger"><i class="fa fa-fw fa-times"></i></a></td>');
		$('#' + tableId + ' tbody').append(row);

		// Add the user id to the selected users
		$('#' + selectedId).val($('#' + selectedId).val() + user.id + '|');

		// Deselect the previous option
		$('#' + listId).selectpicker('val', '');

		// reset the graphical checkboxes
		responseHub.setGraphicRadiosCheckboxes();
	}

	function removeCrewMember(elem) {
		// Get the element to remove
		var link = $(elem);

		// get the hidden id
		var hiddenId = link.closest('table').data('selected-list');
		var tableId = link.closest('table').attr('id');

		// Find the user id
		var userId = link.closest("tr").data('user-id');

		// Remove the user id from the hidden
		$('#' + hiddenId).val($('#' + hiddenId).val().replace(userId, ""));

		// If there is only | characters, set to empty to re-trip validation
		if ($('#' + hiddenId).val().match(/^\|*$/)) {
			$('#' + hiddenId).val('');
		}

		// Remove the row with the user details
		link.closest("tr").remove();

		// If there is no checked crewleader option, check the first in the list
		if ($('#' + tableId + ' tbody input[type="radio"]:checked').length == 0)
		{
			// Set the first row input as checked
			$('#' + tableId + ' tbody tr:first input').attr('checked', 'checked');
		}

		// If there are no rows left, add the default message
		if ($('#' + tableId + ' tbody tr').length == 0) {
			var memberType = (tableId.indexOf('trainer') != -1 ? "trainers" : "members");
			$('#' + tableId + ' tbody ').append('<tr><td colspan="4" class="none-selected">No members have been added to the this crew yet.</td></tr>');
		}

	}

	// Find the user object in the list of users.
	function findUser(userId) {

		// Create the user object
		var user = null;

		// Loop through the users to find the selected one.
		for (var i = 0; i < users.length; i++) {
			if (users[i].id == userId) {
				user = users[i];
				break;
			}
		}

		// return the user object
		return user;
	}

	/**
	 * Gets the height of the containers for the screen size.
	 */
	function getContainerHeight(width) {

		// If the width is < 768 then just set the heights to auto
		if (width < 768) {
			$('.event-job-allocation .jobs-list').css('height', 'auto');
			return;
		}

		// Set the heights of the main containers
		var headerHeight = $('.page-navbar').height();
		var containerHeight = ($(window).height() - headerHeight - 260);

		// return the height
		return containerHeight;

	}

	/**
	 * Sets the container height for the columns. Only for devices above mobile.
	 */
	function setContainerHeights(width) {

		var containerHeight = getContainerHeight(width);

		// Set the height of the columns.
		$('.event-job-allocation .jobs-list').height(containerHeight);

	}

	/**
	 * Loads the crew job assignments for the specific crew
	 * @param {any} crewId
	 */
	function loadCrewJobAssignments(crewId) {

		// Show the loading animation
		$('.loading-crew-details').removeClass('hidden');

		// Clear the current job assignments
		$('.crew-allocation .assigned-jobs').empty();

		// Get the event id
		var eventId = $('#EventId').val();

		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId + '/crew/' + crewId,
			dataType: 'json',
			success: function (data) {

				if (data != null)
				{
					// If there are no jobs, show the no jobs message
					if (data.AssignedJobs.length == 0)
					{
						$('.crew-job-list .no-jobs').removeClass('hidden');
						$('.allocate-jobs').addClass('hidden');
					}
					else {
						$('.crew-job-list .no-jobs').addClass('hidden');
						$('.allocate-jobs').removeClass('hidden');

						// Loop through the assigned jobs
						for (var i = 0; i < data.AssignedJobs.length; i++) {

							// Get the timestamp from the results.
							var timestamp = moment(data.AssignedJobs[i].Timestamp).local().format('DD-MM-YYYY HH:mm:ss');

							// Add the list items for the jobs
							addAssignedListItemMarkup(data.AssignedJobs[i].Id, data.AssignedJobs[i].JobNumber, data.AssignedJobs[i].MessageBody, timestamp);
						}

					}
				}

			},
			complete: function () {
				$('.loading-crew-details').addClass('hidden');
			}
		});

	}

	/**
	 * Submits the job allocations for the crew to the server.
	 */
	function submitJobAllocationToCrew()
	{

		// Disable button while posting
		$('.allocate-jobs button').attr('disabled', 'disabled');
		$('.allocate-jobs button').addClass('disabled');
		$('.allocate-jobs button i').removeClass('fa-indent').addClass('fa-spin fa-spinner');

		var eventId = $('#EventId').val();
		var crewId = $('#CrewSelect').val();

		// Create the array of jobs to be assigned
		var jobIds = [];

		// Loop through each list item and push the job ids to the array
		$('.assigned-jobs li').each(function (index, elem) {
			var jobId = $(elem).data('job-id');
			jobIds.push(jobId);
		});

		// Build the post model
		var postData = {
			CrewId: crewId,
			JobMessageIds: jobIds
		};

		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId + '/crew/' + crewId + '/assign-jobs',
			type: 'post',
			dataType: 'json',
			data: postData,
			success: function (data) {

			},
			error: function (jqXHR, textStatus, errorThrown) {

			}, 
			complete: function () {
				$('.allocate-jobs button').removeAttr('disabled');
				$('.allocate-jobs button').removeClass('disabled');
				$('.allocate-jobs button i').addClass('fa-indent').removeClass('fa-spin fa-spinner');
			}
		});

	}

	/**
	 * Adds the jobs in the list to the map.
	 */
	function addJobsToMap()
	{
		
		// Ensure there are job locations before adding the markers to the map
		if (jobLocations == null || jobLocations.length == 0) {
			return;
		}

		// Loop through the locations
		for (var i = 0; i < jobLocations.length; i++)
		{
			var marker = responseHub.maps.addCustomLocationMarkerToMap(jobLocations[i].lat, jobLocations[i].lon, jobLocations[i].markerIcon, [15, 23], [7, 23], [1, -27]);
			marker.bindPopup('<strong><a href="/jobs/' + jobLocations[i].id + '" target="_blank">' + jobLocations[i].jobNumber + '</strong></a><br /><small>' + jobLocations[i].messageBody + '</small>');
			jobMapMarkers.push(marker);
		}

		// Resize the map to match the markers
		responseHub.maps.zoomToMarkerGroup(jobMapMarkers);
	}

	/**
	 * Adds the assigned job markup to the sortable list.
	 * @param {any} jobId
	 * @param {any} jobNumber
	 * @param {any} jobMessage
	 * @param {any} jobTimestamp
	 */
	function addAssignedListItemMarkup(jobId, jobNumber, jobMessage, jobTimestamp)
	{
		// Add to the assigned crew list
		var assignedListItem = $('<li data-job-id="' + jobId + '" data-job-number="' + jobNumber + '" data-job-message="' + jobMessage + '" data-job-timestamp="' + jobTimestamp + '">');

		// Add the drag handle
		$(assignedListItem).append('<div class="drag-handle"><i class="fa fa-fw fa-2x fa-sort"></i></div>');
		$(assignedListItem).append('<div class="unassign"><button onclick="responseHub.events.unassignJobFromCrew(this);" title="Unassign job from crew"><i class="fa fa-fw fa-times text-danger"></i></div>');

		// Add the job number, message and date
		var assignedItemContent = $('<div class="assigned-content"></div>');
		$(assignedItemContent).append('<h4>' + jobNumber + '<span class="small text-info">' + jobTimestamp + '</span></h4>');
		$(assignedItemContent).append('<p>' + jobMessage + '</p>');
		$(assignedListItem).append(assignedItemContent);

		// Add the list item to the assigned jobs
		$('ul.assigned-jobs').append(assignedListItem);
	}

	/**
	 * Unassigns the job from the crew
	 * @param {any} elem
	 */
	function unassignJobFromCrew(elem)
	{

		// Get the li element
		var assignedListItem = $(elem).closest('li');

		// Get the job id, number, message and date
		var jobId = $(assignedListItem).data('job-id');
		var jobNumber = $(assignedListItem).data('job-number');
		var jobMessage = $(assignedListItem).data('job-message');
		var jobTimestamp = $(assignedListItem).data('job-timestamp');

		// Create the new list item
		var jobListItem = $('<li data-job-number="' + jobNumber + '" data-job-id="' + jobId + '" data-job-message="' + jobMessage + '" data-job-timestamp="' + jobTimestamp + '">');
		jobListItem.append('<h4>' + jobNumber + '<span class="text-info pull-right small">' + jobTimestamp + '</span></h4>');
		jobListItem.append('<div class="message-body"><small class="text-muted">' + jobMessage + '</small></div>');
		jobListItem.append('<div class="job-allocation-actions clearfix"><button class="btn btn-link btn-icon pull-left btn-left-align btn-assign-job" title="Allocate to crew"><i class="fa fa-fw fa-share"></i> Assign to crew</button></div>');

		// Get the index to insert it at
		var insertIndex = -1
		var sortJobNumber = jobNumber.substring(1);
		var maxIndex = ($('.jobs-list ul li').length - 1)
		$('.jobs-list ul li').each(function (index, elem) {
			
			// Get the job number in the list item
			var checkJobNumber = $(elem).data('job-number').substring(1);

			// If it's the first element, and we are already greater than that, then just set the insert index as 0'
			if (index == 0) {
				if (sortJobNumber > checkJobNumber)
				{
					insertIndex = 0;
					return false;
				}
			}
			else 
			{
				// If the current index is the max index, we've got nothing else to check, so just remain as -1 as this will just append to the list
				if (index == maxIndex)
				{
					return false;
				}
				else
				{
					// Get the prev index element
					var prevCheckJobNumber = $(elem).prev().data('job-number').substring(1);

					// If the sort number is > check number but < prev check number, then we want to insert at the current index
					if (sortJobNumber > checkJobNumber && sortJobNumber < prevCheckJobNumber)
					{
						insertIndex = index;
						return false;
					}

				}
			}

		});

		// Add the list item at the specific index
		if (insertIndex != -1) {
			$('.jobs-list ul').insertAt(insertIndex, jobListItem);
		} else {
			$('.jobs-list ul').append(jobListItem);
		}

		// remove the assigned list item
		$(assignedListItem).remove();

		// rebind the assign controls
		bindAssignJobToCrew();

	}

	function bindSortable() {
		$(".assigned-jobs").sortable({
			placeholder: "assigned-jobs-highlight",
			handle: '.drag-handle',
			axis: 'y',
			forceHelperSize: true,
			forcePlaceholderSize: true,
			helper: 'clone',
			opacity: 0.85
		});
	}

	/**
	 * Binds the click event for the assign job to crew button
	 */
	function bindAssignJobToCrew() {

		// unbind all click events
		$('.btn-assign-job').off('click');

		// Bind the click event
		$('.btn-assign-job').click(function () {

			// Get the li element
			var jobListItem = $(this).closest('li');

			// Get the job id, number, message and date
			var jobId = $(jobListItem).data('job-id');
			var jobNumber = $(jobListItem).data('job-number');
			var jobMessage = $(jobListItem).data('job-message');
			var jobTimestamp = $(jobListItem).data('job-timestamp');

			// Adds the assigned job markup to the sortable list
			addAssignedListItemMarkup(jobId, jobNumber, jobMessage, jobTimestamp);

			// Remove from the unassigned job list
			jobListItem.remove();

			// Hide the no assigned jobs message
			$('.crew-job-list .no-jobs').addClass('hidden');
			$('.allocate-jobs').removeClass('hidden');

		});

	}

	/**
	 * Reloads the event data and refreshed the information displayed on screen with the updated data
	 */
	function reloadEventData()
	{

		// Display the loading animation
		$('.event-detail-refresh-loading').removeClass('hidden');

		// Get the event id
		var eventId = $('#EventId').val();

		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId,
			dataType: 'json',
			success: function (data) {

				if (data != null) {

					// Set the name, description and totals
					$('.title-row h1 span').text(data.Name);
					$('p.event-desc').text(data.Description);
					$('.event-total-jobs-count span').text(data.Jobs.length);
					$('.event-unassigned-jobs-count span').text(data.UnassignedJobsCount);
					$('.event-in-progress-jobs-count span').text(data.InProgressJobsCount);
					$('.event-completed-jobs-count span').text(data.CompletedJobsCount);

					// reload all the jobs
					$('#event-jobs-list tbody').empty();

					// Loop through all the jobs. If there are no jobs, just add a message
					if (data.Jobs == null || data.Jobs.length == 0)
					{
						$('#event-jobs-list tbody').append('<tr><td colspan="4">No jobs have been added to the event yet.</td></tr>');
					}
					else
					{
						for (var i = 0; i < data.Jobs.length; i++)
						{
							var row = $('<tr></tr>');

							// Set the assigned checkbox
							row.append('<td>' + (data.Jobs[i].Assigned ? '<center><i class="fa fa-fw fa-check text-primary"></i></center>' : '') + '</td>');

							// Set the job number/link and description
							row.append('<td><a href="/jobs/' + data.Jobs[i].Id + '" target="_blank">' + data.Jobs[i].JobNumber + '</a></td>');
							row.append('<td><small class="text-muted">' + data.Jobs[i].MessageBody + '</small></td>');

							// Get the value of the status based on the value
							var statusContent = '';
							switch (data.Jobs[i].Status)
							{
								case 1:
									statusContent = '<span>' + data.Jobs[i].StatusString + '</span>';
									break;

								case 2:
									statusContent = '<span class="marker-in-progress">' + data.Jobs[i].StatusString + '</span>';
									break;

								case 3:
									statusContent = '<span class="marker-completed">' + data.Jobs[i].StatusString + '</span>';
									break;

								case 4:
									statusContent = '<span class="marker-cancelled">' + data.Jobs[i].StatusString + '</span>';
									break;

								case 5:
									statusContent = '<span class="marker-requires-info">' + data.Jobs[i].StatusString + '</span>';
									break;
							}

							// Append the status column
							row.append('<td>' + statusContent + '</td>');

							// Append the row to the table
							$('#event-jobs-list tbody').append(row);

						}
					}

					// Clear the jobLocations array
					jobLocations = [];

					// Reset the job map makrers from the data in the web service
					for (var i = 0; i < data.Jobs.length; i++) {
						if (data.Jobs[i].Coordinates == null || (data.Jobs[i].Coordinates.Latitude == 0 && data.Jobs[i].Coordinates.Longitude == 0)) {
							continue;
						}

						var markerIcon = "event-unassigned";
						if (data.Jobs[i].Assigned) {
							markerIcon = "event-assigned";
						}

						switch (data.Jobs[i].Status) {
							case 2:
								markerIcon = "event-in-progress";
								break;
							case 3:
								markerIcon = "event-completed";
								break;
							case 4:
								markerIcon = "event-cancelled";
								break;
							case 5:
								markerIcon = "event-requires-info";
								break;
						}

						var jobLocation = {
							id: data.Jobs[i].Id,
							jobNumber: data.Jobs[i].JobNumber,
							messageBody: data.Jobs[i].MessageBody,
							lat: data.Jobs[i].Coordinates.Latitude,
							lon: data.Jobs[i].Coordinates.Longitude,
							markerIcon: markerIcon
						};

						jobLocations.push(jobLocation);

					}

					// If the map is loaded, then reset the markers
					if (responseHub.maps.mapExists()) {

						// Clear the existing map markers
						responseHub.maps.clearMarkers(jobMapMarkers);

						// Add the markers to the map
						addJobsToMap();
					}

					// hide the loading animation
					$('.event-detail-refresh-loading').addClass('hidden');

				}

			},
			complete: function () {
				// hide the loading animation
				$('.event-detail-refresh-loading').addClass('hidden');
			}
		});

	}

	/**
	 * Bind the UI controls.
	 */
	function bindUI()
	{

		// Bind the sortable elements
		bindSortable();

		// Get the container height
		$('.event-job-allocation .jobs-list').scrollator();

		// If the window is resize, reset container heights (i.e. moving to fullscreen).
		$(window).resize(function (e) {
			setContainerHeights(e.target.innerWidth);
			$('.event-job-allocation .jobs-list').scrollator();
		});

		$('#AvailableMembers').on('change', function () {

			// Find the user
			var userId = $(this).val();

			// Add the user to the list
			addUserToList(userId, 'AvailableMembers', 'SelectedMembers', 'crew-members-table');

		});

		$('#CrewSelect').on('change', function () {

			// Enable job assignments
			$('.event-job-allocation .jobs-list button').each(function (index, elem) {
				$(elem).removeClass('disabled');
				$(elem).removeAttr('disabled');
			});

			// Load the crew assignments
			loadCrewJobAssignments($(this).val());

		});

		$('.allocate-jobs button').click(function () {
			submitJobAllocationToCrew();
		});

		// Bind the assign job to crew method
		bindAssignJobToCrew();

		$('.nav-tabs #crew-job-allocation-tab').on('hide.bs.tab', function (e) {
			$('.scrollator_lane_holder').css('opacity', '0');
		});
		
		// Ensure the map is displayed correctly within the tab.
		$(".nav-tabs #map-view-tab").on("shown.bs.tab", function () {

			// Define the map config
			var mapConfig = {
				lat: -37.020100,
				lon: 144.964600,
				zoom: 8,
				minZoom: 4,
				scrollWheel: false,
				mapContainer: 'map-canvas',
				loadCallback: function () {

					responseHub.events.addJobsToMap();

				}
			};

			// Display the map
			responseHub.maps.displayMap(mapConfig);

		});

		// If we are viewing the job, set the interval to reload event data every 60 seconds
		if ($('body.view-event-details').length > 0 && $('#EventFinished').val() == "0")
		{
			viewEventReloadInterval = setInterval(reloadEventData, 60000);
		}
	}

	function loadUI() {
		
		// Initially set the container heights
		setContainerHeights($(window).width());
	}

	// Load the UI
	loadUI();

	// Bind the ui controls
	bindUI();

	return {
		addJobsToMap: addJobsToMap,
		unassignJobFromCrew: unassignJobFromCrew,
		removeCrewMember: removeCrewMember
	}

})();