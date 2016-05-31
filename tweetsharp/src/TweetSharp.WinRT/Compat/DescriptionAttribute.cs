using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
	public class DescriptionAttribute : Attribute
	{

		public DescriptionAttribute(string description)
		{
			this.Description = description;
		}

		public string Description { get; set; }

	}
}
