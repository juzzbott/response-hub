// Map indexes
db.map_indexes.createIndex({ "PageNumber": 1 }, { background: true });
db.map_indexes.createIndex({ "GridReferences.Coordinates": "2dsphere" }, { background: true, name: "Coords_2dsphere" });

// Capcodes
db.capcodes.createIndex({ Name: "text", ShortName: "text" }, { background: true, name: "capcodes_text" });

// Units
db.units.createIndex({ Name: "text" }, { background: true });
db.units.createIndex({ HeadquartersCoordinates: "2dsphere" }, { background: true, name: "HQ_Coords_2dsphere" });
db.units.createIndex({ "Users.UserId": 1 }, { background: true, name: "Users_UserId" });

// Job Messages
db.job_messages.createIndex({ "Location.Coordinates": "2dsphere" }, { background: true, name: "Loc_Coords_2dsphere" });
db.job_messages.createIndex({ MessageContent: "text", JobNumber: "text" }, { background: true, name: "job_messages_text" });

// Users
db.users.createIndex({ FirstName: "text", Surname: "text", EmailAddress: "text" }, { background: true, name: "users_text" });

// Events
db.events.createIndex({ "UnitId": 1 }, { background: true });
db.events.createIndex({ Name: "text" }, { background: true, name: "events_text" });

// Addresses
db.addresses.createIndex({ "Coordinates": "2dsphere" }, { background: true, name: "Coords_2dsphere" });
db.addresses.createIndex({ AddressQueryHash: 1 }, { background: true });

// User sign ins
db.user_sign_ins.createIndex({ "OperationDetails.JobId": 1 }, { background: true, partialFilterExpression: { "OperationDetails.JobId": { $exists: true }, "SignInType": 1 }, name: "SignIn_JobId" });