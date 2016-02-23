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
		$('.job-number').html(jobNumber);
		$('.job-message-body').text($(elem).data('message'));
		$('.job-date').text($(elem).data('date'));

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

		var map = L.map('map').setView([-37.674636, 144.434981], 15);
		topoMapLayer = L.tileLayer('http://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
			attribution: 'Imagery from <a href="http://mapbox.com/about/maps/">MapBox</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
			subdomains: 'abcd',
			id: 'juzzbott.mn24imf3',
			accessToken: 'pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg'
		}).addTo(map);

		// Set the list item click event for message list
		$('.message-list li').click(function () {
			selectMessage($(this));
		});

		// Get the radar image urls
		loopRadarImageUrls();

	}

	function loopRadarImageUrls() {

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