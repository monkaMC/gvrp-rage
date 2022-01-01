using System;

namespace GVRP.Module.Players.Seat
{
    public class PlayerSeatPlaceModule : SqlBaseModule<PlayerSeatPlaceModule, PlayerSeat.Place>
    {
        public override Type[] RequiredModules()
        {
            return new[] {typeof(PlayerSeatModule)};
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `player_seat_place`;";
        }

        protected override void OnItemLoaded(PlayerSeat.Place place)
        {
            var playerSeat = PlayerSeatModule.Instance[place.Hash];
            playerSeat.Places.Add(place);
        }
    }
}