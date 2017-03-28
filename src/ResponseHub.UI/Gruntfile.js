/// <binding ProjectOpened='watch_css, watch_js' />
module.exports = function(grunt) {

	grunt.initConfig({
		pkg: grunt.file.readJSON('package.json'),
		uglify: {
			options: {
				banner: '/*! <%= pkg.name %> <%= grunt.template.today("yyyy-mm-dd") %> */\n'
			},
			build: {
				src: 'src/<%= pkg.name %>.js',
				dest: 'build/<%= pkg.name %>.min.js'
			}
		},
		concat: {
			options: {
				separator: '\r\n\r\n'
			},
			framework_js: {
				src: [
					'bower_components/jquery/dist/jquery.js',
					'bower_components/jquery-validation/dist/jquery.validate.min.js',
					'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
					'bower_components/bootstrap/dist/js/bootstrap.js',
					'bower_components/moment/min/moment.min.js',
					'bower_components/bootstrap-select/dist/js/bootstrap-select.min.js',
					'assets/js/lib/bootstrap-datepicker.min.js',
					'assets/js/lib/bootstrap-tabcollapse.js',
					'assets/js/lib/bootstrap-typeahead.min.js',
					'assets/js/lib/leaflet.js',
					'assets/js/lib/palette.js',
					'assets/js/lib/fm.scrollator.jquery.js',
					'bower_components/dropzone/dist/min/dropzone.min.js',
					'bower_components/blueimp-gallery/js/jquery.blueimp-gallery.min.js',
					'bower_components/blueimp-bootstrap-image-gallery/js/bootstrap-image-gallery.min.js',
					'bower_components/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js',
					'bower_components/Chart.js/dist/Chart.min.js'
				],
				dest: 'assets/js/framework.js'
			},
			script_js: {
				separator: ';',
				banner: '',
				src: [
					'assets/js/modules/_core.js',
					'assets/js/modules/maps.js',
					'assets/js/modules/joblog.js',
					'assets/js/modules/pager-messages.js',
					'assets/js/modules/wallboard.js',
					'assets/js/modules/groups.js',
					'assets/js/modules/password-strength.js',
					'assets/js/modules/capcodes.js',
					'assets/js/modules/resources.js',
					'assets/js/modules/logviewer.js',
					'assets/js/modules/search.js',
					'assets/js/modules/attachments.js',
					'assets/js/modules/gallery.js',
					'assets/js/modules/weather-centre.js',
					'assets/js/modules/sign-on.js',
					'assets/js/modules/reports.js'
				],
				dest: 'assets/js/script.js'
			},
			framework_css: {
				src: [
					'bower_components/bootstrap/dist/css/bootstrap.css',
					'assets/css/lib/bootstrap-grid-xl.css',
					'bower_components/fontawesome/css/font-awesome.min.css',
					'bower_components/bootstrap-select/dist/css/bootstrap-select.min.css',
					'assets/css/lib/bootstrap-datepicker3.min.css',
					'assets/css/lib/leaflet.css',
					'assets/css/lib/fm.scrollator.jquery.css',
					'bower_components/dropzone/dist/min/dropzone.min.css',
					'bower_components/blueimp-gallery/css/blueimp-gallery.min.css',
					'bower_components/blueimp-bootstrap-image-gallery/css/bootstrap-image-gallery.min.css',
					'bower_components/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css'
				],
				dest: 'assets/css/framework.css'
			}
		},
		less: {
			build: {
				files: {
					'assets/css/response-hub.css': 'assets/css/less/response-hub.less'
				}
			}
		},
		watch: {
			css: {
				files: 'assets/css/less/*',
				tasks: ['less:build']
			},
			js: {
				files: 'assets/js/modules/*',
				tasks: ['concat:script_js']
			}
        }
	});
	
	// Load the plugin that provides the tasks.
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-watch');
	
	//  tasks
    grunt.registerTask('watch_css', ['watch:css']);
    grunt.registerTask('watch_js', ['watch:js']);
	grunt.registerTask('build', ['concat:framework_js', 'concat:script_js', 'concat:framework_css', 'less:build']);
	
};