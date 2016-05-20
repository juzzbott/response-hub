responseHub.events = (function () {

	function createCrew() {

		var eventId = "";

		var postData = {
			Name: $('#CrewName').val()
		};

		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId + '/add-crew',
			type: 'post',
			dataType: 'json',
			data: postData,
			success: function (data) {

				// Build the new accordion markuo
				buildNewCrewAccordionMarkup(postData.Name);

				// Hide the no crews message
				if (!$('.no-crews').hasClass("hidden")) {
					$('.no-crews').addClass("hidden");
				}

				// Clear the textbox
				$('#CrewName').val('');

			},
			error: function () {

			},
			complete: function () {

			}
		});

	}

	function buildNewCrewAccordionMarkup(crewName) {

		// Get the count of current accordions and define the accordion name based on the count
		var currentAccordionCount = $('.crew-list .panel').length;
		var accordionId = 'crew-accordion-' + (currentAccordionCount + 1);
		var accordionHeadingId = 'crew-accordion-heading-' + (currentAccordionCount + 1);


		// Create the accordion
		var accordion = $('<div class="panel panel-default"></div>');

		// Create the accordion header
		var accordionHeader = $('<div class="panel-heading" role="tab" id="' + accordionHeadingId + '"><h4 class="panel-title"><a class="accordion-toggle" data-toggle="collapse" href="#' + accordionId + '">Crew: ' + crewName + '</a></h4></div>');

		// Create the accordion body
		var accordionContent = $('<p>Blah blah</p>');

		// Create the accordion content container markup
		var accordionBody = $('<div id="' + accordionId + '" class="panel-collapse collapse in"><div class="panel-body"></div></div>');
		accordionBody.append(accordionContent);

		// Build the accordion and add to the crew list
		accordion.append(accordionHeader);
		accordion.append(accordionBody);

		$('.crew-list').append(accordion);

	}

	return {
		createCrew: createCrew
	}

})();