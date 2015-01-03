using System;
using System.Collections.Generic;
using System.Linq;
using DomainModel;
using Newtonsoft.Json;
using PersistenceService;
using StackExchange.Redis;

namespace RedisProvider
{
    public class RedisProvider : PersistenceProvider
    {
        // Set this with the correct port number you will get from redislabs.com
        // For example, you endpoint might look like this:
        // pub-redis-16627.us-east-1-2.4.ec2.garantiadata.com:16627
        // The port # in this case would be: 16627
        private const int REDIS_PORT = 16627;    

        // This is your redislabs password to your Redis db if you set a password.
        // If you did not set a password, leave this empty.
        private const string REDIS_PASSWORD = "";

        // The endpoint that redislabs.com crated for you. For example, it might
        // look like this:
        // pub-redis-16627.us-east-1-2.4.ec2.garantiadata.com:16627
        // Make sure you remove the port number at the end so that it looks like this:
        // pub-redis-16627.us-east-1-2.4.ec2.garantiadata.com
        private const string REDIS_URL = "";

        private static IDatabase _db;
        private static ConnectionMultiplexer _redis;

        // This will hold our domain class names
        private Dictionary<Type, int> _types = new Dictionary<Type, int>();

        // Each domain class has its own constant. You will need to add
        // additional constants for each new domain class you want to persist.
        private const int TYPE_ADDRESS = 100;
        private const int TYPE_ADDRESSTYPE = 110;
        private const int TYPE_DOMAINOBJECT = 120;
        private const int TYPE_PERSON = 130;

        public RedisProvider()
        {
            SetupTypes();
        }

        private void SetupTypes()
        {
            _types.Add(typeof(Address), TYPE_ADDRESS);
            _types.Add(typeof(AddressType), TYPE_ADDRESSTYPE);
            _types.Add(typeof(DomainObject), TYPE_DOMAINOBJECT);
            _types.Add(typeof(Person), TYPE_PERSON);
        }

        private string CreateKey<T>(T obj = null) where T : DomainObject
        {
            string key = typeof(T) + ":" + DomainObject.Version;
            if (obj != null)
                key = key + ":" + obj.Id;
            return key;
        }

        public override bool Connect(object obj = null)
        {
            if (_db == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All
                };

                var configurationOptions = ConfigurationOptions.Parse(REDIS_URL + ":" + REDIS_PORT);
                configurationOptions.SyncTimeout = 30000;
                configurationOptions.Password = REDIS_PASSWORD;
                _redis = ConnectionMultiplexer.Connect(configurationOptions);
                _db = _redis.GetDatabase();
            }
            return _db != null;
        }

        public override bool Disconnect()
        {
            _db = null;
            return true;
        }

        public override bool Save<T>(T obj)
        {
            // ************************************************************
            // For ultimate performance, switch can be removed and replaced 
            // with explicit Save methods such as SaveDomainObject(obj);
            // ************************************************************
            switch (_types[typeof(T)])
            {
                case TYPE_ADDRESS: return SaveAddress(obj);
                case TYPE_ADDRESSTYPE: return SaveAddressType(obj);
                case TYPE_DOMAINOBJECT: return SaveDomainObject(obj);
                case TYPE_PERSON: return SavePerson(obj);
            }
            return false;
        }

        private bool SaveAddress(DomainObject obj)
        {
            if (obj == null) return true;
            Address address = (Address)obj;
            string json = JsonConvert.SerializeObject(address);
            string listKey = CreateKey<Address>();
            SaveAddressType(address.AddressType);
            return _db.HashSet(listKey, address.Id, json);
        }

        private bool SaveAddressType(DomainObject obj)
        {
            if (obj == null) return true;
            AddressType addressType = (AddressType)obj;
            string json = JsonConvert.SerializeObject(addressType);
            string listKey = CreateKey<AddressType>();
            return _db.HashSet(listKey, addressType.Id, json);
        }

        private bool SavePerson(DomainObject obj)
        {
            if (obj == null) return true;
            Person person = (Person)obj;
            string json = JsonConvert.SerializeObject(person);
            string listKey = CreateKey<Person>();
            SaveAddress(person.HomeAddress);
            SaveAddress(person.WorkAddress);
            return _db.HashSet(listKey, person.Id, json);
        }

