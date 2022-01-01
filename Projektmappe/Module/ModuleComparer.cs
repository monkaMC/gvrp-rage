using System.Collections.Generic;

namespace GVRP.Module
{
    public class ModuleComparer : IComparer<BaseModule>
    {
        public int Compare(BaseModule x, BaseModule y)
        {
            if (x == null && y != null) return -1;
            if (x != null && y == null) return 1;
            return x?.GetOrder().CompareTo(y.GetOrder()) ?? 0;
        }
    }
}