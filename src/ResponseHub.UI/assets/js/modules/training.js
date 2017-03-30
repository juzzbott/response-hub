responseHub.training = (function () {

	function bindUserSelectList() {

		$('#AvailableMembers').on('change', function () {

			// Find the user
			var userId = $(this).val();

			// Add the user to the list
			addUserToList(userId, 'AvailableMembers', 'SelectedMembers', 'training-members-table');
			
		});

		$('#AvailableTrainers').on('change', function () {

			// Find the user
			var userId = $(this).val();

			// Add the user to the list
			addUserToList(userId, 'AvailableTrainers', 'SelectedTrainers', 'training-trainers-table');

		});

	}

	// Adds the user to the specified list, as either a trainer or a member.
	function addUserToList(userId, listId, selectedId, tableId) {


		user = findUser(userId);

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
		row.append('<td><a href="#" onclick="responseHub.training.removeTrainingMember(this); return false;" title="Remove member"><i class="fa fa-fw fa-times"></i></td>');
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

	function bindUI() {
		
		if ($('#add-training-session').length > 0) {
			bindUserSelectList();
		}

		// If there is a graph to display the training for the year in, then show that graph
		if ($('#training-overview-chart').length > 0)
		{
			displayTrainingYearGraph();
		}
	}

	bindUI();

	return {
		removeTrainingMember: removeTrainingMember
	}

})();