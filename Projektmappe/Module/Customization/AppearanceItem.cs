using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Customization
{
    public class AppearanceItem
    {
        public readonly byte Value;
        public readonly float Opacity;

        public AppearanceItem(byte value, float opacity)
        {
            Value = value;
            Opacity = opacity;
        }
    }
}
