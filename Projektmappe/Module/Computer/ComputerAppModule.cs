namespace GVRP.Module.Computer
{
    public class ComputerAppModule : SqlModule<ComputerAppModule, ComputerApp, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `computer_apps`;";
        }
    }
}
