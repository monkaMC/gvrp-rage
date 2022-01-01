using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerChat
    {
        public static void SendChatMessage(this DbPlayer iPlayer, string msg, bool ignorelogin = false)
        {
            if (!iPlayer.IsValid(ignorelogin)) return;

            iPlayer.SendNewNotification(msg);
        }
        
        public static void ClearChat(this DbPlayer iPlayer)
        {
            for (int i = 0; i < 10; i++)
            {
                iPlayer.SendNewNotification("");
                i++;
            }
        }
    }
}