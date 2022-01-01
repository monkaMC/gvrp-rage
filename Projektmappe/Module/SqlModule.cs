using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Logging;

namespace GVRP.Module
{
    public abstract class SqlModule<T, TLoadable, TId> : SqlBaseModule<T, TLoadable>
        where T : Module<T> //, new()
        where TLoadable : Loadable<TId>
    {
        private Dictionary<TId, TLoadable> list;
        
        protected override bool OnLoad()
        {
            list = new Dictionary<TId, TLoadable>();
            return base.OnLoad();
        }

        protected override void OnItemLoad(TLoadable u)
        {
            if (u == null) return;
            var identifier = u.GetIdentifier();
            if (list.ContainsKey(identifier))
            {
                Logger.Print($"Duplicate: {typeof(TLoadable)} ID: {identifier}");
                return;
            }
            list.Add(identifier, u);
        }

        public Dictionary<TId, TLoadable> GetAll()
        {
            return list;
        }
        
        public bool Contains(TId key)
        {
            return list.ContainsKey(key);
        }

        public TLoadable this[TId key]
        {
            get => !list.TryGetValue(key, out var value) ? null : value;
            set => list[key] = value;
        }

        public /*static*/ TLoadable Get(TId key) //Todo: make static
        {
            var module = Instance as SqlModule<T, TLoadable, TId>;
            return module?[key];
        }

        public void Add(TId key, TLoadable value)
        {
            list.Add(key, value);
        }

        public void Edit(TId key, TLoadable value)
        {
            list[key] = value;
        }

        public void Update(TId key, TLoadable value, string tableName, string condition, params object[] data)
        {
            Edit(key, value);
            Change(tableName, condition, data);
        }

        public void Insert(TId key, TLoadable value, string tableName, params object[] data)
        {
            Add(key, value);
            Execute(tableName, data);
        }
    }
}