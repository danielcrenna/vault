using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT && !WINRT
	[Serializable]
#endif
#if !Smartphone && !NET20
	[DataContract]
#endif
	[JsonObject(MemberSerialization.OptIn)]
	public class TwitterConfiguration : PropertyChangedBase
	{
		private int _CharactersReservedPerMedia;
		private int _MaxMediaPerUpload;
		private int _ShortUrlLength;
		private int _ShortUrlLengthHttps;
		private int _PhotoSizeLimit;
		private IEnumerable<string> _NonUserNamePaths;
		private TwitterConfigurationPhotoSizes _PhotoSizes;

		[JsonProperty("characters_reserved_per_media")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int CharactersReservedPerMedia
		{
			get { return _CharactersReservedPerMedia; }
			set
			{
				if (_CharactersReservedPerMedia == value)
				{
					return;
				}

				_CharactersReservedPerMedia = value;
				OnPropertyChanged("CharactersReservedPerMedia");
			}
		}

		[JsonProperty("max_media_per_upload")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int MaxMediaPerUpload
		{
			get { return _MaxMediaPerUpload; }
			set
			{
				if (_MaxMediaPerUpload == value)
				{
					return;
				}

				_MaxMediaPerUpload = value;
				OnPropertyChanged("MaxMediaPerUpload");
			}
		}

		[JsonProperty("short_url_length")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int ShortUrlLength
		{
			get { return _ShortUrlLength; }
			set
			{
				if (_ShortUrlLength == value)
				{
					return;
				}

				_ShortUrlLength = value;
				OnPropertyChanged("ShortUrlLength");
			}
		}

		[JsonProperty("short_url_length_https")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int ShortUrlLengthHttps
		{
			get { return _ShortUrlLengthHttps; }
			set
			{
				if (_ShortUrlLengthHttps == value)
				{
					return;
				}

				_ShortUrlLengthHttps = value;
				OnPropertyChanged("ShortUrlLengthHttps");
			}
		}

		[JsonProperty("photo_size_limit")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int PhotoSizeLimit
		{
			get { return _PhotoSizeLimit; }
			set
			{
				if (_PhotoSizeLimit == value)
				{
					return;
				}

				_PhotoSizeLimit = value;
				OnPropertyChanged("PhotoSizeLimit");
			}
		}

		[JsonProperty("non_username_paths")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual IEnumerable<string> NonUserNamePaths
		{
			get { return _NonUserNamePaths; }
			set
			{
				if (_NonUserNamePaths == value)
				{
					return;
				}

				_NonUserNamePaths = value;
				OnPropertyChanged("NonUserNamePaths");
			}
		}

		[JsonProperty("photo_sizes")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterConfigurationPhotoSizes PhotoSizes
		{
			get { return _PhotoSizes; }
			set
			{
				if (_PhotoSizes == value)
				{
					return;
				}

				_PhotoSizes = value;
				OnPropertyChanged("PhotoSizes");
			}
		}

	}

	public class TwitterConfigurationPhotoSizes : PropertyChangedBase
	{
		private TwitterConfigurationPhotoSize _ThumbPhotoSize;
		private TwitterConfigurationPhotoSize _SmallPhotoSize;
		private TwitterConfigurationPhotoSize _MediumPhotoSize;
		private TwitterConfigurationPhotoSize _LargePhotoSize;

		[JsonProperty("thumb")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterConfigurationPhotoSize Thumb
		{
			get { return _ThumbPhotoSize; }
			set
			{
				if (_ThumbPhotoSize == value)
				{
					return;
				}

				_ThumbPhotoSize = value;
				OnPropertyChanged("Thumb");
			}
		}

		[JsonProperty("small")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterConfigurationPhotoSize Small
		{
			get { return _SmallPhotoSize; }
			set
			{
				if (_SmallPhotoSize == value)
				{
					return;
				}

				_SmallPhotoSize = value;
				OnPropertyChanged("Small");
			}
		}

		[JsonProperty("medium")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterConfigurationPhotoSize Medium
		{
			get { return _MediumPhotoSize; }
			set
			{
				if (_MediumPhotoSize == value)
				{
					return;
				}

				_MediumPhotoSize = value;
				OnPropertyChanged("Medium");
			}
		}

		[JsonProperty("large")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual TwitterConfigurationPhotoSize Large
		{
			get { return _LargePhotoSize; }
			set
			{
				if (_LargePhotoSize == value)
				{
					return;
				}

				_LargePhotoSize = value;
				OnPropertyChanged("Large");
			}
		}


	}

	public class TwitterConfigurationPhotoSize : PropertyChangedBase
	{

		private int _Height;
		private int _Width;
		private string _Resize;

		[JsonProperty("h")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int Height
		{
			get { return _Height; }
			set
			{
				if (_Height == value)
				{
					return;
				}

				_Height = value;
				OnPropertyChanged("Height");
			}
		}

		[JsonProperty("w")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual int Width
		{
			get { return _Width; }
			set
			{
				if (_Width == value)
				{
					return;
				}

				_Width = value;
				OnPropertyChanged("Width");
			}
		}

		[JsonProperty("resize")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Resize
		{
			get { return _Resize; }
			set
			{
				if (_Resize == value)
				{
					return;
				}

				_Resize = value;
				OnPropertyChanged("resize");
			}
		}

		}
}