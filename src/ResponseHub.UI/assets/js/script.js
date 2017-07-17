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

	function toggleSidebar() {

		// If expanded, collapse it, otherwise expand it
		if ($('body').hasClass("sidebar-expanded")) {
			$('body').removeClass("sidebar-expanded");
			$('.sidebar').removeClass("sidebar-expanded");
			$('.main-content').removeClass("sidebar-expanded");
		} else {
			$('body').addClass("sidebar-expanded");
			$('.sidebar').addClass("sidebar-expanded");
			$('.main-content').addClass("sidebar-expanded");
		}

	}

	function bindModals() {
		$('#confirm-delete').on('show.bs.modal', function (e) {
			$(this).find('.btn-ok').attr('href', $(e.relatedTarget).data('href'));
		});
	}

	function bindUI() {

		// Toggle the sidebar menu
		$(".btn-sidebar-toggle").click(function () {
			toggleSidebar();
		});

		// Tab collapse
		if ($('.tab-collapse').length > 0) {
			$('.tab-collapse').tabCollapse();
		}

		// Fix date validation bug in jQuery
		$(document).ready(function () {
			jQuery.validator.methods.date = function (value, element) {

				// Create the regex and return if the value is valid or not
				var dateRegex = /^\d{2}\/\d{2}\/\d{4}$/;
				var validFormat = dateRegex.test(value);

				// If the format is invalid, then return false.
				if (!validFormat) {
					return false;
				}

				// Split the value into days, months, years
				var dateParts = value.split('/');

				// Create the moment object
				var dateObj = moment(dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0] + "T00:00:00.000Z");

				// Determine if the date is valid
				return dateObj.isValid();
			};
		});

		$('.toggle-header a').click(function () {

			// Get the toggle id
			var controlId = $(this).data('toggle-id');

			// If the control has hidden, remove it, otherwise add hidden class
			if ($('#' + controlId).hasClass('hidden'))
			{
				$('#' + controlId).removeClass('hidden');
			}
			else
			{
				$('#' + controlId).addClass('hidden');
			}

			return false;

		});

		$('.btn-search-toggle').click(function () {

			// If the control has hidden, remove it, otherwise add hidden class
			if ($('#navbar-search').css('display') == "none") {
				$('#navbar-search').css('display', 'block');
				$('body').addClass('search-bar-visible');
			}
			else {
				$('#navbar-search').css('display', 'none');
				$('body').removeClass('search-bar-visible');
			}

			return false;

		});
		
		// Bind the time picker
		$('.timepicker-control').datetimepicker({
			format: 'HH:mm',
			allowInputToggle: true,
			icons: {
				time: 'fa fa-fw fa-clock-o',
				date: 'fa fa-fw fa-calendar',
				up: 'fa fa-fw fa-chevron-up',
				down: 'fa fa-fw fa-chevron-down',
				previous: 'fa fa-fw fa-chevron-left',
				next: 'fa fa-fw fa-chevron-right',
				today: 'fa fa-fw fa-bullseye',
				clear: 'fa fa-fw fa-trash-o',
				close: 'fa fa-fw fa-times'
			}
		});

		// Bind the time picker
		$('.timepicker-seconds-control').datetimepicker({
			format: 'HH:mm:ss',
			allowInputToggle: true,
			icons: {
				time: 'fa fa-fw fa-clock-o',
				date: 'fa fa-fw fa-calendar',
				up: 'fa fa-fw fa-chevron-up',
				down: 'fa fa-fw fa-chevron-down',
				previous: 'fa fa-fw fa-chevron-left',
				next: 'fa fa-fw fa-chevron-right',
				today: 'fa fa-fw fa-bullseye',
				clear: 'fa fa-fw fa-trash-o',
				close: 'fa fa-fw fa-times'
			}
		});

		// Add read only to prevent keyboard being shown
		if (isMobile()) {
			$('.timepicker input, .timepicker-seconds input, .datepicker-control input').attr('readonly', 'readonly');
		}

		// Set the graphic radioes and checkboxes
		setGraphicRadiosCheckboxes();

		// Activate selected tab from bootstrap
		var hash = window.location.hash;
		if (hash) {
			var selectedTab = $('.nav li a[href="' + hash + '"]');
			if (selectedTab.length > 0) {
				selectedTab.trigger('click', true);
				removeHash();
			}
		}

	}

	/**
	 * Removes the hash from the url
	 */
	function removeHash() {
		var scrollV, scrollH, loc = window.location;
		if ("pushState" in history)
			history.pushState("", document.title, loc.pathname + loc.search);
		else {
			// Prevent scrolling by storing the page's current scroll offset
			scrollV = document.body.scrollTop;
			scrollH = document.body.scrollLeft;

			loc.hash = "";

			// Restore the scroll offset, should be flicker free
			document.body.scrollTop = scrollV;
			document.body.scrollLeft = scrollH;
		}
	}

	// Create the "graphic radio" and "graphic checkbox" functionality
	function setGraphicRadiosCheckboxes() {
		
		$('.graphic-radio label, .graphic-checkbox label').each(function (index, elem) {
			if ($(elem).find('i').length > 0)
			{
				return;
			}
			$(elem).contents().eq(2).wrap('<span/>');
		});

		$('.graphic-radio label input[type="radio"]').each(function (index, elem) {
			if ($(elem).parent().find('i').length > 0) {
				return;
			}
			$(elem).after('<i class="fa fa-circle-o"></i><i class="fa fa-dot-circle-o"></i>');
		});

		$('.graphic-checkbox label input[type="checkbox"]').each(function (index, elem) {
			if ($(elem).parent().find('i').length > 0) {
				return;
			}
			$(elem).after('<i class="fa fa-fw fa-square-o"></i><i class="fa  fa-fw fa-check-square-o"></i>');
		});

	}

	function overrideValidator() {
		// By default validator ignores hidden fields.
		// change the setting here to ignore nothing
		$.validator.setDefaults({ ignore: null });
	}

	/**
	 * Gets a query string value based on the parameter.
	 * @param {any} name
	 * @param {any} url
	 */
	function getQueryString(name, url) {

		// Default the url to the current location
		if (!url) {
			url = window.location.href;
		}

		// Get the name of the query string
		name = name.replace(/[\[\]]/g, "\\$&");
		var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
			results = regex.exec(url);
		if (!results) return null;
		if (!results[2]) return '';
		return decodeURIComponent(results[2].replace(/\+/g, " "));
	}

	// Bind the modal
	bindModals();

	// Bind the UI
	bindUI();

	// Override the validator ignore
	overrideValidator();

	// return the response hub object
	return {
		apiPrefix: apiPrefix,
		isMobile: isMobile,
		executeFunctionByName: executeFunctionByName,
		setGraphicRadiosCheckboxes: setGraphicRadiosCheckboxes,
		getQueryString: getQueryString
	}

})();

jQuery.fn.insertAt = function (index, element) {
	var lastIndex = this.children().size();
	if (index < 0) {
		index = Math.max(0, lastIndex + 1 + index);
	}
	this.append(element);
	if (index < lastIndex) {
		this.children().eq(index).before(this.children().last());
	}
	return this;
}

responseHub.userList = (function () {



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
		row.append('<td><a href="#" onclick="' + removeCallbackMethodName + '(this); return false;" title="Remove member" class="text-danger"><i class="fa fa-fw fa-times"></i></a></td>');
		$('#' + tableId + ' tbody').append(row);

		// Add the user id to the selected users
		$('#' + selectedId).val($('#' + selectedId).val() + user.id + '|');

		// Deselect the previous option
		$('#' + listId).selectpicker('val', '');
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

	return {
		addUserToList: addUserToList
	}

})();

