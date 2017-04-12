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

		mapMarkers.push(L.marker([lat, lng]).addTo(map));

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
					if (mapMarkers.length > 1)
					{
						mapMarkers[1].setLatLng(new L.LatLng(pos.coords.latitude, pos.coords.longitude));
					}
					else
					{

						// Second location marker doesn exist, so we need to create it

						var currentLocationMarker = new L.HtmlIcon({
							html: '<div><i class="fa fa-dot-circle-o fa-2x current-map-location"></i></div>',
						});	

						// Add the marker to the map
						mapMarkers.push(L.marker([pos.coords.latitude, pos.coords.longitude], { icon: currentLocationMarker }).addTo(map));

						// Get the group of markers, destination and current location, and zoom window to fit
						var group = new L.featureGroup([mapMarkers[0], mapMarkers[1]]);
						map.fitBounds(group.getBounds().pad(0.1));

						var start_loc = mapMarkers[0].getLatLng().lat + ',' + mapMarkers[0].getLatLng().lng;
						var end_loc = pos.coords.latitude + ',' + pos.coords.longitude;

						// Get the directions to the location
						$.ajax({
							url: responseHub.apiPrefix + '/google-api/directions?start_loc=' + start_loc + '&end_loc=' + end_loc,
							dataType: 'json',
							success: function (data) {

								if (data.length > 0)
								{
									var latlngs = [];

									// Loop through the results and create the LatLng objects to add to the poly line
									for (var i = 0; i < data.length; i++)
									{
										latlngs.push(new L.LatLng(data[i].Latitude, data[i].Longitude))
									}

									// Now that we have the lat lngs, add the path to the map
									L.polyline(latlngs, { color: '#3984FF' }).addTo(map);
								}

							}
						});

						// Set the interval to resize the window every 30 secs.
						setInterval(function () {
							
							// Get the group of markers, destination and current location, and zoom window to fit
							var group = new L.featureGroup([mapMarkers[0], mapMarkers[1]]);
							map.fitBounds(group.getBounds().pad(0.1));

						}, 30000)

					}

				},
				function (error) {
					$('#map-messages').append('<p>' + error.code + ': ' + error.message + '</p>');
					console.log(error);
				},
				{
					enableHighAccuracy: true,
					timeout: 5000,
					maximumAge: 0
				}
			);
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
		getCurrentLocation: getCurrentLocation,
		setMapCenter: setMapCenter,
		mapExists: mapExists,
		addCurrentLocationToMap: addCurrentLocationToMap
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