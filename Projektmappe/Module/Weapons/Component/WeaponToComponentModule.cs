using GVRP.Module.Weapons.Data;

namespace GVRP.Module.Weapons.Component
{
    public class WeaponToComponentModule : SqlBaseModule<WeaponToComponentModule, WeaponToComponent>
    {
        public override System.Type[] RequiredModules()
        {
            return new[] {typeof(WeaponDataModule), typeof(WeaponComponentModule)};
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `weapon_to_component`;";
        }

        protected override void OnItemLoaded(WeaponToComponent weaponToComponent)
        {
            var weaponData = WeaponDataModule.Instance[weaponToComponent.WeaponId];
            if (weaponData == null) return;
            var weaponComponent = WeaponComponentModule.Instance[weaponToComponent.ComponentId];
            if (weaponComponent == null) return;
            weaponData.Components.Add(weaponComponent.Hash, weaponComponent.Name);
        }
    }
}