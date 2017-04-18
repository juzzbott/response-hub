responseHub.search = (function () {

	function getNextResults() {

		// Get how many items to skip
		var skip = $('.job-list li').length;

		// Build the filter data
		var filterData = buildFilter(skip);

		// Show the loader
		$('.form-loader').removeClass('hidden');

		// Make the ajax call to get the next results
		$.ajax({
			url: responseHub.apiPrefix + '/search/',
			type: 'POST',
			dataType: 'json',
			data: filterData,
			success: function (data) {

				// Ensure there is results and iterate through them to add to the list of results
				if (data != null && data.Results != null) {
					for (var i = 0; i < data.Results.length; i++) {

						// Add result to the list
						addResultToList(data.Results[i]);

					}

					// if the total count <= what is in the list, hide the more button
					if (data.TotalResults <= $('.job-list li').length) {
						$('#load-more-results').addClass('hidden');
					}

				}

			},
			complete: function () {
				// hide the loader
				$('.form-loader').addClass('hidden');
			}
		});


	}

	function addResultToList(result) {

		// Build the list item
		var li = $('<li data-message-id="' + result.Id + '"></li>');

		// Create the header
		var header = $('<h3></h3>');
		header.append(getPriorityMarkup(result));
		header.append($(getTitleMarkup(result)));

		// Append the header
		li.append(header);

		// Create the message meta
		var messageMeta = $('<p class="job-message-meta text-muted"></p>');
		messageMeta.append(getProgessMarkup(result));
		messageMeta.append($('<span class="capcode-unit-name">' + result.CapcodeUnitName + '</span>'));
		li.append(messageMeta);

		// Add the message text
		li.append('<p>' + result.MessageBody + '</p>');

		// Add the list item to the ul list
		$('.job-list').append(li);

	}

	/**
	 * Gets the priority markup used to render the list item
	 */
	function getPriorityMarkup(result) {

		var cssClass = "";
		switch (result.Priority)
		{
			case 1:
				cssClass = "fa-exclamation-triangle p-message-emergency";
				break;

			case 2:
				cssClass = "fa-exclamation-circle p-message-non-emergency";
				break;

			default:
				cssClass = "fa-info-circle p-message-admin";
				break;
		}

		return $('<i class="fa fa-fw ' + cssClass + '"></li>');

	}

	/**
	 * Gets the markup for the title of the job in the result list.
	 */
	function getTitleMarkup(result) {

		// Get the date of the note
		var jobDate = moment(result.Timestamp).local();

		var controller = (result.Priority == 3 ? "messages" : "jobs");

		if (result.JobNumber == null || result.JobNumber.length == 0) {
			return '<a href="/' + controller + '/' + result.Id + '">' + jobDate.format('YYYY-MM-DD HH:mm') + '</a>';
		} else {
			return '<a href="/' + controller + '/' + result.Id + '">' + result.JobNumber + '</a> <span class="text-muted message-date btn-icon">' + jobDate.format('YYYY-MM-DD HH:mm') + '</span>';
		}

	}

	/**
	 * Gets the progress markup for the search result list item
	 */
	function getProgessMarkup(result) {

		if (result.Cancelled != null)
		{
			return $('<i class="fa fa-ban"></i>');
		}
		else if (result.JobClear != null)
		{
			return $('<i class="fa fa-check-circle-o"></i>');
		}
		else if (result.OnScene != null)
		{
			return $('<i class="fa fa-hourglass-half"></i>');
		}
		else if (result.OnRoute != null)
		{
			return $('<i class="fa fa-arrow-circle-o-right"></i>');
		}
		else
		{
			return $('<i class="fa fa-asterisk"></i>');
		}

	}

	/*
	 * Builds the search filter object to supply to the post data
	 */
	function buildFilter(skip) {

		// Get the keywords
		var keywords = $('#keywords').val();

		// Get the message types
		var messageTypes = Array();
		if ($('#messagetype-job').is(':checked'))
		{
			messageTypes.push(1);
		}
		if ($('#messagetype-message').is(':checked'))
		{
			messageTypes.push(2);
		}

		// Get the dates
		var dateFrom = $('#date-from').val();
		var dateTo = $('#date-to').val();

		return {
			Skip: skip,
			Keywords: keywords,
			MessageTypes: messageTypes,
			DateFrom: dateFrom,
			DateTo: dateTo
		};

	}

	return {
		getNextResults: getNextResults
	}

})();