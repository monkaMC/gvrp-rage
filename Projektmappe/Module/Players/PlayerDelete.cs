

using GTANetworkAPI;

namespace GVRP.Module.Players
{
    public static class PlayerDelete
    {
        public static void DeleteEntity(this Client client)
        {
            var iPlayer = client.GetPlayer();
            if (iPlayer == null) return;
            var character = iPlayer.Character;
            if (character != null)
            {
                client.RemoveEntityDataWhenExists("CustomCharacter");
                client.RemoveEntityDataWhenExists("dataParentsMother");
                client.RemoveEntityDataWhenExists("dataParentsFather");
                client.RemoveEntityDataWhenExists("dataParentsSimilarity");
                client.RemoveEntityDataWhenExists("dataParentsSkinSimilarity");
                client.RemoveEntityDataWhenExists("dataHairColor");
                client.RemoveEntityDataWhenExists("dataHairHighlightColor");
                client.RemoveEntityDataWhenExists("dataBeardColor");
                client.RemoveEntityDataWhenExists("dataEyebrowColor");
                client.RemoveEntityDataWhenExists("dataBlushColor");
                client.RemoveEntityDataWhenExists("dataLipstickColor");
                client.RemoveEntityDataWhenExists("dataChestHairColor");
                client.RemoveEntityDataWhenExists("dataEyeColor");
                client.RemoveEntityDataWhenExists("dataFeaturesLength");
                client.RemoveEntityDataWhenExists("dataAppearanceLength");
                if (iPlayer.Customization != null)
                {
                    for (int i = 0, length = iPlayer.Customization.Features.Length; i < length; i++)
                    {
                        client.RemoveEntityDataWhenExists("dataFeatures-" + i);
                    }

                    for (int i = 0, length = iPlayer.Customization.Appearance.Length; i < length; i++)
                    {
                        client.RemoveEntityDataWhenExists("dataAppearance-" + i);
                        client.RemoveEntityDataWhenExists("dataAppearanceOpacity-" + i);
                    }
                }
            }

            client.RemoveEntityDataWhenExists("phone_calling");
            client.RemoveEntityDataWhenExists("phone_number");
            client.RemoveEntityDataWhenExists("isInjured");
            client.RemoveEntityDataWhenExists("VOICE_RANGE");
            client.RemoveEntityDataWhenExists("death");
        }
    }
}