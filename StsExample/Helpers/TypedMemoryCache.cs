using System;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace StsExample.Helpers
{
    public class TypedMemoryCache<T> : MemoryCache where T : class
    {
        private object WriteLock { get; } = new object();

        private readonly CacheItemPolicy defaultCacheItemPolicy = new CacheItemPolicy
        {
            SlidingExpiration = new TimeSpan(0, 15, 0)
        };
        
        public TypedMemoryCache(string name, NameValueCollection nvc = null, CacheItemPolicy policy = null) : base(name, nvc)
        {
            if (policy != null) defaultCacheItemPolicy = policy;
        }

        public bool TryGetOrSet(string cacheKey, Func<T> getData, out T returnData, CacheItemPolicy policy = null)
        {
            if (TryGet(cacheKey, out returnData))
                return true;

            lock (WriteLock)
            {
                if (TryGet(cacheKey, out returnData))
                    return true;

                returnData = getData();
                Set(cacheKey, returnData, policy ?? defaultCacheItemPolicy);
            }

            return false;
        }

        public bool TryGet(string cacheKey, out T returnItem)
        {
            returnItem = (T)this[cacheKey];
            return returnItem != null;
        }
    }
}