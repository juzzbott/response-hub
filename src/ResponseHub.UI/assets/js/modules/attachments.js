responseHub.attachments = (function () {

	function bindUI() {

		Dropzone.options.dropzoneAttachments = {
			maxFilesize: 100,
			previewsContainer: false,
			init: function () {
				this.on("success", function (file, response) {
					
					// Create the file in the list of attachments
					addAttachment(file, response);

					// Get the extension
					var match = file.name.match(/^.*(\.[a-zA-Z0-9_]+)$/);

					// If there is an ext match
					if (match != null && match.length > 1) {
						var ext = match[1].toLowerCase();

						// If the extension is an img extension, then also add to the gallery
						if (ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".png" || ext == ".gif")
						{
							addGalleryImage(file, response);
						}

					}

					// Update the attachment count
					$('#tab-header-attachments span').text($('.attachment-list li').length);

				});

				this.on("addedfile", function (file) {

					// Clear the error list
					$('#upload-messages').empty();

					// Show the 'uploading' message
					$('.dz-message').empty();
					$('.dz-message').append('<span class="btn-icon"><i class="fa fa-spinner fa-pulse fa-fw"></i>Uploading files...</span>');


				});

				this.on("complete", function () {

					// Reset the 'drop files here' message
					$('.dz-message').empty();
					$('.dz-message').append('<span>Drop files here to upload</span>');

				});
			}
		};

	}

	/**
	 * Adds the uploaded attachment to the list of attachments.
	 */
	function addAttachment(file, response) {

		// Create the list item
		var listItem = $('<li></li>');

		// If the responseObj is null or the success is false, then exit showing error message
		if (response == null || !response.success)
		{
			var errorMessage = (response != null ? response.message : "Unable to upload file.");
			$('#upload-messages').append('<div class="alert alert-danger">Error uploading your file: ' + errorMessage + '</div>');
			return;
		}

		// get the current datetime
		var date = moment().local().format('DD/MM/YYYY HH:mm');

		// add the link span
		listItem.append('<span class="btn-icon"><i class="fa fa-fw fa-download"></i><a href="/media/attachment/' + response.id + '">' + file.name + '</a></span>');
		listItem.append('<span class="attachment-meta text-muted"> ' + date + ' <em>(' + file.type + '</em> ' + getFileSizeDisplay(file.size) + ')</span>');
		listItem.append('<span class="pull-right remove-attachment"><a data-href="/jobs/' + response.jobId + '/remove-attachment/' + response.id + '" title="Remove attachment ' + response.filename + '" data-filename="' + response.filename +'" data-toggle="modal" data-target="#confirm-delete"><i class="fa fa-fw fa-times"></i></a></span>');

		$('.attachment-list').prepend(listItem);

	}

	/**
	 * Adds the image to the image gallery to be used within the attachment image gallery
	 * 
	 */
	function addGalleryImage(file, response)
	{

		// Create the image item
		var imgDiv = $('<div class="image-item"><a href="/media/attachment-resized/' + response.id + '?w=1024&h=768" title="' + file.name + '" data-gallery=""><img src="/media/attachment-resized/' + response.id + '?w=167&h=125"></a></div>');

		// prepend to the links list to be included in the gallery
		$('#attachment-gallery').prepend(imgDiv);

	}

	function getFileSizeDisplay(size) {

		if (size < 1)
		{
			return "0 kb";
		}
		else if (size < 1000000)
		{
			var kb = parseFloat(size / 1000).toFixed(2);
			return kb + " kb";
		}
		else
		{
			var mb = parseFloat(size / 1000000).toFixed(2);
			return mb + " mb";
		}

	}

	$(document).ready(function () {
		if ($('#dropzone-attachments').length > 0) {
			bindUI();
		}
	});

	function preloadAttachments() {

		// If we already have the preload container, then just exit as we don't want to re-add the images
		if ($('#attachment-preload').length > 0) {
			return;
		}

		// Create the attachments container
		var attachments = $('<div id="attachment-preload"></div>');

		// Loop through each image attachment
		$('#attachment-gallery a').each(function () {

			// Get the url to the img
			var imgUrl = $(this).attr('href');
			
			// Add the image to the preload list so that it's preloaded
			attachments.append('<img src="' + imgUrl + '" />');

		});

		$('body').append(attachments);

	}

	return {
		preloadAttachments: preloadAttachments
	}


})();