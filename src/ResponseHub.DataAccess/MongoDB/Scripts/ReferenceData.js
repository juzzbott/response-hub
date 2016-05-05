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
db.capcodes.insert({ _id: UUID('ae372a207823410f858463a534c2ec4c'), CapcodeAddress: "0240001", Name: "State Information", ShortName: "STATE_INFO", Service: NumberInt(5), Created: ISODate("2016-05-05T12:01:07.173+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsGroupCapcode: false });
db.capcodes.insert({ _id: UUID('3d6d478fc65e4a3eacf28fad3b4c1755'), CapcodeAddress: "0240009", Name: "Weather Central Region", ShortName: "BOMCENT", Service: NumberInt(1), Created: ISODate("2016-05-05T12:01:48.201+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsGroupCapcode: false });
db.capcodes.insert({ _id: UUID('d7fada69878746c19c45e61718cd0f68'), CapcodeAddress: "0241209", Name: "Mid-West Region Info", ShortName: "M_W_INFO", Service: NumberInt(5), Created: ISODate("2016-05-05T12:02:31.029+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsGroupCapcode: false });
db.capcodes.insert({ _id: UUID('d8ff70f092ca428ab9be17fd2bd6f814'), CapcodeAddress: "0241249", Name: "Bacchus Marsh SES", ShortName: "BACC", Service: NumberInt(5), Created: ISODate("2016-05-05T12:02:51.174+0000"), Updated: ISODate("0001-01-01T00:00:00.000+0000"), IsGroupCapcode: true });


// Agencies
db.agencies.remove({});
db.agencies.insert({ _id: UUID("34cd9f655e4643b190c60e3bd1d78e70"), Name: "Victoria State Emergency Service" });
db.agencies.insert({ _id: UUID("70a5caf80daa44889ee4e752bb287c13"), Name: "Country Fire Authority" });
db.agencies.insert({ _id: UUID("0038c291b483471c9096c2236794ef40"), Name: "Parks Victoria" });
db.agencies.insert({ _id: UUID("d9273f4fe1224ba6bb14f0d88bc9a86d"), Name: "Victoria Police" });
db.agencies.insert({ _id: UUID("cdaad166ee3a439aa2d3bf52b4fa3f02"), Name: "DELWP" });