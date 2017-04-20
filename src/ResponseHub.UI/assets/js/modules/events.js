responseHub.events = (function () {

	function createCrew() {

		// Ensure the form is valid
		if (!$('#create-crew').valid()) {
			return;
		}

		var eventId = $('#EventId').val();

		var postData = {
			Name: $('#CrewName').val(),
			SelectedMembers: $('#SelectedMembers').val(),
			CrewLeaderId: $('input[name="CrewLeaderId"]:checked').val()
		};

		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId + '/add-crew',
			type: 'post',
			dataType: 'json',
			data: postData,
			success: function (data) {

				if (data.Success) {


					// Build the new accordion markuo
					buildNewCrewAccordionMarkup(data.Crew);

					// Hide the no crews message
					if (!$('.no-crews').hasClass("hidden")) {
						$('.no-crews').addClass("hidden");
					}

					// Clear the textbox
					$('#CrewName').val('');
					$('#SelectedMembers').val('');

					// Clear the crew members
					$('#crew-members-table tbody').empty();
					$('#crew-members-table tbody').append('<tr><td colspan="4">No members have been added to the this crew yet.</td></tr>');

				} else {
					$('#create-crew').prepend('<div class="alert alert-danger alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.ErrorMessage + '</div>');
				}


			},
			error: function () {
				$('#create-crew').prepend('<div class="alert alert-danger alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>There was an error creating the crew.</div>');
			},
			complete: function () {

			}
		});

	}

	function buildNewCrewAccordionMarkup(crew) {

		// Get the count of current accordions and define the accordion name based on the count
		var currentAccordionCount = $('.crew-list .panel').length;
		var accordionId = 'crew-accordion-' + (currentAccordionCount + 1);
		var accordionHeadingId = 'crew-accordion-heading-' + (currentAccordionCount + 1);

		// Get the crew leader details
		var crewLeader = findUser(crew.CrewLeaderId);

		// Create the accordion
		var accordion = $('<div class="panel panel-default"></div>');

		// Create the accordion header
		var accordionHeader = $('<div class="panel-heading" role="tab" id="' + accordionHeadingId + '"><h4 class="panel-title"><a class="accordion-toggle" data-toggle="collapse" href="#' + accordionId + '">' + crew.Name + (crewLeader != null ? ' - Crew leader: ' + crewLeader.name : '') +'</a></h4></div>');

		// Create the accordion body
		var accordionContent = $('<div class="panel-body crew-details"></div>');

		// Add the crew leader
		accordionContent.append("<h4>Crew leader:</h4>");
		if (crewLeader != null)
		{
			accordionContent.append("<p>" + crewLeader.name + "</p>");
		}

		// Add the crew members
		accordionContent.append("<h4>Crew members:</h4>");
		var crewMembersMarkup = $('<p></p>');
		for (var i = 0; i < crew.CrewMembers.length; i++)
		{
			var crewMember = findUser(crew.CrewMembers[i]);
			if (crewMember != null && crewMember.id.toLowerCase() != crew.CrewLeaderId.toLowerCase()) {
				crewMembersMarkup.append(crewMember.name + "<br />");
			}
		}
		// Add the crew members to the list
		accordionContent.append(crewMembersMarkup);

		// Create the accordion content container markup
		var accordionBody = $('<div id="' + accordionId + '" class="panel-collapse collapse in"></div>');
		accordionBody.append(accordionContent);

		// Build the accordion and add to the crew list
		accordion.append(accordionHeader);
		accordion.append(accordionBody);

		$('.crew-list').append(accordion);

	}

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
		row.append('<td><a href="#" onclick="' + removeCallbackMethodName + '(this); return false;" title="Remove member"><i class="fa fa-fw fa-times"></i></td>');
		$('#' + tableId + ' tbody').append(row);

		// Add the user id to the selected users
		$('#' + selectedId).val($('#' + selectedId).val() + user.id + '|');

		// Deselect the previous option
		$('#' + listId).selectpicker('val', '');

		// reset the graphical checkboxes
		responseHub.setGraphicRadiosCheckboxes();
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
		var containerHeight = ($(window).height() - headerHeight - 275);

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

	function submitJobAllocationToCrew()
	{

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

			}
		});

	}

	/**
	 * Binds the click event for the assign job to crew button
	 */
	function bindAssignJobToCrew() {

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
	 * Adds the assigned job markup to the sortable list.
	 * @param {any} jobId
	 * @param {any} jobNumber
	 * @param {any} jobMessage
	 * @param {any} jobTimestamp
	 */
	function addAssignedListItemMarkup(jobId, jobNumber, jobMessage, jobTimestamp)
	{
		// Add to the assigned crew list
		var assignedListItem = $('<li data-job-id="' + jobId + '">');

		// Add the drag handle
		$(assignedListItem).append('<div class="drag-handle"><i class="fa fa-fw fa-2x fa-sort"></i></div>')

		// Add the job number, message and date
		var assignedItemContent = $('<div class="assigned-content"></div>');
		$(assignedItemContent).append('<h4>' + jobNumber + '<span class="small text-info">' + jobTimestamp + '</span></h4>');
		$(assignedItemContent).append('<p>' + jobMessage + '</p>');
		$(assignedListItem).append(assignedItemContent);

		// Add the list item to the assigned jobs
		$('ul.assigned-jobs').append(assignedListItem);
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
			addUserToList(userId, 'AvailableMembers', 'SelectedMembers', 'crew-members-table', 'responseHub.events.removeCrewMember');

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
		createCrew: createCrew
	}

})();