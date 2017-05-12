# Introduction
ResponseHub is a website that allows SES units to manage responses to requests for assistance.  The system works by parsing the data from decoded pager messages, and inserting those messages into the database. Members can login and view jobs for their unit.

ResponseHub functionality includes, but is not limited to:

* Reporting for jobs
 * Marking 'on route', 'on scene' and 'job clear' with timestamps to aid in reporting
 * Signing into a specific job to know which members attended
 * Displaying a map of the job, as well as a route to the job (based on the map reference within the message)
 * Uploading attachments
 * Adding notes to jobs
* A wallboard for overall view of jobs
* Management of a large number of jobs through 'Events'
* A weather centre for having up to date weather information
* Recording of unit training sessions
* Reporting tools for jobs, training and attendance
* Management of units by designated unit administrators
* Security to prevent unauthorised access of users.

**Note:** *ResponseHub is not designed to be an alerting platform. Units and members must still using their primary alerting system for responding to requests for assistance.*

# Build and test
The solution contains the following components:

* __ResponseHub.UI__ - Website application project
* __ResponseHub.PagerDecoder__ - Windows service to read PDW log files and parse pager messages as jobs and insert into the ResponseHub database 
* __ResponseHub.WebTasks__ - Windows service to perform period tasks for the website, such as caching BoM radar and weather data.
* Other projects such as model, data access, application services etc...

## Build results
|Test|Production|
|---|---|
|![alt text](https://responsehub.visualstudio.com/_apis/public/build/definitions/500d2900-f667-4dc5-9eb4-56dc83bcb1e6/1/badge "Test build result")|![alt text](https://responsehub.visualstudio.com/_apis/public/build/definitions/500d2900-f667-4dc5-9eb4-56dc83bcb1e6/2/badge "Production build result")|

## Tests
Tests are written using the xUnit testing framework.
