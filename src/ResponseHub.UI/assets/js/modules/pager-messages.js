responseHub.pagerMessages = (function () {

	/**
	 * Gets the next set of pager messages to display.
	 */
	function getNextPages() {

		// Get the skip count
		var skipCount = $("#all-pages-list li").length;

		$('#all-pages-load-more .loading').removeClass('hidden');
		$('#all-pages-load-more button').addClass('hidden');

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/pager-messages?skip=' + skipCount,
			dataType: 'json',
			success: function (data) {
		
				for (var i = 0; i < data.length; i++) {
		
					addMessageToList(data[i], true);
		
				}
		
				// If there is zero results, hide the show more button
				if (data.length == 0) {
					$("#all-pages-load-more").remove();
				} else {
					$('#all-pages-load-more .loading').addClass('hidden');
					$('#all-pages-load-more button').removeClass('hidden');
				}
		
			}
		});

	}

	function getLatestPages() {

		// Get the latest message id
		var last_id = $("#all-pages-list li:first-child").data("message-id");

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/latest-pager-messages/' + last_id,
			dataType: 'json',
			success: function (data) {

				// Because we are pre-pending these results, we need to reverse them.
				data.reverse();

				for (var i = 0; i < data.length; i++) {
					addMessageToList(data[i], false);
				}

			}
		});

	}

	function addMessageToList(pagerMessage, append) {

		var listItem = $('<li data-message-id="' + pagerMessage.Id + '"></li>');

		var topRow = $('<p class="bottom-0"></p>');

		// Create the icon and name
		var topMarkup = "<strong>";

		// Loop through the capcodes
		var priority = 0;
		var capcode = "";
		var capcodeCount = 0;
		for (var i = 0; i < pagerMessage.Capcodes.length; i++) {

			if (i == 0) {
				priority = pagerMessage.Capcodes[i].Priority;
				capcode = pagerMessage.Capcodes[i].Capcode;
			}
			else if (pagerMessage.Capcodes[i].Priority < priority) {
				priority = pagerMessage.Capcodes[i].Priority;
			}

			capcodeCount++;
		}

		// Set the priority
		// Add the priority icon
		switch (priority) {
			case 1:
				topMarkup += '<i class="fa fa-exclamation-triangle p-message-emergency"></i> ';
				break;

			case 2:
				topMarkup += '<i class="fa fa-exclamation-circle p-message-non-emergency"></i> ';
				break;

			default:
				topMarkup += '<i class="fa fa-info-circle p-message-admin"></i> ';
				break;
		}

		// If there is job number, then show it here
		if (pagerMessage.JobNumber != "") {
			topMarkup += '<a href="/pager-messages/' + pagerMessage.Id + '">' + pagerMessage.JobNumber + "</a> - "
		}

		// Close the job and icon section
		topMarkup += "</strong>";

		// Get the job date
		var jobDate = moment(pagerMessage.Timestamp);
		var localDateString = jobDate.format('HH:mm:ss D MMMM YYYY');
		topMarkup += '<span class="text-info">' + localDateString + '</span> - <span class="text-muted">[' + capcode + '] (' + capcodeCount + ')</span>';
		// Append the top row
		topRow.append($(topMarkup));

		// Append the top row to the list item
		listItem.append(topRow);

		// Set the incident type
		if (pagerMessage.Type == 1) {
			var jobTypes = responseHub.getJobTypes();
			var jobType = jobTypes[0]
			for (var i = 0; i < jobTypes.length; i++) {
				if (jobTypes[i].Id == pagerMessage.JobCode) {
					jobType = jobTypes[i];
					break;
				}
			}
			listItem.append($('<p class="job-type-desc bottom-0">' + jobType.IncidentType + '</p>'))
		}

		listItem.append($('<p class="message-content">' + pagerMessage.MessageContent + '</p>'));

		// Append the list item to list of jobs
		if (append) {
			$("#all-pages-list").append(listItem);
		} else {
			$("#all-pages-list").prepend(listItem);
		}

	}

	function bindUI() {

		if ($("#all-pages-list").length > 0) {

			setInterval(function () {
				getLatestPages();
			}, 30000);

		}

	}

	bindUI();

	return {
		getNextPages: getNextPages,
		getLatestPages: getLatestPages
	}



})();