var gulp = require('gulp');
var less = require('gulp-less');
var path = require('path');
var concat = require('gulp-concat');
 
gulp.task('less', function () {
  return gulp.src('./assets/css/less/*.less')
    .pipe(less({
      paths: [ path.join(__dirname, 'less', 'includes') ]
    }))
    .pipe(gulp.dest('./assets/css'));
});

gulp.task('concat:framework_css', function() {
  return gulp.src([
    'bower_components/bootstrap/dist/css/bootstrap.css',
    'assets/css/lib/bootstrap-grid-xl.css',
    'bower_components/fontawesome/css/font-awesome.min.css',
    'bower_components/bootstrap-select/dist/css/bootstrap-select.min.css',
    'assets/css/lib/jquery-ui.min.css',
    'assets/css/lib/bootstrap-datepicker3.min.css',
    'assets/css/lib/leaflet.css',
    'assets/css/lib/fm.scrollator.jquery.css',
    'bower_components/dropzone/dist/min/dropzone.min.css',
    'bower_components/blueimp-gallery/css/blueimp-gallery.min.css',
    'bower_components/blueimp-bootstrap-image-gallery/css/bootstrap-image-gallery.min.css',
    'bower_components/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css',
    'bower_components/chartist/dist/chartist.min.css'
  ])
    .pipe(concat('framework.css'))
    .pipe(gulp.dest('./assets/css'));
});

gulp.task('concat:framework_js', function() {
  return gulp.src([
    'bower_components/jquery/dist/jquery.js',
    'bower_components/jquery-validation/dist/jquery.validate.min.js',
    'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
    'bower_components/bootstrap/dist/js/bootstrap.js',
    'bower_components/moment/min/moment.min.js',
    'bower_components/bootstrap-select/dist/js/bootstrap-select.min.js',
    'assets/js/lib/jquery-ui.min.js',
    'assets/js/lib/bootstrap-datepicker.min.js',
    'assets/js/lib/bootstrap-tabcollapse.js',
    'assets/js/lib/bootstrap-typeahead.min.js',
    'assets/js/lib/leaflet.js',
    'assets/js/lib/leaflet-html-icons.js',
    'assets/js/lib/palette.js',
    'assets/js/lib/fm.scrollator.jquery.js',
    'bower_components/dropzone/dist/min/dropzone.min.js',
    'bower_components/blueimp-gallery/js/jquery.blueimp-gallery.min.js',
    'bower_components/blueimp-bootstrap-image-gallery/js/bootstrap-image-gallery.min.js',
    'bower_components/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js',
    'bower_components/Chart.js/dist/Chart.min.js',
    'bower_components/chartist/dist/chartist.min.js'
  ])
    .pipe(concat('framework.js'))
    .pipe(gulp.dest('./assets/js'));
});

gulp.task('concat:script_js', function() {
  return gulp.src([
    'assets/js/modules/_core.js',
    'assets/js/modules/user-list.js',
    'assets/js/modules/maps.js',
    'assets/js/modules/job-messages.js',
    'assets/js/modules/pager-messages.js',
    'assets/js/modules/wallboard.js',
    'assets/js/modules/units.js',
    'assets/js/modules/password-strength.js',
    'assets/js/modules/capcodes.js',
    'assets/js/modules/resources.js',
    'assets/js/modules/logviewer.js',
    'assets/js/modules/search.js',
    'assets/js/modules/attachments.js',
    'assets/js/modules/gallery.js',
    'assets/js/modules/weather-centre.js',
    'assets/js/modules/sign-in.js',
    'assets/js/modules/reports.js',
    'assets/js/modules/training.js',
    'assets/js/modules/resources.js',
    'assets/js/modules/events.js',
    'assets/js/modules/validation.js'
  ])
    .pipe(concat('script.js', {newLine: '\r\n'}))
    .pipe(gulp.dest('./assets/js'));
});

gulp.task('build-all', ['concat:framework_css', 'concat:framework_js', 'concat:script_js', 'less']);

gulp.watch('assets/js/modules/*.js', ['concat:script_js']);
gulp.watch('assets/css/less/*.less', ['less']);