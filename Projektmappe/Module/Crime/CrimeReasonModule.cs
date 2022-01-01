using System;

namespace GVRP.Module.Crime
{
    public class CrimeReasonModule : SqlModule<CrimeReasonModule, CrimeReason, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(CrimeCategoryModule) };
        }
        protected override string GetQuery()
        {
            return "SELECT * FROM `crime_reasons`;";
        }

        protected override void OnItemLoaded(CrimeReason crimeReason)
        {
            // Do something with loaded data....
        }
    }
}