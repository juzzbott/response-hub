responseHub.upkeep = (function () {

	/**
	 * Initialises the admin control panel.
	 */
	function init() {

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
		$('#catalog-builder > .container-items > .btn-new-container').on('click', function () {
			addContainerItem($(this).parent());
		});

	}

	function bindUI()
	{
		$('#confirm-delete.delete-container').on('show.bs.modal', function (e) {

			var containerName = $(e.relatedTarget).closest('.container-name').children('.container-name-control').find('input').val();
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
		newContainerName.append('<div class="col-xs-10 col-sm-6 container-name-control"><input class="form-control" type="text" placeholder="Container name"></div>');
		newContainerName.append('<div class="col-xs-1 col-sm-1 container-remove"><button type="button" class="btn btn-link"><i class="fa fa-times-circle-o text-danger"></i></button ></div >')

		newContainer.append(newContainerName);
		newContainer.append('<button type="button" class="btn btn-link btn-new-container">Add new container</button>');
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
			$('#confirm-delete.delete-container').modal('toggle', $(this));
		});
	}

	/**
	 * Build the inventory object
	 */
	function buildInventoryObject()
	{

		// Create the inventory object
		var inventory = {
			containers: [],
			catalogItems: []
		}

		// Define the parent
		var parent = $("#catalog-builder");

		// Start with the containers
		inventory.containers = getContainerItems(parent);

		// Set the catalog items
		inventory.catalogItems = getCatalogItems(parent);

		console.log(inventory);

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

			// Get the catalog items
			var catalogItems = getCatalogItems(elem);

			var container = {
				name: name,
				containers: null,
				catalogItems: catalogItems
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

	// Init the upkeep controls.
	init();
	bindUI();

	return {
		buildInventoryObject: buildInventoryObject
	}

})();