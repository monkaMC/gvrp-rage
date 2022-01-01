using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Items
{
    public enum ContainerTypes
    {
        PLAYER = 1,
        SHELTER = 2,
        HOUSE = 3,
        VEHICLE = 4,
        OBJECT = 5,
        FVEHICLE = 6,
        STATIC = 7,
        STORAGE = 8,
        LABOR_MEERTRAEUBEL = 9,
        FUELSTATION = 10,
        RAFFINERY = 11,
        NIGHTCLUB = 12,
        TeamOrder = 13,
        PRISONLOCKER = 14,
        REFUND = 15,
        HEAP = 16,
        TEAMFIGHT = 17,
        Copinv = 18,
        WEAPON_IMPORT = 19,
        METHLABORATORYINPUT = 21,
        METHLABORATORYOUTPUT = 22,
        METHLABORATORYFUEL = 23,
        BLACKMONEYINVENTORY = 24,
        BLACKMONEYCODES = 25,
        BLACKMONEYBATTERIE = 26,
        WORKSTATIONINPUT = 27,
        WORKSTATIONOUTPUT = 28,
        WORKSTATIONFUEL = 29,
        WEAPONLABORATORYINPUT = 30,
        WEAPONLABORATORYOUTPUT = 31,
        WEAPONLABORATORYFUEL = 32,
        CANNABISLABORATORYINPUT = 33,
        CANNABISLABORATORYOUTPUT = 34,
        CANNABISLABORATORYFUEL = 35,
        MINECONTAINERALU = 36,
        MINECONTAINERBRONCE = 37,
        MINECONTAINERIRON = 38,
        MINECONTAINERZINK = 39,
        MINECONTAINERSCHMELZE = 40,
        MINECONTAINERSCHMELZCOAL = 41,
        PLANNINGROOMWARDROBE = 42,
        MINEBASESTORAGE = 43,
    }

    public class ContainerData
    {
        public int DefaultSlots { get; set; }
        public int DefaultWeight { get; set; }
        public string DatabaseTable { get; set; }
        public string InventoryName { get; set; }

    }

    public static class ContainerMetaData
    {
        public static Dictionary<ContainerTypes, ContainerData> metaDataDic = new Dictionary<ContainerTypes, ContainerData>()
        {
            {ContainerTypes.PLAYER, new ContainerData() { DefaultSlots = 6, DefaultWeight = 25000, DatabaseTable = "container_player", InventoryName = "Rucksack" } },
            {ContainerTypes.SHELTER, new ContainerData() { DefaultSlots = 63, DefaultWeight = 10000000, DatabaseTable = "container_shelter", InventoryName = "Fraktionslager" } },
            {ContainerTypes.HOUSE, new ContainerData() { DefaultSlots = 24, DefaultWeight = 500000, DatabaseTable = "container_house", InventoryName = "Hauslager" } },
            {ContainerTypes.VEHICLE, new ContainerData() { DefaultSlots = 12, DefaultWeight = 100000, DatabaseTable = "container_vehicle", InventoryName = "Kofferraum" } },
            {ContainerTypes.OBJECT, new ContainerData() { DefaultSlots = 6, DefaultWeight = 25000, DatabaseTable = "container_object", InventoryName = "Lager" } },
            {ContainerTypes.FVEHICLE, new ContainerData() { DefaultSlots = 12, DefaultWeight = 100000, DatabaseTable = "container_fvehicle", InventoryName = "Kofferraum" } },
            {ContainerTypes.STATIC, new ContainerData() { DefaultSlots = 48, DefaultWeight = 100000, DatabaseTable = "container_static", InventoryName = "Lager" } },
            {ContainerTypes.STORAGE, new ContainerData() { DefaultSlots = 24, DefaultWeight = 50000000, DatabaseTable = "container_storage_rooms", InventoryName = "Lager" } },
            {ContainerTypes.LABOR_MEERTRAEUBEL, new ContainerData() { DefaultSlots = 24, DefaultWeight = 200000, DatabaseTable = "container_house_meertraeubel", InventoryName = "Trockenkammer" } },
            {ContainerTypes.FUELSTATION, new ContainerData() { DefaultSlots = 24, DefaultWeight = 2500000, DatabaseTable = "container_fuelstations", InventoryName = "Tankstelle" } },
            {ContainerTypes.RAFFINERY, new ContainerData() { DefaultSlots = 12, DefaultWeight = 1000000, DatabaseTable = "container_raffinery", InventoryName = "Oelfoerderpumpe" } },
            {ContainerTypes.NIGHTCLUB, new ContainerData() { DefaultSlots = 48, DefaultWeight = 2500000, DatabaseTable = "container_nightclub", InventoryName = "Nightclub" } },
            {ContainerTypes.TeamOrder, new ContainerData() { DefaultSlots = 48, DefaultWeight = 2500000, DatabaseTable = "container_teamorder", InventoryName = "Team-Bestellungen" } },
            {ContainerTypes.PRISONLOCKER, new ContainerData() { DefaultSlots = 5, DefaultWeight = 10000, DatabaseTable = "container_prisonlocker", InventoryName = "Spind" } },
            {ContainerTypes.Copinv, new ContainerData() { DefaultSlots = 16, DefaultWeight = 200000, DatabaseTable = "container_copinv", InventoryName = "Spind" } },
            {ContainerTypes.REFUND, new ContainerData() { DefaultSlots = 48, DefaultWeight = 2500000, DatabaseTable = "container_refund", InventoryName = "Erstattung" } },
            {ContainerTypes.HEAP, new ContainerData() { DefaultSlots = 48, DefaultWeight = 1000000, DatabaseTable = "container_heap", InventoryName = "Boden" } },
            {ContainerTypes.TEAMFIGHT, new ContainerData() { DefaultSlots = 48, DefaultWeight = 2500000, DatabaseTable = "container_teamfight", InventoryName = "Fraktionskampf" } },
            {ContainerTypes.WEAPON_IMPORT, new ContainerData() { DefaultSlots = 48, DefaultWeight = 1000000, DatabaseTable = "container_weapon_import", InventoryName = "Schmuggelware" } },
            {ContainerTypes.METHLABORATORYINPUT, new ContainerData() { DefaultSlots = 6, DefaultWeight = 27000, DatabaseTable = "container_methlaboratory_input", InventoryName = "Methlabor Rohstoffe" } },
            {ContainerTypes.METHLABORATORYOUTPUT, new ContainerData() { DefaultSlots = 4, DefaultWeight = 120000, DatabaseTable = "container_methlaboratory_output", InventoryName = "Methlabor Endprodukt" } },
            {ContainerTypes.METHLABORATORYFUEL, new ContainerData() { DefaultSlots = 6, DefaultWeight = 600000, DatabaseTable = "container_methlaboratory_fuel", InventoryName = "Methlabor Kraftstoff" } },
            {ContainerTypes.BLACKMONEYINVENTORY, new ContainerData() { DefaultSlots = 16, DefaultWeight = 600000, DatabaseTable = "container_blackmoney_inv", InventoryName = "Gelddruckmaschine" } },
            {ContainerTypes.BLACKMONEYCODES, new ContainerData() { DefaultSlots = 1, DefaultWeight = 10000, DatabaseTable = "container_blackmoney_codes", InventoryName = "Banknotencode Fach" } },
            {ContainerTypes.BLACKMONEYBATTERIE, new ContainerData() { DefaultSlots = 4, DefaultWeight = 100000, DatabaseTable = "container_blackmoney_batteries", InventoryName = "Notstromaggregat" } },
            {ContainerTypes.WEAPONLABORATORYFUEL, new ContainerData() { DefaultSlots = 6, DefaultWeight = 600000, DatabaseTable = "container_weaponlaboratory_fuel", InventoryName = "Waffenlabor Kraftstoff" } },
            {ContainerTypes.WEAPONLABORATORYINPUT, new ContainerData() { DefaultSlots = 6, DefaultWeight = 27000, DatabaseTable = "container_weaponlaboratory_input", InventoryName = "Waffenlabor Rohstoffe" } },
            {ContainerTypes.WEAPONLABORATORYOUTPUT, new ContainerData() { DefaultSlots = 4, DefaultWeight = 120000, DatabaseTable = "container_weaponlaboratory_output", InventoryName = "Waffenlabor Endprodukt" } },
            {ContainerTypes.WORKSTATIONINPUT, new ContainerData() { DefaultSlots = 16, DefaultWeight = 600000, DatabaseTable = "container_workstation_input", InventoryName = "Rohstoff" } },
            {ContainerTypes.WORKSTATIONOUTPUT, new ContainerData() { DefaultSlots = 16, DefaultWeight = 600000, DatabaseTable = "container_workstation_output", InventoryName = "Endprodukt" } },
            {ContainerTypes.WORKSTATIONFUEL, new ContainerData() { DefaultSlots = 4, DefaultWeight = 100000, DatabaseTable = "container_workstation_fuel", InventoryName = "Kraftstoff" } },
            {ContainerTypes.CANNABISLABORATORYFUEL, new ContainerData() { DefaultSlots = 6, DefaultWeight = 600000, DatabaseTable = "container_cannabislaboratory_fuel", InventoryName = "Cannabislabor Kraftstoff" } },
            {ContainerTypes.CANNABISLABORATORYINPUT, new ContainerData() { DefaultSlots = 6, DefaultWeight = 27000, DatabaseTable = "container_cannabislaboratory_input", InventoryName = "Cannabislabor Rohstoffe" } },
            {ContainerTypes.CANNABISLABORATORYOUTPUT, new ContainerData() { DefaultSlots = 4, DefaultWeight = 120000, DatabaseTable = "container_cannabislaboratory_output", InventoryName = "Cannabislabor Endprodukt" } },
            {ContainerTypes.MINECONTAINERALU, new ContainerData() { DefaultSlots = 48, DefaultWeight = 5000000, DatabaseTable = "container_mine_alu", InventoryName = "Aluminium-Erz" } },
            {ContainerTypes.MINECONTAINERBRONCE, new ContainerData() { DefaultSlots = 48, DefaultWeight = 5000000, DatabaseTable = "container_mine_bronce", InventoryName = "Bronze-Erz" } },
            {ContainerTypes.MINECONTAINERIRON, new ContainerData() { DefaultSlots = 48, DefaultWeight = 5000000, DatabaseTable = "container_mine_iron", InventoryName = "Eisen-Erz" } },
            {ContainerTypes.MINECONTAINERZINK, new ContainerData() { DefaultSlots = 48, DefaultWeight = 5000000, DatabaseTable = "container_mine_zink", InventoryName = "Zink-Erz" } },
            {ContainerTypes.MINECONTAINERSCHMELZCOAL, new ContainerData() { DefaultSlots = 16, DefaultWeight = 5000000, DatabaseTable = "container_mine_schmelzcoal", InventoryName = "Brennstoff" } },
            {ContainerTypes.MINECONTAINERSCHMELZE, new ContainerData() { DefaultSlots = 48, DefaultWeight = 5000000, DatabaseTable = "container_mine_schmelzofen", InventoryName = "Schmelzofen" } },
            {ContainerTypes.PLANNINGROOMWARDROBE, new ContainerData() { DefaultSlots = 12, DefaultWeight = 100000, DatabaseTable = "planningroom_wardrobe_container", InventoryName = "Umkleide" } },
            {ContainerTypes.MINEBASESTORAGE, new ContainerData() { DefaultSlots = 63, DefaultWeight = 90000000, DatabaseTable = "container_minestorage", InventoryName = "Ressourcen Lager" } },
        };
    }
}
