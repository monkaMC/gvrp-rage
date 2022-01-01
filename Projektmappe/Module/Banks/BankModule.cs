using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Banks
{
    public sealed class BankModule : SqlModule<BankModule, Bank, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `bank` ORDER BY id;";
        }
    }
}
