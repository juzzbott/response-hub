/*
	Inserts the reference data into the relevant tables.
*/

// Regions
db.regions.remove({});
db.regions.insert({ _id: UUID('72c785fec55247ec93a3c122b314012b'), Name: "Grampians (Mid-West)", Capcode: "", ServiceType: 1 });
db.regions.insert({ _id: UUID('1afc115fed0f4bc18b5ee766b2bec948'), Name: "Melbourne Metropolitan (Central)", Capcode: "", ServiceType: 1 });
db.regions.insert({ _id: UUID('b0d2faff6d0449488c1e0526aebee62a'), Name: "Gippsland (East)", Capcode: "", ServiceType: 1 });
db.regions.insert({ _id: UUID('5695233b74a94f5da214a52474efe769'), Name: "Hume (North East)", Capcode: "", ServiceType: 1 });
db.regions.insert({ _id: UUID('dde442814d2a419a8028900d064d526a'), Name: "Loddon Mallee (North West)", Capcode: "", ServiceType: 1 });
db.regions.insert({ _id: UUID('326d7776bcab4fdd8aa58524168046df'), Name: "Barwon South West (South West)", Capcode: "", ServiceType: 1 });

// Capcodes
db.capcodes.remove({});

// Agencies
db.agencies.remove({});
db.agencies.insert({ _id: UUID("34cd9f655e4643b190c60e3bd1d78e70"), Name: "Victoria State Emergency Service" });
db.agencies.insert({ _id: UUID("70a5caf80daa44889ee4e752bb287c13"), Name: "Country Fire Authority" });
db.agencies.insert({ _id: UUID("0038c291b483471c9096c2236794ef40"), Name: "Parks Victoria" });
db.agencies.insert({ _id: UUID("d9273f4fe1224ba6bb14f0d88bc9a86d"), Name: "Victoria Police" });
db.agencies.insert({ _id: UUID("cdaad166ee3a439aa2d3bf52b4fa3f02"), Name: "DELWP" });