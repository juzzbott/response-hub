db.map_indexes.createIndex({ "GridReferences.Coordinates": "2dsphere" }, { background: true, name: "Coords_2dsphere" });

db.groups.createIndex({ Name: "text" }, { background: true });