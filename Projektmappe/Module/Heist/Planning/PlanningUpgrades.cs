using System.Collections.Generic;

namespace GVRP.Module.Heist.Planning
{
    public class Upgrade
    {
        public uint Id { get; }
        public string Name { get; }
        public List<string> UpgradeNames { get; }
        
        public Upgrade(uint id, string name, List<string> upgradeNames)
        {
            Id = id;
            Name = name;
            UpgradeNames = upgradeNames;
        }
    }
    public static class PlanningUpgrades
    {
        public static Dictionary<int, Upgrade> Upgrades = new Dictionary<int, Upgrade>()
        {
            {1, new Upgrade(1, "Grundraum", new List<string> {"Moderne Optik", "Holzoptik"})},
            {2, new Upgrade(2, "Spiegel", new List<string> {"Deckenspiegel"})},
            {3, new Upgrade(3, "Einrichtungsstyle",  new List<string> {"Censored", "Nyan Bike", "Rainbow Anime", "Degenatron", "Arcade", "Neofuturismus", "Arcade 2", "Adventure", })},
            {4, new Upgrade(4, "Inneneinrichtung", new List<string> {"Multimedia"})},
            {5, new Upgrade(5, "Spielautomaten", new List<string>{"Spielautomaten"})}
        };
    }
}
