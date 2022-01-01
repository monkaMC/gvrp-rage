using System;

namespace GVRP.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DisabledModuleAttribute : Attribute
    {
    }
}