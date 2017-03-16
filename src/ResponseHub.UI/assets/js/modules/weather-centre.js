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
			$('p.radar-loading').remove();
		}, 2000);

	}

	// Load the UI
	loadUI();

	// return object
	return {
		showHideWarnings: showHideWarnings
	}

})();