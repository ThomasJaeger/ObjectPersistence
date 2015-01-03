using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using DomainModel;
using PersistenceService;

namespace db4oProvider
{
    public class db4oProvider : PersistenceProvider
    {
        private IObjectContainer _db;
        private string _fileName = "db.db";

        public IObjectContainer ObjectContainer
        {
            get
            {
                if ((_db == null) || (_db.Ext().IsClosed()))
                {
                    Connect();
                    if (_db == null)
                    {
                        throw new Exception("Can not connect to server.");
                    }
                }
                return _db;
            }
        }

        public override bool Connect(object obj = null)
        {
            var restoredContainer = (IObjectContainer)obj;
            return ConnectAsClientServer(restoredContainer);
        }

        public override bool Disconnect()
        {
            return Close();
        }

        private bool Close()
        {
            bool result = true;

            try
            {
                if (_db != null)
                {
                    _db.Close();
                    _db = null;
                }
            }
            catch
            {
                result = false;
                //throw new Exception(ex.Message);
            }

            return result;
        }

        private bool ConnectAsClientServer(IObjectContainer restoredContainer = null)
        {
            bool _Result = true;

            if (restoredContainer != null)
            {
                _db = restoredContainer;
                return true;
            }

            try
            {
                Db4oFactory.Configure().AllowVersionUpdates(true);
                Db4oFactory.Configure().ActivationDepth(7);
                Db4oFactory.Configure().UpdateDepth(7);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                _db = Db4oFactory.OpenFile(path + _fileName);
            }
            catch (SocketException)
            {
                _Result = false;
            }
            catch (Exception)
            {
                _Result = false;
            }
            return _Result;
        }

        public override bool Save<T>(T obj)
        {
            if (obj == null) return false;
            if (string.IsNullOrEmpty(obj.Id))
                throw new Exception("Id is null or empty for object " + obj);
            bool result = false;
            try
            {
                ObjectContainer.Store(obj);
                Commit();
                result = true;
            }
            catch (Exception)
            {
            }
            return result;
        }

        private bool Commit()
        {
            bool result = false;
            try
            {
                ObjectContainer.Commit();
                result = true;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public override bool DeleteById<T>(string id)
        {
            return ObjectFacade<DomainObject>.DeleteByID(id);
        }

        public override T GetObjectById<T>(string id, bool loadReferencedObjects = true)
        {
            // loadReferencedObjects has no impact since db4o handles this for us
            return ObjectFacade<T>.GetByID(id);
        }

        public override List<T> GetObjects<T>()
        {
            return ObjectFacade<T>.GetAllObjects();
        }

        public override bool SaveObjects<T>(List<T> list)
        {
            foreach (T obj in list)
            {
                ObjectContainer.Store(obj);
            }
            Commit();
            return true;
        }

        public override void DeleteAllObjects()
        {
            try
            {
                IList<DomainObject> list = _db.Query(delegate(DomainObject bo) { return bo.Id != ""; });
                if (list != null)
                    foreach (DomainObject item in list)
                    {
                        _db.Delete(item);
                    }
                _db.Commit();
            }
            catch
            {
            }
        }

        public override T GetObjectByName<T>(string name)
        {
            IDb4oLinqQuery<T> queryResult = from T o in ObjectContainer
                                            where (String.Equals(o.Name, name, StringComparison.CurrentCultureIgnoreCase)) &&
                                                  o.Active
                                            select o;
            return queryResult.FirstOrDefault();
        }

        public override Person GetPersonByEmail(string email)
        {
            Person result = null;
            IDb4oLinqQuery<Person> queryResult = from Person o in ObjectContainer
                                                 where
                                                     (String.Equals(o.Email, email, StringComparison.CurrentCultureIgnoreCase)) &&
                                                     o.Active
                                                 select o;
            result = queryResult.FirstOrDefault();
            return result;
        }
    }
}
