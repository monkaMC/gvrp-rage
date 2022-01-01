using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Customization;
using GVRP.Module.Items;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Windows
{
    public class CustomizationWindow : Window<Func<DbPlayer, CharacterCustomization, bool>>
    {
        private class ShowEvent : Event
        {
            //private string InventoryContent { get; } // --- appears to be empty if used?
            [JsonProperty(PropertyName = "customization")] private CharacterCustomization Customization { get; }
            [JsonProperty(PropertyName = "level")] private int Level { get; }

            public ShowEvent(DbPlayer dbPlayer, CharacterCustomization customization) : base(dbPlayer)
            {
                Customization = customization;
                Level = dbPlayer.HasData("firstCharacter") ? 0 : dbPlayer.Level;
            }
        }
        public override Func<DbPlayer, CharacterCustomization, bool> Show()
        {
            return (player, customization) => OnShow(new ShowEvent(player, customization));
        }

        public CustomizationWindow() : base("CharacterCreator")
        {
        }

        [RemoteEvent]
        public void UpdateCharacterCustomization(Client player, string charakterJSON, int price)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || String.IsNullOrEmpty(charakterJSON)) return;
            CharacterCustomization customization = JsonConvert.DeserializeObject<CharacterCustomization>(charakterJSON);
            int result = dbPlayer.TakeAnyMoney(price);

            if (result != -1)
            {
                // Buy Customization
                dbPlayer.Customization = customization;
                dbPlayer.SaveCustomization();
                dbPlayer.SendNewNotification($"Aussehen geaendert, dir wurden {price}$ vom Konto abgezogen", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
            }
            else
            {
                dbPlayer.SendNewNotification("Nicht genug Geld", notificationType: PlayerNotification.NotificationType.ERROR);
            }

            // Update Charakter
            //dbPlayer.ApplyCharacter();
            dbPlayer.ResetData("firstCharacter");

            dbPlayer.StopCustomization();
        }

        [RemoteEvent]
        public void StopCustomization(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            //dbPlayer.ApplyCharacter();
            dbPlayer.StopCustomization();
        }

    }
}
