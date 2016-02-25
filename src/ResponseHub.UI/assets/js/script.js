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

	function bindModals() {
		$('#confirm-delete').on('show.bs.modal', function (e) {
			$(this).find('.btn-ok').attr('href', $(e.relatedTarget).data('href'));
		});
	}

	// Bind the modal
	bindModals();

	// return the response hub object
	return {
		apiPrefix: apiPrefix,
		isMobile: isMobile,
		executeFunctionByName: executeFunctionByName
	}

})();

responseHub.maps = (function () {

	/**
	 * Contains a dictionary of markers
	 */
	mapMarkers = [];


	/*
	 * The current map object
	 */
	map = null;

	/*
	 * The default layer
	 */
	streetMapLayer = null;

	/*
	 * Topographic map layer
	 */
	topoMapLayer = null;

	/*
	 * Aerial map layer
	 */
	aerialMapLayer = null;

	/*
	 * Contains the array of icons to display over the map
	 */
	leafIcons = [];

	/**
	 * Loads the map onto the screen.
	 */
	function displayMap(mapConfig) {

		// Build the leaf icons
		buildLeafIcons();

		// Builds the leaf layers
		buildLeafBaseLayers();

		// Create the map object
		map = L.map(mapConfig.mapContainer, {
			minZoom: mapConfig.minZoom,
			scrollWheelZoom: mapConfig.scrollWheel,
			layers: [streetMapLayer]
		});

		map.on('baselayerchange', function (e) {
			// Get the map zoom
			var mapZoom = map.getZoom();

			if (e.name == "Aerial" && mapZoom > 17) {
				map.setZoom(17);
			}
		});

		// Create the map layers
		var layers = {
			"Street": streetMapLayer,
			"Topographic": topoMapLayer,
			"Aerial": aerialMapLayer
		};

		L.control.layers(layers).addTo(map);
		L.control.scale().addTo(map);

		// Execute a callback if set.
		if (typeof mapConfig.loadCallback == 'function') {
			map.on('load', function (e) {
				mapConfig.loadCallback();
			});
		}

		// Set the map view.
		map.setView([mapConfig.lat, mapConfig.lon], mapConfig.zoom);

		// return the map for use outside of the responseHub.maps functionality.
		return map;
	}

	/**
	 * Builds the leaf base layers
	 */
	function buildLeafBaseLayers() {

		// Create the default layers
		streetMapLayer = L.tileLayer('http://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
			attribution: 'Imagery from <a href="http://mapbox.com/about/maps/">MapBox</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
			subdomains: 'abcd',
			id: 'juzzbott.mn25f8nc',
			accessToken: 'pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg'
		});

		topoMapLayer = L.tileLayer('http://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
			attribution: 'Imagery from <a href="http://mapbox.com/about/maps/">MapBox</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
			subdomains: 'abcd',
			id: 'juzzbott.mn24imf3',
			accessToken: 'pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg'
		});

		aerialMapLayer = L.tileLayer('http://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
			attribution: 'Imagery from <a href="http://mapbox.com/about/maps/">MapBox</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
			subdomains: 'abcd',
			id: 'juzzbott.mn74md27',
			accessToken: 'pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg',
			maxZoom: 17
		});
	}

	/*
	 * Builds the leaf icons.
	 */
	function buildLeafIcons() {

		// Create the leaf icons
		var LeafIcon = L.Icon.extend({
			options: {
				iconSize: [32, 37],
				iconAnchor: [16, 36],
				popupAnchor: [0, -40]
			}
		});

		var campsiteIcon = new LeafIcon({ iconUrl: '/resources/images/map-markers/campsite.png' });
		campsiteIcon.key = "campsite";
		leafIcons.push(campsiteIcon);

	}

	/*
	 * Add's a pushpin to the map.
	 */
	function addMarkerToMap(id, lat, lng, name, description, url, placeType, addPopupWindow) {

		// Get the marker icon based on the place type name.
		var placeTypeIcon = null;
		if (placeType != null && placeType.IconCssClass != null && placeType.Name.length > 0) {
			for (var i = 0; i < leafIcons.length; i++) {
				if (leafIcons[i].key == placeType.Name.toLowerCase()) {
					placeTypeIcon = leafIcons[i];
					break;
				}
			}
		}

		// Add the marker to the map.
		var marker = null;
		if (placeTypeIcon !== null) {
			marker = L.marker([lat, lng], { icon: placeTypeIcon }).addTo(map);
			mapMarkers.push(marker);
		}

		// If we need to add an info window, then do do so on popup for the marker
		if (addPopupWindow && marker != null) {
			var popupContent = createMapPopupContent(name, description, url);
			marker.bindPopup(popupContent);
		}

	}

	/**
	 * Creates the markup for the info window to be displayed.
	 */
	function createMapPopupContent(title, content, url) {

		// Set the link to null so that we can check for empty links
		url = typeof url !== 'undefined' ? url : null;

		// If there is a link, then set the link
		if (url != null) {
			title = '<a href="' + url + '">' + title + '</a>';
		}

		return '<div id="map-window">' +
	      '<h3>' + title + '</h3>' +
	      '<div class="map-window-content">' + content + '</div>' +
	      '</div>';
	};

	/**
	 * Clears all the markers from the map.
	 */
	function clearMarkers() {

		for (var i = 0; i < mapMarkers.length; i++) {
			map.removeLayer(mapMarkers[i]);
		}
	};

	/**
	 * Gets the current location and sends the coordinates to the specified text boxes.
	 */
	function getCurrentLocation(latSelector, lngSelector) {

		$('#btn-current-location').attr("disabled", "disabled");
		$('.current-location-spinner').removeClass('hidden');

		if (navigator.geolocation) {
			navigator.geolocation.getCurrentPosition(function (pos) {

				var currentLat = (Math.round(pos.coords.latitude * 1000000) / 1000000);
				var currentLng = (Math.round(pos.coords.longitude * 1000000) / 1000000);

				// Set the lat/lng in the text boxes
				$(latSelector).val(currentLat);
				$(lngSelector).val(currentLng);

				$('.current-location-spinner').addClass('hidden');
				$('#btn-current-location').removeAttr("disabled");

			});
		}

	}

	/**
	 * Initialises the map to display on the screen.
	 */
	function initMap() {

		// Set the default images dir
		L.Icon.Default.imagePath = '/assets/images/leaflet';

		if (responseHub.isMobile()) {
			$('#map-canvas').css('height', '450px');
		}

		if (typeof mapConfig != "undefined") {
			displayMap(mapConfig);
		}

	}

	// Initialise the map
	$(document).ready(function () {
		initMap();
	});

	return {
		displayMap: displayMap,
		mapMarkers: mapMarkers,
		addMarkerToMap: addMarkerToMap,
		clearMarkers: clearMarkers,
		getCurrentLocation: getCurrentLocation
	}

})();

