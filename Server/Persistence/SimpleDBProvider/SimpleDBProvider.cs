using System;
using System.Collections.Generic;
using System.Linq;
using DomainModel;
using PersistenceService;
using SimpleDBProvider.DataMappers;

namespace SimpleDBProvider
{
    /// <summary>
    /// Uses Amazon's NoSQL database SimpleDB: http://aws.amazon.com/simpledb
    /// You will need to signup with Amazon AWS in order to test the SimpleDB.
    /// Once you have signed up, paste your accesskey and seceretkey below.
    /// 
    /// You can use the SdbNavigator tool to manage your domains and data
    /// from within the Google Chrome browser. It's very easy to use and allows
    /// you to look inside your database, query, etc.
    /// https://github.com/Reggino/SdbNavigator
    /// 
    /// The SimpleDBprovider is using the DataMapper design pattern. For more
    /// information, see Martin Fowler's DataMapper at:
    /// http://martinfowler.com/eaaCatalog/dataMapper.html and
    /// http://thierryroussel.free.fr/java/books/martinfowler/www.martinfowler.com/isa/databaseMapper.html
    /// 
    /// </summary>
    public class SimpleDBProvider : PersistenceProvider
    {
        private const string ACCESS_KEY = "";
        private const string SECRET_KEY = "";
        private const string DOMAIN_PREFIX = "SimpleDBSample_";
        protected SimpleDBHelper _sdb;

        // This will hold our domain class names
        private Dictionary<Type, int> _types = new Dictionary<Type, int>();

        // Each domain class has its own constant. You will need to add
        // additional constants for each new domain class you want to persist.
        private const int TYPE_ACCOUNT = 100;
        private const int TYPE_ACCOUNT_OWNER = 110;
        private const int TYPE_ADDRESS = 120;
        private const int TYPE_ADDRESSTYPE = 130;
        private const int TYPE_APPLICATION_PERMISSION = 140;
        private const int TYPE_APPLICATION_ROLE = 150;
        private const int TYPE_DOMAINOBJECT = 160;
        private const int TYPE_PERSON = 170;
        private const int TYPE_USER = 180;

        public SimpleDBProvider()
        {
            SetupTypes();
            _sdb = new SimpleDBHelper(ACCESS_KEY, SECRET_KEY, DOMAIN_PREFIX);
            VerifyDomainsInSimpleDB();
        }

        /// <summary>
        /// Verifies if the domains exist in the SimpleDB and
        /// if not, it will create them.
        /// </summary>
        private void VerifyDomainsInSimpleDB()
        {
            var domains = _sdb.GetListOfDomains();
            foreach (var type in _types)
            {
                var domain = type.Key.FullName.Replace(".", "_");
                if (!domains.Contains(domain))
                    _sdb.CreateDomain(domain);    
            }
        }

        private void SetupTypes()
        {
            _types.Add(typeof(Account), TYPE_ACCOUNT);
            _types.Add(typeof(AccountOwner), TYPE_ACCOUNT_OWNER);
            _types.Add(typeof(Address), TYPE_ADDRESS);
            _types.Add(typeof(AddressType), TYPE_ADDRESSTYPE);
            _types.Add(typeof(ApplicationPermission), TYPE_APPLICATION_PERMISSION);
            _types.Add(typeof(ApplicationRole), TYPE_APPLICATION_ROLE);
            _types.Add(typeof(DomainObject), TYPE_DOMAINOBJECT);
            _types.Add(typeof(Person), TYPE_PERSON);
            _types.Add(typeof(User), TYPE_USER);
        }

        public override bool Connect(object obj = null)
        {
            return true;
        }

        public override bool Disconnect()
        {
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
                case TYPE_ACCOUNT: return AccountMapper.Instance.Save(obj as Account);
                case TYPE_ACCOUNT_OWNER: return AccountOwnerMapper.Instance.Save(obj as AccountOwner);
                case TYPE_ADDRESS: return AddressMapper.Instance.Save(obj as Address);
                case TYPE_ADDRESSTYPE: return AddressTypeMapper.Instance.Save(obj as AddressType);
                case TYPE_APPLICATION_PERMISSION: return ApplicationPermissionMapper.Instance.Save(obj as ApplicationPermission);
                case TYPE_APPLICATION_ROLE: return ApplicationRoleMapper.Instance.Save(obj as ApplicationRole);
                case TYPE_DOMAINOBJECT: return DomainObjectMapper.Instance.Save(obj);
                case TYPE_PERSON: return PersonMapper.Instance.Save(obj as Person);
                case TYPE_USER: return UserMapper.Instance.Save(obj as User);
            }
            return false;
        }

        public override bool DeleteById<T>(string id)
        {
            switch (_types[typeof(T)])
            {
                case TYPE_ACCOUNT: return AccountMapper.Instance.Delete(GetObjectById<T>(id) as Account); 
                case TYPE_ACCOUNT_OWNER: return AccountOwnerMapper.Instance.Delete(GetObjectById<T>(id) as AccountOwner); 
                case TYPE_ADDRESS: return AddressMapper.Instance.Delete(GetObjectById<T>(id) as Address); 
                case TYPE_ADDRESSTYPE: return AddressTypeMapper.Instance.Delete(GetObjectById<T>(id) as AddressType);
                case TYPE_APPLICATION_PERMISSION: return ApplicationPermissionMapper.Instance.Delete(GetObjectById<T>(id) as ApplicationPermission);
                case TYPE_APPLICATION_ROLE: return ApplicationRoleMapper.Instance.Delete(GetObjectById<T>(id) as ApplicationRole);
                case TYPE_DOMAINOBJECT: return DomainObjectMapper.Instance.Delete(GetObjectById<T>(id));
                case TYPE_PERSON: return PersonMapper.Instance.Delete(GetObjectById<T>(id) as Person);
                case TYPE_USER: return UserMapper.Instance.Delete(GetObjectById<T>(id) as User); 
            }
            return false;
        }

