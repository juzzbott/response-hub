using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Validators
{
	public class SignInOtherDescriptionAttribute : ValidationAttribute, IClientValidatable
	{



		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			throw new NotImplementedException();
		}
	}
}