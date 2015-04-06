using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;
using SQLServerProvider.DataMappers;

namespace SQLServerProvider
{
    /// <summary>
    ///     Uses Microsoft SQL Server Express database.
    ///     The SQLServerProvider is using the DataMapper design pattern. For more
    ///     information, see Martin Fowler's DataMapper at:
    ///     http://martinfowler.com/eaaCatalog/dataMapper.html and
    ///     http://thierryroussel.free.fr/java/books/martinfowler/www.martinfowler.com/isa/databaseMapper.html
    /// </summary>
    public class SQLServerProvider : PersistenceProvider
    {
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
        // This will hold our domain class names
        private readonly Dictionary<Type, int> _types = new Dictionary<Type, int>();

        public SQLServerProvider()
        {
            SetupTypes();
        }

        private void SetupTypes()
        {
            _types.Add(typeof (Account), TYPE_ACCOUNT);
            _types.Add(typeof (AccountOwner), TYPE_ACCOUNT_OWNER);
            _types.Add(typeof (Address), TYPE_ADDRESS);
            _types.Add(typeof (AddressType), TYPE_ADDRESSTYPE);
            _types.Add(typeof (ApplicationPermission), TYPE_APPLICATION_PERMISSION);
            _types.Add(typeof (ApplicationRole), TYPE_APPLICATION_ROLE);
            _types.Add(typeof (DomainObject), TYPE_DOMAINOBJECT);
            _types.Add(typeof (Person), TYPE_PERSON);
            _types.Add(typeof (User), TYPE_USER);
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
            switch (_types[typeof (T)])
            {
                case TYPE_ACCOUNT:
                    return AccountMapper.Instance.Save(obj as Account);
                case TYPE_ACCOUNT_OWNER:
                    return AccountOwnerMapper.Instance.Save(obj as AccountOwner);
                case TYPE_ADDRESS:
                    return AddressMapper.Instance.Save(obj as Address);
                case TYPE_ADDRESSTYPE:
                    return AddressTypeMapper.Instance.Save(obj as AddressType);
                case TYPE_APPLICATION_PERMISSION:
                    return ApplicationPermissionMapper.Instance.Save(obj as ApplicationPermission);
                case TYPE_APPLICATION_ROLE:
                    return ApplicationRoleMapper.Instance.Save(obj as ApplicationRole);
                case TYPE_DOMAINOBJECT:
                    return DomainObjectMapper.Instance.Save(obj);
                case TYPE_PERSON:
                    return PersonMapper.Instance.Save(obj as Person);
                case TYPE_USER:
                    return UserMapper.Instance.Save(obj as User);
            }
            return false;
        }

