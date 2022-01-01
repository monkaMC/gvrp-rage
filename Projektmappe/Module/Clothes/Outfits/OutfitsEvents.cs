using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GVRP.Module.Players;

namespace GVRP.Module.Clothes.Outfits
{
    public class OutfitsEvents : Script
    {
        [RemoteEvent]
        public void SaveOutfit(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!Regex.IsMatch(returnstring, @"^[a-zA-Z ]+$"))
            {
                dbPlayer.SendNewNotification("Dieser Name ist nicht gueltig!");
                return;
            }

            Outfit outfit = new Outfit()
            {
                PlayerId = dbPlayer.Id,
                Name = returnstring,
                Clothes = dbPlayer.Character.Clothes,
                Props = dbPlayer.Character.EquipedProps
            };

            OutfitsModule.Instance.AddOutfit(dbPlayer, outfit);
            dbPlayer.SendNewNotification("Outfit gespeichert!");
        }
    }
}