responseHub.maps = (function () {

	/**
	 * Contains a dictionary of markers
	 */
	mapMarkers = {};


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
	 * The fuction to set the map bounds interval to resize the map based on your current location
	 */
	mapBoundsInterval = null;

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
		streetMapLayer = L.tileLayer('https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
			attribution: 'Imagery from <a href="http://mapbox.com/about/maps/">MapBox</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
			subdomains: 'abcd',
			id: 'juzzbott.mn25f8nc',
			accessToken: 'pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg'
		});

		topoMapLayer = L.tileLayer('https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
			attribution: 'Imagery from <a href="http://mapbox.com/about/maps/">MapBox</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
			subdomains: 'abcd',
			id: 'juzzbott.mn24imf3',
			accessToken: 'pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg'
		});

		aerialMapLayer = L.tileLayer('https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
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

	function addMarkerToMap(lat, lng) {

		mapMarkers["job_location"] = L.marker([lat, lng]).addTo(map);

	}

	/**
	 * Determines the current location on the map
	 */
	function addCurrentLocationToMap() {
		
		// Get the current location
		if (navigator.geolocation) {
			navigator.geolocation.watchPosition(
				function (pos) {

					// If there is 2 map markers in the collection, then the current position alredy exists and we just want to update it.
					// Otherwise we need to create the new map marker
					if (mapMarkers["current_location"] != null)
					{
						mapMarkers["current_location"].setLatLng(new L.LatLng(pos.coords.latitude, pos.coords.longitude));
					}
					else
					{

						// Second location marker doesn exist, so we need to create it
						var currentLocationMarker = new L.Icon({
							iconUrl: '/assets/images/map-icons/current-location.png',
							iconSize: [21, 21], // size of the icon
							iconAnchor: [10, 10], // point of the icon which will correspond to marker's location
						});	

						// Add the marker to the map
						mapMarkers["current_location"] = L.marker([pos.coords.latitude, pos.coords.longitude], { icon: currentLocationMarker }).addTo(map);

						// Add the route from LHQ to the map
						addPathFromPoint(pos.coords.latitude, pos.coords.longitude, false, '#00B226')

						// Get the group of markers, destination and current location, and zoom window to fit
						zoomToMarkerGroup([mapMarkers["job_location"], mapMarkers["current_location"]]);
						
						// Set the interval to resize the window every 30 secs.
						mapBoundsInterval = setInterval(function () {
							
							// Get the group of markers, destination and current location, and zoom window to fit
							zoomToMarkerGroup([mapMarkers["job_location"], mapMarkers["current_location"]]);

						}, 30000)

					}

				},
				function (error) {
					$('#map-messages').append('<p>' + error.code + ': ' + error.message + '</p>');
					console.log(error);
				},	
				{
					enableHighAccuracy: true,
					timeout: 15000,
					maximumAge: 0
				}
			);
		}

	}

	function addCustomLocationMarkerToMap(lat, lon, icon, iconSize, iconAnchor, popupAnchor)
	{

		var customMarker = new L.Icon({
			iconUrl: '/assets/images/map-icons/' + icon + '.png',
			iconSize: iconSize, // size of the icon
			iconAnchor: iconAnchor, // point of the icon which will correspond to marker's location
			popupAnchor: popupAnchor
		});	
		
		// Add the marker to the map
		return L.marker([lat, lon], { icon: customMarker }).addTo(map);
	}

	function zoomToMarkerGroup(markers)
	{
		// Get the group of markers, destination and current location, and zoom window to fit
		var group = new L.featureGroup(markers);
		map.fitBounds(group.getBounds().pad(0.1));
	}

	function addLhqMarker(lat, lon)
	{

		// Create the custom marker
		var lhqMarker = new L.Icon({
			iconUrl: '/assets/images/map-icons/lhq-marker.png',
			iconSize: [24, 25], // size of the icon
			iconAnchor: [12, 12], // point of the icon which will correspond to marker's location
		});	

		// Add the marker to the map
		mapMarkers["lhq_location"] = L.marker([lat, lon], { icon: lhqMarker }).addTo(map);

		// Add the route from LHQ to the map
		addPathFromPoint(lat, lon, true, '#FF862F');

	}

	function addPathFromPoint(lat, lon, isDistanceFromLhq, pathColour)
	{


		var start_loc = lat + ',' + lon;
		var end_loc = mapMarkers["job_location"].getLatLng().lat + ',' + mapMarkers["job_location"].getLatLng().lng;

		// Get the directions to the location
		$.ajax({
			url: responseHub.apiPrefix + '/google-api/directions?start_loc=' + start_loc + '&end_loc=' + end_loc,
			dataType: 'json',
			success: function (data) {

				if (data != null) {
					var latlngs = [];

					// Loop through the results and create the LatLng objects to add to the poly line
					for (var i = 0; i < data.Coordinates.length; i++) {
						latlngs.push(new L.LatLng(data.Coordinates[i].Latitude, data.Coordinates[i].Longitude))
					}

					if (isDistanceFromLhq == true && $('.lhq-dist-set').length == 0) {
						$('.dist-from-lhq p').empty();
						$('.dist-from-lhq p').text((Math.round((data.TotalDistance / 1000) * 10) / 10) + ' km');
					}

					// Now that we have the lat lngs, add the path to the map
					L.polyline(latlngs, { color: pathColour, weight: 6, opacity: 0.4, clickable: false }).addTo(map);

					// Get the group of markers, destination and current location, and zoom window to fit
					if (!responseHub.isMobile()) {
						zoomToMarkerGroup([mapMarkers["job_location"], mapMarkers["lhq_location"]]);
					}
				}

			}
		});
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
	function clearMarkers(markers) {

		var markersToRemove = (markers != null ? markers : mapMarkers);

		for (var i = 0; i < markersToRemove.length; i++) {
			map.removeLayer(markersToRemove[i]);
		}

		// Clear the markers
		if ($('.leaflet-marker-pane').length > 0) {
			$('.leaflet-marker-pane').empty();
		}

		// Clear the shadows
		if ($('.leaflet-shadow-pane').length > 0) {
			$('.leaflet-shadow-pane').empty();
		}
	};

	/**
	 * Sets the map center to the specified coordinates
	 */
	function setMapCenter(lat, lon) {
		map.setView(new L.LatLng(lat, lon));
	}

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
	 * Determines if the map exists
	 */
	function mapExists() {
		return map != null;
	}

	/**
	 * Initialises the map to display on the screen.
	 */
	function initMap() {

		// Set the default images dir
		L.Icon.Default.imagePath = '/assets/images/leaflet/';

		if (responseHub.isMobile()) {
			$('#map-canvas').css('height', '450px');
		}

		if (typeof mapConfig != "undefined") {
			map = displayMap(mapConfig);
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
		getCurrentLocation: getCurrentLocation,
		setMapCenter: setMapCenter,
		mapExists: mapExists,
		addCurrentLocationToMap: addCurrentLocationToMap,
		addLhqMarker: addLhqMarker,
		addCustomLocationMarkerToMap: addCustomLocationMarkerToMap,
		zoomToMarkerGroup: zoomToMarkerGroup
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

responseHub.jobMessages = (function () {

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

	}

	/**
	 * Adds the job note to the job and displays on the screen.
	 */
	function addJobNote() {

		var jobNote = $('#txtJobNote').val();
		var isWordback = $('#chkWordBack').is(':checked');
		var userDisplayName = $('#note-form').data('user-display-name');

		var postData = {
			Body: jobNote,
			isWordback: isWordback
		};

		// Set the spinner
		$("#btnAddNote").find('i').removeClass('fa-comment-o').addClass('fa-refresh fa-spin');
		$("#btnAddNote").attr('disabled', 'disabled');

		var jobId = $('#Id').val();

		// Clear any existing alerts
		$(".job-note-messages .alert").remove();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/notes',
			type: 'POST',
			dataType: 'json',
			data: postData,
			success: function (data) {

				if (data == null) {
					$(".job-note-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding your note. Please try again shortly.</p>');
					return;
				}

				var noteDate = moment(data.Date);

				var noteMarkup = buildJobNoteMarkup(data.Id, data.Body, noteDate.format('YYYY-MM-DD HH:mm:ss'), data.IsWordBack, userDisplayName);

				$('#job-notes ul').prepend(noteMarkup);
				$('#job-notes').removeClass('hidden');

			},
			error: function (jqXHR, textStatus, errorThrown) {

				// Show the error message
				$(".job-note-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding your note. Please try again shortly.</p>');

			},
			complete: function (jqXHR, textStatus) {

				// Reset the button
				$("#btnAddNote").find('i').addClass('fa-comment-o').removeClass('fa-refresh fa-spin');
				$('#txtJobNote').val('');

			}
		});

	}

	/**
	 * Builds the markup for the job note to be added to the page.
	 */
	function buildJobNoteMarkup(noteId, body, date, isWordback, userDisplayName) {

		// Create the note list item
		var noteListItem = $('<li data-job-note-id="' + noteId + '"></li>');

		// Generate wordback markup if required
		var wordbackMarkup = isWordback ? ' <i class="fa fa-commenting-o wordback-icon"></i> wordback' : '';

		var userDisplayNameMarkup = ' <i class="fa fa-user user-icon"></i> ' + userDisplayName;

		// Append the meta and note information to the list item
		noteListItem.append('<small class="text-muted"><i class="fa fa-clock-o"></i> ' + date + wordbackMarkup + userDisplayNameMarkup + '</small>');
		noteListItem.append('<p class="text-info">' + body + '</p>');

		// return the node list item
		return noteListItem;

	}

	/**
	 * Sets the job status when the button is clicked.
	 */
	function setJobStatusTime(statusType, progressDateTime, sender) {

		var intStatusType;

		switch (statusType) {

			case "on_route":
				intStatusType = 1;
				break;

			case "on_scene":
				intStatusType = 2;
				break;

			case "job_clear":
				intStatusType = 3;
				break;

		}

		// Set the spinner
		if (sender != null) {
			$(sender).find('i').removeClass('fa-check-square-o').addClass('fa-refresh fa-spin');
			$(sender).attr('disabled', 'disabled');
		}

		var jobId = $('#Id').val();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + jobId + '/progress',
			type: 'POST',
			dataType: 'json',
			data: { ProgressType: intStatusType, ProgressDateTime: progressDateTime, Version: parseInt($('#Version').val()) },
			success: function (data) {

				// If there is a failed result, display that
				if (data.Success == true) {

					console.log(data);

					// If the user was signed in, we want to send them to the sign in page to sign other members in, otherwise we just want to display the progress updates
					if (data.UserSignedIn) {
						console.log('redirecting...');
						window.location = window.location + '/sign-in';
					}
					else {

						var progressDate = moment(data.Timestamp).local();

						switch (statusType) {

							case "on_route":
								addProgressMarkup($('.progress-on-route'), "On route", progressDate, data.UserFullName);
								break;

							case "on_scene":
								addProgressMarkup($('.progress-on-scene'), "On scene", progressDate, data.UserFullName);
								break;

							case "job_clear":
								addProgressMarkup($('.progress-job-clear'), "Job clear", progressDate, data.UserFullName);
								break;

						}

						// Update the version
						$('#Version').val(data.NewVersion);

						if (sender != null) {
							$(sender).remove();
						}
						else {
							// Sender is null, so sender is actually the edit form, so we want to close the form
							closeEditProgressForm();
						}
					}

				} else { // data.Success = false

					// Reset the button
					if (sender != null) {
						$(sender).find('i').addClass('fa-check-square-o').removeClass('fa-refresh fa-spin');
						$(sender).removeAttr('disabled');
					}
					else 
					{
						// Sender is null, so sender is actually the edit form, so we want to close the form
						closeEditProgressForm();
					}

					// Clear any existing alerts
					$(".progess-messages .alert").remove();

					// Display the error message
					$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.ErrorMessage + '</p>');

				}

			},
			error: function (jqXHR, textStatus, errorThrown) {
				// Reset the button

				if (sender != null) {
					$(sender).find('i').addClass('fa-check-square-o').removeClass('fa-refresh fa-spin');
					$(sender).removeAttr('disabled');
				}
				else {
					// Sender is null, so sender is actually the edit form, so we want to close the form
					closeEditProgressForm();
				}

				// Clear any existing alerts
				$(".progess-messages .alert").remove();

				// SHow the error message
				$(".progess-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error saving the job message progress.</p>');
			}
		});

	}

	/**
	 * Adds the progress markup to the dom.
	 */
	function addProgressMarkup(elem, progressType, date, userFullName) {

		// empty the current markup to prevent duplicate markup entries
		$(elem).empty();

		$(elem).append("<h4>" + progressType + "</h4>");
		$(elem).append('<span class="btn-icon"><i class="fa fa-fw fa-clock-o"></i>' + date.format('YYYY-MM-DD HH:mm:ss') + '</span><br />');
		$(elem).append('<span class="text-muted btn-icon"><i class="fa fa-fw fa-user"></i>' + userFullName + '</span>');
		$(elem).append('<div><a class="btn btn-link btn-icon action-edit"><i class="fa fa-fw fa-pencil-square-o"></i>Edit</a><a class="btn btn-link btn-icon action-undo"><i class="fa fa-fw fa-undo"></i>Undo</a></div>');

		// Rebind the edit and undo options
		bindJobProgressEditUndo();

	}

	/**
	 * Sends the updated progress type to the server.
	 */
	function submitEditProgressTime()
	{

		// Get the specified time
		var dateVal = $('#EditProgressDate').val() + " " + $('#EditProgressTime').val();

		// Get the progress type
		var progressType = $('#ProgressType').val();

		// Set the button sending status
		$('#edit-progress-update button').addClass('disabled');
		$('#edit-progress-update button').attr('disabled', 'disabled');
		$('#edit-progress-update button i').removeClass('fa-check').addClass('fa-spinner fa-spin');


		// Submit the details of the update
		setJobStatusTime(progressType, dateVal, null);


	}

	/**
	 * Closes the edit progress form controls.
	 */
	function closeEditProgressForm()
	{
		$('#edit-progress-update').addClass('hidden');
		$('#edit-progress-update button').removeClass('disabled');
		$('#edit-progress-update button').removeAttr('disabled');
		$('#edit-progress-update button i').removeClass('fa-spinner').removeClass('fa-spin').addClass('fa-check');
	}

	/**
	 * Gets the next set of pager messages to display.
	 */
	function getNextJobMessages(messageType) {

		// Get the skip count
		var skipCount = $(".job-list li").length;

		$('#jobs-load-more .loading').removeClass('hidden');
		$('#jobs-load-more button').addClass('hidden');

		// Get the date_from and date_to
		var dateTo = responseHub.getQueryString('date_to');
		var dateFrom = responseHub.getQueryString('date_from');

		// Build the filter query
		var filterQuery = '';
		if (dateTo != null || dateFrom != null)
		{
			filterQuery = 'date_from=' + dateFrom + '&date_to=' + dateTo;
		}

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/?skip=' + skipCount + '&msg_type=' + messageType + filterQuery,
			dataType: 'json',
			success: function (data) {

				for (var i = 0; i < data.length; i++) {

					addMessageToList(data[i], true);

				}

				// If there is zero results, hide the show more button
				if (data.length == 0) {
					$("#jobs-load-more").remove();
				} else {
					$('#jobs-load-more .loading').addClass('hidden');
					$('#jobs-load-more button').removeClass('hidden');
				}

			}
		});

	}

	/**
	 * Adds the job message to the list of existing job messages.
	 * @param {any} pagerMessage
	 * @param {any} append
	 */
	function addMessageToList(jobMessage, append) {

		var listItem = $('<li data-message-id="' + jobMessage.Id + '"></li>');

		// Get the job date
		var jobDate = moment(jobMessage.Timestamp);
		var localDateString = jobDate.local().format('DD/MM/YYYY HH:mm:ss');

		// Create the header
		var header = $('<h3></h3>');

		// Set the priority
		// Add the priority icon
		switch (jobMessage.Priority) {
			case 1:
				header.append('<i class="fa fa-fw fa-exclamation-triangle p-message-emergency"></i>');
				break;

			case 2:
				header.append('<i class="fa fa-fw fa-exclamation-circle p-message-non-emergency"></i>');
				break;

			default:
				header.append('<i class="fa fa-fw fa-info-circle p-message-admin"></i>');
				break;
		}

		// If there is job number, then show it here
		if (jobMessage.JobNumber != "") {
			header.append('<a href="/jobs/' + jobMessage.Id + '">' + jobMessage.JobNumber + '</a><span class="text-muted message-date btn-icon">' + localDateString + ' </span>');
		}
		else {
			header.append('<a href="/jobs/' + jobMessage.Id + '">' + localDateString + '</a>');
		}

		// Add the header
		listItem.append(header);

		// Add the job meta data
		var metaContainer = $('<p class="job-message-meta text-muted"></p>');
		var statusSpan = $('<span class="job-status"></span>');

		// Set the job status
		if (jobMessage.Cancelled != null)
		{
			statusSpan.append('<i class="fa fa-ban job-cancelled"></i>');
		}
		else if (jobMessage.JobClear != null)
		{
			statusSpan.append('<i class="fa fa-check-circle-o job-clear"></i>');
		}
		else if (jobMessage.OnScene != null)
		{
			statusSpan.append('<i class="fa fa-hourglass-half on-scene"></i>');
		}
		else if (jobMessage.OnRoute != null)
		{
			statusSpan.append('<i class="fa fa-arrow-circle-o-right on-route"></i>');
		}
		else
		{
			statusSpan.append('<i class="fa fa-asterisk"></i>');
		}

		// Add the job status span to the metadata container
		metaContainer.append(statusSpan);

		// Add the unit capcode name
		metaContainer.append('<span class="capcode-unit-name">' + jobMessage.CapcodeUnitName + '</span>');

		// Append the job message meta
		listItem.append(metaContainer);

		// Append the pager message content
		listItem.append($('<p>' + jobMessage.MessageBody + '</p>'));

		// Append the list item to list of jobs
		if (append) {
			$(".job-list").append(listItem);
		} else {
			$(".job-list").prepend(listItem);
		}

	}

	/**
	 * Gets the distance between two jobs, and displays to the user.
	 */
	function getDistanceBetweenJobs()
	{

		// Ensure the form is valid
		if (!$('#dist-between-jobs form').valid())
		{
			return;
		}
		
		// Disable the button to start with
		$('#dist-between-jobs button').attr('disabled', 'disabled');
		$('#dist-between-jobs button').addClass('disabled');
		$('#dist-between-jobs button i').removeClass('fa-search').addClass('fa-spin fa-spinner');

		// Get the current job id and referenced job number
		var currentJobId = $('#Id').val();
		var referencedJobNumber = $('#DistanceFromJobNumber').val();

		// Clear any previous results
		$('#dist-results').empty();


		$.ajax({
			url: responseHub.apiPrefix + '/job-messages/' + currentJobId + '/distance-from-job/' + referencedJobNumber,
			dataType: 'json',
			success: function (data) {

				if (data != null)
				{

					// Is not successull, then display error message
					if (!data.Success)
					{
						$('#dist-results').append('<div class="alert alert-danger alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.Error + '</div>');
					}
					else
					{
						$('#dist-results').append('<h3>Distance results:</h4>');
						$('#dist-results').append('<p>Distance to job <strong><a href="/jobs/' + data.ReferencedJobId + '">' + data.ReferencedJobNumber + '</a></strong>: ' + (Math.round((data.Distance / 1000) * 10) / 10) + ' km');
					}

				}

			}, 
			error: function (jqXHR, textStatus, errorThrown) {
				$('#dist-results').append('<div class="alert alert-danger alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>There was an error determining the distance between jobs.</div>');
			},
			complete: function () {
				// Re-enable the button to start with
				$('#dist-between-jobs button').removeAttr('disabled');
				$('#dist-between-jobs button').removeClass('disabled');
				$('#dist-between-jobs button i').removeClass('fa-spin fa-spinner').addClass('fa-search');
			}
		});

	}

	/**
	 * Set the signed in member to the list
	 */
	function setSignedInMember(elem) {

		// Get the current list of members
		var selectedMembers = $('#SelectedMembers').val();

		// Get the selected if
		var selectedMemberId = $(elem).val() + '|';

		// If the element is checked, add the id to the list, otherwise remove it from the list
		if ($(elem).is(':checked'))
		{
			$('#SelectedMembers').val(selectedMembers + selectedMemberId);
		}
		else
		{
			$('#SelectedMembers').val(selectedMembers.replace(selectedMemberId, ''));
		}

	}

	/**
	 * Binds the job progress actions
	 */
	function bindJobProgressEditUndo()
	{

		// Unbind all click events
		$('.progress-action .action-edit').off('click');
		$('.progress-action .action-undo').off('click');

		// Set the click event handler for the progress action
		$('.progress-action .action-edit').click(function () {
			
			// From the undo link, get the progress action container
			var progressAction = $(this).closest('.progress-action');

			// Get the progress type from the data attribute
			var progressType = progressAction.data('progress-type');

			// Set the progress type hidden value and the h3 title
			$('#ProgressType').val(progressType);

			headerText = "Update progress";
			if (progressType == "on_route")
			{
				headerText = "Update on route time";
			}
			else if (progressType == "on_scene")
			{
				headerText = "Update on scene time";
			}
			else if (progressType == "job_clear")
			{
				headerText = "Update job clear time";
			}

			// Show the update form.
			var currentDate = moment().local();
			$('#EditProgressDate').val(currentDate.format('YYYY-MM-DD'));
			$('#EditProgressTime').val(currentDate.format('HH:mm:ss'));
			$('#edit-progress-update h4').text(headerText);
			$('#edit-progress-update').removeClass('hidden');

			// If we are on mobile, then scroll to the form
			if (responseHub.isMobile()) {
				$(window).scrollTop($("#edit-progress-update").offset().top - 50);
			}

		});

		// Bind the 'undo' event
		$('.progress-action .action-undo').click(function () {

			var intStatusType;
			
			// From the undo link, get the progress action container
			var progressAction = $(this).closest('.progress-action');

			// Get the progress type from the data attribute
			var progressType = progressAction.data('progress-type');

			switch (progressType) {

				case "on_route":
					intStatusType = 1;
					break;

				case "on_scene":
					intStatusType = 2;
					break;

				case "job_clear":
					intStatusType = 3;
					break;

			}

			// Get the job id
			var jobId = $('#Id').val();

			// Get the element from the curret click event
			var elem = $(this);

			// Disable and set spinner
			$(elem).attr('disabled', 'disabled');
			$(elem).addClass('disabled');
			$(elem).find('i').removeClass('fa-undo').addClass('fa-spinner fa-spin');

			// Close the edit form just in case it's for the same type
			closeEditProgressForm();

			// Create the ajax request
			$.ajax({
				url: responseHub.apiPrefix + '/job-messages/' + jobId + '/progress/delete',
				type: 'POST',
				dataType: 'json',
				data: { ProgressType: intStatusType, Version: parseInt($('#Version').val()) },
				success: function (data) {

					// If there is a failed result, display that
					if (data.Success == true) {

						// Remove the progress details
						progressAction.find('.progress-time').empty();

						// Get the button label
						var buttonLabel = '';
						var buttonClass = '';
						if (progressType == "on_route") {
							buttonLabel = 'On route';
							buttonClass = 'btn-on-route';
						}
						else if (progressType == "on_scene") {
							buttonLabel = 'On scene';
							buttonClass = 'btn-on-scene';
						}
						else if (progressType == "job_clear") {
							buttonLabel = 'Job clear';
							buttonClass = 'btn-job-clear';
						}

						// Add the progress button back in
						progressAction.append('<button class="btn btn-primary btn-icon btn-block btn-lg ' + buttonClass + '"><i class="fa fa-fw fa-check-square-o"></i> ' + buttonLabel + '</button>');

						// Rebind the progress actions
						bindJobProgressActions();

						// Update the version
						$('#Version').val(data.NewVersion);

					}
					else
					{

						// Reset the button
						$(elem).removeAttr('disabled');
						$(elem).removeClass('disabled');
						$(elem).find('i').removeClass('fa-spinner fa-spin').addClass('fa-undo');

						// Clear any existing alerts
						$(".progess-messages .alert").remove();

						// Display the error message
						$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' + data.ErrorMessage + '</p>');

					}

				}, 
				error: function (jqXHR, textStatus, errorThrown)
				{
					// Re-enable spinner
					$(progressAction).find('.action-undo').removeAttr('disabled');
					$(progressAction).find('.action-undo').removeClass('disabled');
					$(progressAction).find('.action-undo i').removeClass('fa-spinner').removeClass('fa-spin').addClass('fa-undo');
					
					// Clear any existing alerts
					$(".progess-messages .alert").remove();

					// Display the error message
					$(".progess-messages").append('<div class="alert alert-warning alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error clearing job progress.</p>');

				}
			});

		});

	}

	/**
	 * Bind the job progress button events.
	 */
	function bindJobProgressActions()
	{

		$(".btn-on-route").off('click');
		$(".btn-on-route").click(function () {
			setJobStatusTime('on_route', "", this);
		});

		$(".btn-on-scene").off('click');
		$(".btn-on-scene").click(function () {
			setJobStatusTime('on_scene', "", this);
		});

		$(".btn-job-clear").off('click');
		$(".btn-job-clear").click(function () {
			setJobStatusTime('job_clear', "", this);
		});

	}

	/**
	 * Binds the UI controls
	 */
	function bindUI() {

		// Bind the progress update actions
		bindJobProgressActions();
		bindJobProgressEditUndo();

		$('#btnAddNote').click(function () {
			addJobNote();
		});

		$('#txtJobNote').on('keyup', function () {

			if ($(this).val() == '') {
				$('#btnAddNote').attr('disabled', 'disabled');
			} else {
				$('#btnAddNote').removeAttr('disabled');
			}

		});

		$('#confirm-delete.delete-attachment').on('show.bs.modal', function (e) {

			// Generate the confirm message
			var message = $(this).find('.modal-body p').text();
			message = message.replace('#FILE#', $(e.relatedTarget).data('filename'));

			// Set the confirm message
			$(this).find('.modal-body p').text(message);

		});
		
	}

	// Bind the UI
	bindUI();

	return {
		getNextJobMessages: getNextJobMessages,
		submitEditProgressTime: submitEditProgressTime,
		closeEditProgressForm: closeEditProgressForm,
		getDistanceBetweenJobs: getDistanceBetweenJobs,
		setSignedInMember: setSignedInMember
	};

})();

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

		// Set the priority
		// Add the priority icon
		switch (pagerMessage.Priority) {
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
			topMarkup += pagerMessage.JobNumber + " - "
		}

		// Close the job and icon section
		topMarkup += "</strong>";

		// Get the job date
		var jobDate = moment(pagerMessage.Timestamp);
		var localDateString = jobDate.format('HH:mm:ss D MMMM YYYY');
		topMarkup += '<span class="text-info">' + localDateString + '</span> - <span class="text-muted">(' + pagerMessage.Capcode + ')</span>';
		// Append the top row
		topRow.append($(topMarkup));

		// Append the top row to the list item
		listItem.append(topRow);
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

