responseHub.allPages = (function () {

	/**
	 * Gets the next set of pager messages to display.
	 */
	function getNextPages() {

		// Get the skip count
		var skipCount = $("#all-pages-list li").length;

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/all-pages?skip=' + skipCount,
			dataType: 'json',
			success: function (data) {

				for (var i = 0; i < data.length; i++) {

					var listItem = $("<li></li>");

					var topRow = $('<p class="bottom-0"></p>');

					// Create the icon and name
					var topMarkup = "<strong>";

					// Set the priority
					// Add the priority icon
					switch (data[i].Priority) {
						case 1:
							topMarkup += '<i class="fa fa-exclamation-triangle p-message-emergency"></i>';
							break;

						case 2:
							topMarkup += '<i class="fa fa-exclamation-circle p-message-non-emergency"></i>';
							break;

						default:
							topMarkup += '<i class="fa fa-info-circle p-message-admin"></i>';
							break;
					}

					// If there is job number, then show it here
					if (data[i].JobNumber != "")
					{
						topMarkup += data[i].JobNumber + " - "
					}

					// Close the job and icon section
					topMarkup += "</strong>";

					// Get the job date
					var jobDate = moment(data[i].Timestamp);
					var localDateString = jobDate.format('HH:mm:ss D MMMM YYYY');
					topMarkup += '<span class="text-info">' + localDateString + '</span>';
					// Append the top row
					topRow.append($(topMarkup));

					// Append the top row to the list item
					listItem.append(topRow);
					listItem.append($("<p>" + data[i].MessageContent + "</p>"));

					// Append the list item to list of jobs
					$("#all-pages-list").append(listItem);

				}

				// If there is zero results, hide the show more button
				if (data.length == 0) {
					$("#all-pages-load-more").remove();
				}

			}
		});

	}

	return {
		getNextPages: getNextPages
	}

})();