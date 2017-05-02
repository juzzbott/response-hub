using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.UI.Validators
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SignInTypeDescriptionAttribute : ValidationAttribute, IClientValidatable
	{

		public string SignInTypeField { get; set; }

		public SignInType ExpectedSignInType { get; set; }

		/// <summary>
		/// Creates a new instance of the SignInTypeDescription attribute
		/// </summary>
		/// <param name="signInTypeField"></param>
		public SignInTypeDescriptionAttribute(string signInTypeField, SignInType expectedSignInType)
		{
			SignInTypeField = signInTypeField;
			ExpectedSignInType = expectedSignInType;
		}

		/// <summary>
		/// Determines if the field is valid. Valid is if the specific option is selected and there is a value in the corresponding description input field.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// Set the default validation result
			ValidationResult validationResult = ValidationResult.Success;

			try
			{

				// Get the "SignInType" field from reflecting the context
				PropertyInfo signInTypeProperty = validationContext.ObjectType.GetProperty(SignInTypeField);

				// Ensure the sign in type property is a "SignInType" enum
				if (signInTypeProperty.PropertyType.Equals(typeof(SignInType)))
				{

					string descriptionValue = (string)value;
					SignInType signInType = (SignInType)signInTypeProperty.GetValue(validationContext.ObjectInstance, null);

					// If the sign in type is the expected sign in type, then make sure there is a value in the textbox
					if (signInType == ExpectedSignInType && String.IsNullOrEmpty(descriptionValue))
					{
						validationResult = new ValidationResult(ErrorMessageString);
					}

				}
				else
				{
					validationResult = new ValidationResult("An error occurred while validating the property. SignInTypeField is not of type SignInType");	
				}

			}
			catch (Exception ex)
			{
				ILogger log = ServiceLocator.Get<ILogger>();
				log.Error(String.Format("Error validating SignOnTypeDescription attribute. Message: {0}", ex.Message), ex);
			}

			// return the validation result
			return validationResult;
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{

			// The value we set here are needed by the jQuery adapter
			ModelClientValidationRule signInTypeDescriptionSetRule = new ModelClientValidationRule();
			signInTypeDescriptionSetRule.ErrorMessage = ErrorMessage;
			signInTypeDescriptionSetRule.ValidationType = "signintypedescriptionset"; // This is the name the jQuery adapter will use
																	
			//"otherpropertyname" is the name of the jQuery parameter for the adapter, must be LOWERCASE!
			signInTypeDescriptionSetRule.ValidationParameters.Add("signintypefield", SignInTypeField);
			signInTypeDescriptionSetRule.ValidationParameters.Add("expectedsignintype", ExpectedSignInType);

			yield return signInTypeDescriptionSetRule;

		}

	}
}