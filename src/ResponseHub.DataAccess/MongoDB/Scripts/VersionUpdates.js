// Define the schema info
var schema_info_id = ObjectId("58e492e962cb703bc785da68");

// Define the max schema version
var schema_version = 5

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
			// Update the training types to lists of training type ids
			db.training_sessions.update({}, { $unset: { TrainingTypeId: 1 }, $set: { TrainingTypeIds: [BinData(4, "Q6gHw6s3Qne1pwKjx84MDg==")] }, $set: { Name: "General" } }, { multi: true });
			break;

		case 4:
			db.training_types.insert({ "_id": BinData(4, "Q6gHw6s3Qne1pwKjx84MDg=="), "Name": "General", "ShortName": "General", "Description": "Useful for recording general, non-specific training sessions.", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "dYqgBf1wSZSJ30xE0Dx+1A=="), "Name": "General Rescue", "ShortName": "GR", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "3pMdEgOzSUe1o4z6lTlQ2Q=="), "Name": "Road Rescue", "ShortName": "RCR", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "6BwTSBNTQCidRye5Ai2sgw=="), "Name": "Land Search", "ShortName": "Land search", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "D7I2vux2STCKWux+QDxXGA=="), "Name": "Crewperson / Coxswain", "ShortName": "Rescue boat", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "ROHCgtThS72VyEeNzjZu7g=="), "Name": "Chainsaw", "ShortName": "Chainsaw", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "DateF0YlSfiqQU9wIrJxdw=="), "Name": "Storm & Water", "ShortName": "Storm", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "ZVrPDC3VQyaxr0rRDyu1nA=="), "Name": "Communications", "ShortName": "Comms", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "PGUdiwx+S/u83ZQXsBow+A=="), "Name": "Safe Work at Height System", "ShortName": "SWAHS", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "pn0jSZikQbqI3UOpmPDilA=="), "Name": "First Aid", "ShortName": "First aid", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "sxRqJ5OYQRi1VaMBmVTjBw=="), "Name": "Casualty Handling", "ShortName": "Cas handling", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "6gLx6LzrSZmNgH3ZnuSbMA=="), "Name": "Four Wheel Drive", "ShortName": "4x4", "Description": "", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "D0c7VLbvRzafARf5t5b9YA=="), "Name": "Map and Navigation", "ShortName": "Map & Nav", "Description": "Reading and interpreting maps", "SortOrder": NumberInt(0) });
			db.training_types.insert({ "_id": BinData(4, "rmcdDgsSRZ6iGAF9ELPl2A=="), "Name": "Other", "ShortName": "Other", "Description": "", "SortOrder": NumberInt(99) });

		case 5:
			db.job_messages.update({}, { $set: { Version: NumberInt(1) } }, { multi: true });
			break;
	}

	// Write the new schema version to the database
	db.schema_info.update({ _id: schema_info_id }, { $set: { version: current_version } })

}