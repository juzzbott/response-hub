responseHub.upkeep = (function () {

	/**
	 * Initialises the upkeep control panel.
	 */
	function init()
	{

		// Initialise the asset inventory screens
		initAssetInventory();

		// Initialise the tasks screen
		initTasks();

	}

	/**
	 * Initialises the asset inventory controls
	 */
	function initAssetInventory()
	{
		// If there is no inventory builder, return
		if ($('#inventory-builder').length == 0) {
			return;
		}

		// Initialise the inventory builder
		initInventoryBuilder();

		// Initialise the inventory
		if ($('#InventoryJson').val() != "") {

			// Get the inventory object
			var inventory = JSON.parse($('#InventoryJson').val())

			setInventory(inventory);
			setInventoryBuilder(inventory);
		}

		// Add the catalog item event handlers
		$(".catalog-item input[type='text']").each(function () {

			// Set the events for the input control.
			setCatalogItemEvents($(this));

		});

		// Add the container name event handlers
		$(".container-name input[type='text']").each(function () {

			// Set the events for the container name input
			setContainerNameEvents($(this));

		});

		// Find the last catalog container add new button and set the event
		$('#inventory-builder > .container-items > .btn-new-container').on('click', function () {
			addContainerItem($(this).parent(), "");
		});

		// Bind the container remove elements
		bindContainerRemove();
	}

	/**
	 * Initialises the tasks screen controls.
	 */
	function initTasks() {

		// If there is no task items div, then just return
		if ($('#task-items').length == 0)
		{
			return;
		}

		// Parse the JSON task items
		var taskItems = JSON.parse($('#TaskItemsJson').val());

		// If it's not null and there is at least one element, then populate the task items
		if (taskItems != null && taskItems.length > 0) {
			for (var i = 0; i < taskItems.length; i++) {
				addTaskItem(taskItems[i]);
			}
		}
		
		// Hide the loading animation.
		$('#task-items .content-loading').remove();

		// Add the blank task item
		addTaskItem("");

	}

	/**
	 * Binds the UI controls
	 */
	function bindUI()
	{

		bindAssetInventoryUI();

		$('#confirm-delete.delete-task').on('show.bs.modal', function (e) {

			// Generate the confirm message
			$(this).find('.modal-body p span').text($(e.relatedTarget).data('task-name'));

		});

		if ($('#tasks-table').length > 0) {
			bindUpkeepTasks();
		}

	}

	/**
	 * Binds the Asset inventory UI controls.
	 */
	function bindAssetInventoryUI()
	{
		
		// If there is no inventory builder, return
		if ($('#inventory-builder').length == 0) {
			return;
		}

		$('#confirm-delete.delete-container').on('show.bs.modal', function (e) {

			var containerName = $(e.relatedTarget).closest('.container-name').children('.container-name-row').find('.container-name-control input').val();
			if (containerName == "") {
				containerName = "unnamed";
			}

			// Set the container name in the confirm message
			$(this).find('.modal-body p span').text(containerName);

			$(this).find('.btn-ok').off('click');
			$(this).find('.btn-ok').click(function () {
				$(e.relatedTarget).closest('.catalog-container').remove();
				$('#confirm-delete.delete-container').modal('hide');
			});

		});
	}

	/**
	 * Set the inventory list items in the inventory screen. This is the read only, table view of the inventory.
	 * @param {any} inventory
	 */
	function setInventory(inventory)
	{

		var table = $('<table class="table table-responsive table-condensed table-striped"><thead><tr><th width="75">Qty</th><th>Item</th></tr></thead><tbody></tbody></table>');

		// Ensure we have an inventory to list
		if (inventory == null)
		{
			return;
		}

		// Loop through the containers
		table = addInventoryContainers(inventory.Containers, table);

		// add the items
		table = addInventoryItems(inventory.Items, table);

		// If there is an empty last row, remove it
		if (table.find('tbody tr:last-child td').hasClass('empty-row'))
		{
			table.find('tbody tr:last-child').remove();
		}

		// Add the table to the inventory list div
		$('#inventory-list').append(table);

		// Hide the loading animation.
		$('#inventory-list .content-loading').remove();

	}

	/**
	 * Adds the inventory containers to the screen from the supplied json. 
	 * @param {any} containers
	 * @param {any} table
	 */
	function addInventoryContainers(containers, table)
	{

		// Loop through the containers
		for (var i = 0; i < containers.length; i++)
		{

			// Append the name
			table.find('tbody').append('<tr><td colspan="2"><strong><em><small>' + containers[i].Name + '</small></em></strong></td></tr>');

			// append the containers
			table = addInventoryContainers(containers[i].Containers, table);

			// Append the container items
			table = addInventoryItems(containers[i].Items, table);

			table.find('tbody').append('<tr><td colspan="2" class="empty-row">&nbsp;</td></tr>');

		}

		// return the table
		return table;

	}

	/**
	 * Adds the inventory catalog items to the screen from the supplied json
	 * @param {any} items
	 * @param {any} table
	 */
	function addInventoryItems(items, table)
	{

		// Loop through 
		for (var i = 0; i < items.length; i++) {
			table.find('tbody').append('<tr><td>' + items[i].Quantity + '</td><td>' + items[i].Name + '</td></tr>')
		}

		return table;
	}

	/**
	 * Sets the inventory builder based on the data in the supplied json.
	 * @param {any} inventory
	 */
	function setInventoryBuilder(inventory)
	{
		// Ensure we have an inventory to list
		if (inventory == null) {
			return;
		}

		// Determine the intial container parent
		var containerParent = $('#inventory-builder > .container-items');

		// Remove the initial "blank container"
		$('#inventory-builder > .container-items > .catalog-container').remove();

		// Loop through the containers
		addInventoryBuilderContainers(inventory.Containers, containerParent);
	}

	/**
	 * Adds the containers for the inventory to the inventory builder.
	 */
	function addInventoryBuilderContainers(containers, containerParent)
	{

		// Loop through the containers
		for (var i = 0; i < containers.length; i++)
		{

			// Create the container
			var newContainer = addContainerItem(containerParent, containers[i].Name);

			// Get the newContainer container-items to use as the parent for recursion
			var newContainerItemsParent = newContainer.find('.container-items');
			var catalogItems = newContainer.find('.catalog-items');

			// If there are "sub containers" then add those
			if (containers[i].Containers != null && containers[i].Containers.length > 0)
			{
				addInventoryBuilderContainers(containers[i].Containers, newContainerItemsParent);
			}

			// Add the items for the current container
			addInventoryBuilderCatalogItems(containers[i].Items, catalogItems)
			
		}
	}

	/**
	 * Adds the catalog items to the inventory builder.
	 */
	function addInventoryBuilderCatalogItems(catalogItems, containerParent)
	{
		// Loop through the items
		for (var i = 0; i < catalogItems.length; i++)
		{
			addCatalogItem(containerParent, catalogItems[i].Name, catalogItems[i].Quantity);
		}

		// Add the blank option to allow new items to be added
		addCatalogItem(containerParent, "", 1);

	}

	/*
	 * Set the events for the catalog item input control. 
	 */
	function setCatalogItemEvents(catalogInput) {

		$(catalogInput).on('keyup', function () {

			// If there is a value, and no empty inputs last, then add a new catalog item to the catalog-items parent. 
			if ($(this).val().length > 0 && $(this).closest('.catalog-items').find('input').last().val() != "") {
				// Add the catalog item.
				addCatalogItem($(this).closest('.catalog-items'), "", 1);
			}

		});

		$(catalogInput).on('blur', function () {

			// If there is no value, and it's not last, then remove it
			if ($(this).val().length == 0 && !$(this).closest('.catalog-item').is(':last-child')) {

				$(this).closest('.catalog-item').fadeOut(350, function () {
					$(this).closest('.catalog-item').remove();
				});

			}
		});

	}

	/*
	 * Add the catalog item to the page.
	 */
	function addCatalogItem(itemParent, name, qty) {

		// Create the new catalog item.
		var newItem = $('<div class="catalog-item clearfix"></div>');
		newItem.append('<div class="handle item-handle pull-left"></div>');
		newItem.append('<div class="col-xs-2 col-sm-1"><input class="form-control catalog-item-qty" type="text" value="' + qty + '"></div>');
		newItem.append('<div class="col-xs-9 col-sm-5"><input class="form-control catalog-item-name" type="text" placeholder="Catalog item" value="' + name + '"></div>');

		var newItemInput = $(newItem).find('input');
		setCatalogItemEvents(newItemInput);

		// Append the catalog item container.
		$(itemParent).append(newItem);

	}

	/*
	 * Sets the events for the container name input field.
	 */
	function setContainerNameEvents(containerNameInput) {

		// Get the catalog container object
		var catalogContainer = $(containerNameInput).closest('.catalog-container');
		var catalogItemsContainer = $(containerNameInput).closest('.catalog-container').children('.container-items').first();

		$(containerNameInput).on('keyup', function () {

			// If there is a value, and no empty inputs last, then add a new catalog item to the catalog-items parent. 
			if ($(this).val().length > 0 && catalogContainer.find(".catalog-items:last input[type='text']").last().length == 0) {
				// Add the catalog item.
				addCatalogItem(catalogContainer.find(".catalog-items:last"), "", 1);
			}

		});
		
		catalogContainer.find('.btn-new-container').on('click', function () {
			addContainerItem(catalogItemsContainer, "");
		});

	}

	/**
	 * Adds a new container item to the page.
	 */
	function addContainerItem(containerParent, containerName) {
		
		var newContainer = $('<div class="catalog-container"></div>');

		var newContainerName = $('<div class="container-name clearfix"></div>');
		newContainerName.append('<div class="handle container-handle pull-left"></div>');
		var containerNameControls = $('<div class="col-xs-12 col-sm-6 container-name-row"></div>');
		containerNameControls.append('<div class="container-name-control"><input class="form-control" type="text" placeholder="Container name" value="' + containerName + '"></div>');
		containerNameControls.append('<div class="container-remove"><button type="button" class="btn btn-link"><i class="fa fa-times-circle-o text-danger"></i></button></div>');
		newContainerName.append(containerNameControls);

		newContainer.append(newContainerName);
		newContainer.append('<div class="container-items"><button type="button" class="btn btn-link btn-new-container">Add new container</button></div>');
		newContainer.append('<div class="catalog-items"></div>');

		// Set the container events
		var newContainerInput = newContainer.find(".container-name input[type='text']");
		setContainerNameEvents(newContainerInput);

		// Add the container to the page
		$(containerParent).children('.btn-new-container').before(newContainer);

		// Rebind container remove button
		bindContainerRemove();

		// return the new container item
		return newContainer;

	}

	/**
	 *Bind the container remove event.
	 */
	function bindContainerRemove() {

		$('.container-remove button').off('click');
		$('.container-remove button').click(function () {

			// If there is no name, no containers and no catalog items, just remove it. 
			var containerName = $(this).closest('.catalog-container').children('.container-name').find('input').val();
			var catalogItemsExists = true;
			var containerItemsExists = true;

			// Detemine if the catalog items exists
			var catalogItemCount = $(this).closest('.catalog-container').children('.catalog-items').children('.catalog-item').length;
			if (catalogItemCount == 0)
			{
				catalogItemsExists = false;
			}
			if (catalogItemCount == 1)
			{
				var firstCatalogItemName = $(this).closest('.catalog-container').children('.catalog-items').find('.catalog-item .catalog-item-name').val();
				if (firstCatalogItemName == "")
				{
					catalogItemsExists = false;
				}
			}

			// Determine if any container items exists
			var containerItemCount = $(this).closest('.catalog-container').children('.container-items').children('.catalog-container').length;
			if (containerItemCount == 0)
			{
				containerItemsExists = false;
			}

			if (containerName == "" && !catalogItemsExists && !containerItemsExists)
			{
				$(this).closest('.catalog-container').remove();
				return;
			}

			$('#confirm-delete.delete-container').modal('toggle', $(this));
		});
	}

	/**
	 * Build the inventory object
	 */
	function buildInventory()
	{

		// Create the inventory object
		var inventory = {
			Containers: [],
			Items: []
		}

		// Define the parent
		var parent = $("#inventory-builder");

		// Start with the containers
		inventory.Containers = getContainerItems(parent);

		// Set the catalog items
		inventory.Items = getCatalogItems(parent);

		$('#InventoryJson').val(JSON.stringify(inventory));

	}

	/**
	 * Gets the container items for the specified parent item
	 * @param {any} parent
	 */
	function getContainerItems(parent)
	{

		// array to store the container items
		var containerItems = [];

		// Loop through each child catagery items
		$(parent).children(".container-items").children(".catalog-container").each(function (index, elem) {

			// get the name
			var name = $(elem).find('.container-name:first-child input').val();

			// Get the containers
			var containers = getContainerItems(elem);

			// Get the catalog items
			var catalogItems = getCatalogItems(elem);

			var container = {
				Name: name,
				Containers: containers,
				Items: catalogItems
			};
			containerItems.push(container);

		});

		// return the container items
		return containerItems;

	}

	/**
	 * Gets the catalog items for the parent item
	 * @param {any} parent
	 */
	function getCatalogItems(parent)
	{

		// Create the array of catalog items
		catalogItems = [];

		// Loop through each child catalog items
		$(parent).children(".catalog-items").children(".catalog-item").each(function (index, elem) {

			// Get the quanity and the name
			var qty = parseInt($(elem).find('.catalog-item-qty').val());
			var name = $(elem).find('.catalog-item-name').val();
			
			var item = {
				Quantity: qty,
				Name: name
			};

			// Can't add an item without a name, so make sure the name is set before adding it
			if (name != "")
			{
				catalogItems.push(item);
			}
		}); 

		// return the catalog items
		return catalogItems

	}

	/**
	 * Initialises the inventory builder.
	 */
	function initInventoryBuilder()
	{
		// Create the container items
		var containerItems = $('<div class="container-items"><button type="button" class="btn btn-link btn-new-container">Add new container</button></div>');

		// Add the container items to the builder, and then set the first container item
		$('#inventory-builder').append(containerItems);

		// Add the first container items element.
		addContainerItem(containerItems, "");

		// Create the initial catalog item
		var catalogItems = $('<div class="catalog-items"></div>');
		$('#inventory-builder').append(catalogItems);
		
		// Add the first catalog item element.
		addCatalogItem(catalogItems, "", 1);

		// Hide the loading animation.
		$('#inventory-builder .content-loading').remove();

	}

	/**
	 * Adds the new task item control.
	 * @param {string} name
	 */
	function addTaskItem(name)
	{

		var taskControl = $('<div class="task-item clearfix"></div>');
		taskControl.append('<div class="handle item-handle pull-left"></div>');
		taskControl.append('<div class="col-sm-6"><input class="form-control task-item-name" type="text" value="' + name + '"></div>');

		var taskControlInput = $(taskControl).find('input');
		setTaskItemEvents(taskControlInput);

		$('#task-items').append(taskControl);

	}

	function setTaskItemEvents(taskNameInput)
	{
		$(taskNameInput).on('keyup', function () {
			
			// If there is a value, and no empty inputs last, then add a new catalog item to the catalog-items parent. 
			if ($(this).val().length > 0 && $("#task-items .task-item:last-child input[type='text']").last().val() != "") {
				
				// Add the task item.
				addTaskItem("");
			}

		});

		$(taskNameInput).on('blur', function () {

			// If there is no value, and it's not last, then remove it
			if ($(this).val().length == 0 && !$(this).closest('.task-item').is(':last-child')) {

				$(this).closest('.task-item').fadeOut(350, function () {
					$(this).closest('.task-item').remove();
				});

			}
		});
	}

	function buildTaskItems() {

		// Create the items array
		var items = [];

		$("#task-items").children(".task-item").each(function (index, elem) {

			// Get the name
			var name = $(elem).find('.task-item-name').val();

			// Can't add an item without a name, so make sure the name is set before adding it
			if (name != "") {
				items.push(name);
			}
		}); 

		// Set the value of the hidden field containing the json for the items
		$('#TaskItemsJson').val(JSON.stringify(items));

	}

	/**
	 * REPORTS
	 */
	function removeTaskItemFromReport(elem) {
		// Get the element to remove
		var link = $(elem);

		// get the hidden id
		var hiddenId = link.closest('table').data('selected-list');
		var tableId = link.closest('table').attr('id');

		// Find the task id
		var taskId = link.closest("tr").data('task-id');

		// Remove the task id from the hidden
		$('#' + hiddenId).val($('#' + hiddenId).val().replace(taskId, ""));

		// If there is only | characters, set to empty to re-trip validation
		if ($('#' + hiddenId).val().match(/^\|*$/)) {
			$('#' + hiddenId).val('');
		}

		// Remove the row with the user details
		link.closest("tr").remove();

		// If there are no rows left, add the default message
		if ($('#' + tableId + ' tbody tr').length == 0) {
			$('#' + tableId + ' tbody ').append('<tr><td colspan="3" class="none-selected">No tasks have been added to the this report yet.</td></tr>');
		}

	}

	function bindUpkeepTasks() {

		$('#AvailableTasks').on('change', function () {

			var listId = 'AvailableTasks'
			var selectedId = 'SelectedTasks'
			var tableId = 'tasks-table';

			// Find the taskId
			var taskId = $(this).val();

			// Get the task
			var task = findTask(taskId);

			// If the task id already exists in the selected tasks element, just return
			if ($('#' + selectedId).val().indexOf(taskId) != -1) {
				$('#' + listId).selectpicker('val', '');
				return;
			}

			// If the first table row in the body is nothing selecting, then remove it
			if ($('#' + tableId + ' td.none-selected').length > 0) {
				$('#' + tableId + ' tbody tr').remove();
			}

			// Build the markup 
			var row = $('<tr data-task-id="' + task.id + '"></tr>');
			row.append('<td>' + task.name + '</td>');
			row.append('<td>&nbsp;</td>');
			row.append('<td><a href="#" onclick="responseHub.upkeep.removeTaskItemFromReport(this); return false;" title="Remove task" class="text-danger"><i class="fa fa-fw fa-times"></i></a></td>');
			$('#' + tableId + ' tbody').append(row);

			// Add the task id to the selected tasks
			$('#' + selectedId).val($('#' + selectedId).val() + task.id + '|');

			// Deselect the previous option
			$('#' + listId).selectpicker('val', '');
		});

	}

	// Find the task object in the list of users.
	function findTask(taskId) {

		// Create the task object
		var task = null;

		// Loop through the tasks to find the selected one.
		for (var i = 0; i < tasks.length; i++) {
			if (tasks[i].id == taskId) {
				task = tasks[i];
				break;
			}
		}

		// return the task object
		return task;
	}

	// Init the upkeep controls.
	init();
	bindUI();

	return {
		buildInventory: buildInventory,
		buildTaskItems: buildTaskItems,
		removeTaskItemFromReport: removeTaskItemFromReport
	}

})();