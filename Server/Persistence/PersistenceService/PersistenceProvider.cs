using System.Collections.Generic;
using System.Configuration.Provider;
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
    }
}