responseHub.units = (function () {

	function bindUI() {
		// Bind the use current location
		$('#btn-current-location').on('click', function () {
			responseHub.maps.getCurrentLocation('#Latitude', '#Longitude');
		});

		$('#confirm-delete.delete-user').on('show.bs.modal', function (e) {
			
			// Generate the confirm message
			var message = $(this).find('.modal-body p').text();
			message = message.replace('#USER#', $(e.relatedTarget).data('user-name'));

			// Set the confirm message
			$(this).find('.modal-body p').text(message);

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

responseHub.capcodes = (function () {


	/**
	 * Sets the capcode tag from clicking or tabbing the item in the auto-complete box
	 */
	function setCapcodeTag(name, capcodeAddress, id) {

		// Get the current capcode ids
		capcodeIds = $('#AdditionalCapcodes').val();

		// First, check to ensure the cap code doesn't already exist. If it does, then just exist.
		if (capcodeIds.indexOf(id) >= 0) {
			// Capcode already selected
			return;
		}

		// Add the capcode tag
		var capcodeTag = $('<span class="label label-primary" data-capcode-id="' + id + '">' + name + ' (' + capcodeAddress + ')<a><i class="fa fa-times"></i></a></span>');
		capcodeTag.find('a').click(function () {

			// Get the id of the capcode
			var capcodeId = $(this).parent().data('capcode-id');

			// Remove the capcode tag
			removeCapcodeTag(capcodeId);

		});

		// Add the current id to the list of capcode ids
		capcodeIds += id + ",";

		// Remove the hidden tag from the capcode tag list and append the capcode tag
		$('#AdditionalCapcodes').val(capcodeIds);
		$('.capcode-list-tags').removeClass('hidden');
		$('.capcode-list-tags').append(capcodeTag);

	}

	function removeCapcodeTag(id) {

		// Get the current capcode ids
		capcodeIds = $('#AdditionalCapcodes').val();

		// Remove the capcode id
		capcodeIds = capcodeIds.replace(id + ",", "");
		$('#AdditionalCapcodes').val(capcodeIds);

		// Remove the tag from the list
		$('.capcode-list-tags').find("[data-capcode-id='" + id + "']").remove();

		if ($('.capcode-list-tags').children().length == 0) {
			$('.capcode-list-tags').addClass('hidden');
		}

	}

	function bindCapcodeAutocomplete() {

		// Set the autocomplete functionality for capcodes.
		$("input[data-capcode-autocomplete='true']").typeahead({
			source: unitCapcodes,
			onSelect: function (item) {
				$("input[data-capcode-autocomplete='true']").val(item.value);
			}
		});
	}

	/**
	 * Binds the events to the UI controls.
	 */
	function bindUI() {

		$(document).ready(function () {
			$('#AvailableCapcodes').on('changed.bs.select', function (e) {
			
				// Get the selected id
				var selectedId = $('#AvailableCapcodes').selectpicker('val');

				// If there is no selected id, just return
				if (selectedId == "") {
					return;
				}

				// Get the option that was selected
				var selectedOpt = $("#AvailableCapcodes option[value='" + selectedId + "']");

				// Add the tag to the list
				if (selectedOpt.length > 0) {
					setCapcodeTag(selectedOpt.data('name'), selectedOpt.data('capcode-address'), selectedId);
				}

			});
		});

		// Clicke event for capcodes rendered on the page
		$('.capcode-list-tags span a').click(function () {

			// Get the id of the capcode
			var capcodeId = $(this).parent().data('capcode-id');

			// Remove the capcode tag
			removeCapcodeTag(capcodeId);

		});

		if ($("input[data-capcode-autocomplete='true']").length > 0) {
			bindCapcodeAutocomplete();
		}

	}

	// Bind the ui elements.
	bindUI();

	// Create the return object
	return {
		setCapcodeTag: setCapcodeTag
	}

})();

responseHub.resources = (function () {

	/** 
	 * Adds a resource to the system for the specified unit. 
	 */
	function addResource() {

		var buttonCtl = $("#add-resource");

		// Set the spinner
		buttonCtl.find('i').removeClass('fa-plus').addClass('fa-refresh fa-spin');
		buttonCtl.attr('disabled', 'disabled');

		// Get the unit id
		var eventId = $("#EventId").val();

		// Create the post data object
		var postData = {
			'Name': $("#Name").val(),
			'AgencyId': $("#AgencyId").val(),
			'UserId': null,
			'Type': 2 // Additional resource
		};

		// Clear any existing alerts
		$(".add-resource-messages .alert").remove();

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/events/' + eventId + '/resources',
			type: 'POST',
			dataType: 'json',
			data: postData,
			success: function (data) {

				if (data == null) {
					// Show the error message
					$(".add-resource-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding the resource. Please try again shortly.</p>');
					return;
				}

			},
			error: function (jqXHR, textStatus, errorThrown) {
				
				// Show the error message
				$(".add-resource-messages").append('<div class="alert alert-danger alert-dismissable" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Sorry, there was an error adding the resource. Please try again shortly.</p>');

			},
			complete: function () {

				// Reset the button
				buttonCtl.find('i').addClass('fa-plus').removeClass('fa-refresh fa-spin');
				buttonCtl.removeAttr('disabled');

				// Reset fields
				$("#AdditionalResourceModel_Name").val('');
				$("#AdditionalResourceModel_AgencyId").val('');

			}
		});
	}

	return {
		addResource: addResource
	}

})();

responseHub.logs = (function () {

	function bindUI() {

		if ($('.log-file-select').length > 0) {

			$('.log-file-select').on('change', function () {

				// Get the current location without querystrings
				var url = window.location.href.split('?')[0];

				// Get the log file
				var logFile = $(this).val();

				// redirect to the selected log file
				window.location.href = url + "?file=" + logFile;

			});

		}

	}

	// Bind the UI
	bindUI();

})();

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

responseHub.attachments = (function () {

	function bindUI() {

		Dropzone.options.dropzoneAttachments = {
			maxFilesize: 100,
			previewsContainer: false,
			init: function () {
				this.on("success", function (file, response) {
					
					// Create the file in the list of attachments
					addAttachment(file, response);

					// Get the extension
					var match = file.name.match(/^.*(\.[a-zA-Z0-9_]+)$/);

					// If there is an ext match
					if (match != null && match.length > 1) {
						var ext = match[1].toLowerCase();

						// If the extension is an img extension, then also add to the gallery
						if (ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".png" || ext == ".gif")
						{
							addGalleryImage(file, response);
						}

					}

					// Update the attachment count
					$('#tab-header-attachments span').text($('.attachment-list li').length);

				});

				this.on("addedfile", function (file) {

					// Clear the error list
					$('#upload-messages').empty();

					// Show the 'uploading' message
					$('.dz-message').empty();
					$('.dz-message').append('<span class="btn-icon"><i class="fa fa-spinner fa-pulse fa-fw"></i>Uploading files...</span>');


				});

				this.on("complete", function () {

					// Reset the 'drop files here' message
					$('.dz-message').empty();
					$('.dz-message').append('<span>Drop files here to upload</span>');

				});
			}
		};

	}

	/**
	 * Adds the uploaded attachment to the list of attachments.
	 */
	function addAttachment(file, response) {

		// Create the list item
		var listItem = $('<li></li>');

		// If the responseObj is null or the success is false, then exit showing error message
		if (response == null || !response.success)
		{
			var errorMessage = (response != null ? response.message : "Unable to upload file.");
			$('#upload-messages').append('<div class="alert alert-danger">Error uploading your file: ' + errorMessage + '</div>');
			return;
		}

		// get the current datetime
		var date = moment().local().format('DD/MM/YYYY HH:mm');

		// add the link span
		listItem.append('<span class="btn-icon"><i class="fa fa-fw fa-download"></i><a href="/media/attachment/' + response.id + '">' + file.name + '</a></span>');
		listItem.append('<span class="attachment-meta text-muted"> ' + date + ' <em>(' + file.type + '</em> ' + getFileSizeDisplay(file.size) + ')</span>');
		listItem.append('<span class="pull-right remove-attachment"><a data-href="/jobs/' + response.jobId + '/remove-attachment/' + response.id + '" title="Remove attachment ' + response.filename + '" data-filename="' + response.filename +'" data-toggle="modal" data-target="#confirm-delete"><i class="fa fa-fw fa-times"></i></a></span>');

		$('.attachment-list').prepend(listItem);

	}

	/**
	 * Adds the image to the image gallery to be used within the attachment image gallery
	 * 
	 */
	function addGalleryImage(file, response)
	{

		// Create the image item
		var imgDiv = $('<div class="image-item"><a href="/media/attachment-resized/' + response.id + '?w=1024&h=768" title="' + file.name + '" data-gallery=""><img src="/media/attachment-resized/' + response.id + '?w=167&h=125"></a></div>');

		// prepend to the links list to be included in the gallery
		$('#attachment-gallery').prepend(imgDiv);

	}

	function getFileSizeDisplay(size) {

		if (size < 1)
		{
			return "0 kb";
		}
		else if (size < 1000000)
		{
			var kb = parseFloat(size / 1000).toFixed(2);
			return kb + " kb";
		}
		else
		{
			var mb = parseFloat(size / 1000000).toFixed(2);
			return mb + " mb";
		}

	}

	$(document).ready(function () {
		if ($('#dropzone-attachments').length > 0) {
			bindUI();
		}
	});

	function preloadAttachments() {

		// If we already have the preload container, then just exit as we don't want to re-add the images
		if ($('#attachment-preload').length > 0) {
			return;
		}

		// Create the attachments container
		var attachments = $('<div id="attachment-preload"></div>');

		// Loop through each image attachment
		$('#attachment-gallery a').each(function () {

			// Get the url to the img
			var imgUrl = $(this).attr('href');
			
			// Add the image to the preload list so that it's preloaded
			attachments.append('<img src="' + imgUrl + '" />');

		});

		$('body').append(attachments);

	}

	return {
		preloadAttachments: preloadAttachments
	}


})();

responseHub.gallery = (function () {

})();

responseHub.wallboard = (function () {

	var currentRadarImageIndex = { };



	/**
	 * Determines if the specific warnings should be displayed on the screen.
	 */
	function showHideWarnings(warningsContainer) {

		// If the container has the hidden class, remove it, otherwise add it
		if ($('.' + warningsContainer).hasClass('hidden')) {
			$('.' + warningsContainer).removeClass('hidden')
		}
		else {
			$('.' + warningsContainer).addClass('hidden')
		}

	}

	/**
	 * Loads the UI of the weather centre
	 */
	function loadUI() {

		// Get the radar image urls
		loopRadarImageUrls();
	}

	/**
	 * Creates the loop for iterating through the list of radar images.
	 */
	function loopRadarImageUrls() {

		$('.radar-container').each(function (index, elem) {


			// If there are no radar images, then show error message
			if ($(elem).find('.radar-image-preload').length == 0 || $(elem).find('.radar-image-preload img').length == 0) {
				$(elem).empty();
				$(elem).append('<div class="error-summary ">Unable to load radar information.</div>');
				return;
			}

			// Get the radar code
			var radarCode = $(elem).data('radar-code');
			currentRadarImageIndex[radarCode] = 0;

			// Get the initial image and image count
			var initialImage = $(elem).find('.radar-image-preload img:first');
			var imageCount = $(elem).find('.radar-image-preload img').length;

			setInterval(function () {
			
				// Get the next radar image. If the index exceeds the length, reset back to index 0
				nextIndex = currentRadarImageIndex[radarCode] + 1;
				if (nextIndex >= imageCount) {
					nextIndex = 0;
				}
			
				var indexImage = $(elem).find('.radar-image-preload img:eq(' + nextIndex + ')');
						
				// Get the radar image div and update the background property
				$(elem).find('.radar-loop').css('background', 'url(\'' + indexImage.attr('src') + '\')');
			
				// Reset the current index
				currentRadarImageIndex[radarCode] = nextIndex;
			
			}, 250);

		});

		setTimeout(function () {
			$('.radar-image.radar-loop').css('display', 'block');
			$('.radar-loading').remove();
		}, 2000);

	}

	// Load the UI
	loadUI();

	// return object
	return {
		showHideWarnings: showHideWarnings
	}

})();

responseHub.signIn = (function () {

	// Sets the job number and the job id when selected.
	function setOperationJobNumber(jobNumber, jobId) {

		// Set the jbo number and id in the hidden field
		$('#OperationDescription').val(jobNumber);
		$('#OperationJobId').val(jobId);
	}

	// Shows the elements for operation details
	function setActivityDetails(activity) {
		$('#operation-task').addClass('hidden');
		$('#training-task').addClass('hidden');
		$('#other-task').addClass('hidden');

		// Clear any error messages
		$('.training-types-messages .field-validation-error').empty().addClass('field-validation-valid').removeClass('field-validation-error');
		$('#operation-task .input-validation-error, #training-task .input-validation-error, #other-task .input-validation-error').removeAttr('aria-invalid').removeClass('input-validation-error');
		$('.validation-summary-errors').addClass('validation-summary-valid').removeClass('validation-summary-errors');
		$('.validation-summary-valid ul').empty().append('<li style="display:none"></li>');

		switch (activity) {
			case 'operation':
				$('#operation-task').removeClass('hidden');
				break;

			case 'training':
				$('#training-task').removeClass('hidden');
				break;

			case 'other':
				$('#other-task').removeClass('hidden');
				break;
		}
	}

	// Shows the sign out form for the specific sign in entry
	function showSignOutForm(elem) {

		$(elem).addClass('hidden');
		$(elem).closest('.sign-out-row').find('form').removeClass('hidden');

	}

	function hideSignOutForm(elem)
	{
		$(elem).closest('.sign-out-row').find('form').addClass('hidden');
		$('.show-sign-out-form').removeClass('hidden');
	}

	/**
	 * Signs the current user into the job.
	 * @param {any} jobMessageId
	 * @param {any} description
	 */
	function signInToJob(jobMessageId, description) {

		// Disable and set spinner
		$('.member-sign-in button').attr('disabled', 'disabled').addClass('disabled');
		$('.member-sign-in button i').removeClass('fa-sign-in').addClass('fa-spin fa-spinner');

		// Create the ajax request
		$.ajax({
			url: responseHub.apiPrefix + '/sign-in',
			type: 'POST',
			dataType: 'json',
			data: { JobMessageId: jobMessageId, Description: description, SignInType: 1 },
			success: function (data) {

				if (data != null) {

					// Show the message and remove the button
					$('.member-sign-in button').remove();
					$('.member-sign-in').append('<div class="alert alert-success alert-dismissible" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>You have been successfully signed in.</div>')

					// remove the no members row
					$('.no-members-signed-in').remove();

					// add the information to the table
					var row = $('<tr></tr>');
					row.append('<td>' + data.FullName + '</td>');
					row.append('<td>' + data.MemberNumber + '</td>');

					var signInDate = moment(data.SignInTime).local();
					row.append('<td>' + signInDate.format('YYYY-MM-DD HH:mm') + '</td>');

					// Add the row to the table
					$('#signed-in-members tbody').append(row);

					// increment the user count
					var memberCount = parseInt($('#tab-header-members .member-count').text());
					$('#tab-header-members .member-count').text(memberCount + 1);


				}
				else {
					$('.member-sign-in').append('<p class="text-danger">There was an error signing you into the job.</p>');
					$('.member-sign-in button').removeAttr('disabled').removeClass('disabled');
					$('.member-sign-in button i').removeClass('fa-spin fa-spinner').addClass('fa-sign-in');
				}

			},
			error: function (jqXHR, errorThrown, textStatus) {
				$('.member-sign-in').append('<p class="text-danger">There was an error signing you into the job.</p>');
				$('.member-sign-in button').removeAttr('disabled').removeClass('disabled');
				$('.member-sign-in button i').removeClass('fa-spin fa-spinner').addClass('fa-sign-in');
			}
		});

	}

	function bindUI()
	{

		// Bind the other type select box to determine if the other option description should be shown
		$('#SignInTypeOther').change(function () {

			if ($(this).val() == "99")
			{
				$('.other-type-other').removeClass('hidden');
			} else {
				$('.other-type-other').addClass('hidden');
			}

		});

	}

	// Bind ui
	bindUI();

	return {
		setOperationJobNumber: setOperationJobNumber,
		setActivityDetails: setActivityDetails,
		showSignOutForm: showSignOutForm,
		hideSignOutForm: hideSignOutForm,
		signInToJob: signInToJob
	}

})();

responseHub.reports = (function () {


	function displayTrainingReportGraph() {

		var jsonData = $('#training-overview-chart-data').val().replace(/&quot;/g, '"');
		var chartData = JSON.parse(jsonData);

		new Chartist.Bar('#training-overview-chart', chartData, {
			distributeSeries: true,
			showGridBackground: true,
			axisY: {
				onlyInteger: true
			},
			axisX: {
				scaleMinSpace: 5
			}
		});

	}

	function bindUI() {

		if ($('#training-report').length > 0) {
			displayTrainingReportGraph();
		}
	}

	bindUI();

})();

responseHub.training = (function () {

	function bindUserSelectList() {

		$('#AvailableMembers').on('change', function () {

			// Find the user
			var userId = $(this).val();

			// Add the user to the list
			responseHub.userList.addUserToList(userId, 'AvailableMembers', 'SelectedMembers', 'training-members-table', 'responseHub.training.removeTrainingMember');
			
		});

		$('#AvailableTrainers').on('change', function () {

			// Find the user
			var userId = $(this).val();

			// Add the user to the list
			responseHub.userList.addUserToList(userId, 'AvailableTrainers', 'SelectedTrainers', 'training-trainers-table', 'responseHub.training.removeTrainingMember');

		});

	}

	function removeTrainingMember(elem)
	{
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
		if ($('#' + hiddenId).val().match(/^\|*$/))
		{
			$('#' + hiddenId).val('');
		}

		// Remove the row with the user details
		link.closest("tr").remove();

		// If there are no rows left, add the default message
		if ($('#' + tableId + ' tbody tr').length == 0) {
			var memberType = (tableId.indexOf('trainer') != -1 ? "trainers" : "members");
			$('#' + tableId + ' tbody ').append('<tr><td colspan="3" class="none-selected">No ' + memberType + ' have been added to the this training session yet.</td></tr>');
		}

	}

	function displayTrainingYearGraph() {
		var jsonData = $('#training-overview-chart-data').val().replace(/&quot;/g, '"');
		var chartData = JSON.parse(jsonData);

		new Chartist.Bar('#training-overview-chart', chartData, {
			distributeSeries: true,
			showGridBackground: true,
			axisY: {
				onlyInteger: true
			},
			axisX: {
				scaleMinSpace: 5
			}
		});

	}

	/**
	 * Sets the training type tag from clicking or tabbing the item in the auto-complete box
	 */
	function setTrainingTypeTag(name, id) {

		// Get the current training type ids
		trainingTypeIds = $('#TrainingTypes').val();

		// First, check to ensure the training type doesn't already exist. If it does, then just exist.
		if (trainingTypeIds.indexOf(id) >= 0) {
			// Training type already selected
			return;
		}

		// Add the training type tag
		var trainingTypeTag = $('<span class="label label-primary" data-training-type-id="' + id + '">' + name + '<a><i class="fa fa-times"></i></a></span>');
		trainingTypeTag.find('a').click(function () {

			// Get the id of the training type
			var trainingTypeId = $(this).parent().data('training-type-id');

			// Remove the training type tag
			removeTrainingTypeTag(trainingTypeId);

		});

		// Add the current id to the list of training type ids
		trainingTypeIds += id + "|";

		// If there is no name for the session, then add it here
		if ($('#Name').val() == "")
		{
			$('#Name').val(name);
		}

		// Remove the hidden tag from the training type tag list and append the training type tag
		$('#TrainingTypes').val(trainingTypeIds);
		$('.training-types-list-tags').removeClass('hidden');
		$('.training-types-list-tags').append(trainingTypeTag);

	}

	function removeTrainingTypeTag(id) {

		// Get the current training type ids
		trainingTypeIds = $('#TrainingTypes').val();
		
		// Remove the training type id
		trainingTypeIds = trainingTypeIds.replace(id + "|", "");

		// Update the training type ids
		$('#TrainingTypes').val(trainingTypeIds);

		// If there is no training types, then clear the name field
		if (trainingTypeIds == "")
		{
			$('#Name').val('');
		}

		// Remove the tag from the list
		$('.training-types-list-tags').find("[data-training-type-id='" + id + "']").remove();

		if ($('.training-types-list-tags').children().length == 0) {
			$('.training-types-list-tags').addClass('hidden');
		}

	}

	function bindTrainingTypeAutocomplete() {

		// Set the autocomplete functionality for training types.
		$("input[data-training-type-autocomplete='true']").typeahead({
			source: trainingTypes,
			onSelect: function (item) {
				$("input[data-training-type-autocomplete='true']").val(item.value);
			}
		});
	}

	function bindUI() {
		
		if ($('#add-training-session').length > 0) {
			bindUserSelectList();
		}

		// If there is a graph to display the training for the year in, then show that graph
		if ($('#training-overview-chart').length > 0)
		{
			displayTrainingYearGraph();
		}



		$(document).ready(function () {
			$('#AvailableTrainingTypes').on('changed.bs.select', function (e) {

				// Get the selected id
				var selectedId = $('#AvailableTrainingTypes').selectpicker('val');

				// If there is no selected id, just return
				if (selectedId == "") {
					return;
				}

				// Get the option that was selected
				var selectedOpt = $("#AvailableTrainingTypes option[value='" + selectedId + "']");

				// Add the tag to the list
				if (selectedOpt.length > 0) {
					setTrainingTypeTag(selectedOpt.data('name'), selectedId);
				}

			});
		});

		// Clicke event for training types rendered on the page
		$('.training-types-list-tags span a').click(function () {

			// Get the id of the training type
			var trainingTypeId = $(this).parent().data('training-type-id');

			// Remove the training type tag
			removeTrainingTypeTag(trainingTypeId);

		});

		if ($("input[data-training-type-autocomplete='true']").length > 0) {
			bindTrainingTypeAutocomplete();
		}
	}

	bindUI();

	return {
		removeTrainingMember: removeTrainingMember,
		setTrainingTypeTag: setTrainingTypeTag
	}

})();

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
		
		// Enable job assignments
		$('.event-job-allocation .jobs-list button').each(function (index, elem) {
			$(elem).removeClass('disabled');
			$(elem).removeAttr('disabled');
		});

		// Show the loading animation
		$('.loading-crew-details').removeClass('hidden');

		// Clear the current job assignments
		$('.crew-allocation .assigned-jobs').empty();

		// Get the event id
		var eventId = $('#EventId').val();

		// Set the current crew id
		$('#selected-crew-id').val(crewId);

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

				// Disable the button
				$('.allocate-jobs button').attr('disabled', 'disabled').addClass('disabled');

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

				$('.allocate-jobs button i').addClass('fa-indent').removeClass('fa-spin fa-spinner');

				// If there are no jobs assigned, hide the button
				if ($('.assigned-jobs li').length == 0) {
					$('.allocate-jobs').addClass('hidden');
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {

				$('.allocate-jobs button').removeAttr('disabled');
				$('.allocate-jobs button').removeClass('disabled');
				$('.allocate-jobs button i').addClass('fa-indent').removeClass('fa-spin fa-spinner');
			}, 
			complete: function () {
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

		// If there are no jobs assigned, show the no jobs message
		if ($('.assigned-jobs li').length == 0) {
			$('.crew-job-list .no-jobs').removeClass('hidden');
		}

		// Enable the assign button
		$('.allocate-jobs button').removeClass("disabled").removeAttr("disabled");

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

			// Enable the assign button
			$('.allocate-jobs button').removeClass("disabled").removeAttr("disabled");

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

		$('#CrewSelect').on('change', function (e) {
			
			// The button is not disabled, then there are changes and we shouldn't allow that'
			if ($('.allocate-jobs button[disabled="disabled"]').length == 0)
			{
				$('#confirm-change-crew').modal('show');
				return;
			} else {

				var crewId = $(this).val();

				// Load the crew assignments
				loadCrewJobAssignments(crewId);

			}

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
		removeCrewMember: removeCrewMember,
		loadCrewJobAssignments: loadCrewJobAssignments
	}

})();

// Value is the element to be validated, params is the array of name/value pairs of the parameters extracted from the HTML, element is the HTML element that the validator is attached to
$.validator.addMethod("signintypedescriptionset", function (value, element, params) {
	
	// Get the element as a jq object
	var jqElem = $(element);

	// Get the current value of the selected sign in type
	var signInType = $("input[name='SignInType']:checked").val();
	var signInTypeDesc = getSignInTypeDescription(signInType);

	// Get the selected type from the element
	var validationSignInType = $(jqElem).data("val-signintypedescriptionset-expectedsignintype");
	
	// If the selected sign in type matches the expected sign in type and the value is empty, return false to set as invalid
	if (signInTypeDesc == validationSignInType && value == '')
	{
		return false;
	}

	// return true to default to true validation result
	return true;

});

function getSignInTypeDescription(signInType)
{
	if (signInType == "1")
	{
		return "Operation";
	}
	else if (signInType == "2") {
		return "Training";
	}
	else if (signInType == "4") {
		return "Other"
	}
}

// Value is the element to be validated, params is the array of name/value pairs of the parameters extracted from the HTML, element is the HTML element that the validator is attached to
$.validator.addMethod("othersignindescriptionset", function (value, element, params) {


	// Get the current value of the selected other sign in type
	var otherType = $("#SignInTypeOther").val();

	// If the other sign in type is set to "Other" and there is no description, fail validation
	if (otherType == "99" && value == '')
	{
		return false;
	}

	return true;

});


/* The adapter signature:
adapterName is the name of the adapter, and matches the name of the rule in the HTML element.
 
params is an array of parameter names that you're expecting in the HTML attributes, and is optional. If it is not provided,
then it is presumed that the validator has no parameters.
 
fn is a function which is called to adapt the HTML attribute values into jQuery Validate rules and messages.
 
The function will receive a single parameter which is an options object with the following values in it:
element
The HTML element that the validator is attached to
 
form
The HTML form element
 
message
The message string extract from the HTML attribute
 
params
The array of name/value pairs of the parameters extracted from the HTML attributes
 
rules
The jQuery rules array for this HTML element. The adapter is expected to add item(s) to this rules array for the specific jQuery Validate validators
that it wants to attach. The name is the name of the jQuery Validate rule, and the value is the parameter values for the jQuery Validate rule.
 
messages
The jQuery messages array for this HTML element. The adapter is expected to add item(s) to this messages array for the specific jQuery Validate validators that it wants to attach, if it wants a custom error message for this rule. The name is the name of the jQuery Validate rule, and the value is the custom message to be displayed when the rule is violated.
*/
$.validator.unobtrusive.adapters.add("signintypedescriptionset", ["signintypefield", "expectedsignintype"], function (options) {
	options.rules["signintypedescriptionset"] = "#" + options.params.signintypefield;
	options.messages["signintypedescriptionset"] = options.message;
});


$.validator.unobtrusive.adapters.add("othersignindescriptionset", ["othertypefield"], function (options) {
	options.rules["othersignindescriptionset"] = "#" + options.params.othertypefield;
	options.messages["othersignindescriptionset"] = options.message;
});