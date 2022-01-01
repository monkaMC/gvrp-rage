using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;

namespace GVRP.Module.Jailescape
{
    public class JailescapeModule : SqlModule<JailescapeModule, Jailtunnel, uint>
    {
        public static uint SchaufelId = 171;

        public static Vector3 PrisonZone = new Vector3(1681, 2604, 44);
        public static float minDistance = 200;
        public static float maxDistance = 500;

        public static JumpPoint jailTunnelEntrance = null;
        public static JumpPoint jailTunnelEscape = null;

        public DateTime LastTunnelDigged = DateTime.Now.AddMinutes(-46);

        protected override string GetQuery()
        {
            return "SELECT * FROM `jailtunnel`;";
        }

        public void SetLastDigged()
        {
            LastTunnelDigged = DateTime.Now;
        }

        public bool CanTunnelDigged()
        {
            if (LastTunnelDigged.AddMinutes(30) > DateTime.Now) return false; // nur alle 30 min ein Tunnel
            return true;
        }

        public bool IsInTunnelDiggingRange(DbPlayer dbPlayer)
        {
            if (dbPlayer.Player.Position.DistanceTo(PrisonZone) > minDistance && dbPlayer.Player.Position.DistanceTo(PrisonZone) < maxDistance) return true;
            return false;
        }

        public override void OnMinuteUpdate()
        {


            // Schließe Tunnel wieder...
            if (LastTunnelDigged.AddMinutes(2) < DateTime.Now && jailTunnelEntrance != null && jailTunnelEscape != null)
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
                {

                    if (JailescapeModule.jailTunnelEntrance != null)
                    {
                        JumpPointModule.Instance.jumpPoints.Remove(JailescapeModule.jailTunnelEntrance.Id);
                        if (JailescapeModule.jailTunnelEntrance.ColShape != null)
                        {
                            JailescapeModule.jailTunnelEntrance.ColShape?.Delete();
                        }
                        JailescapeModule.jailTunnelEntrance = null;
                    }

                    if (JailescapeModule.jailTunnelEscape != null)
                    {
                        JumpPointModule.Instance.jumpPoints.Remove(JailescapeModule.jailTunnelEscape.Id);
                        if (JailescapeModule.jailTunnelEscape.ColShape != null)
                        {
                            JailescapeModule.jailTunnelEscape.ColShape?.Delete();
                        }
                        JailescapeModule.jailTunnelEscape = null;
                    }
                }));
            }
        }
    }
}
