using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using Newtonsoft.Json;
using System;
using GVRP.Module.Teams;

namespace GVRP.Module.News.App
{
    public class NewsListApp : SimpleApp
    {
        public NewsListApp() : base("NewsListApp")
        {
        }

        public class NewsFound
        {
            [JsonProperty(PropertyName = "id")] public uint Id { get; }
            [JsonProperty(PropertyName = "title")] public string Title { get; }
            [JsonProperty(PropertyName = "content")] public string Content { get; }
            [JsonProperty(PropertyName = "typeId")] public int TypeId { get; }

            public NewsFound(uint id, string title, string content, int typeId)
            {
                Id = id;
                Title = title;
                Content = content;
                TypeId = typeId;
            }
        }

        [RemoteEvent]
        public void requestNews(Client player)
        {
            SendNewsList(player);
        }

        [RemoteEvent]
        public void addNews(Client player, int newsType, string title, string content)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != 4) return;

            // Add News
            title = title.Replace("\"", "");
            content = content.Replace("\"", "");

            title = title.Replace("\"\"", "");
            content = content.Replace("\"\"", "");

            var l_NewsID = (uint)Main.newsList.Count + 1;
            Main.newsList.Add(new NewsFound(l_NewsID, title + " (" + (l_NewsID.ToString()) + ")", content, newsType));

            Main.newsList.Sort(delegate (NewsFound x, NewsFound y)
            {
                return y.Id.CompareTo(x.Id);
            });

            // Update NewsList
            this.SendNewsList(player);

            if (newsType == 0)
                Main.sendNotificationToPlayersWhoCanReceive("[NEWS] Es wurde ein Wetterbericht veroeffentlicht. Check die News App!", "Weazel News");
            else
                Main.sendNotificationToPlayersWhoCanReceive("[NEWS] Es wurde eine News veroeffentlicht. Check die News App!", "Weazel News");
        }

        private void SendNewsList(Client player)
        {
            TriggerEvent(player, "updateNews", NAPI.Util.ToJson(Main.newsList));
        }

        public void deleteNews(uint p_NewsID)
        {
            if (Main.newsList.Exists(n => n.Id == p_NewsID))
            {
                NewsFound l_News = Main.newsList.Find(n => n.Id == p_NewsID);
                Main.newsList.Remove(l_News);
            }
        }


        [RemoteEvent]
        public void removeNews(Client player, uint p_NewsID)
        {
            deleteNews(p_NewsID);
        }
    }
}