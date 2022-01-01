using GTANetworkAPI;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Attachments
{
    public class AttachmentModule : Module<AttachmentModule>
    {
        // Handle attachment
        public void HandleAttachment(DbPlayer dbPlayer, int uid, bool remove)
        {
            if (dbPlayer == null) return;

            int id = 0;

            if (dbPlayer.Attachments.Count == 0)
            {
                if (!remove)
                {
                    dbPlayer.Attachments.Add(dbPlayer.Attachments.Count + 1, uid);
                }
            }
            else
            {
                id = dbPlayer.Attachments.Where(st => st.Value == uid || st.Key != 0).FirstOrDefault().Key;

                if (remove)
                {
                    dbPlayer.Attachments.Remove(id);
                }
                else
                {
                    dbPlayer.Attachments.Add(dbPlayer.Attachments.Count + 1, uid);
                }
            }

            Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position).TriggerEvent("setAttachments", dbPlayer.Player, SerializeAttachments(dbPlayer));
            //dbPlayer.Player.SetSharedData("attachmentsData", SerializeAttachments(dbPlayer));
        }

        public void HandleVehicleAttachment(SxVehicle Vehicle, int uid, bool remove)
        {
            if (Vehicle == null || !Vehicle.IsValid()) return;

            int id = 0;

            if (Vehicle.Attachments.Count == 0)
            {
                if (!remove)
                {
                    Vehicle.Attachments.Add(Vehicle.Attachments.Count + 1, uid);
                }
            }
            else
            {
                id = Vehicle.Attachments.Where(st => st.Value == uid || st.Key != 0).FirstOrDefault().Key;

                if (remove)
                {
                    Vehicle.Attachments.Remove(id);
                }
                else
                {
                    Vehicle.Attachments.Add(Vehicle.Attachments.Count + 1, uid);
                }
            }

            Players.Players.Instance.GetPlayersInRange(Vehicle.entity.Position).TriggerEvent("setAttachments", Vehicle.entity, SerializeVehicleAttachments(Vehicle));
        }

        // Add attachment
        public void AddAttachment(DbPlayer dbPlayer, Attachment type)
        {
            HandleAttachment(dbPlayer, (int)type, false);
        }

        public void AddAttachmentVehicle(SxVehicle Vehicle, Attachment type)
        {
            HandleVehicleAttachment(Vehicle, (int)type, false);
        }

        public void RemoveVehicleAttachment(SxVehicle Vehicle, Attachment type)
        {
            HandleVehicleAttachment(Vehicle, (int)type, true);
        }
        
        // Remove attachment
        public void RemoveAttachment(DbPlayer dbPlayer, Attachment type)
        {
            HandleAttachment(dbPlayer, (int)type, true);
        }

        // Serialize attachments
        public string SerializeAttachments(DbPlayer dbPlayer)
        {
            return string.Join("|", dbPlayer.Attachments.Select(x => x.Value).ToArray());
        }

        public string SerializeVehicleAttachments(SxVehicle Vehicle)
        {
            return string.Join("|", Vehicle.Attachments.Select(x => x.Value).ToArray());
        }
    }

    public enum Attachment
    {
        BOX,
        BEER,
        TRASH,
        FISHINGROD,
        HANDY,
        DRILL,
        FORKLIFTBOX
    }
}