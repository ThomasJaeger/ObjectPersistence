using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using DomainModel;

namespace PersistenceService
{
    public abstract class PersistenceProvider : ProviderBase
    {
        public abstract bool Connect(object obj = null);
        public abstract bool Disconnect();
        public abstract bool Save<T>(T obj) where T : DomainObject, new();
        public abstract bool DeleteById<T>(string id) where T : DomainObject, new();
        public abstract T GetObjectById<T>(string id, bool loadReferencedObjects = true) where T : DomainObject, new();
        public abstract List<T> GetObjects<T>() where T : DomainObject, new();
        public abstract bool SaveObjects<T>(List<T> list) where T : DomainObject, new();
        public abstract T GetObjectByName<T>(string name) where T : DomainObject, new();
        public abstract Person GetPersonByEmail(string email);
        public abstract void DeleteAllObjects();
        public abstract Account GetAccountByAccountNumber(string accountNumber);
        public abstract Account GetAccountByAccountOwnerEmail(string email);
        public abstract User GetUserByEmail(string email);

        /// <summary>
        /// We need to create our initial seed data of the system
        /// If this is the very first node in a cloud-cluster, this 
        /// should be the only node running to cold-start the cloud system.
        /// Once this process is complete, the rest of the nodes can be started.
        /// </summary>
        public void CreateSeedData()
        {
            CreateTypes(Persistence.Instance.Provider.GetObjects<AddressType>(), AddressType.GetAll<AddressType>());
            // Add additional types here
            // ...
        }

        /// <summary>
        /// Verifies if the type exists and if it deos not, saves the type
        /// to the persistence store only once. This also works when new types
        /// will be added in a future maintenance release of the system.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="all"></param>
        protected void CreateTypes<T>(List<T> list, IEnumerable<T> all) where T : Enumeration, new()
        {
            foreach (var userType in all)
            {
                // We can't use ID's here because the Enumeration ID's are created as new
                // ID's when we start the system (DomainObject.ID). So, we want something
                // that is business unique such as the DisplayName that will stay the same.
                if (list.FirstOrDefault(l => l.DisplayName == userType.DisplayName) == null)
                    Save(userType);
            }
        }
    }
}
