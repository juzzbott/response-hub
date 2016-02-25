responseHub.wallboard = (function () {

	var currentRadarImageIndex = 0;

	function showHideWarnings(warningsContainer) {

		// If the container has the hidden class, remove it, otherwise add it
		if ($('.' + warningsContainer).hasClass('hidden')) {
			$('.' + warningsContainer).removeClass('hidden')
		}
		else
		{
			$('.' + warningsContainer).addClass('hidden')
		}

	}

	function setContainerHeights(width) {

		// If the width is < 768 then just set the heights to auto
		if (width < 768) {
			$('.wallboard-sidebar, .wallboard-main, .wallboard-warnings').css('height', 'auto');
			return;
		}

		// Set the heights of the main containers
		var headerHeight = $('.page-navbar').height();
		var containerHeight = ($(window).height() - headerHeight);

		$('.wallboard-sidebar, .wallboard-main, .wallboard-warnings').height(containerHeight);

	}

	function selectMessage(elem) {

		// If the current element is already selected, just return
		if (elem.hasClass('selected')) {
			return;
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
		if ($(elem).data('priority').toLowerCase() == 'emergency') {
			$('#message-type').attr('class', 'fa fa-exclamation-triangle p-message-emergency');
		} else if ($(elem).data('priority').toLowerCase() == 'nonemergency') {
			$('#message-type').attr('class', 'fa fa-exclamation-circle p-message-non-emergency');
		} else {
			$('#message-type').attr('class', 'fa fa-info-circle p-message-admin');
		}

		// Set the active class on the list item
		$('.message-list li').removeClass('selected');
		$(elem).addClass('selected');

	}

	function bindUI() {

		// If the window is resize, reset container heights (i.e. moving to fullscreen).
		$(window).resize(function (e) {
			setContainerHeights(e.target.innerWidth);
		});

	}

	function loadUI() {

		// If the body class does not contain the 'wallboard-layout' class, exit as we aren't in wallboard view.

		if (!$('body').hasClass('wallboard-layout')) {
			return;
		}

		// Initially set the container heights
		setContainerHeights($(window).width());

		// Set the list item click event for message list
		$('.message-list li').click(function () {
			selectMessage($(this));
		});

		// Get the radar image urls
		loopRadarImageUrls();

	}

	function loopRadarImageUrls() {

		// If there are no radar images, then show error message
		if (radarImages == null || radarImages.length == 0) {
			$('.radar-container').empty();
			$('.radar-container').append('<div class="error-summary ">Unable to load radar information.</div>');
			return;
		}

		// Iterate through the radar images and set the prefix
		for (var i = 0; i < radarImages.length; i++) {
			radarImages[i] = 'http://ws.cdn.bom.gov.au/radar/' + radarImages[i];
		}

		// Create the div to store the initial image
		$('.radar-container').append('<div class="radar-image radar-loop" style="background: url(\'' + radarImages[0] + '\')"></div>');

		setInterval(function () {
		
			// Get the next radar image. If the index exceeds the length, reset back to index 0
			nextIndex = currentRadarImageIndex + 1;
			if (nextIndex >= radarImages.length) {
				nextIndex = 0;
			}

			// Get the radar image div and update the background property
			$('.radar-loop').css('background', 'url(\'' + radarImages[nextIndex] + '\')');
		
			// Reset the current index
			currentRadarImageIndex = nextIndex;
		
		}, 250);

	}

	// Bind and load the UI
	bindUI();
	loadUI();
	
	return {
		showHideWarnings: showHideWarnings
	}

})();