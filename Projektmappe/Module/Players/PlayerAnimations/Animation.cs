using System;

namespace GVRP.Module.Players.PlayerAnimations
{
    public enum AnimationScenarioType
    {
        Animation = 1,
        Scenario = 2
    }

    public enum AnimationLevels
    {
        NonRelevant = 0,
        User = 1,
        UserUsing = 2,
        UserCop = 3,
        ServerAction = 4,
        ServerDeath = 5
    }

    public class AnimationScenario
    {
        public string Context1 { get; set; }
        public string Context2 { get; set; }
        public int Lifetime { get; set; }
        public AnimationLevels AnimationLevel { get; set; }
        public AnimationScenarioType Type { get; set; }
        public DateTime StartTime { get; set; }
        public bool Active { get; set; }
        public bool Repeat { get; set; }
        public int SpecialFlag { get; set; }

        public AnimationScenario()
        {
            Active = false;
            Context1 = "";
            Context2 = "";
            Type = AnimationScenarioType.Animation;
            Lifetime = 5;
            Repeat = false;
            StartTime = DateTime.Now;
            AnimationLevel = AnimationLevels.User;
        }
    }
}