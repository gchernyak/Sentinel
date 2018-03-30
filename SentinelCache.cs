using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Sentinel
{
    public static class SentinelCache
    {
        static TimeSpan _initialAddDelay;  // Note expiration events empirically have 20 second resolution
        static TimeSpan _temporaryBlockDuration;
        static TimeSpan _updateInterval;
        static TimeSpan _maximumCacheSurvival;

        static SentinelCache()
        {
            
        }


        private static string SentinelKey(string basedOnDocumentId)
        {
            return string.Format("ApiSentinel:" + basedOnDocumentId);
        }

        public static Delegate ExecutionMethod;

        /// <summary>
        /// Entry point to start sentinel cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="del"></param>
        /// <param name="name"></param>
        /// <param name="initialAddDelay"></param>
        /// <param name="temporaryBlockDuration"></param>
        /// <param name="updateInterval"></param>
        /// <param name="maximumCacheSurvival"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetGenericResults<T>(Func<T> del, string name, TimeSpan initialAddDelay, TimeSpan temporaryBlockDuration, TimeSpan updateInterval,
            TimeSpan maximumCacheSurvival)
        {
            SentinelCache._initialAddDelay = initialAddDelay;
            SentinelCache._temporaryBlockDuration = temporaryBlockDuration;
            SentinelCache._updateInterval = updateInterval;
            SentinelCache._maximumCacheSurvival = maximumCacheSurvival;

            string entry = HttpRuntime.Cache[typeof(T).Name] as string;
            ExecutionMethod = del;
            if (entry != null)
            {
                return del() as IEnumerable<T>;
            }

            RequestInitialAddToCache(name);
            return new List<T>();
        }

        /// <summary>
        /// Add a sentinel object to cache with a short expiration time; when it expires, we'll do the add to cache. 
        /// This gets us asynchronous behavior and locking, as the Add will fail if a sentinel is already there.
        /// </summary>
        /// <param name="name"></param>
        private static void RequestInitialAddToCache(string name)
        {
            HttpRuntime.Cache.Add(SentinelKey(name), name,
                null, DateTime.Now.Add(_initialAddDelay), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, OnSentinelRemoved);
        }
        /// <summary>
        /// Removing sentinel and adding a new one after the "value" cache has been updated.
        /// </summary>
        /// <param name="sentinelKey"></param>
        /// <param name="sentinelValue"></param>
        /// <param name="reasonRemoved"></param>
        private static void OnSentinelRemoved(string sentinelKey, Object sentinelValue, CacheItemRemovedReason reasonRemoved)
        {
            if ((reasonRemoved == CacheItemRemovedReason.Expired) && (sentinelValue is string))
            {
                string basedOnObjectName = (string)sentinelValue;

                TemporarilyBlockRequests(basedOnObjectName);
                UpdateCache(basedOnObjectName);
                RequestLaterUpdateToCache(basedOnObjectName);
            }
        }
        /// <summary>
        /// Blocks request temporarily
        /// </summary>
        /// <param name="name"></param>
        private static void TemporarilyBlockRequests(string name)
        {
            HttpRuntime.Cache.Insert(SentinelKey(name), name,
                null, DateTime.Now.Add(_temporaryBlockDuration), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, OnSentinelRemoved);
        }
        /// <summary>
        /// Updates the "value" cache
        /// </summary>
        /// <param name="name"></param>
        private static void UpdateCache(string name)
        {
            try
            {
                var result = ExecutionMethod.DynamicInvoke();
                HttpRuntime.Cache.Insert(name, result,
                    null, DateTime.Now.Add(_maximumCacheSurvival), Cache.NoSlidingExpiration);
            }
            catch (Exception e)
            {
                if ((HttpRuntime.Cache[name] as string) == null)
                    HttpRuntime.Cache.Insert(name, "Error",
                        null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);
            }

        }

        private static void RequestLaterUpdateToCache(string basedOnDocumentId)
        {
            HttpRuntime.Cache.Insert(SentinelKey(basedOnDocumentId), basedOnDocumentId,
                null, DateTime.Now.Add(_updateInterval), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.AboveNormal, OnSentinelRemoved);
        }
    }
}