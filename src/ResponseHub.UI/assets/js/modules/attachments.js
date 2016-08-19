responseHub.attachments = (function () {

	function bindUI() {

		Dropzone.options.dropzoneAttachments = {
			maxFilesize: 100,
			previewTemplate: '',
			init: function () {
				this.on("addedfile", function (file) {
					console.log(file);
				});
			}
		};

	}

	$(document).ready(function () {
		if ($('#dropzone-attachments').length > 0) {
			bindUI();
		}
	});


})();