        private bool SaveDomainObject(DomainObject obj)
        {
            if (obj == null) return true;
            string json = JsonConvert.SerializeObject(obj);
            string listKey = CreateKey<DomainObject>();
            return _db.HashSet(listKey, obj.Id, json);
        }

        public override bool DeleteById<T>(string id)
        {
            string listKey = CreateKey<T>();
            return _db.HashDelete(listKey, id);
        }

        public override T GetObjectById<T>(string id, bool loadReferencedObjects = true)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string listKey = CreateKey<T>();
            string json = _db.HashGet(listKey, id);
            if (string.IsNullOrEmpty(json)) return null;
            T obj = JsonConvert.DeserializeObject<T>(json);

            // If we want to just populate the properties of the domain object
            // and not the referenced complex objets, set loadReferencedObjects = false
            // This is good for loading the referenced objects seperately 
            // (see LoadReferencedObjects)
            if (loadReferencedObjects)
                obj = (T)LoadReferencedObjects(obj);
            return obj;
        }

        public override List<T> GetObjects<T>()
        {
            List<T> list = new List<T>();
            string listKey = CreateKey<T>();
            HashEntry[] values = _db.HashGetAll(listKey);
            T obj = null;
            foreach (var hashEntry in values)
            {
                obj = JsonConvert.DeserializeObject<T>(hashEntry.Value);
                list.Add(obj);
            }
            return list;
        }

        public override bool SaveObjects<T>(List<T> list)
        {
            string listKey = CreateKey<T>();
            _db.KeyDelete(listKey);
            HashEntry[] items = new HashEntry[list.Count];
            for (int i = 0; i < list.Count - 1; i++)
            {
                string json = JsonConvert.SerializeObject(list[i]);
                items[i] = new HashEntry(list[i].Id, json);
            }
            _db.HashSet(listKey, items);
            return true;
        }

        public override void DeleteAllObjects()
        {
            RedisKey[] keys = new RedisKey[_types.Count];

            int i = 0;
            foreach (var type in _types)
            {
                RedisKey key = type.Key + ":" + DomainObject.Version;
                keys[i] = key;
                i++;
            }

            _db.KeyDelete(keys);
        }

        public override T GetObjectByName<T>(string name)
        {
            List<T> list = GetObjects<T>();
            var queryResult = from T o in list
                              where o.Name.ToUpper() == name.ToUpper() && o.Active
                              select o;
            return queryResult.FirstOrDefault();
        }

        public override Person GetPersonByEmail(string email)
        {
            List<Person> list = GetObjects<Person>();
            var queryResult = from Person o in list
                              where
                                  o.Email.ToUpper() == email.ToUpper() &&
                                  o.Active
                              select o;
            Person result = queryResult.FirstOrDefault();
            return result;
        }

        private DomainObject LoadReferencedObjects(DomainObject obj)
        {
            // *******************************************************
            // Execute specific object loading to guarantee referenced
            // objects are loaded with latest version from persistence
            // *******************************************************
            switch (_types[obj.GetType()])
            {
                case TYPE_ADDRESS: return LoadAddress((Address)obj);
                case TYPE_ADDRESSTYPE: return LoadAddressType((AddressType) obj);
                case TYPE_DOMAINOBJECT: break;
                case TYPE_PERSON: return LoadPerson((Person)obj);
            }
            return obj;
        }

        private AddressType LoadAddressType(AddressType o)
        {
            string listKey = CreateKey<AddressType>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return null;
            AddressType obj = JsonConvert.DeserializeObject<AddressType>(json);
            return obj;
        }

        private Person LoadPerson(Person o)
        {
            string listKey = CreateKey<Person>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return o;
            o = JsonConvert.DeserializeObject<Person>(json);
            o.HomeAddress = LoadAddress(o.HomeAddress);
            o.WorkAddress = LoadAddress(o.WorkAddress);
            return o;
        }

        private Address LoadAddress(Address o)
        {
            string listKey = CreateKey<Address>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return o;
            o = JsonConvert.DeserializeObject<Address>(json);
            o.AddressType = GetObjectById<AddressType>(o.AddressType.Id, false); // Refresh simple properties only
            return o;
        }
    }
}
