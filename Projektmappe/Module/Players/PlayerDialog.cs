
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.GTAN;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public class Dialogs
    {
        public static uint menu_player = 4;
        public static uint menu_vehinventory = 5;
        public static uint menu_show_wanteds = 6;
        public static uint menu_Userskinchoose = 7;
        public static uint menu_fishing = 9;
        public static uint menu_account_licenses = 11;
        public static uint menu_givelicenses = 12;
        public static uint menu_fmembers = 13;
        public static uint menu_fmanager = 14;
        public static uint menu_invited = 15;
        public static uint menu_jobaccept = 16;
        public static uint menu_help = 18;
        public static uint menu_mdc = 19;
        public static uint menu_job_createlicenses = 20;
        public static uint menu_bankkredit = 21;
        public static uint menu_takelic = 22;
        public static uint menu_carshop = 23;
        public static uint menu_pilot = 24;
        public static uint menu_academic = 25;
        public static uint menu_quitjob = 26;
        public static uint menu_login = 27;
        public static uint menu_register = 28;
        public static uint menu_life = 29;
        public static uint menu_info = 30;

        public static uint menu_bus = 31;

        public static uint menu_shop_interior = 33;

        public static uint menu_shop_stores = 34;
        public static uint menu_pd_su = 36;
        public static uint menu_plain = 37;
        public static uint menu_playerfskinchoose = 38;
        public static uint menu_shop_gangstores = 39;
        public static uint menu_findrob = 40;
        public static uint menu_garden = 42;
        public static uint menu_trucker = 43;
        public static uint menu_trucker_gps = 44;
        public static uint menu_farmer = 46;
        public static uint menu_weapondealer = 47;
        public static uint menu_tuning_overlay = 48;
        public static uint menu_tuning_intern = 49;
        public static uint menu_fisher = 50;
        public static uint menu_taxilist = 51;
        public static uint menu_shop_mechanic = 52;
        public static uint menu_taxi = 53;
        public static uint menu_weapondealergps = 54;
        public static uint menu_shop_changecar = 55;
        public static uint menu_garage_overlay = 56;
        public static uint menu_garage_getlist = 57;
        public static uint menu_garage_setlist = 58;
        public static uint menu_shop_input = 59;
        public static uint menu_ressourcemap = 60;
        public static uint menu_keys_input = 62;
        public static uint menu_house_main = 64;
        public static uint menu_weapondealer_input = 65;
        public static uint menu_ad_input = 66;
        public static uint menu_givemoney_input = 68;
        public static uint menu_servicelist = 75;
        public static uint menu_house_keller = 76;
        public static uint menu_house_garage = 77;
        public static uint menu_bizbank = 78;
        public static uint menu_bizmanage = 79;
        public static uint menu_housekeys_input = 113;

        public static uint menu_wardrobe = 80;
        public static uint menu_wardrobe_selection = 81;
        public static uint menu_bizname = 83;
        public static uint menu_bizinvitename = 84;
        public static uint menu_bizacceptinvite = 85;
        public static uint menu_bizmembers = 86;
        public static uint menu_bizmembermanage = 87;
        public static uint menu_bizmembersalary = 88;


        public static uint menu_shop_clothes = 98;
        public static uint menu_shop_clothes_selection = 99;
        
        public static uint menu_shop_license = 101;
        public static uint menu_shop_ammunation = 102;
        public static uint menu_shop_rebel_weapons = 103;
        public static uint menu_shop_sotw = 104;
        public static uint menu_shop_assets = 105;
        public static uint menu_shop_ammunation_main = 106;
        public static uint menu_shop_ammunation_ammo = 107;

        public static uint window_inventory_single = 108;
        public static uint window_inventory_multi = 109;
        public static uint menu_adminObject = 110;
        public static uint menu_identity_card = 111;
        public static uint menu_garage = 112;
        public static uint menu_jailinhabits = 113;
        public static uint menu_dealerhint = 114;
    }
    
    public static class PlayerDialog
    {
        public static void CloseUserDialog(this DbPlayer dbPlayer, uint dialogid)
        {
            dbPlayer.watchDialog = 0;
            dbPlayer.Player.TriggerEvent("deleteDialog");
            //dbPlayer.Freeze(false);
            dbPlayer.Player.TriggerEvent("freezePlayer", false);

        }
        
        public static void CreateUserDialog(this Client player, uint dialogid, string template = "",
            int optional = 0, string optinal = "")
        {
            if (player == null) return;

            // Spezial bei Register
            if (dialogid == Dialogs.menu_register)
            {
                player.TriggerEvent("deleteDialog");
                player.TriggerEvent("createDialog", template, player.Name);
            }
            {
                DbPlayer iPlayer = player.GetPlayer();
                if (iPlayer == null) return;
                if (iPlayer.watchDialog > 0)
                {
                    iPlayer.CloseUserDialog(iPlayer.watchDialog);
                }

                if (!player.IsInVehicle)
                {
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    //iPlayer.Freeze(true);
                }

                iPlayer.watchDialog = dialogid;

                if (template == "login")
                {   
                    player.TriggerEvent("openDialog", template, "{\"name\": \"" + player.Name + "\"}");
                }
                else
                {
                    player.TriggerEvent("createDialog", template, "");
                }
            }
        }
    }
}