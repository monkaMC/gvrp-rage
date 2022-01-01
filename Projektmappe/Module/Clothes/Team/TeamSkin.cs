using System.Collections.Generic;
using GTANetworkAPI;

namespace GVRP.Module.Clothes.Team
{
    public class TeamSkin
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public PedHash Hash { get; set; }
        public uint TeamId { get; set; }
        public List<TeamSkinCloth> Clothes { get; set; }
        public List<TeamSkinProp> Props { get; set; } //Todo: dict slot/List<TeamSkinProp> maybe?

        public TeamSkin()
        {
        }
    }
}