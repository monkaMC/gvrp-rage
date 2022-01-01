namespace GVRP.Module.Computer
{
    public enum ComputerTypes
    {
        Computer = 1,
        AdminTablet = 2,
    }

    public sealed class ComputerModule : Module<ComputerModule>
    {
        public override bool Load(bool reload = false)
        {
            return true;
        }
    }
}
