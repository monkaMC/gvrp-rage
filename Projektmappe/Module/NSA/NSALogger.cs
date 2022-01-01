using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.NSA
{
    public class NSALogger : Module<NSALogger>
    {
        string BankTable = "nsa_bankhistory";
        string BankTransferTable = "nsa_banktransferhistory";

        public void LogBank(DbPlayer dbPlayer, int amount, string notice)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO `{BankTable}` (`player`, `amount`, `notice`) VALUES ('{dbPlayer.Id}', '{amount}', '{notice}')");
        }

        public void LogBankTransfer(DbPlayer dbPlayer, DbPlayer destPlayer, int amount, string notice)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO `{BankTransferTable}` (`player1`, `player2`, `amount`, `notice`) VALUES ('{dbPlayer.Id}', '{destPlayer.Id}', '{amount}', '{notice}')");
        }
    }
}
