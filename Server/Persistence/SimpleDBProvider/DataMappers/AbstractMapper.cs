using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public abstract class AbstractMapper<T> where T : DomainObject, new()
    {
        private const string ACCESS_KEY = "";
        private const string SECRET_KEY = "";
        protected const string DOMAIN_PREFIX = "SimpleDBSample_"; // Your application or system name
        protected string _domain;
        protected string _fullDomain;
        protected SimpleDBHelper _sdb;
        public string FindStatement { get; set; }
        protected abstract T LoadRow(Item item);
        public abstract bool Save(T obj);
        public abstract bool Delete(T obj);

        public AbstractMapper()
        {
            _domain = typeof(T).FullName.Replace(".", "_");
            _fullDomain = _domain + "_" + DomainObject.CURRENT_DOMAIN_VERSION + "_";
            _sdb = new SimpleDBHelper(ACCESS_KEY, SECRET_KEY, DOMAIN_PREFIX);
        }

        public virtual T GetByID<U>(string id) where U : DomainObject
        {
            string newId = _fullDomain + id.Substring(id.LastIndexOf("_") + 1);
            return AbstractFind("select * from " + DOMAIN_PREFIX + _domain + " WHERE itemName() = '" + newId + "'");
        }

        protected T AbstractFind(string findStatement)
        {
            T result = null;
            try
            {
                FindStatement = findStatement;
                if (string.IsNullOrEmpty(FindStatement))
                    throw new Exception("Find Statement is null");

                SelectRequest selectRequestAction = new SelectRequest(findStatement);
                SelectResponse selectResponse = _sdb.Client.Select(selectRequestAction);
                if (selectResponse.Items.Count>0)
                {
                    result = Load(selectResponse.Items);
                }
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
                return result;
            }
            return result;
        }

        private T DoLoad(List<Item> list)
        {
            T obj = null;
            try
            {
                foreach (Item item in list)
                {
                    obj = LoadRow(item);
                    break;
                }
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
                return obj;
            }
            return obj;
        }

        protected T Load(List<Item> list)
        {
            T result = null;
            try
            {
                result = DoLoad(list);
            }
            catch (Exception ex)
                {
                // Log (ex.StackTrace);
                return result;
            }
            return result;
        }

        public virtual bool SaveAll(List<T> objs)
        {
            try
            {
                foreach (var p in objs)
                {
                    if (!Save(p))
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                // log (ex.StackTrace);
                return false;
            }
        }

        public virtual bool DeleteAll(List<T> objs)
        {
            foreach (var obj in objs)
            {
                Delete(obj);
            }
            return true;
        }

        public virtual T GetPropertiesFromParentObject<U>(U parentObject)
        {
            var result = new T();
            var dtoProperties = parentObject.GetType().GetProperties();
            var boProperties = typeof (T).GetProperties();
            foreach (var dtoProperty in dtoProperties)
            {
                var value = dtoProperty.GetValue(parentObject, null);
                var boProperty = boProperties.First(p => p.Name == dtoProperty.Name);
                if (value != null)
                    boProperty.SetValue(result, value, null);
            }
            return result;
        }

        public virtual List<T> GetAll()
        {
            List<T> list = new List<T>();

            SelectRequest selectRequestAction = new SelectRequest("select * from " + DOMAIN_PREFIX + _domain);
            SelectResponse selectResponse = _sdb.Client.Select(selectRequestAction);
            if (selectResponse.Items.Count > 0)
            {
                SelectResult selectResult = selectResponse.SelectResult;
                foreach (Item item in selectResult.Items)
                {
                    list.Add(LoadRow(item));
                }
            }

            return list;
        }
    }
}