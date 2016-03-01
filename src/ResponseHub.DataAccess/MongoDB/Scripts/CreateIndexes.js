// Map indexes
db.map_indexes.createIndex({ "PageNumber": 1 }, { background: true });
db.map_indexes.createIndex({ "GridReferences.Coordinates": "2dsphere" }, { background: true, name: "Coords_2dsphere" });

// Groups
db.groups.createIndex({ Name: "text" }, { background: true });
db.groups.createIndex({ HeadquartersCoordinates: "2dsphere" }, { background: true, name: "HQ_Coords_2dsphere" });
db.groups.createIndex({ "Users.UserId": 1 }, { background: true, name: "Users_UserId" })

// Users
db.users.createIndex({ FirstName: "text", Surname: "text", EmailAddress: "text" }, { background: true, name: "users_text" });