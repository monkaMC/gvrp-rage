using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Keys;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Handler
{
    public class HouseKeyHandler
    {
        public static HouseKeyHandler Instance { get; } = new HouseKeyHandler();

        private HouseKeyHandler()
        {

        }

        public void AddHouseKey(DbPlayer iPlayer, House house)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                if (iPlayer.HouseKeys.Contains(house.Id)) return;
                iPlayer.HouseKeys.Add(house.Id);
                MySQLHandler.ExecuteAsync($"INSERT INTO `house_keys` (`player_id`, `house_id`) VALUES ('{iPlayer.Id}', '{house.Id}');");
            }));
        }

        public void DeleteHouseKey(DbPlayer iPlayer, House house)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                if (iPlayer == null || !iPlayer.IsValid() || !iPlayer.HouseKeys.Contains(house.Id)) return;
                iPlayer.HouseKeys.Remove(house.Id);
                MySQLHandler.ExecuteAsync($"DELETE FROM `house_keys` WHERE `house_id` = '{house.Id}' AND `player_id` = '{iPlayer.Id}';");
            }));
        }

        public void DeleteAllHouseKeys(House house)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (var iPlayer in Players.Instance.GetValidPlayers())
                {
                    if (iPlayer?.HouseKeys == null) continue;
                    if (iPlayer.HouseKeys.Contains(house.Id))
                    {
                        iPlayer.HouseKeys.Remove(house.Id);
                    }
                }
                MySQLHandler.ExecuteAsync($"DELETE FROM `house_keys` WHERE `house_id` = '{house.Id}';");
            }));
        }

        public async Task LoadHouseKeys(DbPlayer iPlayer)
        {
            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                await keyConn.OpenAsync();
                keyCmd.CommandText =
                    $"SELECT house_id FROM `house_keys` WHERE player_id = '{iPlayer.Id}';";
                using (var keyReader = keyCmd.ExecuteReader())
                {
                    if (keyReader.HasRows)
                    {
                        while (keyReader.Read())
                        {
                            var keyId = (uint)keyReader.GetInt32(0);
                            if (!iPlayer.HouseKeys.Contains(keyId))
                            {
                                iPlayer.HouseKeys.Add(keyId);
                            }
                        }
                    }
                }
                await keyConn.CloseAsync();
            }
        }

        public List<VHKey> GetAllKeysPlayerHas(DbPlayer iPlayer)
        {
            List<VHKey> houses = new List<VHKey>();
            foreach (uint house in iPlayer.HouseKeys.ToList())
            {
                houses.Add(new VHKey("" + house, house));
            }

            if (iPlayer.ownHouse[0] != 0) houses.Add(new VHKey("" + iPlayer.ownHouse[0], iPlayer.ownHouse[0]));

            return houses;
        }

        public List<VHKey> GetOwnHouseKey(DbPlayer iPlayer)
        {
            List<VHKey> houses = new List<VHKey>();
            if (iPlayer.ownHouse[0] != 0) houses.Add(new VHKey("" + iPlayer.ownHouse[0], iPlayer.ownHouse[0]));
            return houses;
        }

    }
}