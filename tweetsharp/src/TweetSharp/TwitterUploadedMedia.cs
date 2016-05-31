using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
	public class TwitterUploadedMedia : PropertyChangedBase,
																IComparable<TwitterUploadedMedia>,
																IEquatable<TwitterUploadedMedia>,
																ITwitterModel
	{

		private string _Media_Id;
		private long _Size;
		private UploadedImage _Image;


		[JsonProperty("media_id")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual string Media_Id
		{
			get { return _Media_Id; }
			set
			{
				if (_Media_Id == value)
				{
					return;
				}

				_Media_Id = value;
				OnPropertyChanged("Media_Id");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual long Size
		{
			get { return _Size; }
			set
			{
				if (_Size == value)
				{
					return;
				}

				_Size = value;
				OnPropertyChanged("Size");
			}
		}

#if !Smartphone && !NET20
		[DataMember]
#endif
		public virtual UploadedImage Image
		{
			get { return _Image; }
			set
			{
				if (_Image == value)
				{
					return;
				}

				_Image = value;
				OnPropertyChanged("Image");
			}
		}


		#region ITwitterModel Members

		public string RawSource
		{
			get;
			set;
		}

		#endregion

		#region IComparable<TwitterUploadedMedia> Members

		public int CompareTo(TwitterUploadedMedia other)
		{
			if (other == null) return -1;

			throw new NotImplementedException();
		}

		#endregion

		#region IEquatable<TwitterUploadedMedia> Members

		public bool Equals(TwitterUploadedMedia other)
		{
			if (other == null) return false;

			return ReferenceEquals(this, other) || Equals(other.Media_Id, Media_Id);
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == typeof(TwitterUploadedMedia) && Equals((TwitterUploadedMedia)obj);
		}

		public override int GetHashCode()
		{
			return (Media_Id != null ? Media_Id.GetHashCode() : 0);
		}

		public static bool operator ==(TwitterUploadedMedia left, TwitterUploadedMedia right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(TwitterUploadedMedia left, TwitterUploadedMedia right)
		{
			return !Equals(left, right);
		}

	}

	public class UploadedImage
	{
		[JsonProperty("w")]
#if !Smartphone && !NET20
		[DataMember]
#endif
		public int Width { get; set; }
#if !Smartphone && !NET20
		[DataMember]
#endif

		[JsonProperty("h")]
		public int Height { get; set; }
#if !Smartphone && !NET20
		[DataMember]
#endif

		public string ImageType { get; set; }
	}

	public class MediaFile
	{
		public string FileName { get; set; }
		public System.IO.Stream Content { get; set; }
	}
}