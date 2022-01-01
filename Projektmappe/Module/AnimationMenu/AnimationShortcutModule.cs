using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.RemoteEvents;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationShortcutModule : Module<AnimationShortcutModule>
    {
        public static uint maxKeys = 16;

        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new AnimationShortCutMenuBuilder());
            MenuManager.Instance.AddBuilder(new AnimationShortCutSlotMenuBuilder());
            return base.OnLoad();
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {

            if (reader.GetString("animation_shortcuts") == "") dbPlayer.AnimationShortcuts = new Dictionary<uint, uint>();
            else dbPlayer.AnimationShortcuts = NAPI.Util.FromJson<Dictionary<uint, uint>>(reader.GetString("animation_shortcuts"));

            for (uint i = 0; i < 16; i++)
            {
                if (!dbPlayer.AnimationShortcuts.ContainsKey(i)) dbPlayer.AnimationShortcuts.Add(i, 0);
            }
        }

    }
    public static class PlayerAnimationShortcutsExtension
    {
        public static void SaveAnimationShortcuts(this DbPlayer dbPlayer)
        {
            if (dbPlayer.AnimationShortcuts == null) return;
            MySQLHandler.ExecuteAsync($"UPDATE player SET animation_shortcuts = '{NAPI.Util.ToJson(dbPlayer.AnimationShortcuts)}' WHERE id = '{dbPlayer.Id}'");
        }

        public static void UpdateAnimationShortcuts(this DbPlayer dbPlayer)
        {
            // To Event.. setNMenuItems
            dbPlayer.Player.TriggerEvent("setNMenuItems", dbPlayer.GetJsonAnimationsShortcuts());
        }

        public static string GetJsonAnimationsShortcuts(this DbPlayer dbPlayer)
        {
            List<AnimationShortCutJson> animationShortCutJsons = new List<AnimationShortCutJson>();

            foreach(KeyValuePair<uint, uint> kvp in dbPlayer.AnimationShortcuts)
            {
                AnimationShortCutJson item = new AnimationShortCutJson() { Slot = (int)kvp.Key, Name = "Nicht belegt", Icon = "" };

                // Specials
                if (kvp.Key == 0) // general stop anim..
                {
                    item = new AnimationShortCutJson() { Slot = (int)kvp.Key, Name = "Animation beenden", Icon = "Abbrechen.png" };
                    animationShortCutJsons.Add(item);
                    continue;
                }
                else if (kvp.Key == 1) // set anim slots
                {
                    item = new AnimationShortCutJson() { Slot = (int)kvp.Key, Name = "Animationen belegen", Icon = "Bearbeiten.png" };
                    animationShortCutJsons.Add(item);
                    continue;
                }

                if(kvp.Value == 0) // nicht belegt
                {
                    item = new AnimationShortCutJson() { Slot = (int)kvp.Key, Name = "Nicht belegt", Icon = "Leer.png" };
                    animationShortCutJsons.Add(item);
                    continue;
                }
                else
                {
                    if (AnimationItemModule.Instance.Contains(kvp.Value))
                    {
                        AnimationItem animationItem = AnimationItemModule.Instance.Get(kvp.Value);
                        item = new AnimationShortCutJson() { Slot = (int)kvp.Key, Name = animationItem.Name, Icon = "Animation.png" }; //animationItem.Icon };
                        animationShortCutJsons.Add(item);
                    }
                    else
                    {
                        item = new AnimationShortCutJson() { Slot = (int)kvp.Key, Name = "Nicht belegt", Icon = "Leer.png" };
                        animationShortCutJsons.Add(item); // add defaults...
                    }
                }
            }

            return NAPI.Util.ToJson(animationShortCutJsons);
        }
    }

    public class AnimationShortCutJson
    {
        [JsonProperty(PropertyName = "slot")]
        public int Slot { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }
    }
}
