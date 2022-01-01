using GTANetworkAPI;
using GVRP.Module.Clothes;
using GVRP.Module.GTAN;
using GVRP.Module.Outfits;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerClothes
    {
        public static void SetNacked(this DbPlayer iPlayer)
        {
            if (!iPlayer.IsFreeMode()) return;
            if (iPlayer.HasData("naked"))
            {
                iPlayer.ResetData("naked");
            }
            else
            {
                iPlayer.SetData("naked", true);
            }

            iPlayer.RefreshNacked();
        }

        public static bool IsFreeMode(this DbPlayer iPlayer)
        {
            return iPlayer.Character.Skin == PedHash.FreemodeMale01 ||
                   iPlayer.Character.Skin == PedHash.FreemodeFemale01;
        }

        public static void RefreshNacked(this DbPlayer iPlayer)
        {
            if (!iPlayer.IsFreeMode()) return;
            if (iPlayer.HasData("naked"))
            {
                // Wenn naked
                if (iPlayer.IsFemale())
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
            }
        }

        public static void SetPlayerJailClothes(this DbPlayer iPlayer)
        {
            if (!iPlayer.IsFreeMode()) return;
            if (iPlayer.Customization != null)
            {
                iPlayer.SetClothes(2, iPlayer.Customization.Hair.Hair, 0);
            }
            
            if (iPlayer.jailtime[0] > 0)
            {
                iPlayer.SetOutfit(OutfitTypes.Jail);
            }
        }

        public static void ApplyArmorVisibility(this DbPlayer iPlayer)
        {
            if (!iPlayer.IsFreeMode()) return;

            // Apply Armor visibility
            if (iPlayer.Armor[0] > 99)
            {
                switch(iPlayer.VisibleArmorType)
                {
                    case 2:                     //Police Weste //Rang 0
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 11, 3);
                        else
                            iPlayer.SetClothes(9, 12, 3);
                        break;
                    case 3:                    //Police GTF Weste //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 7, 0);
                        else
                            iPlayer.SetClothes(9, 7, 0);
                        break;
                    case 4:                 //Police HP Weste //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 11, 0);
                        else
                            iPlayer.SetClothes(9, 12, 0);
                        break;
                    case 5:                 //Police Corrections Weste //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 10, 2);
                        else
                            iPlayer.SetClothes(9, 11, 2);
                        break;
                    case 6:                 //Police PIA //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 11, 4);
                        else
                            iPlayer.SetClothes(9, 12, 4);
                        break;
                    case 7:                     //Sheriff Weste (Schwarz) //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 11, 2);
                        else
                            iPlayer.SetClothes(9, 12, 1);
                        break;
                    case 8:                     //Sheriff GTF Weste //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 7, 1);
                        else
                            iPlayer.SetClothes(9, 7, 1);
                        break;
                    case 9:                     //Sheriff K9 Weste (Gelb) //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 10, 4);
                        else
                            iPlayer.SetClothes(9, 11, 4);
                        break;
                    case 10:                    //Sheriff K9 Weste (Schwarz) //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 10, 3);
                        else
                            iPlayer.SetClothes(9, 11, 3);
                        break;
                    case 11:                    //S.W.A.T. Weste //Rang 12
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 7, 4);
                        else
                            iPlayer.SetClothes(9, 7, 3);
                        break;
                    case 12:                    //FIB Weste (Schwarz)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 9, 3);
                        else
                            iPlayer.SetClothes(9, 10, 3);
                        break;
                    case 13:                    //FIB Federal Agent Weste (Blau) 
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 7, 2);
                        else
                            iPlayer.SetClothes(9, 7, 2);
                        break;
                    case 14:                     //FIB K9 Weste
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 10, 1);
                        else
                            iPlayer.SetClothes(9, 11, 1);
                        break;
                    case 15:                    //FIB GTF Weste (Blau)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 10, 0);
                        else
                            iPlayer.SetClothes(9, 11, 0);
                        break;
                    case 16:                    //IAA Weste 
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 7, 3);
                        else
                            iPlayer.SetClothes(9, 7, 4);
                        break;
                    case 17:                    //FIB Weste (Grün)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 9, 4);
                        else
                            iPlayer.SetClothes(9, 10, 4);
                        break;
                    case 18:                    //Gewöhnliche Weste (Staatsfraktionen)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 17, 2);
                        else
                            iPlayer.SetClothes(9, 15, 2);
                        break;
                    case 19:                    //FIB GTF Weste (Schwarz)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 9, 0);
                        else
                            iPlayer.SetClothes(9, 10, 0);
                        break;
                    case 20:                    //FIB Federal Agent Weste (Schwarz)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 9, 1);
                        else
                            iPlayer.SetClothes(9, 10, 1);
                        break;
                    case 21:                    //FIB Federal Agent Weste (Grün)
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 9, 2);
                        else
                            iPlayer.SetClothes(9, 10, 2);
                        break;
                    case 22:                    //ARMY MP Weste
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 19, 0);
                        else
                            iPlayer.SetClothes(9, 17, 0);
                        break;
                    case 23:                    //ARMY Air Force Weste
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 19, 1);
                        else
                            iPlayer.SetClothes(9, 17, 1);
                        break;
                    case 24:                    //ARMY Combat Weste
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 26, 0);
                        else
                            iPlayer.SetClothes(9, 24, 0);
                        break;
                    case 25:                    //ARMY Infantry Weste
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 26, 1);
                        else
                            iPlayer.SetClothes(9, 24, 1);
                        break;
                    case 26:                    //ARMY Coast Guard Weste
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 26, 2);
                        else
                            iPlayer.SetClothes(9, 24, 2);
                        break;
                    case 27:                    //IAA SAD Weste 
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 4, 2);
                        else
                            iPlayer.SetClothes(9, 3, 2);
                        break;
                    case 28:                    //FIB iwas 1 
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 4, 0);
                        else
                            iPlayer.SetClothes(9, 3, 0);
                        break;
                    case 29:                    //FIB iwas 2
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 3, 1);
                        else
                            iPlayer.SetClothes(9, 3, 1);
                        break;
                    case 30:                    //Underarmor coppa
                        iPlayer.SetClothes(9, 0, 0);
                        break;
                    default:
                        if (iPlayer.IsFemale())
                            iPlayer.SetClothes(9, 17, 2);
                        else
                            iPlayer.SetClothes(9, 15, 2);
                        break;
                }
            }
            else if (iPlayer.Armor[0] <= 99)
            {
                if (iPlayer.Armor[0] <= 0)
                {
                    iPlayer.SetClothes(9, 0, 0);
                    iPlayer.visibleArmor = false;
                    return;
                }
                else if (!iPlayer.visibleArmor)
                    iPlayer.SetClothes(9, 0, 0);
            }
        }
    }
}