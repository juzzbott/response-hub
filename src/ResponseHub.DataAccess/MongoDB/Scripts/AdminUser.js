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

// Users
db.users.insert({
	_id: UUID('32a5b3f3e6f04c5c85e26bfc1d773d54'),
	UserName: "admin@responsehub.com.au",
	PasswordHash: "ANl4mBpCh+YW/tcGhVrwu/17L45LbwOWWyamBpwrEcA2RNRVU1SboovQEZGYXRizbw==",
	EmailAddress: "admin@responsehub.com.au",
	FirstName: "ResponseHub",
	Surname: "Administrator",
	Created: ISODate("2016-02-03T12:55:44.279+0000"),
	PasswordResetToken: null,
	Claims: [
		{
			Type: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value: "System Administrator", Issuer: "ResponseHub"
		},
		{
			Type: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value: "General User", Issuer: "ResponseHub"
		}
	],
	Logins: [],
	ActivationCode: null
})