using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players;

namespace GVRP.Module.Pet
{
    public class PetCommands : Script
    {
        [Command(GreedyArg = true)]
        public void getpet(Client player, string petName)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            foreach (var pet in PetModule.Instance.GetAll())
            {
                pet.Value.Name.ToLower().Contains(petName.ToLower());
                dbPlayer.LoadPet(pet.Value, dbPlayer.Player.Position, dbPlayer.Player.Heading, dbPlayer.Player.Dimension);
                break;
            }

        }

        [Command(GreedyArg = true)]
        public void followme(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            dbPlayer.SetPetFollow(dbPlayer.Player);
        }
    }
}