        public override bool DeleteById<T>(string id)
        {
            switch (_types[typeof (T)])
            {
                case TYPE_ACCOUNT:
                    return AccountMapper.Instance.Delete(GetObjectById<T>(id) as Account);
                case TYPE_ACCOUNT_OWNER:
                    return AccountOwnerMapper.Instance.Delete(GetObjectById<T>(id) as AccountOwner);
                case TYPE_ADDRESS:
                    return AddressMapper.Instance.Delete(GetObjectById<T>(id) as Address);
                case TYPE_ADDRESSTYPE:
                    return AddressTypeMapper.Instance.Delete(GetObjectById<T>(id) as AddressType);
                case TYPE_APPLICATION_PERMISSION:
                    return ApplicationPermissionMapper.Instance.Delete(GetObjectById<T>(id) as ApplicationPermission);
                case TYPE_APPLICATION_ROLE:
                    return ApplicationRoleMapper.Instance.Delete(GetObjectById<T>(id) as ApplicationRole);
                case TYPE_DOMAINOBJECT:
                    return DomainObjectMapper.Instance.Delete(GetObjectById<T>(id));
                case TYPE_PERSON:
                    return PersonMapper.Instance.Delete(GetObjectById<T>(id) as Person);
                case TYPE_USER:
                    return UserMapper.Instance.Delete(GetObjectById<T>(id) as User);
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

            switch (_types[typeof (T)])
            {
                case TYPE_ACCOUNT:
                    obj = AccountMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_ACCOUNT_OWNER:
                    obj = AccountOwnerMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_ADDRESS:
                    obj = AddressMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_ADDRESSTYPE:
                    obj = AddressTypeMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_APPLICATION_PERMISSION:
                    obj = ApplicationPermissionMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_APPLICATION_ROLE:
                    obj = ApplicationRoleMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_DOMAINOBJECT:
                    obj = DomainObjectMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_PERSON:
                    obj = PersonMapper.Instance.GetByID(id) as T;
                    break;
                case TYPE_USER:
                    obj = UserMapper.Instance.GetByID(id) as T;
                    break;
            }

            IdentityMap.Update(obj);
            return obj;
        }

        public override List<T> GetObjects<T>()
        {
            var list = new List<T>();

            switch (_types[typeof (T)])
            {
                case TYPE_ACCOUNT:
                    list = AccountMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_ACCOUNT_OWNER:
                    list = AccountOwnerMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_ADDRESS:
                    list = AddressMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_ADDRESSTYPE:
                    list = AddressTypeMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_APPLICATION_PERMISSION:
                    list = ApplicationPermissionMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_APPLICATION_ROLE:
                    list = ApplicationRoleMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_DOMAINOBJECT:
                    list = DomainObjectMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_PERSON:
                    list = PersonMapper.Instance.GetAll() as List<T>;
                    break;
                case TYPE_USER:
                    list = UserMapper.Instance.GetAll() as List<T>;
                    break;
            }

            foreach (var obj in list)
            {
                IdentityMap.Update(obj);
            }

            return list;
        }

        public override bool SaveObjects<T>(List<T> list)
        {
            switch (_types[typeof (T)])
            {
                case TYPE_ACCOUNT:
                    return AccountMapper.Instance.SaveAll(list as List<Account>);
                case TYPE_ACCOUNT_OWNER:
                    return AccountOwnerMapper.Instance.SaveAll(list as List<AccountOwner>);
                case TYPE_ADDRESS:
                    return AddressMapper.Instance.SaveAll(list as List<Address>);
                case TYPE_ADDRESSTYPE:
                    return AddressTypeMapper.Instance.SaveAll(list as List<AddressType>);
                case TYPE_APPLICATION_PERMISSION:
                    return ApplicationPermissionMapper.Instance.SaveAll(list as List<ApplicationPermission>);
                case TYPE_APPLICATION_ROLE:
                    return ApplicationRoleMapper.Instance.SaveAll(list as List<ApplicationRole>);
                case TYPE_DOMAINOBJECT:
                    return DomainObjectMapper.Instance.SaveAll(list as List<DomainObject>);
                case TYPE_PERSON:
                    return PersonMapper.Instance.SaveAll(list as List<Person>);
                case TYPE_USER:
                    return UserMapper.Instance.SaveAll(list as List<User>);
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
            IdentityMap.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("DELETE FROM AccountOwners;\n\n");
            sb.Append("DELETE FROM Accounts;\n\n");
            sb.Append("DELETE FROM Addresses;\n\n");
            sb.Append("DELETE FROM AddressTypes;\n\n");
            sb.Append("DELETE FROM ApplicationPermissions;\n\n");
            sb.Append("DELETE FROM ApplicationPermissionsApplicationRoles;\n\n");
            sb.Append("DELETE FROM ApplicationRoles;\n\n");
            sb.Append("DELETE FROM DomainObjects;\n\n");
            sb.Append("DELETE FROM Person;\n\n");
            sb.Append("DELETE FROM Users;\n\n");
            sb.Append("DELETE FROM UsersApplicationRoles;\n\n");

            SqlConnection connection = new SqlConnection(AbstractMapper<DomainObject>.SQL_CONNECTION_STRING);
            SqlCommand cmd = new SqlCommand(sb.ToString(), connection);
            SqlTransaction trans = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                trans = connection.BeginTransaction();
                cmd.Transaction = trans;
                int rows = cmd.ExecuteNonQuery();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
            }
            finally
            {
                connection.Close();
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