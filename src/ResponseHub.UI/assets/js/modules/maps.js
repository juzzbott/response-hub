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

	function addPagerMessageMarkerToMap(lat, lng, id, jobNumber, messageContent, timestamp, jobType)
	{
		mapMarkers[id] = L.marker([lat, lng]).addTo(map).bindPopup('<a href="/pager-messages/' + id + '"><strong>' + jobNumber + ' - <span style="color: #666">' + timestamp + '</span></strong></a><br /><span style="text-transform:uppercase; color: #17a2b8">' + jobType + '</span><br /><span>' + messageContent + '</span>');
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
		addPagerMessageMarkerToMap: addPagerMessageMarkerToMap,
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