// The location marker & lat/lng values used by the map modal.
var dialogMap = null;
var locationMarker = null;
var selectedLat = null;
var selectedLng = null;

$(document).ready(function () {

	/* Load the map when the modal is shown */
	$('#map-modal').on('hidden.bs.modal', function (e) {

		// Clear existing marker if set
		//if (locationMarker != null) {
		//	locationMarker.setMap(null);
		//}

	});

	/* Load the map when the modal is shown */
	$('#map-modal').on('shown.bs.modal', function (e) {

		var mapConfig = {
			lat: -36.854167,
			lon: 144.281111,
			zoom: 6,
			minZoom: 4,
			scrollWheel: true,
			mapContainer: 'map-canvas',
			loadCallback: null
		};

		// Only set the map if it's null.
		if (dialogMap == null) {

			dialogMap = responseHub.maps.displayMap(mapConfig);
			dialogMap.on('click', function (e) {
				selectedLat = (Math.round(e.latlng.lat * 1000000) / 1000000);
				selectedLng = (Math.round(e.latlng.lng * 1000000) / 1000000);

				$(".current-coords").text(selectedLat + ', ' + selectedLng)

				if (locationMarker == null) {
					locationMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(dialogMap);
				} else {
					locationMarker.setLatLng(e.latlng).update();
				}

			});
		}

		// Set the set coords button click
		$(".set-coords").click(function () {

			$("#Latitude").val(selectedLat);
			$("#Longitude").val(selectedLng);

			// Close the modal
			$('#map-modal').modal('hide');

		});

	});

});

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

responseHub.groups = (function () {

	function bindUI() {
		// Bind the use current location
		$('#btn-current-location').on('click', function () {
			responseHub.maps.getCurrentLocation('#Latitude', '#Longitude');
		});
	}

	// Bind the ui
	bindUI();

})();

var passwordStrength = (function () {

	function scorePassword(pass) {
		var score = 0;
		if (!pass)
			return score;

		// award every unique letter until 5 repetitions
		var letters = new Object();
		for (var i = 0; i < pass.length; i++) {
			letters[pass[i]] = (letters[pass[i]] || 0) + 1;
			score += 5.0 / letters[pass[i]];
		}

		// bonus points for mixing it up
		var variations = {
			digits: /\d/.test(pass),
			lower: /[a-z]/.test(pass),
			upper: /[A-Z]/.test(pass),
			nonWords: /\W/.test(pass),
		}

		variationCount = 0;
		for (var check in variations) {
			variationCount += (variations[check] == true) ? 1 : 0;
		}
		score += (variationCount - 1) * 10;

		return parseInt(score);
	}

	function getPasswordStrength(pass) {
		var score = scorePassword(pass);
		if (score > 70) {
			return "strong";
		}

		if (score > 50) {
			return "medium";
		}

		return "weak";
	}

	function bindUI() {

		$('input[data-password-strength-target]').on('keyup', function () {

			if ($(this).val() == "") {
				return;
			}

			// Get the password strength
			var passwordStrength = getPasswordStrength($(this).val());

			// Remove existing strength meters.
			var passwordStrengthControl = $($(this).data("password-strength-target"));
			passwordStrengthControl.removeClass("weak");
			passwordStrengthControl.removeClass("medium");
			passwordStrengthControl.removeClass("strong");

			// If there is a password strength, add it as a class to the control
			if (passwordStrength != "") {
				passwordStrengthControl.addClass(passwordStrength);
			}

		});

	}

	$(document).ready(function () {
		bindUI();
	});

})();