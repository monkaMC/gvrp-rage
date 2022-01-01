

/*
 /*var client = new HttpClient();
            var result = client.GetAsync("https://pastebin.com/raw/pwkh0uRP").Result;
            if (result.IsSuccessStatusCode)
            {
                var file = System.Text.Encoding.UTF8.GetString(result.Content.ReadAsByteArrayAsync().Result);
                var lines = file.Split('\n');
                foreach (var line in lines)
                {
                    MySQLHandler.ExecuteAsync($"INSERT INTO `item_placement_files` (hash, name, active) VALUES('{line}', '{line}', '{0}')");
                }
            }
            */

using GTANetworkAPI;

namespace GVRP.Module.ItemPlacementFiles
{
    public class ItemPlacementFilesModule : SqlModule<ItemPlacementFilesModule, ItemPlacementFile, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `item_placement_files` ORDER BY `active`;";
        }

        protected override bool OnLoad()
        {
            NAPI.World.ResetIplList();
            return base.OnLoad();
        }

        protected override void OnItemLoaded(ItemPlacementFile itemPlacementFile)
        {
            if (itemPlacementFile.Active)
            {
                NAPI.World.RequestIpl(itemPlacementFile.Hash);
            }
            else
            {
                NAPI.World.RemoveIpl(itemPlacementFile.Hash);
            }
        }
    }
}