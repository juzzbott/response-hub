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