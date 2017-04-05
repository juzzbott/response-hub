// Change LocationInfo to AddressInfo class on the Address property and remove existing AddressInfo string
db.job_messages.update({}, { $unset: { "Location.AddressInfo": 1 } }, { multi: true })
db.job_messages.update({}, { $set: { "Location.Address": { "AddressId": null, "FormattedAddress": "" } } }, { multi: true })

// Set the MessageType property
db.job_messages.update({ JobNumber: { $ne: "" } }, { $set: { Type: 1 } }, { multi: 1 })
db.job_messages.update({ JobNumber: "" }, { $set: { Type: 2 } }, { multi: 1 })

// Set the user status to 2 - Active
db.users.update({}, { $set: { Status: 2 } }, { multi: true })

// Update the training session types - General
db.training_sessions.update({}, { $unset: { TrainingType: 1 }, $set: { TrainingTypeId: BinData(4, "Q6gHw6s3Qne1pwKjx84MDg==") } }, { multi: true })