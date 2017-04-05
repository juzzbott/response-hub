//
//
//// Change LocationInfo to AddressInfo class on the Address property and remove existing AddressInfo string
//db.job_messages.update({}, { $unset: { "Location.AddressInfo": 1 } }, { multi: true })
//db.job_messages.update({}, { $set: { "Location.Address": { "AddressId": null, "FormattedAddress": "" } } }, { multi: true })
//
//// Set the MessageType property
//db.job_messages.update({ JobNumber: { $ne: "" } }, { $set: { Type: 1 } }, { multi: 1 })
//db.job_messages.update({ JobNumber: "" }, { $set: { Type: 2 } }, { multi: 1 })
//
//// Set the user status to 2 - Active
//db.users.update({}, { $set: { Status: 2 } }, { multi: true })

var schema_info_id = ObjectId("58e492e962cb703bc785da68");

var schema_version = 3

// Ensure we have a schema to start with
var schema_count = db.schema_info.count({})

// If there is no schema version, then create one at version 1
if (schema_count == 0) {
	db.schema_info.insert({ _id: schema_info_id, version: 0 })
}

// Get the schema version from the database
var schema_info = db.schema_info.findOne({ _id: schema_info_id })
var current_version = schema_info.version

// Do while the schema version is < max schema version
while (current_version < schema_version) {

	// Increment the schema version
	current_version++;

	// Print the current version
	print(current_version);

	// Perform the schema upgrade
	switch (current_version) {

		case 1:
			// Map indexes
			db.map_indexes.createIndex({ "PageNumber": 1 }, { background: true });
			db.map_indexes.createIndex({ "GridReferences.Coordinates": "2dsphere" }, { background: true, name: "Coords_2dsphere" });

			// Capcodes
			db.capcodes.createIndex({ Name: "text", ShortName: "text" }, { background: true, name: "capcodes_text" });

			// Groups
			db.groups.createIndex({ Name: "text" }, { background: true });
			db.groups.createIndex({ HeadquartersCoordinates: "2dsphere" }, { background: true, name: "HQ_Coords_2dsphere" });
			db.groups.createIndex({ "Users.UserId": 1 }, { background: true, name: "Users_UserId" });

			// Job Messages
			db.job_messages.createIndex({ "Location.Coordinates": "2dsphere" }, { background: true, name: "Loc_Coords_2dsphere" });
			db.job_messages.createIndex({ MessageContent: "text", JobNumber: "text" }, { background: true, name: "job_messages_text" });

			// Users
			db.users.createIndex({ FirstName: "text", Surname: "text", EmailAddress: "text" }, { background: true, name: "users_text" });

			// Events
			db.events.createIndex({ "GroupId": 1 }, { background: true });
			db.events.createIndex({ Name: "text" }, { background: true, name: "events_text" });

			// Addresses
			db.addresses.createIndex({ "Coordinates": "2dsphere" }, { background: true, name: "Coords_2dsphere" });
			db.addresses.createIndex({ AddressQueryHash: 1 }, { background: true });

			// User sign ins
			db.user_sign_ins.createIndex({ "OperationDetails.JobId": 1 }, { background: true, partialFilterExpression: { "OperationDetails.JobId": { $exists: true }, "SignInType": 1 }, name: "SignIn_JobId" });
			break;

		case 2:
			// Update the training session types - General
			db.training_sessions.update({}, { $unset: { TrainingType: 1 }, $set: { TrainingTypeId: BinData(4, "Q6gHw6s3Qne1pwKjx84MDg==") } }, { multi: true });
			break;

		case 3:
			break;
	}

	// Write the new schema version to the database
	db.schema_info.update({ _id: schema_info_id }, { $set: { version: current_version } })

}