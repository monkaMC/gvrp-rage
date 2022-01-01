using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Kasino
{
    public class KasinoEvents : Script
    {
        [RemoteEvent]
        public void StartDiceGame(Client player, string returnString)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!dbPlayer.HasData("casino_dice"))
            {
                return;
            }

            if (int.TryParse(returnString, out int einsatz))
            {
                KasinoDice kasinoDice = dbPlayer.GetData("casino_dice");
                if (kasinoDice.IsInGame) return;

                if (einsatz < kasinoDice.MinPrice)
                {
                    dbPlayer.SendNewNotification($"Der Mindesteinsatz beträgt : {kasinoDice.MinPrice} $", PlayerNotification.NotificationType.CASINO, KasinoDiceModule.Instance.DiceString);
                    return;
                }
                else if (einsatz > kasinoDice.MaxPrice)
                {
                    dbPlayer.SendNewNotification($"Der Maximaleinsatz beträgt : {kasinoDice.MaxPrice} $", PlayerNotification.NotificationType.CASINO, KasinoDiceModule.Instance.DiceString);
                    return;
                }


                List<Client> surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPosition(Convert.ToDouble(kasinoDice.Radius), kasinoDice.Position);

                if (surroundingUsers.Count > 1)
                {
                    foreach (Client targetPlayer in surroundingUsers.ToList())
                    {
                        DbPlayer targetDbPlayer = targetPlayer.GetPlayer();
                        if (targetDbPlayer == null || !targetDbPlayer.IsValid() || (!targetDbPlayer.Rank.CanAccessFeature("casino") && !KasinoModule.Instance.CasinoGuests.Contains(targetDbPlayer)))
                        {
                            continue;
                        }

                        if (KasinoDiceModule.Instance.GetAll().Where(kd => kd.Value.Participant.Contains(targetDbPlayer)).Count() > 0) continue;

                        if (targetDbPlayer.money[0] >= einsatz)
                        {
                            ComponentManager.Get<ConfirmationWindow>().Show()(targetDbPlayer, new ConfirmationObject($"Würfelparty", $"Möchtest du die Würfelrunde mit Einsatz von {einsatz} $ annehmen?", "addCasinoDiceConfirm", dbPlayer.GetName(), kasinoDice.Id.ToString()));
                        }
                        else
                        {
                            surroundingUsers.Remove(targetPlayer);
                            targetDbPlayer.SendNewNotification("Du hast nicht genug Geld um mitspielen zu können.", PlayerNotification.NotificationType.CASINO, KasinoDiceModule.Instance.DiceString);
                        }
                    }
                }



                if (surroundingUsers.Count > 1)
                {
                    kasinoDice.StartTime = DateTime.Now;
                    kasinoDice.Price = einsatz;
                    kasinoDice.IsInGame = true;
                }
                else
                {
                    dbPlayer.SendNewNotification("Alleine kannst du nicht spielen.", PlayerNotification.NotificationType.CASINO, KasinoDiceModule.Instance.DiceString);
                }
            }

        }


        [RemoteEvent]
        public void addCasinoDiceConfirm(Client p_Player, string p_InvitingPersonName, string p_CasinoDeviceId)
        {
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            KasinoDice kasinoDice = KasinoDiceModule.Instance.Get(Convert.ToUInt32(p_CasinoDeviceId));
            if (kasinoDice == null || !kasinoDice.IsInGame)
            {
                return;
            }

            
            if (DateTime.Now >= kasinoDice.StartTime.AddSeconds(15))
            {
                dbPlayer.SendNewNotification("Diese Runde ist bereits abgelaufen.", PlayerNotification.NotificationType.CASINO, KasinoDiceModule.Instance.DiceString);
                return;
            }
            dbPlayer.TakeMoney(kasinoDice.Price);
            kasinoDice.Participant.Add(dbPlayer);
            foreach (var targetPlayer in kasinoDice.Participant.ToList())
            {
                targetPlayer.SendNewNotification($"{dbPlayer.Player.Name} ist nun auch dabei!", PlayerNotification.NotificationType.CASINO, KasinoDiceModule.Instance.DiceString);
            }
        }




    }
}
