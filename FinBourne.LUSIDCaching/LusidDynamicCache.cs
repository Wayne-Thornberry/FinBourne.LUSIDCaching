using System;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace FinBourne.LUSIDCaching
{
    public class LusidDynamicCache : MemoryCache
    {
        private static int _cacheLimit = 30;
        private static string _leastAccessedItem;
        private static object o = new object();
        private static LusidDynamicCache _cache;

        public static int Capacity => _cacheLimit;

        static LusidDynamicCache()
        {
            // Sets up a LusidDynamicCache extending a MemoryCache with default MemoryCache configurations. These can also be pulled from the app.config or appsettings.json
            _cache = new LusidDynamicCache();
        }

        public LusidDynamicCache(NameValueCollection config = null) : base("LusidDynamicCache", config)
        {
            if (config != null)
            {
                var limit = config["cacheLimit"];
                if (!string.IsNullOrEmpty(limit))
                {
                    int.TryParse(limit, out _cacheLimit);
                }
            }
        }

        /// <summary>
        /// Allows the configuraiton of the cache limit, Default is 30
        /// </summary>
        /// <param name="cacheLimit"></param>
        public static void Configure(int cacheLimit)
        {
            // Hard coded limit of 30 
            if (cacheLimit <= 0)
                _cacheLimit = 30;
            else
                _cacheLimit = cacheLimit;
        }

        /// <summary>
        /// Get item will retrive the specific object from the cache with the provided key
        /// A call to get an item, if found, will update the last accessed ticks on the object to keep it fresh
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetItem(string key)
        {
            lock (o)
            {
                var val = (LusidDynamicCacheObject)_cache[key];
                if (val == null)
                    return null;

                val.LastAccessed = DateTime.UtcNow.Ticks;
                return val.Value;
            }
        } 

        /// <summary>
        /// Adding an object to the MemoryCache will first peak at the memory cache in attempt to determin what to cleanup.
        /// Similar to Redis, there is no active cleaning messure and is only provoked on add
        /// 
        /// If found, this method will remove that item and invoke a callback, the client can decide what to do with the callback!
        /// A new item is then cached with the current UTC ticks
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="removalCallback"></param>
        /// <returns></returns>
        public static LusidDynamicCacheObject AddItem(string key, object obj, Action<LusidDynamicCacheObject> removalCallback = null)
        {  
            lock (o)
            {
                PeakAtLast();

                var count = _cache.GetCount();
                if (count > _cacheLimit)
                {
                    var removedItem = _leastAccessedItem;
                    var item = (LusidDynamicCacheObject) _cache[removedItem];
                    _cache.Remove(removedItem);
                    _leastAccessedItem = key;
                    removalCallback?.Invoke(item);
                }

                var it = new LusidDynamicCacheObject() { Key = key, Value = obj, LastAccessed = DateTime.UtcNow.Ticks };

                _cache.Add(new CacheItem(key, new LusidDynamicCacheObject() { Key = key, Value = obj, LastAccessed = DateTime.UtcNow.Ticks }), new CacheItemPolicy()
                {
                    
                });

                return it;
            }
        }

        private static void PeakAtLast()
        {
            var lastDT = DateTime.UtcNow.Ticks;
            foreach (var it in _cache)
            {
                LusidDynamicCacheObject item = (LusidDynamicCacheObject) it.Value;
                if (item.LastAccessed < lastDT)
                {
                    lastDT = item.LastAccessed;
                    _leastAccessedItem = item.Key;
                }
            }
        }
    }
}