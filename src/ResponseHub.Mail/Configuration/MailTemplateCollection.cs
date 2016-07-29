using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Mail.Configuration
{
	public class MailTemplateCollection : ConfigurationElementCollection
	{

		public MailTemplateElement this[int index]
		{
			get
			{
				return (MailTemplateElement)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		public new MailTemplateElement this[string name]
		{
			get
			{
				return (MailTemplateElement)BaseGet(name);
			}
			set
			{
				if (BaseGet(name) != null)
				{
					BaseRemoveAt(BaseIndexOf(BaseGet(name)));
				}
				BaseAdd(value);
			}
		}

		/// <summary>
		/// Creates a new instance of the MailTemplate configuration element.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new MailTemplateElement();
		}

		/// <summary>
		/// Gets the configuration element key for the mail template configuration element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((MailTemplateElement)element).Name;
		}

	}
}
