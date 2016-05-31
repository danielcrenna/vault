using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSharp
{
#if PLATFORM_SUPPORTS_ASYNC_AWAIT
	public class TwitterAsyncResult<T>
	{

		private readonly TwitterResponse _Response;

		private readonly T _Value;

		public TwitterAsyncResult(T value, TwitterResponse response)
		{
			_Response = response;
			_Value = value;
		}

		public TwitterResponse Response
		{
			get { return _Response; }
		}

		public T Value
		{
			get { return _Value; }
		} 

	}
#endif
}
