
using GTANetworkAPI;
using System;
using System.Threading;
using System.Threading.Tasks;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Players
{
    //Todo: maybe make vehicle check here as well instead of parameter
    public static class PlayerState
    {
        private const string CuffedMedic = "CuffedMedic";
        private const string AdminDutyEvent = "updateAduty";
        private const string CuffedEvent = "updateCuffed";
        private const string TiedEvent = "upadeTied";
        private const string DutyEvent = "updateDuty";

        public static void SetCuffed(this DbPlayer iPlayer, bool cuffed, bool inVehicle = false)
        {
            if (iPlayer.HasData("SMGkilledPos"))
            {
                iPlayer.SetStunned(false);
            }
            if (cuffed)
            {
                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                iPlayer.Player.TriggerEvent("freezePlayer", true);

                Voice.VoiceModule.Instance.turnOffFunk(iPlayer);
            }
            else
            {
                iPlayer.StopAnimation();
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                if (iPlayer.HasData("follow"))
                    iPlayer.ResetData("follow");
            }

            if (cuffed)
            {
                iPlayer.SetData("vehicleCuffed", true);
            }
            else
            {
                if (iPlayer.HasData("vehicleCuffed"))
                {
                    iPlayer.ResetData("vehicleCuffed");
                }
            }

            iPlayer.IsCuffed = cuffed;
            iPlayer.Player.TriggerEvent(CuffedEvent, cuffed);
        }

        public static void SetStunned(this DbPlayer iPlayer, bool stunned)
        {
            if (stunned)
            {
                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor3_beatup", "guard_beatup_kickidle_dockworker");
                iPlayer.Player.TriggerEvent("freezePlayer", true);

                Voice.VoiceModule.Instance.turnOffFunk(iPlayer);
                iPlayer.IsCuffed = stunned;
                iPlayer.Player.TriggerEvent(CuffedEvent, stunned);
            }
            else if (iPlayer.HasData("SMGkilledPos"))
            {
                iPlayer.StopAnimation();
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                iPlayer.IsCuffed = stunned;
                iPlayer.Player.TriggerEvent(CuffedEvent, stunned);
                iPlayer.ResetData("SMGkilledPos");
            }
        }

        public static void SetMedicCuffed(this DbPlayer iPlayer, bool cuffed, bool inVehicle = false)
        {
            if (cuffed)
            {
                iPlayer.SetData(CuffedMedic, true);
                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                //iPlayer.Player.Freeze(true);
            }
            else
            {
                iPlayer.ResetData(CuffedMedic);
                iPlayer.StopAnimation();
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                //iPlayer.Player.Freeze(false);
            }
        }

        public static bool IsMedicCuffed(this Client player)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return false;

            return iPlayer.HasData(CuffedMedic);
        }

        public static void SetTied(this DbPlayer iPlayer, bool tied, bool inVehicle = false)
        {
            if (tied)
            {
                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                //iPlayer.Player.Freeze(true);
                Voice.VoiceModule.Instance.turnOffFunk(iPlayer);
                Voice.VoiceModule.Instance.turnOffFunk(iPlayer);

                // Cancel Phonecall
                iPlayer.Player.TriggerEvent("hangupCall");
                iPlayer.Player.TriggerEvent("cancelPhoneCall");
                iPlayer.ResetData("current_caller");
            }
            else
            {
                iPlayer.StopAnimation();
                //To make sure player can move
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                //iPlayer.Player.Freeze(false);
            }

            iPlayer.IsTied = tied;
            iPlayer.Player.TriggerEvent(TiedEvent, tied);
        }

        public static void SetDuty(this DbPlayer iPlayer, bool duty)
        {
            if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return;
            iPlayer.Duty = duty;
            iPlayer.Player.TriggerEvent(DutyEvent, duty);
        }

        public static bool IsInDuty(this DbPlayer iPlayer)
        {
            return iPlayer.Duty;
        }

        public static void SetNames(this DbPlayer dbPlayer, bool names)
        {
            dbPlayer.CanSeeNames = names;
            dbPlayer.Player.TriggerEvent("setPlayerNametags", names);
        }

        public static void SetAdminDuty(this DbPlayer dbPlayer, bool aduty)
        {
            if (aduty)
            {
                dbPlayer.SetData("armorbefore", dbPlayer.Player.Armor);
            }
            else
            {
                if (dbPlayer.HasData("armorbefore"))
                {
                    dbPlayer.Player.Armor = dbPlayer.GetData("armorbefore");
                    dbPlayer.ResetData("armorbefore");
                }
            }
            dbPlayer.RankDuty = aduty ? DbPlayer.RankDutyStatus.AdminDuty : DbPlayer.RankDutyStatus.OffDuty;
            // dbPlayer.Player.TriggerEvent(AdminDutyEvent, aduty);
            dbPlayer.Player.TriggerEvent("setPlayerAduty", aduty);
            dbPlayer.Player.TriggerEvent("updateAduty", aduty);
            dbPlayer.ApplyArmorVisibility();
        }

        public static bool IsInAdminDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.AdminDuty;
        }

        public static void SetCasinoDuty(this DbPlayer dbPlayer, bool state)
        {
            dbPlayer.RankDuty = state ? DbPlayer.RankDutyStatus.CasinoDuty : DbPlayer.RankDutyStatus.OffDuty;
            dbPlayer.Player.TriggerEvent("setPlayerCduty", state);
        }

        public static bool IsInCasinoDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.CasinoDuty;
        }

        public static void SetGuideDuty(this DbPlayer dbPlayer, bool state)
        {
            dbPlayer.RankDuty = state ? DbPlayer.RankDutyStatus.GuideDuty : DbPlayer.RankDutyStatus.OffDuty;

            if (state)
            {
                dbPlayer.SetData("armorbefore", dbPlayer.Player.Armor);

                dbPlayer.Player.SetSkin(dbPlayer.Customization.Gender == 0 ? PedHash.FilmDirector : PedHash.ShopMidSFY);
            }
            else
            {
                if (dbPlayer.HasData("armorbefore"))
                {
                    dbPlayer.Player.Armor = dbPlayer.GetData("armorbefore");
                    dbPlayer.ResetData("armorbefore");
                }

                dbPlayer.ApplyCharacter();
                dbPlayer.ApplyArmorVisibility();
            }
        }

        public static void SetGameDesignDuty(this DbPlayer dbPlayer, bool state)
        {
            dbPlayer.RankDuty = state ? DbPlayer.RankDutyStatus.GameDesignDuty : DbPlayer.RankDutyStatus.OffDuty;

            if (state)
            {
                dbPlayer.SetData("armorbefore", dbPlayer.Player.Armor);
                int rnd = Utils.RandomNumber(0, 1);
                if (rnd == 1)
                {
                    dbPlayer.Player.SetSkin(dbPlayer.Customization.Gender == 0 ? PedHash.Construct01SMY : PedHash.ShopMidSFY);
                }
                else
                {
                    dbPlayer.Player.SetSkin(dbPlayer.Customization.Gender == 0 ? PedHash.Construct02SMY : PedHash.ShopMidSFY);
                }
            }
            else
            {
                if (dbPlayer.HasData("armorbefore"))
                {
                    dbPlayer.Player.Armor = dbPlayer.GetData("armorbefore");
                    dbPlayer.ResetData("armorbefore");
                }

                dbPlayer.ApplyCharacter();
                dbPlayer.ApplyArmorVisibility();
            }
        }

        public static bool IsInGuideDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.GuideDuty;
        }

        public static bool IsInGameDesignDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.GameDesignDuty;
        }
    }
}