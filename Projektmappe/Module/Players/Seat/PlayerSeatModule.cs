using System;
using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Seat
{
    public class PlayerSeatModule : SqlModule<PlayerSeatModule, PlayerSeat, int>
    {
        // NetHandle, list of obfuscated places as indexes
        // false = free
        // true = occupied
        private readonly Dictionary<int, bool[]> occupiedSeats = new Dictionary<int, bool[]>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `player_seat`;";
        }

        protected override bool OnLoad()
        {
            occupiedSeats.Clear();
            return base.OnLoad();
        }

        [RemoteEvent]
        public void Interact(Client player, int model, Vector3 position, Vector3 rotation, NetHandle seatNetHandle)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (Configuration.Instance.DevMode)
            {
                player.SendNotification($"model {model}");
            }

            if (player.HasSharedData("SIT"))
            {
                CancelInteract(player);
            }
            else
            {
                var playerSeat = this[model];

                if (playerSeat == null || playerSeat.Places.Count == 0) return;

                var benchrot = rotation;

                var benchpos = position;

                var q = (double) benchrot.Z;

                q += 180; //Reset gta degree (-180 to 180)

                q = Math.PI * q / 180.0; // Convert to radian

                var seatNetHandleValue = seatNetHandle.Value;

                var freeIndex = -1;

                Vector3 destination = null;

                if (!occupiedSeats.TryGetValue(seatNetHandleValue, out var occupiedInts)
                ) // Noone sitted on this seat in the server period
                {
                    occupiedInts = new bool[playerSeat.Places.Count];
                    occupiedSeats[seatNetHandleValue] = occupiedInts;
                }

                var closestDistance = -1f;
                var heading = -1f;
                for (int i = 0, length = occupiedInts.Length; i < length; i++)
                {
                    if (occupiedInts[i]) continue;
                    var freeSeatPlace = playerSeat.Places[i];
                    if (freeSeatPlace == null) continue;
                    var x = freeSeatPlace.Offset.X;
                    var y = freeSeatPlace.Offset.Y;
                    var z = freeSeatPlace.Offset.Z;

                    var x2 = x * Math.Cos(q) - y * Math.Sin(q);
                    var y2 = x * Math.Sin(q) + y * Math.Cos(q);
                    var z2 = z;

                    var currrentDestination = new Vector3(benchpos.X + x2, benchpos.Y + y2, benchpos.Z + z2);
                    var distance = currrentDestination.DistanceTo(player.Position);
                    if (distance < closestDistance || closestDistance < 0)
                    {
                        closestDistance = distance;
                        freeIndex = i;
                        heading = freeSeatPlace.Heading;
                        destination = currrentDestination;
                    }
                }

                if (destination == null || freeIndex == -1) return; //No free seat

                if (destination.DistanceTo(player.Position) > 4f) return;
                
                occupiedInts[freeIndex] = true;
                
                //player.FreezePosition = true;
                
                player.Position = destination;
                benchrot.Z -= 180 + heading;
                player.Rotation = benchrot;

                dbPlayer.CurrentSeat = seatNetHandleValue;
                dbPlayer.CurrentSeatIndex = freeIndex;

                player.SetSharedData("SIT", true);
            }
        }

        [RemoteEvent]
        public void CancelInteract(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!player.HasSharedData("SIT")) return;
            //player.FreezePosition = false;

            RemovePlayerSeatReservation(dbPlayer);

            player.ResetSharedData("SIT");
        }

        private void RemovePlayerSeatReservation(DbPlayer dbPlayer)
        {
            if (dbPlayer.CurrentSeat != -1 && dbPlayer.CurrentSeatIndex != -1)
            {
                if (occupiedSeats.TryGetValue(dbPlayer.CurrentSeat, out var occupiedInts))
                {
                    if (occupiedInts.Length > dbPlayer.CurrentSeatIndex)
                    {
                        occupiedInts[dbPlayer.CurrentSeatIndex] = false;
                    }
                }
            }

            dbPlayer.CurrentSeat = -1;
            dbPlayer.CurrentSeatIndex = -1;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            RemovePlayerSeatReservation(dbPlayer);
        }

        public override bool OnPlayerDeathBefore(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            RemovePlayerSeatReservation(dbPlayer);
            return false;
        }
    }
}