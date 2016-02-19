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
				separator: '\r\n\r\n',
			},
			framework_js: {
				src: [
					'bower_components/jquery/dist/jquery.js',
					'bower_components/jquery-validation/dist/jquery.validate.min.js',
					'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
					'bower_components/bootstrap/dist/js/bootstrap.js',
					'bower_components/moment/min/moment.min.js',
					'assets/js/lib/leaflet.js'
				],
				dest: 'assets/js/framework.js'
			},
			script_js: {
				separator: ';',
				banner: '',
				src: ['assets/js/modules/_core.js', 'assets/js/modules/joblog.js', 'assets/js/modules/wallboard.js'],
				dest: 'assets/js/script.js'
			},
			framework_css: {
				src: [
					'bower_components/bootstrap/dist/css/bootstrap.css',
					'bower_components/fontawesome/css/font-awesome.min.css',
					'assets/css/lib/leaflet.css'
				],
				dest: 'assets/css/framework.css',
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
				tasks: ['less:build'],
			},
			js: {
				files: 'assets/js/modules/*',
				tasks: ['concat:script_js'],
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