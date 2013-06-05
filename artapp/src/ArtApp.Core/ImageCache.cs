using System;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;

namespace ArtApp
{
	public static class ImageCache
	{
		private static readonly Dictionary<string, UIImage> _images = new Dictionary<string, UIImage>();
		private static readonly string _path;
		
		public static int Count
		{
			get
			{
				return _images.Count;	
			}
		}
		
		public static IEnumerable<string> Keys
		{
			get
			{
				return _images.Keys;	
			}
		}

		static ImageCache()
		{
			_path = Path.Combine(AppGlobal.LibraryCachesPath, "Images");			
			if(!Directory.Exists(_path))
			{
				Directory.CreateDirectory(_path);
			}
		}
		
		public static void DumpDiskCache()
		{
			if(Directory.Exists(_path))
			{
				var files = Directory.GetFiles(_path, "*.*", SearchOption.AllDirectories);
				foreach(var file in files)
				{
					File.Delete(file);		
				}
			}
		}
		
		public static void StoreAs(string key, UIImage image)
		{
			if(_images.ContainsKey(key))
			{
				_images.Remove(key);	
			}
			var path = Path.Combine(_path, key);
			Save(path, image);
			_images.Add(key, image);
		}
		
		public static UIImage Get(string key)
		{
			if(!_images.ContainsKey(key))
			{
				var path = Path.Combine(_path, key);
				
				UIImage image;
				if(File.Exists(path))
				{
					image = UIImageEx.FromFile(path);	
				}
				else
				{
					return null;
				}
				
				return image;
			}			
			return _images[key];
		}
		
		public static UIImage Get(string key, Func<UIImage> generate)
		{
			if(!_images.ContainsKey(key))
			{
				var path = Path.Combine(_path, key);
				
				UIImage image;
				if(File.Exists(path))
				{
					image = UIImageEx.FromFile(path);	
				}
				else
				{
					image = generate();
					Save(path, image);
				}
				
				return image;
			}			
			return _images[key];
		}
		
		public static void Save(string path, UIImage image)
		{
			if(File.Exists(path))
			{
				File.Delete(path);	
			}
			NSError err = null;
			image.AsPNG().Save(path, true, out err);
		}
		
		public static void Clear()
		{
			_images.Clear();
		}
	}
}

