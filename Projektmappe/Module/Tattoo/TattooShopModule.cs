namespace GVRP.Module.Tattoo
{
    public class TattooShopModule : SqlModule<TattooShopModule, TattooShop, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `tattoo_shops`;";
        }

        private static uint blip = 75;
        private static int color = 0;

        protected override void OnLoaded()
        {
            base.OnLoaded();
        }
        protected override void OnItemLoaded(TattooShop tattooShop)
        {
            Main.ServerBlips.Add(Spawners.Blips.Create(tattooShop.Position, tattooShop.Name, blip, 1.0f, color:color));
            return;
        }
    }
}
