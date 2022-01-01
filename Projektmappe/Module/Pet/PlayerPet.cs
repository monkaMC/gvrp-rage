using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Pet
{
    public class PlayerPet
    {
        public PetData PetData { get; }
        public Ped Entity { get; }
        public Entity followAction { get; set; }

        public PlayerPet(PetData pet, Ped entity)
        {
            PetData = pet;
            Entity = entity;
            followAction = null;
        }
    }

    public static class PlayerPetHelper
    {
        public static bool ExistPet(this DbPlayer dbPlayer)
        {
            return dbPlayer.PlayerPet != null && dbPlayer.PlayerPet.Entity != null;
        }

        public static void LoadPet(this DbPlayer dbPlayer, PetData pet, Vector3 spawnPos, float heading = 0.0f, uint dimension = 0)
        {
            dbPlayer.RemovePet();
            dbPlayer.PlayerPet = new PlayerPet(pet, NAPI.Ped.CreatePed(pet.Model, spawnPos, heading, dimension));
        }

        public static void RemovePet(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.ExistPet()) return;
            dbPlayer.PlayerPet.Entity.Delete();
            dbPlayer.PlayerPet = null;
        }

        public static void SetPetFollow(this DbPlayer dbPlayer, Entity entity)
        {
            if (!dbPlayer.ExistPet()) return;
            dbPlayer.PlayerPet.followAction = entity;
        }

        public static void SetPetUnfollow(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.ExistPet()) return;
            dbPlayer.PlayerPet.followAction = null;
        }

        public static bool IsPetFollowing(this DbPlayer dbPlayer)
        {
            return (dbPlayer.ExistPet() && dbPlayer.PlayerPet.followAction != null && NAPI.Entity.DoesEntityExist(dbPlayer.PlayerPet.followAction));
        }
    }
}
