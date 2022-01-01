using System;
using GTANetworkAPI;
using GVRP.Module.Players.Db;
using GVRP.Module.Pet;

namespace GVRP.Module.Pet
{
    public sealed class PetModule : SqlModule<PetModule, PetData, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `pets`;";
        }

        protected override bool OnLoad()
        {
            return base.OnLoad();
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            dbPlayer.RemovePet();
        }
    }
}