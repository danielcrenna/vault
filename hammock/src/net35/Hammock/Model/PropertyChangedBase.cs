using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Hammock.Model
{
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20 && !ClientProfiles && !NETCF
    [DataContract]
#endif
  public class PropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

#if !SILVERLIGHT
        [field: NonSerialized]
#endif
        public virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}