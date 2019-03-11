// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet.Helpers
{
    internal static class DictionaryHelper
    {
        /// <summary>
        /// Returns the existing value from the target dictionary for the given key.
        /// If the key is not present, it instantiates a new <see cref="TValue"/>, puts it in the dictionary and returns it.
        /// </summary>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : class, new()
        {
            dict.TryGetValue(key, out var value);

            if (value == null)
            {
                value = new TValue();
                dict[key] = value;
            }

            return value;
        }
    }
}