// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.Helpers;
using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public class ObjectPool
    {
        /// <summary>
        /// The <see cref="ObjectPool"/>'s configuration.
        /// </summary>
        protected Config Config { get; set; }

        /// <summary>
        /// The available object pools.
        /// </summary>
        protected Dictionary<Type, Stack<object>> Pools { get; set; } = new Dictionary<Type, Stack<object>>();

        /// <summary>
        /// The available arrays containing object pools.
        /// </summary>
        protected Dictionary<Type, Dictionary<int, Stack<object>>> ArrayPools { get; set; } = new Dictionary<Type, Dictionary<int, Stack<object>>>();

        /// <summary>
        /// An object pool used for 
        /// </summary>
        /// <param name="config"></param>
        public ObjectPool(Config config)
        {
            Config = config;
        }

        /// <summary>
        /// Clears the <see cref="Pools"/> and <see cref="ArrayPools"/> of any data.
        /// </summary>
        public void Clear()
        {
            Pools.Clear();
            ArrayPools.Clear();
        }

        /// <summary>
        /// Retrieves an array of Type <see cref="T"/>, if one is found with the provided capacity, otherwise a new array is returned.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> contained within the array to retrieve.</typeparam>
        /// <param name="capacity">The capacity of the array to retrieve.</param>
        /// <returns>Returns an array of the provided capacity, if found, otherwise a new array is created and returned.</returns>
        public virtual T[] GetArray<T>(int capacity)
        {
            // Checks if pooling is enabled.
            // If not then return a new array of the provided capacity.
            if (!Config.PoolingEnabled)
            {
                return new T[capacity];
            }

            // Attempt to retrieve an array pool of the provided Type.
            // If an array pool of the provided Type does not exist then one is created and it is returned.
            var poolsDict = ArrayPools.GetOrCreate(typeof(T));

            // Attempt to retrieve stack of objects of the provided Type.
            // If a stack of the provided Type does not exist then one is created and it is returned.
            var stack = poolsDict.GetOrCreate(capacity);

            // If the stack is populated return the top most object as an array of Type T.
            // Otherwise return the newly created array.
            if (stack.Count > 0)
            {
                return stack.Pop() as T[];
            }

            return new T[capacity];
        }

        /// <summary>
        /// Retrieves the top most object from the pool, if <see cref="Config.PoolingEnabled"/> is true and the pool contains objects, otherwise a new object of Type T is created and returned.
        /// </summary>
        /// <typeparam name="T">The object Type to retrieve.</typeparam>
        /// <returns>Returns an object of Type <see cref="T"/>.</returns>
        public virtual T GetObject<T>() where T : class, new()
        {
            if (Config.PoolingEnabled)
            {
                var pool = Pools.GetOrCreate(typeof(T));

                if (pool.Count > 0)
                {
                    return pool.Pop() as T;
                }
            }
            return new T();
        }

        public virtual void ReturnObject<T>(T obj) where T : class
        {
            if (!Config.PoolingEnabled || obj == null)
            {
                return;
            }

            var pool = Pools.GetOrCreate(typeof(T));
            pool.Push(obj);
        }

        public virtual void ReturnObject<T>(T[] obj) where T : class
        {
            if (!Config.PoolingEnabled || obj == null)
            {
                return;
            }

            for (int i = 0; i < obj.Length; ++i)
            {
                ReturnObject(obj[i]);
                obj[i] = null;
            }

            var poolsDict = ArrayPools.GetOrCreate(typeof(T));
            var stack = poolsDict.GetOrCreate(obj.Length);
            stack.Push(obj);
        }

        public virtual void ReturnObject<K, T>(Dictionary<K, T> obj)
        {
            if (!Config.PoolingEnabled || obj == null)
            {
                return;
            }

            obj.Clear();

            var pool = Pools.GetOrCreate(obj.GetType());
            pool.Push(obj);
        }

        public virtual void ReturnChildren<T>(List<T> list) where T : class
        {
            if (Config.PoolingEnabled)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    ReturnObject<T>(list[i]);
                }
            }

            list.Clear();
        }

        public virtual void ReturnChildren<K, T>(Dictionary<K, T> dict) where T : class
        {
            if (Config.PoolingEnabled)
            {
                using (var enumerator = dict.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var e = enumerator.Current;
                        ReturnObject(e.Value);
                    }
                }
            }

            dict.Clear();
        }
    }
}