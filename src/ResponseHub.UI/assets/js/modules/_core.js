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

	function getJobTypes() {
		var jobTypes = [
			{ "Id": 0, "ShortCode": "UKN", "IncidentType": "Unknown" },
			{ "Id": 1, "ShortCode": "INCI", "IncidentType": "Incident" },
			{ "Id": 2, "ShortCode": "STRU", "IncidentType": "Structure Fire" },
			{ "Id": 3, "ShortCode": "G&S", "IncidentType": "Grass & Scrub" },
			{ "Id": 4, "ShortCode": "NOST", "IncidentType": "Non-Structure Fire" },
			{ "Id": 5, "ShortCode": "ALAR", "IncidentType": "Alarm" },
			{ "Id": 6, "ShortCode": "RESC", "IncidentType": "Rescue" },
			{ "Id": 7, "ShortCode": "ASSIST POLICE", "IncidentType": "Assist Police" },
			{ "Id": 8, "ShortCode": "ASSIST AV", "IncidentType": "Assist Ambulance" },
			{ "Id": 9, "ShortCode": "FLOOD", "IncidentType": "Flood" },
			{ "Id": 10, "ShortCode": "TREE DWN / TRF HZD", "IncidentType": "Tree Down / Traffic Hazard" },
			{ "Id": 11, "ShortCode": "BLD DMG", "IncidentType": "Building Damage" },
			{ "Id": 12, "ShortCode": "TREE DOWN", "IncidentType": "Tree Down" },
			{ "Id": 13, "ShortCode": "ANIMAL INCIDENT", "IncidentType": "Animal Incident" },
			{ "Id": 14, "ShortCode": "STCO", "IncidentType": "Structure Collapse" }
		];
		return jobTypes;
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
		getQueryString: getQueryString,
		getJobTypes: getJobTypes
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