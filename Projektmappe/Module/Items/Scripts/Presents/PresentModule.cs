using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GVRP.Module.Items.Scripts.Presents
{
    public class PresentModule : SqlModule<PresentModule, Present, uint>
    {
        public static List<Present> PresentExport = new List<Present>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `presents`;";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }

        public override bool Load(bool reload = false)
        {
            PresentExport = new List<Present>();
            return base.Load();
        }

        protected override void OnItemLoaded(Present loadable)
        {
            for (int i = 0; i < loadable.Percent; i++)
            {
                PresentModule.PresentExport.Add(loadable);
            }
        }

        public List<Present> GetByItemId(uint id)
        {
            return PresentExport.Where(i => i.Item.Id == id).ToList();
        }
    }
}
