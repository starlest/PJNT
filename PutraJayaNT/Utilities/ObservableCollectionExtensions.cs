﻿using System;
using System.Collections.ObjectModel;

namespace PutraJayaNT.Utilities
{
    public static class ObservableCollectionExtensions
    {
        public static void RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            for (var i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                    collection.RemoveAt(i);      
            }
        }
    }
}
