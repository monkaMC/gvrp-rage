using System;
using System.Threading.Tasks;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Players
{
    public static class PlayerAnimation
    {
        public static async void PlayAnimation(this DbPlayer iPlayer, AnimationScenarioType Type, string Context1,
            string Context2 = "", int lifetime = 5, bool repeat = false,
            AnimationLevels AnimationLevel = AnimationLevels.User, int specialflag = 0, bool noFreeze = false)
        {
            
                if (((int)AnimationLevel < (int)iPlayer.AnimationScenario.AnimationLevel))
                {
                    return;
                }

                iPlayer.AnimationScenario.Context1 = Context1;
                iPlayer.AnimationScenario.Context2 = Context2;
                iPlayer.AnimationScenario.Lifetime = lifetime;
                iPlayer.AnimationScenario.AnimationLevel = AnimationLevel;
                iPlayer.AnimationScenario.StartTime = DateTime.Now;
                iPlayer.AnimationScenario.Repeat = repeat;
                iPlayer.AnimationScenario.SpecialFlag = specialflag;

                if (Type == AnimationScenarioType.Animation)
                {
                    // do animation
                    iPlayer.Player.PlayAnimation(Context1, Context2, specialflag);
                }
                else
                {
                    //do Scenario
                    iPlayer.Player.PlayScenario(Context1);
                }

                iPlayer.AnimationScenario.Active = true;
            
        }

        //public static async void StopAnimation(this DbPlayer iPlayer, AnimationLevels AnimationLevel = AnimationLevels.User)
        //{
            
        //        if (!iPlayer.AnimationScenario.Active)
        //        {
        //            if ((int)iPlayer.AnimationScenario.AnimationLevel > (int)AnimationLevel)
        //            {
        //                return;
        //            }
        //        }
        //        else iPlayer.AnimationScenario.Active = false;

        //        iPlayer.Player.StopAnimation();
        //        //iPlayer.Player.FreezePosition = false;
        //        iPlayer.AnimationScenario.Active = false;
        //        iPlayer.AnimationScenario.AnimationLevel = 0;
            
        //}

        public static async void StopAnimation(this DbPlayer iPlayer, AnimationLevels AnimationLevel = AnimationLevels.User)
        {

            if (!iPlayer.AnimationScenario.Active)
            {
                if ((int) iPlayer.AnimationScenario.AnimationLevel > (int) AnimationLevel)
                {
                    return;
                }
            }
            else iPlayer.AnimationScenario.Active = false;

            iPlayer.Player.StopAnimation();
            iPlayer.AnimationScenario.Active = false;
            iPlayer.AnimationScenario.AnimationLevel = 0;
           
        }

    public static bool IsInAnimation(this DbPlayer iPlayer)
        {
            return (iPlayer.AnimationScenario.Active &&
                    iPlayer.AnimationScenario.AnimationLevel > AnimationLevels.NonRelevant);
        }
    }
}