        public override T GetObjectById<T>(string id, bool loadReferencedObjects = true)
        {
            if (string.IsNullOrEmpty(id)) return null;

            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(IdentityMap.CreateKey<T>(id)))
                return IdentityMap.Get(IdentityMap.CreateKey<T>(id)) as T;

            T obj = null;

            switch (_types[typeof(T)])
            {
                case TYPE_ACCOUNT: obj = AccountMapper.Instance.GetByID<Account>(id) as T; break;
                case TYPE_ACCOUNT_OWNER: obj = AccountOwnerMapper.Instance.GetByID<AccountOwner>(id) as T; break;
                case TYPE_ADDRESS: obj = AddressMapper.Instance.GetByID<Address>(id) as T; break;
                case TYPE_ADDRESSTYPE: obj = AddressTypeMapper.Instance.GetByID<AddressType>(id) as T; break;
                case TYPE_APPLICATION_PERMISSION: obj = ApplicationPermissionMapper.Instance.GetByID<ApplicationPermission>(id) as T; break;
                case TYPE_APPLICATION_ROLE: obj = ApplicationRoleMapper.Instance.GetByID<ApplicationRole>(id) as T; break;
                case TYPE_DOMAINOBJECT: obj = DomainObjectMapper.Instance.GetByID<DomainObject>(id) as T; break;
                case TYPE_PERSON: obj = PersonMapper.Instance.GetByID<Person>(id) as T; break;
                case TYPE_USER: obj = UserMapper.Instance.GetByID<User>(id) as T; break;
            }

            IdentityMap.Update(obj);
            return obj;
        }

        public override List<T> GetObjects<T>()
        {
            List<T> list = new List<T>();

            switch (_types[typeof(T)])
            {
                case TYPE_ACCOUNT: list = AccountMapper.Instance.GetAll() as List<T>; break;
                case TYPE_ACCOUNT_OWNER: list = AccountOwnerMapper.Instance.GetAll() as List<T>; break;
                case TYPE_ADDRESS: list = AddressMapper.Instance.GetAll() as List<T>; break;
                case TYPE_ADDRESSTYPE: list = AddressTypeMapper.Instance.GetAll() as List<T>; break;
                case TYPE_APPLICATION_PERMISSION: list = ApplicationPermissionMapper.Instance.GetAll() as List<T>; break;
                case TYPE_APPLICATION_ROLE: list = ApplicationRoleMapper.Instance.GetAll() as List<T>; break;
                case TYPE_DOMAINOBJECT: list = DomainObjectMapper.Instance.GetAll() as List<T>; break;
                case TYPE_PERSON: list = PersonMapper.Instance.GetAll() as List<T>; break;
                case TYPE_USER: list = UserMapper.Instance.GetAll() as List<T>; break;
            }

            foreach (var obj in list)
            {
                IdentityMap.Update(obj);
            }

            return list;
        }

        public override bool SaveObjects<T>(List<T> list)
        {
            switch (_types[typeof(T)])
            {
                case TYPE_ACCOUNT: return AccountMapper.Instance.SaveAll(list as List<Account>);
                case TYPE_ACCOUNT_OWNER: return AccountOwnerMapper.Instance.SaveAll(list as List<AccountOwner>);
                case TYPE_ADDRESS: return AddressMapper.Instance.SaveAll(list as List<Address>);
                case TYPE_ADDRESSTYPE: return AddressTypeMapper.Instance.SaveAll(list as List<AddressType>);
                case TYPE_APPLICATION_PERMISSION: return ApplicationPermissionMapper.Instance.SaveAll(list as List<ApplicationPermission>);
                case TYPE_APPLICATION_ROLE: return ApplicationRoleMapper.Instance.SaveAll(list as List<ApplicationRole>);
                case TYPE_DOMAINOBJECT: return DomainObjectMapper.Instance.SaveAll(list as List<DomainObject>);
                case TYPE_PERSON: return PersonMapper.Instance.SaveAll(list as List<Person>);
                case TYPE_USER: return UserMapper.Instance.SaveAll(list as List<User>);
            }
            return false;
        }

        public override T GetObjectByName<T>(string name)
        {
            return DomainObjectMapper.Instance.GetByName(name) as T;
        }

        public override Person GetPersonByEmail(string email)
        {
            return PersonMapper.Instance.GetPersonByEmail(email);
        }

        public override void DeleteAllObjects()
        {
            try
            {
                // It's much easier to delete the domains that going through each
                // attribute and items in SimpleDB. So, after the domains are 
                // deleted, we simply create the domains again.
                _sdb.DeleteDomain(typeof(Account).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(AccountOwner).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(Address).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(AddressType).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(ApplicationPermission).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(ApplicationRole).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(DomainObject).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(Person).FullName.Replace(".", "_"));
                _sdb.DeleteDomain(typeof(User).FullName.Replace(".", "_"));

                // Create the domains again
                VerifyDomainsInSimpleDB();
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
        }

        public override Account GetAccountByAccountNumber(string accountNumber)
        {
            return AccountMapper.Instance.GetAccountByAccountNumber(accountNumber);
        }

        public override Account GetAccountByAccountOwnerEmail(string email)
        {
            return AccountMapper.Instance.GetAccountByAccountOwnerEmail(email);
        }

        public override User GetUserByEmail(string email)
        {
            return UserMapper.Instance.GetUserByEmail(email);
        }
    }
}