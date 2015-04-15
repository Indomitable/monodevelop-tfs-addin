using System;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    static class CollectionHelper
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            //return new T[] {item};
            yield return item;
        }

        
    }
}