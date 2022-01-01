using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.AnimationMenu
{
    public enum AnimationFlagList
    {
        WalkAndLoop = 8,
        StandAndLoop = 7,
        Stand = 9,
        Walk = 10,
    }
    public sealed class AnimationMenuModule : Module<AnimationMenuModule>
    {
        public Dictionary<uint, int> animFlagDic = new Dictionary<uint, int>();
        
        public override bool Load(bool reload = false)
        {
            animFlagDic = new Dictionary<uint, int>();

            animFlagDic.Add(1, (int)AnimationFlags.Loop);
            animFlagDic.Add(2, (int)AnimationFlags.StopOnLastFrame);
            animFlagDic.Add(3, (int)AnimationFlags.OnlyAnimateUpperBody);
            animFlagDic.Add(4, (int)AnimationFlags.AllowPlayerControl);
            animFlagDic.Add(5, (int)AnimationFlags.Cancellable);
            animFlagDic.Add(6, (int)(AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl));
            animFlagDic.Add(7, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop));
            animFlagDic.Add(8, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody));
            animFlagDic.Add(9, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.StopOnLastFrame | AnimationFlags.Loop));
            animFlagDic.Add(10, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.StopOnLastFrame | AnimationFlags.Loop));

            return true;
        }
    }
}
