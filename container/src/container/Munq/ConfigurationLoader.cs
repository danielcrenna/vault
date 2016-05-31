// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Munq.Configuration
{
	/// <summary>
	/// This static class contains methods to discover classes that implement the IMunqConfig interface
	/// and to call the RegisterIn method on them.
	/// </summary>
	public static class ConfigurationLoader
	{
		/// <summary>
		/// Finds all the types that implement the IMunqConfig interface, create an instance and 
		/// then call the RegisterIn method on the type.  The bin directory is checked for web
		/// applications, the current directory for Windows applications.
		/// </summary>
		/// <param name="container">The Munq IOC container to register class factories in.</param>
		public static void FindAndRegisterDependencies(IocContainer container)
		{
			// get all the assemblies in the bin directory
			string binPath = HttpContext.Current != null ? HttpContext.Current.Server.MapPath("/bin")
														 : Environment.CurrentDirectory;
			CallRegistrarsInDirectory(container, binPath);
		}

		/// <summary>
		/// Finds all the types that implement the IMunqConfig interface, create an instance and 
		/// then call the RegisterIn method on the type.
		/// </summary>
		/// <param name="container">The Munq IOC container to register the class factories in.</param>
		/// <param name="binPath">The path of the directory to search.</param>
		/// <param name="filePattern">The optional file pattern for files to check. The default is *.dll</param>
		public static void CallRegistrarsInDirectory(IocContainer container, string binPath, string filePattern = "*.dll")
		{
			var assemblyNames = Directory.GetFiles(binPath, filePattern);

			foreach (var filename in assemblyNames)
				CallRegistrarsInAssembly(container, filename);

		}

		/// <summary>
		/// Finds all classes the IMunqConfig interface in an assembly and call the RegisterIn
		/// method on each.
		/// </summary>
		/// <param name="container">The Munq IOC Container.</param>
		/// <param name="filename">The full filename to be loaded and checked.</param>
		public static void CallRegistrarsInAssembly(IocContainer container, string filename)
		{
			var assembly = Assembly.LoadFile(filename);
			// find all the types that implements IMunqConfig ...
			var registrars = assembly.GetExportedTypes()
				 .Where(type => type.GetInterface(typeof(IMunqConfig).ToString()) != null);

			// and call the RegisterIn method on each
			foreach (var registrar in registrars)
				(Activator.CreateInstance(registrar) as IMunqConfig).RegisterIn(container);
		}
	}
}
