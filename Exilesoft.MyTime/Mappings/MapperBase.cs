using System;
using System.Collections.Generic;

namespace Exilesoft.MyTime.Mappings
{
    public abstract class MapperBase<TFirst, TSecond>
    {

        #region Methods

        public abstract TFirst Map(TSecond element);

        public List<TFirst> Map(List<TSecond> elements, Action<TFirst> callback)
        {
            var objectCollection = new List<TFirst>();

            if (elements != null)
            {
                foreach (TSecond element in elements)
                {
                    TFirst newObject = Map(element);
                    if (newObject != null)
                    {
                        if (callback != null)
                            callback(newObject);
                        objectCollection.Add(newObject);
                    }
                }
            }
            return objectCollection;
        }

        public abstract TSecond Map(TFirst element);

        public List<TSecond> Map(List<TFirst> elements)
        {
            var objectCollection = new List<TSecond>();

            if (elements != null)
            {
                foreach (TFirst element in elements)
                {
                    TSecond newObject = Map(element);
                    if (newObject != null)
                    {
                        objectCollection.Add(newObject);
                    }
                }
            }
            return objectCollection;
        }

        public List<TSecond> Map(List<TFirst> elements, Action<TSecond> callback)
        {
            var objectCollection = new List<TSecond>();

            if (elements != null)
            {
                foreach (TFirst element in elements)
                {
                    TSecond newObject = Map(element);
                    if (newObject != null)
                    {
                        if (callback != null)
                            callback(newObject);
                        objectCollection.Add(newObject);
                    }
                }
            }
            return objectCollection;
        }

        public List<TFirst> Map(List<TSecond> elements)
        {
            var objectCollection = new List<TFirst>();

            if (elements != null)
            {
                foreach (TSecond element in elements)
                {
                    TFirst newObject = Map(element);
                    if (newObject != null)
                    {
                        objectCollection.Add(newObject);
                    }
                }
            }
            return objectCollection;
        }

        #endregion

    }

}
