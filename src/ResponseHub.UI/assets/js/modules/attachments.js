responseHub.attachments = (function () {

	function bindUI() {

		Dropzone.options.dropzoneAttachments = {
			maxFilesize: 100
		};

	}

	$(document).ready(function () {
		if ($('#dropzone-attachments').length > 0) {
			bindUI();
		}
	});


})();