using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerVoice
    {
        public static bool CanUseMegaphone(this DbPlayer iPlayer)
        {
            return iPlayer.IsACop() || 
                   iPlayer.TeamId == (int) teams.TEAM_MEDIC ||
                   iPlayer.TeamId == (int) teams.TEAM_DPOS || 
                   iPlayer.TeamId == (int) teams.TEAM_NEWS ||
                   iPlayer.TeamId == (int) teams.TEAM_GOV ||
                   iPlayer.TeamId == (int) teams.TEAM_DRIVINGSCHOOL;
        }

        //Todo: needs externalized player module
        public static void RadioToggle(this DbPlayer iPlayer)
        {
            /*if (!iPlayer.HasData("RADIO_LS"))
            {
                if (iPlayer.player.VehicleSeat == -1)
                {
                    for (int index = 0; index < Players.Count; index++)
                    {
                        if (!validatePlayer(index)) continue;
                        if (API.isPlayerInAnyVehicle(Players[index].player) &&
                            iPlayer.player.Vehicle == Players[index].player.Vehicle)
                        {
                            Players[index].SetData("RADIO_LS", 1);
                            VoiceList.setPlayerRadio(Players[index], 0);
                            API.sendNotificationToPlayer(Players[index].player,
                                "Sie hören ~y~Radio Los Santos");
                        }
                    }
                    return;
                }
                else
                {
                    iPlayer.SendNewNotification(
                        msgServerInfo + "Für diese Aktion müssen Sie Fahrer des Fahrzeuges sein!");
                    return;
                }
            }
            else
            {
                if (API.getPlayerVehicleSeat(iPlayer.player) == -1)
                {
                    for (int index = 0; index < Players.Count; index++)
                    {
                        if (!validatePlayer(index)) continue;
                        if (API.isPlayerInAnyVehicle(Players[index].player) &&
                            iPlayer.player.Vehicle == Players[index].player.Vehicle)
                        {
                            API.resetEntityData(Players[index].player, "RADIO_LS");
                            VoiceList.setPlayerRadio(Players[index], 0);
                            API.sendNotificationToPlayer(Players[index].player,
                                "Radio ausgeschaltet");
                        }
                    }
                    return;
                }
                else
                {
                    iPlayer.SendNewNotification(
                        msgServerInfo + "Für diese Aktion müssen Sie Fahrer des Fahrzeuges sein!");
                    return;
                }
            }*/
        }
    }
}