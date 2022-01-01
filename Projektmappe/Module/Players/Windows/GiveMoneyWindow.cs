using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.Players.Db;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.RemoteEvents;

namespace GVRP.Module.Players.Windows
{

    public enum TransferType
    {
        BAR=0,
        ÜBERWEISUNG=1
    }


    public class GiveMoneyWindow : Window<Func<DbPlayer, DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "name")] private string DestinationName { get; }

            public ShowEvent(DbPlayer dbPlayer, DbPlayer destinationPlayer) : base(dbPlayer)
            {
                DestinationName = destinationPlayer.GetName();
            }
        }

        public GiveMoneyWindow() : base("GiveMoney")
        {
        }

        public override Func<DbPlayer, DbPlayer, bool> Show()
        {
            return (player, destinationPlayer) => OnShow(new ShowEvent(player, destinationPlayer));
        }

        [RemoteEvent]
        public void GivePlayerMoney(Client player, string destinationPlayerName, int money)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;
                if (!dbPlayer.CanAccessRemoteEvent()) return;

                if (money <= 0)
                {
                    dbPlayer.SendNewNotification("Der Betrag ist ungueltig!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var destinationDbPlayer = Players.Instance.FindPlayer(destinationPlayerName);
                if (destinationDbPlayer == null) return;
                if (!destinationDbPlayer.IsValid()) return;
                if (destinationDbPlayer.Id == dbPlayer.Id) return;
                if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20f) return;

                if (dbPlayer.TakeMoney(money))
                {
                    if (!destinationDbPlayer.GiveMoney(money))
                    {
                        dbPlayer.GiveMoney(money);
                        dbPlayer.SendNewNotification($"Das Geld konnte nicht uebergeben werden! Du hast {money} zurueckbekommen.", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }
                    dbPlayer.SendNewNotification($"Du hast {money}$ uebergeben!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    destinationDbPlayer.SendNewNotification($"Dir wurden {money}$ zugesteckt!", notificationType: PlayerNotification.NotificationType.SUCCESS);


                    SaveToPayLog(dbPlayer.Id.ToString(), destinationDbPlayer.Id.ToString(), money, TransferType.BAR);

                    return;
                }
                dbPlayer.SendNewNotification($"Leider konnten wir dir das Geld nicht abziehen. Vorgang abgebrochen.", notificationType: PlayerNotification.NotificationType.ADMIN);
            }));
        }

        public static void SaveToPayLog(string u1, string u2, int value, TransferType transferType)
        {
            u1 = u1 ?? "undefined";
            u2 = u2 ?? "undefined";
            var query = $"INSERT INTO `paylog` (`s1`,`s2`, `amount`, `type`) VALUES ('{u1}', '{u2}','{value}','{(int)transferType}');";
            MySQLHandler.ExecuteAsync(query);
        }

    }
}