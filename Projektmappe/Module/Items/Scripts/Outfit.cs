using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Clothes;
using GVRP.Module.Customization;
using GVRP.Module.Logging;
using GVRP.Module.Outfits;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
//Possible problem. Removed on use, but not possible to add without weapon. Readd item?
namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Outfit(DbPlayer iPlayer, ItemModel ItemData, Item item)
        {
            try
            {
                if (iPlayer.Player.IsInVehicle) return false;

                string outfit = ItemData.Script.ToLower().Replace("outfit_", "");

                if (outfit.Length <= 0) return false;

                if (outfit == "original")
                {
                    Chats.sendProgressBar(iPlayer, 4000);
                    iPlayer.Player.TriggerEvent("freezePlayer", true);

                    if(item.Data == null || !item.Data.ContainsKey("owner") || item.Data["owner"] != iPlayer.Id)
                    {
                        iPlayer.SendNewNotification("Du kannst keine Kleidung von anderen Personen anziehen!");
                        return false;
                    }
                    iPlayer.Container.RemoveItem(ItemData, 1);

                    if (item.Data != null && item.Data.ContainsKey("props") && item.Data.ContainsKey("cloth"))
                    {
                        string clothesstring = item.Data["cloth"];
                        Dictionary<int, uint> clothDic = clothesstring.TrimEnd(';').Split(';').ToDictionary(it => Convert.ToInt32(it.Split('=')[0]), it => Convert.ToUInt32(it.Split('=')[1]));

                        string propsstring = item.Data["props"];
                        Dictionary<int, uint> PropsDic = propsstring.TrimEnd(';').Split(';').ToDictionary(it => Convert.ToInt32(it.Split('=')[0]), it => Convert.ToUInt32(it.Split('=')[1]));

                        iPlayer.Character.Clothes = clothDic;
                        iPlayer.Character.EquipedProps = PropsDic;
                    }

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                    await Task.Delay(4000);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    iPlayer.ApplyCharacter();
                    iPlayer.Player.TriggerEvent("freezePlayer", false);

                    ClothModule.SaveCharacter(iPlayer);

                    iPlayer.SendNewNotification("Sie haben die Kleidung erfolgreich angezogen!");
                    return true;
                }

                if (!Int32.TryParse(outfit, out int outfitid))
                {
                    return false;
                }

                // Heist check
                if (outfitid == 66 && !iPlayer.HasData("heistActive"))
                {
                    iPlayer.SendNewNotification("Kann nur angezogen werden, wenn ein Heist aktiv ist!", PlayerNotification.NotificationType.ERROR);
                    return false;
                }

                Chats.sendProgressBar(iPlayer, 4000);
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                iPlayer.Container.RemoveItem(ItemData, 1);

                Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
                Data.Add("cloth", String.Join(';', string.Join(";", iPlayer.Character.Clothes.Select(x => x.Key + "=" + x.Value).ToArray())));
                Data.Add("props", String.Join(',', string.Join(";", iPlayer.Character.EquipedProps.Select(x => x.Key + "=" + x.Value).ToArray())));
                Data.Add("owner", iPlayer.Id);

                iPlayer.Container.AddItem(737, 1, Data);
                iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                await Task.Delay(4000);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                // Armor westen
                if (ItemData.Id == 865) iPlayer.SetArmor(100);

                iPlayer.Player.TriggerEvent("freezePlayer", false);
                OutfitsModule.Instance.SetPlayerOutfit(iPlayer, outfitid, true);

                ClothModule.SaveCharacter(iPlayer);

                iPlayer.SendNewNotification(
                    "Sie haben die Kleidung erfolgreich angezogen!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }
    }
}
