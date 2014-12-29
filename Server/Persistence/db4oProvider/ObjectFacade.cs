using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using DomainModel;
using PersistenceService;

namespace db4oProvider
{
    public sealed class ObjectFacade<T> where T : DomainObject, new()
    {
        private static IObjectContainer _ObjectContainer;

        public static IObjectContainer ObjectContainer
        {
            get
            {
                if ((_ObjectContainer == null) || (_ObjectContainer.Ext().IsClosed()))
                    _ObjectContainer = (Persistence.Instance.Provider as db4oProvider).ObjectContainer;
                return _ObjectContainer;
            }
        }

        public static bool DeleteByID(string id)
        {
            IList<T> result =
                ObjectContainer.Query(delegate(T bo) { return bo.Id == id; });
            if ((result == null) || (result.Count < 1))
                return false;
            _ObjectContainer.Delete(result[0]);
            _ObjectContainer.Commit();
            return true;
        }

        public static T GetByID(string id)
        {
            IList<T> result =
                ObjectContainer.Query(delegate(T bo) { return (bo.Id == id) && bo.Active; });
            if ((result == null) || (result.Count < 1))
                return null;
            else
                return result[0];
        }

        public static T GetByName(string name)
        {
            IList<T> result =
                ObjectContainer.Query(delegate(T bo) { return (bo.Name == name) && bo.Active; });
            if ((result == null) || (result.Count < 1))
                return null;
            else
                return result[0];
        }

        public static List<T> GetAllObjects()
        {
            var list = new List<T>();

            IList<T> result = ObjectContainer.Query<T>(typeof (T));
//            if (result != null)
//                foreach (T item in result)
//                {
//                    if (item.Active)
//                        list.Add(item);
//                }
//            IOrderedEnumerable<T> queryResult = from T o in list
//                                                orderby o.Created descending
//                                                select o;
//            return queryResult.ToList();
            if (result != null)
                return result.ToList();
            return list;
        }

        //        public static T GetFirstFoundObject()
        //        {
        //            IList<T> _Items = ObjectContainer.Query<T>(typeof(T));
        //            if ((_Items == null) || (_Items.Count < 1))
        //            {
        //                return CreateObject();
        //            }
        //            else
        //            {
        //                //_Items[0].ID = ObjectContainer.Ext().GetObjectInfo(_Items[0]).GetUUID().GetLongPart();
        //                return _Items[0];
        //            }
        //        }
        //
        //        public static T CreateObject()
        //        {
        //            T c = new T();
        //            return c;
        //        }
    }
}
