using GTANetworkAPI;
using System;
using System.Linq;
using GVRP.Module.Clothes;
using GVRP.Module.Commands;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Outfits
{
    public enum OutfitTypes
    {
        Cocain = 73,
        Jail = 67,
    }

    public class OutfitsModule : Module<OutfitsModule>
    {
        public uint GetPropValue(uint id, bool prop = false)
        {
            return prop ? id*1000 : id * 50000;
        }

        public void SetPlayerOutfit(DbPlayer dbPlayer, int outfitId, bool overwrite = false)
        {
            Client player = dbPlayer.Player;

            // find outfit by id and actual gender
            Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o => ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId-1) == outfitId && o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01) ? true: false);
            if (outfit == null) return;

            if(overwrite) dbPlayer.Character.Clothes.Clear();

            foreach(OutfitComponent kvp in outfit.Components)
            {
                dbPlayer.SetClothes(kvp.Slot, kvp.Component, kvp.Texture);
                if(overwrite) dbPlayer.Character.Clothes.Add(kvp.Slot, GetPropValue(kvp.Id));
            }

            // clear all
            player.ClearAccessory(0);
            player.ClearAccessory(1);
            player.ClearAccessory(2);
            player.ClearAccessory(6);
            player.ClearAccessory(7);
            if(overwrite) dbPlayer.Character.EquipedProps.Clear();

            foreach (OutfitProp kvp in outfit.Props)
            {
                player.ClearAccessory(kvp.Slot);
                player.SetAccessories(kvp.Slot, kvp.Component, kvp.Texture);
                if(overwrite) dbPlayer.Character.EquipedProps.Add(kvp.Slot, GetPropValue(kvp.Id, true));
            }

            // Fix Hair
            // Set Hair
            dbPlayer.SetClothes(2, dbPlayer.Customization.Hair.Hair, 0);
            NAPI.Player.SetPlayerHairColor(player, dbPlayer.Customization.Hair.Color, dbPlayer.Customization.Hair.HighlightColor);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandoutfit(Client player, string outfitstring)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.CanAccessMethod()) return;
            if (!iPlayer.IsValid()) return;

            if (!Int32.TryParse(outfitstring, out int outfitId)) return;

            SetPlayerOutfit(iPlayer, outfitId);
            iPlayer.SendNewNotification("Outfit " + outfitId);
        }
    }
}
