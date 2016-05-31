using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if !SILVERLIGHT
using System.Collections.Specialized;
#else
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Web
{
    public class WebPairCollection : IList<WebPair>
    {
        private IList<WebPair> _parameters;

        public virtual WebPair this[string name]
        {
            get
            {
                var parameters = this.Where(p => p.Name.Equals(name));
                
                if(parameters.Count() == 0)
                {
                    return null;
                }

                if(parameters.Count() == 1)
                {
                    return parameters.Single();
                }

                var value = string.Join(",", parameters.Select(p => p.Value).ToArray());
                return new WebPair(name, value);
            }
        }

        public virtual IEnumerable<string> Names
        {
            get { return _parameters.Select(p => p.Name); }
        }

        public virtual IEnumerable<string> Values
        {
            get { return _parameters.Select(p => p.Value); }
        }

        public WebPairCollection(IEnumerable<WebPair> parameters)
        {
            _parameters = new List<WebPair>(parameters);
        }

        public WebPairCollection(NameValueCollection collection) : this()
        {
            AddCollection(collection);
        }

        public virtual void AddRange(NameValueCollection collection)
        {
            AddCollection(collection);
        }

        private void AddCollection(NameValueCollection collection)
        {
            var parameters = collection.AllKeys.Select(key => new WebPair(key, collection[key]));
            foreach (var parameter in parameters)
            {
                _parameters.Add(parameter);
            }
        }

        public WebPairCollection(IDictionary<string, string> collection) : this()
        {
            AddCollection(collection);
        }

        public void AddCollection(IDictionary<string, string> collection)
        {
            foreach (var parameter in collection.Keys.Select(key => new WebPair(key, collection[key])))
            {
                _parameters.Add(parameter);
            }
        }

        public WebPairCollection()
        {
            _parameters = new List<WebPair>(0);
        }

        public WebPairCollection(int capacity)
        {
            _parameters = new List<WebPair>(capacity);
        }

        private void AddCollection(IEnumerable<WebPair> collection)
        {
            foreach (var pair in collection.Select(parameter => new WebPair(parameter.Name, parameter.Value)))
            {
                _parameters.Add(pair);
            }
        }

        public virtual void AddRange(WebPairCollection collection)
        {
            AddCollection(collection);
        }

        public virtual void AddRange(IEnumerable<WebPair> collection)
        {
            AddCollection(collection);
        }

        public virtual void Sort(Comparison<WebPair> comparison)
        {
            var sorted = new List<WebPair>(_parameters);
            sorted.Sort(comparison);
            _parameters = sorted;
        }

        public virtual bool RemoveAll(IEnumerable<WebPair> parameters)
        {
            var array = parameters.ToArray();
            var success = array.Aggregate(true, (current, parameter) => current & _parameters.Remove(parameter));
            return success && array.Length > 0;
        }

        public virtual void Add(string name, string value)
        {
            var pair = new WebPair(name, value);
            _parameters.Add(pair);
        }

        #region IList<WebParameter> Members

        public virtual IEnumerator<WebPair> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(WebPair parameter)
        {
            
            _parameters.Add(parameter);
        }

        public virtual void Clear()
        {
            _parameters.Clear();
        }

        public virtual bool Contains(WebPair parameter)
        {
            return _parameters.Contains(parameter);
        }

        public virtual void CopyTo(WebPair[] parameters, int arrayIndex)
        {
            _parameters.CopyTo(parameters, arrayIndex);
        }

        public virtual bool Remove(WebPair parameter)
        {
            return _parameters.Remove(parameter);
        }

        public virtual int Count
        {
            get { return _parameters.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return _parameters.IsReadOnly; }
        }

        public virtual int IndexOf(WebPair parameter)
        {
            return _parameters.IndexOf(parameter);
        }

        public virtual void Insert(int index, WebPair parameter)
        {
            _parameters.Insert(index, parameter);
        }

        public virtual void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public virtual WebPair this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }

        #endregion
    }
}