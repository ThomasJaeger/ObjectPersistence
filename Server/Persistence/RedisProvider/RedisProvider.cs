using System;
using System.Collections.Generic;
using System.Linq;
using DomainModel;
using Newtonsoft.Json;
using PersistenceService;
using StackExchange.Redis;

namespace RedisProvider
{
    /// <summary>
    /// Uses Redislabs' Redis NoSQL database service: http://www.redislabs.com
    /// You will need to signup with Redislabs.com. You can signup for a FREE
    /// 25 MB database. Once you have signed up, create your free database
    /// and paste your endpoint below. Make sure you set the correct port #.
    /// </summary>
    public class RedisProvider : PersistenceProvider
    {
        // Set this with the correct port number you will get from redislabs.com
        // For example, you endpoint might look like this:
        // pub-redis-16627.us-east-1-2.4.ec2.garantiadata.com:16627
        // The port # in this case would be: 16627
        private const int REDIS_PORT = 17044;    

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
        private const int TYPE_ACCOUNT = 100;
        private const int TYPE_ACCOUNT_OWNER = 110;
        private const int TYPE_ADDRESS = 120;
        private const int TYPE_ADDRESSTYPE = 130;
        private const int TYPE_DOMAINOBJECT = 140;
        private const int TYPE_PERSON = 150;
        private const int TYPE_USER = 160;

        public RedisProvider()
        {
            SetupTypes();
        }

        private void SetupTypes()
        {
            _types.Add(typeof(Account), TYPE_ACCOUNT);
            _types.Add(typeof(AccountOwner), TYPE_ACCOUNT_OWNER);
            _types.Add(typeof(Address), TYPE_ADDRESS);
            _types.Add(typeof(AddressType), TYPE_ADDRESSTYPE);
            _types.Add(typeof(DomainObject), TYPE_DOMAINOBJECT);
            _types.Add(typeof(Person), TYPE_PERSON);
            _types.Add(typeof(User), TYPE_USER);
        }

        public override bool Connect(object obj = null)
        {
            if (_db == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    //ReferenceLoopHandling = ReferenceLoopHandling.Ignore,    // will not serialize an object if it is a child object of itself
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,  // is useful if objects are nested but not indefinitely
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects, // serialize an object that is nested indefinitely
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
                case TYPE_ACCOUNT: return SaveAccount(obj);
                case TYPE_ACCOUNT_OWNER: return SaveAccountOwner(obj);
                case TYPE_ADDRESS: return SaveAddress(obj);
                case TYPE_ADDRESSTYPE: return SaveAddressType(obj);
                case TYPE_DOMAINOBJECT: return SaveDomainObject(obj);
                case TYPE_PERSON: return SavePerson(obj);
                case TYPE_USER: return SaveUser(obj);
            }
            return false;
        }

        /// <summary>
        /// Save AccountOwner object. AccountOwner has an Account object.
        /// This account object will only be saved if saveAccount is true (default).
        /// If SaveAccountOwner is called from the SaveAccount method, then
        /// saveAccount is false since it is already saved by the SaveAccount method.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveAccount">Check if we need to save cyclic referenced Account object</param>
        /// <returns></returns>
        private bool SaveAccountOwner(DomainObject obj, bool saveAccount = true)
        {
            if (obj == null) return true;
            AccountOwner accountOwner = (AccountOwner)obj;
            string json = JsonConvert.SerializeObject(accountOwner);
            string listKey = IdentityMap.CreateKey<AccountOwner>();
            if (saveAccount) 
                SaveAccount(accountOwner.Account);
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, accountOwner.Id, json);
        }

