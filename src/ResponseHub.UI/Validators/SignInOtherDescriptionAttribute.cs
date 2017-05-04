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
	public class SignInOtherDescriptionAttribute : ValidationAttribute, IClientValidatable
	{
		
		public string OtherTypeField{ get; set; }

		public SignInOtherDescriptionAttribute(string otherTypeField)
		{
			OtherTypeField = otherTypeField;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// Set the default validation result
			ValidationResult validationResult = ValidationResult.Success;

			try
			{

				// Get the "OtherType" field from reflecting the context
				PropertyInfo otherTypeProperty = validationContext.ObjectType.GetProperty(OtherTypeField);

				// Ensure the sign in type property is a "SignInType" enum
				if (otherTypeProperty.PropertyType.Equals(typeof(OtherSignInType)))
				{

					string descriptionValue = (string)value;
					OtherSignInType otherType = (OtherSignInType)otherTypeProperty.GetValue(validationContext.ObjectInstance, null);

					// If the sign in type is the expected sign in type, then make sure there is a value in the textbox
					if (otherType == OtherSignInType.Other && String.IsNullOrEmpty(descriptionValue))
					{
						validationResult = new ValidationResult(ErrorMessageString);
					}

				}
				else
				{
					validationResult = new ValidationResult("An error occurred while validating the property. OtherTypeField is not of type OtherSignInType");
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
			ModelClientValidationRule rule = new ModelClientValidationRule();
			rule.ErrorMessage = ErrorMessage;
			rule.ValidationType = "othersignindescriptionset"; // This is the name the jQuery adapter will use

			//"otherpropertyname" is the name of the jQuery parameter for the adapter, must be LOWERCASE!
			rule.ValidationParameters.Add("othertypefield", OtherTypeField);

			yield return rule;
		}
	}
}