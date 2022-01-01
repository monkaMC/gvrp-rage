using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Clothes;
using GVRP.Module.Clothes.Character;
using GVRP.Module.Einreiseamt;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Customization
{
    public static class CustomizationModuleFunctions
    {
        public static readonly Vector3 creatorCharPos = new Vector3(402.8664, -996.4108, -99.00027);
        public static readonly Vector3 creatorPos = new Vector3(402.8664, -997.5515, -98.5);
        public static readonly Vector3 cameraLookAtPos = new Vector3(402.8664, -996.4108, -98.5);
        public const float FacingAngle = -185.0f;
        
        public static void ApplyCharacter(this DbPlayer iPlayer, bool loadArmorHealthFromDBValue = false)
        {

            if (iPlayer.HasData("clonePerson"))
            {
                DbPlayer target = Players.Players.Instance.GetByDbId(iPlayer.GetData("clonePerson"));
                if(target != null && target.IsValid())
                {
                    ApplyCharacterFromOther(iPlayer, target);
                    return;
                }
            }

            if (iPlayer.Customization == null) iPlayer.Customization = new CharacterCustomization();

            var armor = 0;
            var health = 100;

            if (loadArmorHealthFromDBValue)
            {
                armor = iPlayer.Player.Armor;
                health = iPlayer.Player.Health;
            }
            else
            {
                armor = iPlayer.Armor[0];
                health = iPlayer.Hp;
            }
            if((PedHash)iPlayer.Player.Model != PedHash.FreemodeFemale01 && (PedHash)iPlayer.Player.Model != PedHash.FreemodeMale01) iPlayer.Player.SetSkin(iPlayer.Character.Skin);

            iPlayer.Player.Armor = armor;
            iPlayer.Player.Health = health;

            var headBlend = new HeadBlend
            {
                ShapeFirst = iPlayer.Customization.Parents.MotherShape,
                ShapeSecond = iPlayer.Customization.Parents.FatherShape,
                ShapeThird = 0,
                SkinFirst = iPlayer.Customization.Parents.MotherSkin,
                SkinSecond = iPlayer.Customization.Parents.FatherSkin,
                SkinThird = 0,
                ShapeMix = iPlayer.Customization.Parents.Similarity,
                SkinMix = iPlayer.Customization.Parents.SkinSimilarity,
                ThirdMix = 0
            };

            var headOverlays = new Dictionary<int, HeadOverlay>(iPlayer.Customization.Appearance.Length);

            for (int i = 0, length = iPlayer.Customization.Appearance.Length; i < length; i++)
            {
                headOverlays[i] = new HeadOverlay
                {
                    Index = iPlayer.Customization.Appearance[i].Value,
                    Opacity = iPlayer.Customization.Appearance[i].Opacity,
                    Color = (byte)GetHeadOverlayColor(iPlayer.Customization, i)
                };
            }
            
            List<Decoration> decorations = new List<Decoration>();
            foreach (uint assetsTattooId in iPlayer.Customization.Tattoos)
            {
                if (!AssetsTattooModule.Instance.GetAll().ContainsKey(assetsTattooId)) continue;
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(assetsTattooId);
                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = iPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                decorations.Add(decoration);
            }

            NAPI.Player.SetPlayerCustomization(iPlayer.Player, iPlayer.Customization.Gender == 0, headBlend, iPlayer.Customization.EyeColor,
                iPlayer.Customization.Hair.Color, iPlayer.Customization.Hair.HighlightColor,
                iPlayer.Customization.Features, headOverlays, decorations.ToArray());

            // Set Hair
            iPlayer.SetClothes(2, iPlayer.Customization.Hair.Hair, 0);
            NAPI.Player.SetPlayerHairColor(iPlayer.Player, iPlayer.Customization.Hair.Color, iPlayer.Customization.Hair.HighlightColor);

            ClothModule.Instance.RefreshPlayerClothes(iPlayer);

            // Remove Mask
            iPlayer.SetClothes(1, 0, 0);

            //Resync Weapons
            iPlayer.LoadPlayerWeapons();

            // Set to fist
            iPlayer.Player.GiveWeapon(WeaponHash.Unarmed, 1);

            iPlayer.ApplyPlayerHealth();
        }

        public static void ApplyCharacterFromOther(this DbPlayer iPlayer, DbPlayer destinationPlayer)
        {

            if (iPlayer.Customization == null) iPlayer.Customization = new CharacterCustomization();

            var armor = iPlayer.Player.Armor;
            var health = iPlayer.Player.Health;
            if ((PedHash)iPlayer.Player.Model != PedHash.FreemodeFemale01 && (PedHash)iPlayer.Player.Model != PedHash.FreemodeMale01) iPlayer.Player.SetSkin(iPlayer.Character.Skin);
            iPlayer.Player.Armor = armor;
            iPlayer.Player.Health = health;

            var headBlend = new HeadBlend
            {
                ShapeFirst = destinationPlayer.Customization.Parents.MotherShape,
                ShapeSecond = destinationPlayer.Customization.Parents.FatherShape,
                ShapeThird = 0,
                SkinFirst = destinationPlayer.Customization.Parents.MotherSkin,
                SkinSecond = destinationPlayer.Customization.Parents.FatherSkin,
                SkinThird = 0,
                ShapeMix = destinationPlayer.Customization.Parents.Similarity,
                SkinMix = destinationPlayer.Customization.Parents.SkinSimilarity,
                ThirdMix = 0
            };

            var headOverlays = new Dictionary<int, HeadOverlay>(destinationPlayer.Customization.Appearance.Length);

            for (int i = 0, length = destinationPlayer.Customization.Appearance.Length; i < length; i++)
            {
                headOverlays[i] = new HeadOverlay
                {
                    Index = destinationPlayer.Customization.Appearance[i].Value,
                    Opacity = destinationPlayer.Customization.Appearance[i].Opacity,
                    Color = (byte)GetHeadOverlayColor(destinationPlayer.Customization, i)
                };
            }

            List<Decoration> decorations = new List<Decoration>();
            foreach (uint assetsTattooId in destinationPlayer.Customization.Tattoos)
            {
                if (!AssetsTattooModule.Instance.GetAll().ContainsKey(assetsTattooId)) continue;
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(assetsTattooId);
                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = destinationPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                decorations.Add(decoration);
            }

            NAPI.Player.SetPlayerCustomization(iPlayer.Player, destinationPlayer.Customization.Gender == 0, headBlend, destinationPlayer.Customization.EyeColor,
                destinationPlayer.Customization.Hair.Color, destinationPlayer.Customization.Hair.HighlightColor,
                destinationPlayer.Customization.Features, headOverlays, decorations.ToArray());

            // Set Hair
            iPlayer.SetClothes(2, destinationPlayer.Customization.Hair.Hair, 0);
            NAPI.Player.SetPlayerHairColor(iPlayer.Player, destinationPlayer.Customization.Hair.Color, destinationPlayer.Customization.Hair.HighlightColor);

            ClothModule.Instance.RefreshPlayerClothes(iPlayer);

            // Remove Mask
            iPlayer.SetClothes(1, 0, 0);

            //Resync Weapons
            iPlayer.LoadPlayerWeapons();

            // Set to fist
            iPlayer.Player.GiveWeapon(WeaponHash.Unarmed, 1);
        }

        private static int GetHeadOverlayColor(CharacterCustomization customization, int overlayId)
        {
            switch (overlayId)
            {
                case 1:
                    return customization.BeardColor;
                case 2:
                    return customization.EyebrowColor;
                case 5:
                    return customization.BlushColor;
                case 8:
                    return customization.LipstickColor;
                case 10:
                    return customization.ChestHairColor;
                default:
                    return 0;
            }
        }

        public static void ClearDecorations(this DbPlayer iPlayer)
        {
            iPlayer.Player.TriggerEvent("clearPlayerDecorations");
        }

        public static void ApplyDecorations(this DbPlayer iPlayer)
        {

            iPlayer.ClearDecorations();

            List<Decoration> decorations = new List<Decoration>();
            foreach (uint assetsTattooId in iPlayer.Customization.Tattoos)
            {
                if (!AssetsTattooModule.Instance.GetAll().ContainsKey(assetsTattooId)) continue;
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(assetsTattooId);
                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = iPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                NAPI.Player.SetPlayerDecoration(iPlayer.Player, decoration);
            }
        }

        public static void SaveCustomization(this DbPlayer iPlayer)
        {
            try
            {

                var customizationString = JsonConvert.SerializeObject(iPlayer.Customization) ?? "";
                var query =
                    $"UPDATE `player` SET customization = '{customizationString}' WHERE id = '{iPlayer.Id}';";
                query +=
                    $"UPDATE `player_character` SET skin = '{Enum.GetName(typeof(PedHash), iPlayer.Character.Skin)}' WHERE player_id = '{iPlayer.Id}';";
                MySQLHandler.ExecuteAsync(query);
            }
            catch (Exception exception)
            {
                Logger.Crash(exception);
            }
        }

        public static void AddTattoo(this DbPlayer iPlayer, uint tattooId)
        {
            if (!iPlayer.Customization.Tattoos.Contains(tattooId)) iPlayer.Customization.Tattoos.Add(tattooId);
            iPlayer.SaveCustomization();
            iPlayer.ApplyDecorations();
        }
        
        public static void RemoveTattoo(this DbPlayer iPlayer, uint tattooId)
        {
            if (iPlayer.Customization.Tattoos.Contains(tattooId)) iPlayer.Customization.Tattoos.Remove(tattooId);
            iPlayer.SaveCustomization();
            iPlayer.ApplyDecorations();
        }

        public static void StartCustomization(this DbPlayer iPlayer)
        {
            if (iPlayer == null || !iPlayer.IsValid())
            {
                return;
            }
            var player = iPlayer.Player;

            iPlayer.SetData("lastPosition", player.Position);
            iPlayer.SetData("lastDimension", player.Dimension);
            
            player.Dimension = iPlayer.Id;
            player.Transparency = 255;
            player.Rotation = new Vector3(0f, 0f, FacingAngle);
            player.SetPosition(creatorCharPos);

            var character = iPlayer.Character;

            if (iPlayer.Customization == null)
            {
                iPlayer.Customization = new CharacterCustomization();
                iPlayer.SetData("firstCharacter", true);
                SetCreatorClothes(iPlayer);
            }

            ComponentManager.Get<CustomizationWindow>().Show()(iPlayer, iPlayer.Customization);
        }

        public static void SetCreatorClothes(this DbPlayer iPlayer)
        {
            if (iPlayer.Customization == null) return;

            // clothes
            for (var i = 0; i < 10; i++) iPlayer.Player.ClearAccessory(i);

            iPlayer.SetClothes(1, 0, 0);

            if (iPlayer.Customization.Gender == 0)
            {
                // Oberkörper frei
                iPlayer.SetClothes(11, 15, 0);
                // Unterhemd frei
                iPlayer.SetClothes(8, 57, 0);
                // Torso frei
                iPlayer.SetClothes(3, 15, 0);
            }
            else
            {
                // Naked (.)(.)
                iPlayer.SetClothes(3, 15, 0);
                iPlayer.SetClothes(4, 15, 0);
                iPlayer.SetClothes(8, 0, 99);
                iPlayer.SetClothes(11, 0, 99);
            }

            iPlayer.SetClothes(2, iPlayer.Customization.Hair.Hair, 0);
        }

        public static void StopCustomization(this DbPlayer iPlayer)
        {
            var player = iPlayer.Player;

            if (iPlayer.NeuEingereist())
            {
                var pos = new GTANetworkAPI.Vector3(-1108.67, -2843.62, 22.7491);
                float heading = 336.17f;
                uint dimension = 188;

                player.Freeze(true, true, true);
                player.Position = pos;
                player.Dimension = dimension;
                player.SetRotation(heading);

                Task.Run(async () =>
                {
                    await Task.Delay(20000);
                    iPlayer.Player.Freeze(false, true, true);
                });

                return;
            }
            else
            {
                player.SetPosition(new Vector3(298.7902, -584.4927, 43.26085));
                player.Dimension = 0;
            }

            if (iPlayer.HasData("lastPosition"))
            {
                player.SetPosition((Vector3)iPlayer.GetData("lastPosition"));
                if(iPlayer.HasData("lastDimension"))
                {
                    player.Dimension = iPlayer.GetData("lastDimension");
                }
                if (iPlayer.Dimension[0] != 0)
                {
                    Task.Run(async () =>
                    {
                        player.TriggerEvent("freezePlayer", true);
                        player.SetPosition((Vector3)iPlayer.GetData("lastPosition"));
                        await Task.Delay(1000);
                        player.SetPosition((Vector3)iPlayer.GetData("lastPosition"));
                        await Task.Delay(1500);
                        player.TriggerEvent("freezePlayer", false);
                    });
                }
                iPlayer.ResetData("lastPosition");
                iPlayer.ResetData("lastDimension");
                iPlayer.SetMedicCuffed(false);
                iPlayer.SetCuffed(false);
            }
            else
            {
                iPlayer.Player.SetPosition(new Vector3(298.7902, -584.4927, 43.26085));
            }

            // revert back to last save data
            if (iPlayer.HasData("ChangedGender"))
            {
                //character.customization =TODO: old customization 
                iPlayer.ResetData("ChangedGender");
            }

            iPlayer.ApplyCharacter();
            ClothModule.Instance.RefreshPlayerClothes(iPlayer);
            //iPlayer.Player.TriggerEvent("DestroyCamera");
            if (iPlayer.HasData("firstCharacter"))
            {
                iPlayer.ResetData("firstCharacter");
            }
        }
    }
}
