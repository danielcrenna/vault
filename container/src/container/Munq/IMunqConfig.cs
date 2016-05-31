// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

namespace Munq.Configuration
{
	/// <summary>
	/// This interface is defined on classes that are used to dynamically register dependencies 
	/// in the container.  Classes implementing this interface can be discovered and the RegisterIn
	/// method automatically called.
	/// </summary>
	public interface IMunqConfig
	{
		/// <summary>
		/// Classes that implement this interface are automatically called to
		/// register type factories in the Munq IOC container
		/// </summary>
		/// <param name="container">The Munq IOC Container.</param>
		void RegisterIn(IDependecyRegistrar container);
	}
}
