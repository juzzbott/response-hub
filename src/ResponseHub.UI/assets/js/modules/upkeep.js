responseHub.upkeep = (function () {

	/**
	 * Initialises the admin control panel.
	 */
	function init()
	{

		// If there is no inventory builder, return
		if ($('#inventory-builder').length == 0)
		{
			return;
		}

		// Initialise the inventory builder
		initInventoryBuilder();

		// Initialise the inventory
		if ($('#InventoryJson').val() != "")
		{

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
			addContainerItem($(this).parent());
		});

		// Bind the container remove elements
		bindContainerRemove();

	}

	function bindUI()
	{
		
		// If there is no inventory builder, return
		if ($('#inventory-builder').length == 0) {
			return;
		}

		$('#confirm-delete.delete-container').on('show.bs.modal', function (e) {

			var containerName = $(e.relatedTarget).closest('.container-name').children('.container-name-row').find('.container-name-control input').val();
			if (containerName == "")
			{
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

	function addInventoryContainers(containers, table)
	{

		// Loop through the containers
		for (var i = 0; i < containers.length; i++)
		{

			// Append the name
			table.find('tbody').append('<tr><td colspan="2"><strong><em><small><center>' + containers[i].Name + '</center></small></em></strong></td></tr>');

			// append the containers
			table = addInventoryContainers(containers[i].Containers, table);

			// Append the container items
			table = addInventoryItems(containers[i].Items, table);

			table.find('tbody').append('<tr><td colspan="2" class="empty-row">&nbsp;</td></tr>');

		}

		// return the table
		return table;

	}

	function addInventoryItems(items, table)
	{
		// Loop through 
		for (var i = 0; i < items.length; i++) {
			table.find('tbody').append('<tr><td>' + items[i].Quantity + '</td><td>' + items[i].Name + '</td></tr>')
		}

		return table;
	}

	function setInventoryBuilder(inventory)
	{

	}

	/*
	 * Set the events for the catalog item input control. 
	 */
	function setCatalogItemEvents(catalogInput) {

		$(catalogInput).on('keyup', function () {

			// If there is a value, and no empty inputs last, then add a new catalog item to the catalog-items parent. 
			if ($(this).val().length > 0 && $(this).closest('.catalog-items').find('input').last().val() != "") {
				// Add the catalog item.
				addCatalogItem($(this).closest('.catalog-items'));
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
	function addCatalogItem(itemParent) {

		// Create the new catalog item.
		var newItem = $('<div class="catalog-item clearfix"></div>');
		newItem.append('<div class="handle item-handle pull-left"></div>');
		newItem.append('<div class="col-xs-2 col-sm-1"><input class="form-control catalog-item-qty" type="text" value="1"></div>');
		newItem.append('<div class="col-xs-9 col-sm-5"><input class="form-control catalog-item-name" type="text" placeholder="Catalog item"></div>');

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
				addCatalogItem(catalogContainer.find(".catalog-items:last"));
			}

		});
		
		catalogContainer.find('.btn-new-container').on('click', function () {
			addContainerItem(catalogItemsContainer);
		});

	}

	/**
	 * Adds a new container item to the page.
	 */
	function addContainerItem(containerParent) {
		
		var newContainer = $('<div class="catalog-container"></div>');

		var newContainerName = $('<div class="container-name clearfix"></div>');
		newContainerName.append('<div class="handle container-handle pull-left"></div>');
		var containerNameControls = $('<div class="col-xs-12 col-sm-6 container-name-row"></div>');
		containerNameControls.append('<div class="container-name-control"><input class="form-control" type="text" placeholder="Container name"></div>');
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
			containers: [],
			catalogItems: []
		}

		// Define the parent
		var parent = $("#inventory-builder");

		// Start with the containers
		inventory.containers = getContainerItems(parent);

		// Set the catalog items
		inventory.catalogItems = getCatalogItems(parent);

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
				name: name,
				containers: containers,
				items: catalogItems
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
				quantity: qty,
				name: name
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

	function initInventoryBuilder()
	{
		// Create the container items
		var containerItems = $('<div class="container-items"><button type="button" class="btn btn-link btn-new-container">Add new container</button></div>');

		// Add the container items to the builder, and then set the first container item
		$('#inventory-builder').append(containerItems);

		// Add the first container items element.
		addContainerItem(containerItems);

		// Create the initial catalog item
		var catalogItems = $('<div class="catalog-items"></div>');
		$('#inventory-builder').append(catalogItems);
		
		// Add the first catalog item element.
		addCatalogItem(catalogItems);

		// Hide the loading animation.
		$('#inventory-builder .content-loading').remove();

	}

	// Init the upkeep controls.
	init();
	bindUI();

	return {
		buildInventory: buildInventory
	}

})();