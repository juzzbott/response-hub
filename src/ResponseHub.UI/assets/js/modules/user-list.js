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