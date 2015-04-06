using System.Collections.Generic;
using DomainModel;

namespace PersistenceService
{
    /// <summary>
    ///     IdentityMap is used to prevent duplicate loading of objects from
    ///     persistence stores. It is also used to prevent recursive loading of objects that
    ///     reference each other. In addition, it is also used as a cache to save expensive
    ///     trips to databases when trying load objects. Some persistence stores like db4o may
    ///     already implement an object cache and these providers may not need an
    ///     Identity Map. For more information see Martin Fowler's Identity Map pattern:
    ///     http://thierryroussel.free.fr/java/books/martinfowler/www.martinfowler.com/isa/identityMap.html
    ///     ****************************************************************************
    ///     IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT!
    ///     ****************************************************************************
    ///     This Identity Map uses a local dictionary but in a production environment
    ///     such as a large cluster on Amazon's EC2 nodes, for example, this Identity
    ///     Map should be a distributed memory cache system such as memcached or Redis
    ///     because each node needs access to the same memory pool (the identity map).
    ///     If you use the local dictionary in a cluster, you will hand out stale
    ///     versions of your objects that are most likely different between the
    ///     differenet nodes the more time passes. So, use a distributed memory cache
    ///     like Redis or Memcached instead.
    ///     ****************************************************************************
    ///     IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT! IMPORTANT!
    ///     ****************************************************************************
    /// </summary>
    public static class IdentityMap
    {
        private static readonly Dictionary<string, DomainObject> _identityMap = new Dictionary<string, DomainObject>();

        public static DomainObject Get(string id)
        {
            return _identityMap[id];
        }

        public static DomainObject Get(DomainObject obj)
        {
            string id = CreateKey<DomainObject>(obj);
            return _identityMap[id];
        }

        public static void Remove(DomainObject obj)
        {
            string id = CreateKey<DomainObject>(obj);
            _identityMap.Remove(id);
        }

        public static bool ContainsId(string id)
        {
            return _identityMap.ContainsKey(id);
        }

        public static bool ContainsId(DomainObject obj)
        {
            string id = CreateKey<DomainObject>(obj);
            return _identityMap.ContainsKey(id);
        }

        public static void Update(DomainObject obj)
        {
            if (obj == null) return;
            string id = CreateKey<DomainObject>(obj);
            if (_identityMap.ContainsKey(id))
                _identityMap[id] = obj;
            else
                _identityMap.Add(id, obj);
        }

        /// <summary>
        /// Deletes all objects from the identity map
        /// </summary>
        public static void Clear()
        {
            _identityMap.Clear();
        }

        public static string CreateKey<T>(DomainObject obj = null)
        {
            string key = typeof(T).FullName.Replace(".", "_") + "_" + DomainObject.CURRENT_DOMAIN_VERSION;
            if (obj != null)
                key = key + "_" + obj.Id;
            return key;
        }

        public static string CreateKey<T>(string id)
        {
            string key = typeof(T).FullName.Replace(".", "_") + "_" + DomainObject.CURRENT_DOMAIN_VERSION;
            key = key + "_" + id;
            return key;
        }

        public static Dictionary<string, DomainObject>.ValueCollection Values()
        {
            return _identityMap.Values;
        }
    }
}