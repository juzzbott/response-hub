// Value is the element to be validated, params is the array of name/value pairs of the parameters extracted from the HTML, element is the HTML element that the validator is attached to
$.validator.addMethod("signintypedescriptionset", function (value, element, params) {

	

	// Get the element as a jq object
	var jqElem = $(element);

	// Get the current value of the selected sign in type
	var signInType = $("input[name='SignInType']:checked").val();
	var signInTypeDesc = getSignInTypeDescription(signInType);

	// Get the selected type from the element
	var validationSignInType = $(jqElem).data("val-signintypedescriptionset-expectedsignintype");

	console.log(signInType);
	console.log(signInTypeDesc);
	console.log(validationSignInType);
	console.log("------------------------------");

	// If the selected sign in type matches the expected sign in type and the value is empty, return false to set as invalid
	if (signInTypeDesc == validationSignInType && value == '')
	{
		return false;
	}

	// return true to default to true validation result
	return true;

});

function getSignInTypeDescription(signInType)
{
	if (signInType == "1")
	{
		return "Operation";
	}
	else if (signInType == "2") {
		return "Training";
	}
	else if (signInType == "4") {
		return "Other"
	}
}


/* The adapter signature:
adapterName is the name of the adapter, and matches the name of the rule in the HTML element.
 
params is an array of parameter names that you're expecting in the HTML attributes, and is optional. If it is not provided,
then it is presumed that the validator has no parameters.
 
fn is a function which is called to adapt the HTML attribute values into jQuery Validate rules and messages.
 
The function will receive a single parameter which is an options object with the following values in it:
element
The HTML element that the validator is attached to
 
form
The HTML form element
 
message
The message string extract from the HTML attribute
 
params
The array of name/value pairs of the parameters extracted from the HTML attributes
 
rules
The jQuery rules array for this HTML element. The adapter is expected to add item(s) to this rules array for the specific jQuery Validate validators
that it wants to attach. The name is the name of the jQuery Validate rule, and the value is the parameter values for the jQuery Validate rule.
 
messages
The jQuery messages array for this HTML element. The adapter is expected to add item(s) to this messages array for the specific jQuery Validate validators that it wants to attach, if it wants a custom error message for this rule. The name is the name of the jQuery Validate rule, and the value is the custom message to be displayed when the rule is violated.
*/
$.validator.unobtrusive.adapters.add("signintypedescriptionset", ["signintypefield", "expectedsignintype"], function (options) {
	options.rules["signintypedescriptionset"] = "#" + options.params.signintypefield;
	options.messages["signintypedescriptionset"] = options.message;
});