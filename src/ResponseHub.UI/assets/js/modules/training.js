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
		console.log(jsonData);

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