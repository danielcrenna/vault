using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSharp
{
	public class TwitterErrors : ITwitterModel
	{
		public IList<TwitterError> errors { get; set; }

		#region ITwitterModel Members

		public virtual string RawSource { get; set; }

		#endregion
	}
}
