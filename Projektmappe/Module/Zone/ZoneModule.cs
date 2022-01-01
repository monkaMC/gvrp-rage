using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GVRP.Module.Zone
{

    public class ZoneModule : SqlModule<ZoneModule, Zone, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `zones`;";
        }
        //public static Vector3 wCpPoint = new Vector3(-1900, 4618, 57);
        //public static Vector3 mCpPoint = new Vector3(-190, 4229, 45);
        //public static Vector3 eCpPoint = new Vector3(2677, 4837, 44);

        //public Blip wCpBlip = Spawners.Blips.Create(wCpPoint, "West-CP", 238, 1.0f, true, 1, 255);
        //public Blip mCpBlip = Spawners.Blips.Create(mCpPoint, "Mid-CP", 238, 1.0f, true, 1, 255);
        //public Blip eCpBlip = Spawners.Blips.Create(eCpPoint, "East-CP", 238, 1.0f, true, 1, 255);

        //public bool wCpStatus = false;
        //public bool mCpStatus = false;
        //public bool eCpStatus = false;


        //public override bool Load(bool reload = false)
        //{
        //    MenuManager.Instance.AddBuilder(new ZoneCheckpointMenuBuilder());
        //    return base.Load(reload);
        //}

        public Zone GetZone(Vector3 position)
        {
            return ZoneModule.Instance.GetAll().FirstOrDefault(zone => zone.Value.IsPositionInside(position)).Value;
        }

        public bool IsInNorthZone(Vector3 vector3)
        {
            return (vector3.Y > 4230);
        }

        public bool IsInSouthZone(Vector3 vector3)
        {
            return !IsInNorthZone(vector3);
        }

        //public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        //{
        //    if (dbPlayer.Player.IsInVehicle) return false;
        //    if (key == Key.E)
        //    {
        //        if (!dbPlayer.IsACop() || dbPlayer.TeamRank < 6 || !dbPlayer.IsInDuty())
        //        {
        //            return false;
        //        }
        //        if (dbPlayer.Player.Position.DistanceTo(wCpPoint) < 5.0f || dbPlayer.Player.Position.DistanceTo(mCpPoint) < 5.0f || dbPlayer.Player.Position.DistanceTo(eCpPoint) < 5.0f)
        //        {
        //            MenuManager.Instance.Build(PlayerMenu.ZoneCPMenu, dbPlayer).Show(dbPlayer);
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public async Task OpenCheckpoint(Vector3 Position, bool open)
        //{
        //    if(Position.DistanceTo(wCpPoint) < 5.0f)
        //    {
        //        if(!open)
        //        {
        //            // Close
        //            await Chats.SendGlobalMessage($"Grenznachricht: Der West-Highway Checkpoint ist nun geschlossen!", COLOR.LIGHTBLUE, ICON.GOV);
        //            wCpBlip.Color = 1;
        //        }
        //        else
        //        {
        //            // Open
        //            await Chats.SendGlobalMessage($"Grenznachricht: Der West-Highway Checkpoint ist nun geoeffnet!", COLOR.LIGHTBLUE, ICON.GOV);
        //            wCpBlip.Color = 2;
        //        }
        //        wCpStatus = open;
        //    }
        //    else if (Position.DistanceTo(mCpPoint) < 5.0f)
        //    {
        //        if (!open)
        //        {
        //            // Close
        //            await Chats.SendGlobalMessage($"Grenznachricht: Der Mid-Highway Checkpoint ist nun geschlossen!", COLOR.LIGHTBLUE, ICON.GOV);
        //            mCpBlip.Color = 1;
        //        }
        //        else
        //        {
        //            // Open
        //            await Chats.SendGlobalMessage($"Grenznachricht: Der Mid-Highway Checkpoint ist nun geoeffnet!", COLOR.LIGHTBLUE, ICON.GOV);
        //            mCpBlip.Color = 2;
        //        }
        //        mCpStatus = open;

        //    }
        //    else if (Position.DistanceTo(eCpPoint) < 5.0f)
        //    {
        //        if (!open)
        //        {
        //            // Close
        //            await Chats.SendGlobalMessage($"Grenznachricht: Der East-Highway Checkpoint ist nun geschlossen!", COLOR.LIGHTBLUE, ICON.GOV);
        //            eCpBlip.Color = 1;
        //        }
        //        else
        //        {
        //            // Open
        //            await Chats.SendGlobalMessage($"Grenznachricht: Der East-Highway Checkpoint ist nun geoeffnet!", COLOR.LIGHTBLUE, ICON.GOV);
        //            eCpBlip.Color = 2;
        //        }
        //        eCpStatus = open;
        //    }
        //}
    }
}