        private bool SaveAccount(DomainObject obj)
        {
            if (obj == null) return true;
            Account account = (Account)obj;
            string json = JsonConvert.SerializeObject(account);
            string listKey = IdentityMap.CreateKey<Account>();
            SaveAccountOwner(account.AccountOwner, false);
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, account.Id, json);
        }

        private bool SaveUser(DomainObject obj)
        {
            if (obj == null) return true;
            User user = (User)obj;
            string json = JsonConvert.SerializeObject(user);
            string listKey = IdentityMap.CreateKey<User>();
            //SaveApplicationRoles(user.ApplicationRoles);
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, user.Id, json);
        }

        private bool SaveAddress(DomainObject obj)
        {
            if (obj == null) return true;
            Address address = (Address)obj;
            string json = JsonConvert.SerializeObject(address);
            string listKey = IdentityMap.CreateKey<Address>();
            SaveAddressType(address.AddressType);
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, address.Id, json);
        }

        private bool SaveAddressType(DomainObject obj)
        {
            if (obj == null) return true;
            AddressType addressType = (AddressType)obj;
            string json = JsonConvert.SerializeObject(addressType);
            string listKey = IdentityMap.CreateKey<AddressType>();
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, addressType.Id, json);
        }

        private bool SavePerson(DomainObject obj)
        {
            if (obj == null) return true;
            Person person = (Person)obj;
            string json = JsonConvert.SerializeObject(person);
            string listKey = IdentityMap.CreateKey<Person>();
            SaveAddress(person.HomeAddress);
            SaveAddress(person.WorkAddress);
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, person.Id, json);
        }

        private bool SaveDomainObject(DomainObject obj)
        {
            if (obj == null) return true;
            string json = JsonConvert.SerializeObject(obj);
            string listKey = IdentityMap.CreateKey<DomainObject>();
            IdentityMap.Remove(obj);
            return _db.HashSet(listKey, obj.Id, json);
        }

        public override bool DeleteById<T>(string id)
        {
            string listKey = IdentityMap.CreateKey<T>();
            T obj = GetObjectById<T>(id);
            IdentityMap.Remove(obj);
            return _db.HashDelete(listKey, id);
        }

        public override T GetObjectById<T>(string id, bool loadReferencedObjects = true)
        {
            if (string.IsNullOrEmpty(id)) return null;
            
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(IdentityMap.CreateKey<T>(id)))
                return IdentityMap.Get(IdentityMap.CreateKey<T>(id)) as T;

            string listKey = IdentityMap.CreateKey<T>();
            string json = _db.HashGet(listKey, id);
            if (string.IsNullOrEmpty(json)) return null;
            T obj = JsonConvert.DeserializeObject<T>(json);

            // If we want to just populate the properties of the domain object
            // and not the referenced complex objets, set loadReferencedObjects = false
            // This is good for loading the referenced objects seperately 
            // (see LoadReferencedObjects)
            if (loadReferencedObjects)
                obj = (T)LoadReferencedObjects(obj);

            IdentityMap.Update(obj);

            return obj;
        }

        public override List<T> GetObjects<T>()
        {
            List<T> list = new List<T>();
            string listKey = IdentityMap.CreateKey<T>();
            HashEntry[] values = _db.HashGetAll(listKey);
            T obj = null;
            foreach (var hashEntry in values)
            {
                obj = JsonConvert.DeserializeObject<T>(hashEntry.Value);
                IdentityMap.Update(obj);
                list.Add(obj);
            }
            return list;
        }

        public override bool SaveObjects<T>(List<T> list)
        {
            string listKey = IdentityMap.CreateKey<T>();
            _db.KeyDelete(listKey);
            HashEntry[] items = new HashEntry[list.Count];
            for (int i = 0; i < list.Count - 1; i++)
            {
                IdentityMap.Remove(list[i]);
                string json = JsonConvert.SerializeObject(list[i]);
                items[i] = new HashEntry(list[i].Id, json);
            }
            _db.HashSet(listKey, items);
            return true;
        }

        public override void DeleteAllObjects()
        {
            RedisKey[] keys = new RedisKey[_types.Count];
            DomainObject obj = new DomainObject();  // Just need access to the current version

            int i = 0;
            foreach (var type in _types)
            {
                RedisKey key = type.Key + ":" + obj.Version;
                keys[i] = key;
                i++;
            }
            IdentityMap.Clear();
            _db.KeyDelete(keys);
        }

        public override Account GetAccountByAccountNumber(string accountNumber)
        {
            List<Account> list = GetObjects<Account>();
            var queryResult = from Account o in list
                              where
                                  (String.Equals(o.AccountNumber, accountNumber, StringComparison.CurrentCultureIgnoreCase)) &&
                                  o.Active
                              select o;
            Account result = queryResult.FirstOrDefault();
            IdentityMap.Update(result);
            return result;
        }

        public override Account GetAccountByAccountOwnerEmail(string email)
        {
            List<Account> list = GetObjects<Account>();
            var queryResult = from Account o in list
                where
                    o.AccountOwner.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && o.Active
                select o;
            Account result = queryResult.FirstOrDefault();
            IdentityMap.Update(result);
            return result;
        }

        public override User GetUserByEmail(string email)
        {
            List<User> list = GetObjects<User>();
            var queryResult = from User o in list
                              where
                                  o.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && o.Active
                              select o;
            User result = queryResult.FirstOrDefault();
            IdentityMap.Update(result);
            return result;
        }

        public override T GetObjectByName<T>(string name)
        {
            List<T> list = GetObjects<T>();
            var queryResult = from T o in list
                              where o.Name.ToUpper() == name.ToUpper() && o.Active
                              select o;
            T obj = queryResult.FirstOrDefault();
            IdentityMap.Update(obj);
            return obj;
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
            IdentityMap.Update(result);
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
                case TYPE_ACCOUNT: return LoadAccount((Account)obj);
                case TYPE_ACCOUNT_OWNER: return LoadAccountOwner((AccountOwner)obj);
                case TYPE_ADDRESS: return LoadAddress((Address)obj);
                case TYPE_ADDRESSTYPE: return LoadAddressType((AddressType) obj);
                case TYPE_DOMAINOBJECT: break;  // Does not contain any referenced objects
                case TYPE_PERSON: return LoadPerson((Person)obj);
                case TYPE_USER: return LoadUser((User) obj);
            }
            return obj;
        }

        private User LoadUser(User user)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(user))
                return IdentityMap.Get(user) as User;

            // TODO: Handle ApplicationRoles

            return null;
        }

        private AccountOwner LoadAccountOwner(AccountOwner o)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(o))
                return IdentityMap.Get(o) as AccountOwner;

            string listKey = IdentityMap.CreateKey<AccountOwner>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return o;
            o = JsonConvert.DeserializeObject<AccountOwner>(json);
            o.Account = LoadAccount(o.Account);
            IdentityMap.Update(o);
            return o;
        }

        private Account LoadAccount(Account o)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(o))
                return IdentityMap.Get(o) as Account;

            string listKey = IdentityMap.CreateKey<Account>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return o;
            o = JsonConvert.DeserializeObject<Account>(json);
            o.AccountOwner = LoadAccountOwner(o.AccountOwner);
            IdentityMap.Update(o);
            return o;
        }

        private AddressType LoadAddressType(AddressType o)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(o))
                return IdentityMap.Get(o) as AddressType;

            string listKey = IdentityMap.CreateKey<AddressType>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return null;
            AddressType obj = JsonConvert.DeserializeObject<AddressType>(json);
            IdentityMap.Update(obj);
            return obj;
        }

        private Person LoadPerson(Person o)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(o))
                return IdentityMap.Get(o) as Person;

            string listKey = IdentityMap.CreateKey<Person>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return o;
            o = JsonConvert.DeserializeObject<Person>(json);
            o.HomeAddress = LoadAddress(o.HomeAddress);
            o.WorkAddress = LoadAddress(o.WorkAddress);
            IdentityMap.Update(o);
            return o;
        }

        private Address LoadAddress(Address o)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(o))
                return IdentityMap.Get(o) as Address;

            string listKey = IdentityMap.CreateKey<Address>();
            string json = _db.HashGet(listKey, o.Id);
            if (string.IsNullOrEmpty(json)) return o;
            o = JsonConvert.DeserializeObject<Address>(json);
            o.AddressType = LoadAddressType(o.AddressType);
            IdentityMap.Update(o);
            return o;
        }
    }
}
