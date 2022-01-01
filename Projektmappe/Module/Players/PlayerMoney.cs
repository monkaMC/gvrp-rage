using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerMoney
    {
        /**
         * Takes an specific amount of money from an player
         *
         * @param amount an positive number that represents the amount of money to take from the player
         *
         * @return true if enough money was available, else false
         */

        public static int TakeAnyMoney(this DbPlayer player, int amount, bool ignoreMinuse = false)
        {
            //if (amount < 0) return -1;
            if (player.TakeMoney(amount)) return 0;
            if (player.TakeBankMoney(amount, null, ignoreMinuse)) return 1;
            player.Save();
            return -1;
        }

        public static bool TakeMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.money[0] < money) return false;
            if (player.money[0] - money > player.money[0]) return false;
            player.money[0] = player.money[0] - money;
            player.Player.TriggerEvent("updateMoney", player.money[0]);
            player.Save();
            return true;
        }

        public static bool TakeBlackMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.blackmoney[0] < money) return false;
            if (player.blackmoney[0] - money > player.blackmoney[0]) return false;
            player.blackmoney[0] = player.blackmoney[0] - money;
            player.Player.TriggerEvent("updateBlackMoney", player.blackmoney[0]);
            player.Save();
            return true;
        }

        public static bool GiveMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.money[0] + money < player.money[0]) return false;
            player.money[0] = player.money[0] + money;
            player.Player.TriggerEvent("updateMoney", player.money[0]);
            player.Save();
            return true;
        }


        public static bool GiveBlackMoneyBank(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.blackmoneybank[0] + money < player.blackmoneybank[0]) return false;
            player.blackmoneybank[0] = player.blackmoneybank[0] + money;
            player.Save();
            return true;
        }

        public static bool TakeBlackMoneyBank(this DbPlayer player, int money, string description = null)
        {
            if (money < 0) return false;
            if (player.blackmoneybank[0] < money) return false;
            if (player.blackmoneybank[0] - money > player.blackmoneybank[0]) return false;
            player.blackmoneybank[0] = player.blackmoneybank[0] - money;
            player.Save();
            return true;
        }

        public static bool GiveBlackMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.blackmoney[0] + money < player.blackmoney[0]) return false;
            player.blackmoney[0] = player.blackmoney[0] + money;
            player.Player.TriggerEvent("updateBlackMoney", player.blackmoney[0]);
            player.Save();
            return true;
        }

        public static bool TakeBankMoney(this DbPlayer player, int money, string description = null, bool ignoreMinus = false)
        {
            if (money < 0) return false;
            if (player.bank_money[0] < money && !ignoreMinus) return false;
            if (player.bank_money[0] - money > player.bank_money[0] && !ignoreMinus) return false;
            player.bank_money[0] = player.bank_money[0] - money;
            if (description != null)
            {
                player.AddPlayerBankHistory(-money, description);
            }
            player.Save();
            return true;
        }

        public static void TakeSafeBankMoney(this DbPlayer player, int money, string description = null)
        {
            if (money < 0) return;
            if (player.bank_money[0] - money > player.bank_money[0]) return;
            player.bank_money[0] = player.bank_money[0] - money;
            if (description != null)
            {
                player.AddPlayerBankHistory(-money, description);
            }
            player.Save();
            return;
        }

        public static bool GiveBankMoney(this DbPlayer player, int money, string description = null)
        {
            if (money < 1) return false;
            if (player.bank_money[0] + money < player.bank_money[0]) return false;
            player.bank_money[0] = player.bank_money[0] + money;
            if (description != null)
            {
                player.AddPlayerBankHistory(money, description);
            }
            player.Save();
            return true;
        }

        public static bool TakeOrGiveBankMoney(this DbPlayer player, int money, bool canMinus = false)
        {
            if (money < 0)
            {
                if (canMinus)
                {
                    player.TakeSafeBankMoney(-money);
                    return true;
                }
                else return player.TakeBankMoney(-money);
            }
            if (money > 0) return player.GiveBankMoney(money);
            player.Save();
            return false;
        }

        public static void GiveMoneyToPlayer(this DbPlayer iPlayer, DbPlayer dPlayer, int amount)
        {
            // not a valid destination player
            if (dPlayer == null)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPlayer());
                return;
            }

            // destination is source
            if (iPlayer.Id == dPlayer.Id)
            {
                iPlayer.SendNewNotification(MSG.Money.PlayerSelfMoney());
                return;
            }

            // not a valid amount
            if (amount + dPlayer.money[0] < dPlayer.money[0])
            {
                iPlayer.SendNewNotification(MSG.Money.InvalidAmount());
                return;
            }

            // Take money from source or error
            if (!iPlayer.TakeMoney(amount))
            {
                iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(amount));
                return;
            }

            // transfer money to destination
            dPlayer.GiveMoney(amount);
            
            iPlayer.SendNewNotification(MSG.Money.PlayerGiveMoney(amount));
            dPlayer.SendNewNotification(MSG.Money.PlayerGotMoney(amount));

            SaveToPayLog(iPlayer.Player.Name, dPlayer.Player.Name, amount);
        }
        
        public static void GiveEarning(this DbPlayer dbPlayer, int amount)
        {
            if (dbPlayer.paycheck[0] + amount < dbPlayer.paycheck[0]) return;
            dbPlayer.paycheck[0] = dbPlayer.paycheck[0] + amount;
        }

        public static void ResetMoney(this DbPlayer dbPlayer)
        {
            dbPlayer.money[0] = 0;
            dbPlayer.Player.TriggerEvent("updateMoney", 0);
        }

        private static void SaveToPayLog(string u1, string u2, int value)
        {
            u1 = u1 ?? "undefined";
            u2 = u2 ?? "undefined";
            var query = $"INSERT INTO `paylog` (`s1`,`s2`, `amount`) VALUES ('{u1}', '{u2}', '{value}');";
            MySQLHandler.ExecuteAsync(query);
        }

        public static int GetCapital(this DbPlayer dbPlayer)
        {
            /*var businessMoney = 0;
            foreach (var membership in dbPlayer.BusinessMemberships)
            {
                if (membership.Value.Owner != 1) continue;
                var business = Businesses.Instance.GetById(membership.Value.BusinessId);
                if (business != null)
                {
                    businessMoney = business.Money;
                }
            }*/
            
            return/*businessMoney +*/ dbPlayer.money[0] + dbPlayer.bank_money[0];
        }
    }
}