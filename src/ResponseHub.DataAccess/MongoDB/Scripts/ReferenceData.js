function HexToBase64(hex) {
	var base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
	var base64 = "";
	var group;
	for (var i = 0; i < 30; i += 6) {
		group = parseInt(hex.substr(i, 6), 16);
		base64 += base64Digits[(group >> 18) & 0x3f];
		base64 += base64Digits[(group >> 12) & 0x3f];
		base64 += base64Digits[(group >> 6) & 0x3f];
		base64 += base64Digits[group & 0x3f];
	}
	group = parseInt(hex.substr(30, 2), 16);
	base64 += base64Digits[(group >> 2) & 0x3f];
	base64 += base64Digits[(group << 4) & 0x3f];
	base64 += "==";
	return base64;
}

function UUID(uuid) {
	var hex = uuid.replace(/[{}-]/g, ""); // remove extra characters
	var base64 = HexToBase64(hex);
	return new BinData(4, base64); // new subtype 4
}

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
db.capcodes.insert({ _id: UUID('ae372a207823410f858463a534c2ec4c'), CapcodeAddress: "0240001", Name: "State Information", ShortName: "STATE_INFO", Service: NumberInt(5), Created: ISODate("2016-05-05T12:01:07.173+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsUnitCapcode: false });
db.capcodes.insert({ _id: UUID('3d6d478fc65e4a3eacf28fad3b4c1755'), CapcodeAddress: "0240009", Name: "Weather Central Region", ShortName: "BOMCENT", Service: NumberInt(1), Created: ISODate("2016-05-05T12:01:48.201+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsUnitCapcode: false });
db.capcodes.insert({ _id: UUID('d7fada69878746c19c45e61718cd0f68'), CapcodeAddress: "0241209", Name: "Mid-West Region Info", ShortName: "M_W_INFO", Service: NumberInt(5), Created: ISODate("2016-05-05T12:02:31.029+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsUnitCapcode: false });
db.capcodes.insert({ _id: UUID('d8ff70f092ca428ab9be17fd2bd6f814'), CapcodeAddress: "0241249", Name: "Bacchus Marsh SES", ShortName: "BACC", Service: NumberInt(5), Created: ISODate("2016-05-05T12:02:51.174+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsUnitCapcode: true });


// Agencies
db.agencies.remove({});
db.agencies.insert({ _id: UUID("34cd9f655e4643b190c60e3bd1d78e70"), Name: "Victoria State Emergency Service" });
db.agencies.insert({ _id: UUID("70a5caf80daa44889ee4e752bb287c13"), Name: "Country Fire Authority" });
db.agencies.insert({ _id: UUID("0038c291b483471c9096c2236794ef40"), Name: "Parks Victoria" });
db.agencies.insert({ _id: UUID("d9273f4fe1224ba6bb14f0d88bc9a86d"), Name: "Victoria Police" });
db.agencies.insert({ _id: UUID("cdaad166ee3a439aa2d3bf52b4fa3f02"), Name: "DELWP" });


// Training Types
db.training_types.remove({});
db.training_types.insert({ _id: UUID("43a807c3ab374277b5a702a3c7ce0c0e"), Name: "General", ShortName: "General", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("758aa005fd70499489df4c44d03c7ed4"), Name: "General Rescue", ShortName: "GR", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("de931d1203b34947b5a38cfa953950d9"), Name: "Road Rescue", ShortName: "RCR", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("e81c1348135340289d4727b9022dac83"), Name: "Land Search", ShortName: "Land search", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("0fb236beec7649308a5aec7e403c5718"), Name: "Crewperson / Coxswain", ShortName: "Rescue boat", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("44e1c282d4e14bbd95c8478dce366eee"), Name: "Chainsaw", ShortName: "Chainsaw", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("0dab5e17462549f8aa414f7022b27177"), Name: "Storm & Water", ShortName: "Storm", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("655acf0c2dd54326b1af4ad10f2bb59c"), Name: "Communications", ShortName: "Comms", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("3c651d8b0c7e4bfbbcdd9417b01a30f8"), Name: "Safe Work at Height System", ShortName: "SWAHS", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("a67d234998a441ba88dd43a998f0e294"), Name: "First Aid", ShortName: "First aid", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("b3146a2793984118b555a3019954e307"), Name: "Casualty Handling", ShortName: "Cas handling", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("ea02f1e8bceb49998d807dd99ee49b30"), Name: "Four Wheel Drive", ShortName: "4x4", Description: "", SortOrder: 0 });
db.training_types.insert({ _id: UUID("ae671d0e0b12459ea218017d10b3e5d8"), Name: "Other", ShortName: "Other", Description: "", SortOrder: 99 });