using System;

namespace GVRP.Module.RemoteEvents
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RemoteEventPermissionAttribute : Attribute
    {
        public uint? TeamId { get; set; } = null;
        public bool PlayerRankPermission { get; set; } = false;
        public bool AllowedDeath { get; set; } = true;
        public bool AllowedOnCuff { get; set; } = true;
        public bool AllowedOnTied { get; set; } = true;
    }
}