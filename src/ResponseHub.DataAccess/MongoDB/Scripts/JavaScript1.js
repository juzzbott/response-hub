// Change LocationInfo to AddressInfo class on the Address property and remove existing AddressInfo string
db.job_messages.update({}, { $unset: { "Location.AddressInfo": 1 } }, { multi: true })
db.job_messages.update({}, { $set: { "Location.Address": { "AddressId": null, "FormattedAddress": "" } } }, { multi: true })