using GTANetworkAPI;
using System;
using System.Linq;
using GVRP.Module.Items;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubModule : SqlModule<NightClubModule, NightClub, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(BusinessModule), typeof(NightClubItemModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `business_nightclubs`;";
        }

        public NightClub GetByDimension(uint dimension)
        {
            return Instance.GetAll().Values.Where(fs => fs.Id == dimension).FirstOrDefault();
        }

        public NightClub GetThis(Vector3 position)
        {
            return Instance.GetAll().Values.FirstOrDefault(fs => fs.Position.DistanceTo(position) < 10.0f);
        }

        protected override void OnItemLoaded(NightClub loadable)
        {
            loadable.NightClubShopItems = NightClubItemModule.Instance.GetAll().Values.Where(nci => nci.NightClubId == loadable.Id).ToList();
            loadable.Container = ContainerManager.LoadContainer(loadable.Id, ContainerTypes.NIGHTCLUB);
        }
    }
}
