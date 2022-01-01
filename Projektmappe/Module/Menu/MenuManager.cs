using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu
{
    public enum PlayerMenu : uint
    {
        GpsVehicles = 150,
        Account = 151,
        AccountLicense = 152,
        AccountVehicleKeys = 165,
        Gps = 153,
        GpsJobs = 154,
        Garage = 155,
        Keys = 156,
        GpsPublicPlaces = 157,
        TeamWardrobe = 158,
        TeamWardrobeClothes = 159,
        TeamWardrobeSkins = 160,
        TeamWardrobeClothesSelection = 161,
        TeamWardrobeProps = 162,
        TeamWardrobePropsSelection = 163,
        GpsGarage = 164,
        GangwarInfo = 166,
        BusinessBank = 167,
        BusinessEnter = 168,
        BusinessSafe = 169,
        BusinessManage = 170,
        BusinessManageMembers = 174,
        BusinessManageMember = 175,
        Armory = 171,
        ArmoryWeapons = 172,
        ArmoryItems = 173,
        MechanicTune = 176,
        AccountKeyChooseMenu = 177,
        AccountHouseKeys = 178,
        ItemOrderMenu = 179,
        ItemOrderOrdersMenu = 180,
        ItemOrderItemsMenu = 181,
        TattooLicenseMenu = 182,
        TattooBuyMenu = 183,
        CustomizationMenu = 184,
        TattooLaseringMenu = 185,
        FreiberufMowerMenu = 186,
        TattooBankMenu = 187,
        AnimationMenuOv = 188,
        AnimationMenuIn = 189,
        Government = 202,
        StorageMenu = 203,
        GangwarDealer = 204,
        FuelStationMenu = 205,
        RaffineryMenu = 206,
        FarmProcessMenu = 207,
        FuelStationFillMenu = 208,
        CocainDeliverMenu = 209, // Not used
        NightClubMenu = 210,
        NightClubManageMenu = 211,
        NightClubPriceMenu = 212,
        CrimeJailMenu = 213,
        ZoneCPMenu = 214,
        EinreiseAmtMenu = 215,
        CrimeArrestMenu = 216,
        AnimationShortCutSlotMenu = 217,
        AnimationShortCutMenu = 218,
        WarehouseBuyMenu = 219,
        WarehouseSellMenu = 220,
        WarehouseMenu = 221,
        AmmoArmorieMenu = 222,
        AmmoPackageOrderMenu = 223,
        AmmoArmoriePriceMenu = 224,
        VehicleRentMenu = 225,
        NSAComputerMenu = 226,
        NSAVehicleModifyMenu = 229,
        NSAVehicleListMenu = 230,
        NSACallListMenu = 231,
        NSAObservationsSubMenu = 232,
        NSABankMenu = 233,
        NSAObservationsList = 234,
        OutfitsMenu = 235,
        OutfitsSubMenu = 236,
        FIBPhoneHistoryMenu = 237,
        DealerSellMenu = 238,
        DeliveryJobMenu = 239,
        FreiberufGarbageMenu = 240,
        StadtHalleMenu = 241,
        ArmoryAmmo = 242,
        ShelterMenu = 243,
        ShelterFightMenu = 244,
        ExchangeMenu = 245,
        ExchangeVehicleMenu = 246,
        GangwarVehicleMenu = 247,
        LSCVehicleListMenu = 248,
        LSCPaintMenu = 249,
        LSCTuningMenu = 250,
        NSAPeilsenderMenu = 251,
        MobileClothMenu = 252,
        ArmoryArmorMenu = 253,
        WeaponImportMainMenu = 254,
        WeaponImportOrdersMenu = 255,
        WeaponImportNewOrder = 256,
        WeaponImportOrderDetailsMenu = 257,
        LaboratoryWeaponMenu = 258,
        ItemExportsMenu = 259,
        LaboratoryOpenInvMenu = 260,
        AsservatenkammerMenu = 261,
        AsservatenkammerDeliverMenu = 262,
        RacingEnterMenu = 263,
        GuentherAusgangMenu = 264,
        BlacklistMenu = 265,
        NSAKeyMenu = 266,
        BlacklistTypeMenu = 267,
        NSATransactionHistory = 268,
        NSAEnergyHistory = 269,
        VoltageMenu = 270,
        NSADoorUsedsMenu = 271,
        Altkleider = 272,
        HouseRentContract = 273,
        HackingVoltageMenu = 274,
        LaborArmorMenu = 275,
        PlanningroomPurchaseMenu = 276,	
        PlanningroomUpgradeMenu = 277,	
        PlanningroomUpgradeSelectionMenu = 278,	
        PlanningroomKellerUpgradeMenu = 279,	
        ExchangeElectronicMenu = 280,	
        ExchangeTrashMenu = 281,
        PlanningroomVehicleModifyMenu = 282,
        PlanningroomVehicleTuningMenu = 283,
        PlanningroomHeistMenu = 284,
        PlanningroomPreQuestMenu = 285,
        PlanningroomPreQuestSelectionMenu = 286,
        PlanningroomCraftingMenu = 287,
        NightClubAnpassung = 288,
        NightClubAnpassungHandler = 289,
        CarsellBuyMenu = 290,
        CarsellMenu = 291,
        CarsellDeleteMenu = 292,
        CarsellCustomerMenu = 293,
        CarsellTuneWheelMenu = 294,
        CarsellBuySubMenu = 295,
        CarsellDeliverCustomerMenu = 296,
    }

    public class MenuManager
    {
        private readonly Dictionary<PlayerMenu, IMenuEventHandler> menuEventHandlers;

        private readonly Dictionary<PlayerMenu, MenuBuilder> menuBuilders;

        public static MenuManager Instance { get; } = new MenuManager();

        public MenuManager()
        {
            menuEventHandlers = new Dictionary<PlayerMenu, IMenuEventHandler>();
            menuBuilders = new Dictionary<PlayerMenu, MenuBuilder>();
        }

        public void AddBuilder(MenuBuilder menuBuilder)
        {
            if (menuBuilders.ContainsKey(menuBuilder.Menu)) Logging.Logger.Debug("CRASHBUG: " + menuBuilder.Menu.ToString());

            menuBuilders.Add(menuBuilder.Menu, menuBuilder);
            var eventHandler = menuBuilder.GetEventHandler();
            if (eventHandler != null)
            {
                menuEventHandlers.Add(menuBuilder.Menu, eventHandler);
            }
        }

        public Menu Build(PlayerMenu menu, DbPlayer iPlayer)
        {
            return menuBuilders.TryGetValue(menu, out var value) ? value.Build(iPlayer) : null;
        }

        public void OnSelect(PlayerMenu menu, int index, DbPlayer iPlayer)
        {
            if (menuEventHandlers.ContainsKey(menu))
            {
                if (menuEventHandlers[menu].OnSelect(index, iPlayer))
                {
                    DismissMenu(iPlayer.Player, (uint) menu);
                }
            }
        }

        public static void DismissCurrent(DbPlayer iPlayer)
        {
            iPlayer.Player.TriggerEvent("componentServerEvent", "NativeMenu", "hide");
        }

        public static void DismissMenu(Client player, uint menuId)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null) return;
            DismissCurrent(iPlayer);
            iPlayer.WatchMenu = 0;
        }
    }
}