using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Configurations;
using GVRP.Module.Injury;
using GVRP.Module.Logging;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Swat;

namespace GVRP.Module.Players
{
    public static class PlayerDb
    {
        public static DbPlayer GetPlayer(this Client client)
        {
            if (client == null) return null;
            if (!client.HasData("player")) return null;
            var dbPlayer = client.GetData("player");

            if (dbPlayer is DbPlayer userlist)
            {
                return userlist;
            }

            return null;
        }

        public static bool CanInteractAntiFlood(this DbPlayer iPlayer)
        {
            if (iPlayer.LastInteracted.AddSeconds(10) > DateTime.Now)
            {
                iPlayer.SendNewNotification("Bitte warte kurz!");
                return false;
            }
            else
            {
                iPlayer.LastInteracted = DateTime.Now;
                return true;
            }
        }

        public static async Task<bool> CanPressE(this DbPlayer iPlayer)
        {
            if (iPlayer.LastEInteract.AddSeconds(3) > DateTime.Now)
                return false;
            else
            {
                iPlayer.LastEInteract = DateTime.Now;
                return true;
            }
        }

        public static bool IsValid(this DbPlayer iPlayer, bool ignorelogin = false)
        {
            return iPlayer != null && Players.Instance.DoesPlayerExists(iPlayer.Id) && !iPlayer.Player.IsNull && iPlayer.Player != null && (ignorelogin || iPlayer.AccountStatus == AccountStatus.LoggedIn);
        }

        public static void Save(this DbPlayer iPlayer, bool disconnect = false)
        {
            try
            {
                if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return;
            var update = iPlayer.GetUpdateQuery(disconnect);

            if (update == "") return;
            if (!update.Contains("UPDATE")) return;
            MySQLHandler.ExecuteAsync(update);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string GetUpdateQuery(this DbPlayer iPlayer, bool disconnect = false)
        {
            try
            {
                string update = "UPDATE `player` SET ";

            //   iPlayer.SaveJobSkill();
 

                List<string> ups = new List<string>();
            string xstr = "";

            string query;
            
            if (iPlayer.Swat == 0)
            {
                if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.TeamId, DbPlayer.Value.TeamId, out query))
                {
                    ups.Add(query);
                }

                if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.TeamRank, DbPlayer.Value.TeamRang, out query))
                {
                    ups.Add(query);
                }
            }
            
            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Level, DbPlayer.Value.Level, out query))
            {
                ups.Add(query);
            }
                        
            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Injury.Id, DbPlayer.Value.DeathStatus, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Swat, DbPlayer.Value.Swat, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.UHaftTime, DbPlayer.Value.UHaftTime, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Einwanderung, DbPlayer.Value.Einwanderung, out query))
            {
                ups.Add(query);
            }
            
            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.SwatDuty, DbPlayer.Value.SwatDuty, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Teamfight, DbPlayer.Value.Teamfight, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Suspension, DbPlayer.Value.Suspension, out query))
            {
                ups.Add(query);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.money, "Money")) != "")
            {
                iPlayer.money[1] = iPlayer.money[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.blackmoney, "blackmoney")) != "")
            {
                iPlayer.blackmoney[1] = iPlayer.blackmoney[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.blackmoneybank, "blackmoneybank")) != "")
            {
                iPlayer.blackmoneybank[1] = iPlayer.blackmoneybank[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.bank_money, "BankMoney")) != "")
            {
                iPlayer.bank_money[1] = iPlayer.bank_money[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.payday, "payday")) != "")
            {
                iPlayer.payday[1] = iPlayer.payday[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.rp, "rp")) != "")
            {
                iPlayer.rp[1] = iPlayer.rp[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.ownHouse, "ownHouse")) != "")
            {
                iPlayer.ownHouse[1] = iPlayer.ownHouse[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.wanteds, "wanteds")) != "")
            {
                iPlayer.wanteds[1] = iPlayer.wanteds[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Car, "Lic_Car")) != "")
            {
                iPlayer.Lic_Car[1] = iPlayer.Lic_Car[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_LKW, "Lic_LKW")) != "")
            {
                iPlayer.Lic_LKW[1] = iPlayer.Lic_LKW[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Bike, "Lic_Bike")) != "")
            {
                iPlayer.Lic_Bike[1] = iPlayer.Lic_Bike[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_PlaneA, "Lic_PlaneA")) != "")
            {
                iPlayer.Lic_PlaneA[1] = iPlayer.Lic_PlaneA[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_PlaneB, "Lic_PlaneB")) != "")
            {
                iPlayer.Lic_PlaneB[1] = iPlayer.Lic_PlaneB[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Boot, "Lic_Boot")) != "")
            {
                iPlayer.Lic_Boot[1] = iPlayer.Lic_Boot[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Taxi, "Lic_Taxi")) != "")
            {
                iPlayer.Lic_Taxi[1] = iPlayer.Lic_Taxi[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Gun, "Lic_Gun")) != "")
            {
                iPlayer.Lic_Gun[1] = iPlayer.Lic_Gun[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Biz, "Lic_Biz")) != "")
            {
                iPlayer.Lic_Biz[1] = iPlayer.Lic_Biz[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_FirstAID, "Lic_FirstAID")) != "")
            {
                iPlayer.Lic_FirstAID[1] = iPlayer.Lic_FirstAID[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.Lic_Transfer, "lic_transfer")) != "")
            {
                iPlayer.Lic_Transfer[1] = iPlayer.Lic_Transfer[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.spawnchange, "spawnchange")) != "")
            {
                iPlayer.spawnchange[1] = iPlayer.spawnchange[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.job, "job")) != "")
            {
                iPlayer.job[1] = iPlayer.job[0];
                ups.Add(xstr);
            }

      //      if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.jobskill, "jobskills")) != "")
      //      {
      //          iPlayer.jobskill[1] = iPlayer.jobskill[0];
      //          ups.Add(xstr);
      //      }
                        
            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.jailtime, "jailtime")) != "")
            {
                iPlayer.jailtime[1] = iPlayer.jailtime[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.uni_points, "uni_points")) != "")
            {
                iPlayer.uni_points[1] = iPlayer.uni_points[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.uni_economy, "uni_economy")) != "")
            {
                iPlayer.uni_economy[1] = iPlayer.uni_economy[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.uni_business, "uni_business")) != "")
            {
                iPlayer.uni_business[1] = iPlayer.uni_business[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.uni_workaholic, "uni_workaholic")) != "")
            {
                iPlayer.uni_workaholic[1] = iPlayer.uni_workaholic[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.hasPerso, "Perso")) != "")
            {
                iPlayer.hasPerso[1] = iPlayer.hasPerso[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.donator, "Donator")) != "")
            {
                iPlayer.donator[1] = iPlayer.donator[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.deadtime, "Deadtime")) != "")
            {
                iPlayer.deadtime[1] = iPlayer.deadtime[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.fspawn, "fspawn")) != "")
            {
                iPlayer.fspawn[1] = iPlayer.fspawn[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataString(iPlayer.hasPed, "hasPed")) != "")
            {
                iPlayer.hasPed[1] = iPlayer.hasPed[0];
                ups.Add(xstr);
            }
            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.timeban, "timeban")) != "")
            {
                iPlayer.timeban[1] = iPlayer.timeban[0];
                ups.Add(xstr);
            }

       //     if ((xstr = Helper.Helper.ComplainPlayerDataString(iPlayer.job_skills, "job_skills")) != "")
       //     {
            //    iPlayer.job_skills[1] = iPlayer.job_skills[0];
       //         ups.Add(xstr);
        //    }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.warns, "warns")) != "")
            {
                iPlayer.warns[1] = iPlayer.warns[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.fgehalt, "fgehalt")) != "")
            {
                iPlayer.fgehalt[1] = iPlayer.fgehalt[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.paycheck, "paycheck")) != "")
            {
                iPlayer.paycheck[1] = iPlayer.paycheck[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.handy, "handy")) != "")
            {
                iPlayer.handy[1] = iPlayer.handy[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.guthaben, "guthaben")) != "")
            {
                iPlayer.guthaben[1] = iPlayer.guthaben[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.married, "married")) != "")
            {
                iPlayer.married[1] = iPlayer.married[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(iPlayer.grade, "grade")) != "")
            {
                iPlayer.grade[1] = iPlayer.grade[0];
                ups.Add(xstr);
            }


            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.Duty, DbPlayer.Value.Duty, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.IsCuffed, DbPlayer.Value.IsCuffed, out query))
            {
                ups.Add(query);
            }
            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.IsTied, DbPlayer.Value.IsTied, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.MetaData.Health, DbPlayer.Value.Hp, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.MetaData.Armor, DbPlayer.Value.Armor, out query))
            {
                ups.Add(query);
            }
                        
            // Wenn DC Speicher lastpos, duty, hp und Armor
            if (disconnect)
            {
                ups.Add("`tax_sum` = '" + iPlayer.VehicleTaxSum + "'");

                if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.IsCuffed, DbPlayer.Value.IsCuffed, out query))
                {
                    ups.Add(query);
                }
                if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.IsTied, DbPlayer.Value.IsTied, out query))
                {
                    ups.Add(query);
                }

                if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.MetaData.Health, DbPlayer.Value.Hp, out query))
                {
                    ups.Add(query);
                }
                
                if (Helper.Helper.CheckPlayerData(iPlayer, iPlayer.MetaData.Armor, DbPlayer.Value.Armor, out query))
                {
                    ups.Add(query);
                }
            }
                        

            // Save immer Injured Position
            if (iPlayer.isInjured())
            {
                // Position Saving
                string px = "";
                string py = "";
                string pz = "";

                if (iPlayer.dead_x[0] != iPlayer.dead_x[1])
                {
                    px = iPlayer.dead_x[0].ToString().Replace(",", ".");
                }

                if (iPlayer.dead_y[0] != iPlayer.dead_y[1])
                {
                    py = iPlayer.dead_y[0].ToString().Replace(",", ".");
                }

                if (iPlayer.dead_z[0] != iPlayer.dead_z[1])
                {
                    pz = iPlayer.dead_z[0].ToString().Replace(",", ".");
                }
                
                // Manuell die Deathpos
                if (px != "") ups.Add("`dead_x` = '" + px + "'");
                if (py != "") ups.Add("`dead_y` = '" + py + "'");
                if (pz != "") ups.Add("`dead_z` = '" + pz + "'");
            }

            // Immer Position aus dem MetaData Saven
            string lx = iPlayer.MetaData.Position.X.ToString().Replace(",", ".");
            string ly = iPlayer.MetaData.Position.Y.ToString().Replace(",", ".");
            string lz = iPlayer.MetaData.Position.Z.ToString().Replace(",", ".");
            string heading = iPlayer.MetaData.Heading.ToString().Replace(",", ".");
            
            // Manuell die lastpos
            if (lx != "") ups.Add("`pos_x` = '" + lx + "'");
            if (ly != "") ups.Add("`pos_y` = '" + ly + "'");
            if (lz != "") ups.Add("`pos_z` = '" + lz + "'");
            if (heading != "") ups.Add("`pos_heading` = '" + heading + "'");
            ups.Add("`dimension` = '" + iPlayer.MetaData.Dimension + "'");
            
            if (iPlayer.SocialClubName == "") ups.Add("`SCName` = '" + iPlayer.Player.SocialClubName + "'");

            if ((xstr = Helper.Helper.GetWeapons(iPlayer)) != "")
            {
                ups.Add(xstr);
            }

            string updateX = "";
            if (ups.Count > 0)
            {
                updateX = string.Join(", ", ups);
                if (!disconnect)
                {
                    updateX = updateX + ", `Online` = '1'";
                }
                else
                {
                    updateX = updateX + ", `Online` = '0'";
                }

                updateX = updateX + ", `lastSaved` = '" +
                          Helper.Helper.GetTimestamp(DateTime.Now) + "'";
                updateX = update + updateX + " WHERE `id` = '" + iPlayer.Id + "';";
            }

            return updateX;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;

        }

        public static void SaveWallpaper(this DbPlayer iPlayer)
        {
          //  MySQLHandler.ExecuteAsync("UPDATE player SET `wallpaperId` = '" + iPlayer.wallpaper.Id + "' WHERE id = '" + iPlayer.Id + "';");
        }

        public static void SaveRingtone(this DbPlayer iPlayer)
        {
   //         MySQLHandler.ExecuteAsync("UPDATE player SET `klingeltonId` = '" + iPlayer.ringtone.Id + "' WHERE id = '" + iPlayer.Id + "';");
        }

        public static void SaveJobSkill(this DbPlayer iPlayer)
        {
       //     string playerjob = Convert.ToString(iPlayer.job[0]);
       //     string jobskills = iPlayer.job_skills[0];
            //      int actualskill = Convert.ToInt32(iPlayer.jobskill[0]);
            //      string newinventory = "";
            //     bool found = false;

            //          if (jobskills != "")
            //         {
            //             string[] Items = jobskills.Split(',');
            //           foreach (string item in Items)
            //     {
            //               string[] parts = item.Split(':');
            //               if (parts[0] == playerjob)
            //             {
            //                  if (newinventory == "")
            //                  {
            //                     newinventory = playerjob + ":" + Convert.ToString(actualskill);
            //                   }
            //                  else
            //                {
            //                     newinventory =
            //                     newinventory + "," + playerjob + ":" +
            //                        Convert.ToString(actualskill);
            //               }

            //              found = true;

            //         }

            //             else

            //            {

            //            if (newinventory == "")

            //              {

            //               newinventory = parts[0] + ":" + parts[1];

            //            }

            //                   else

            //                 {

            //           newinventory = newinventory + "," + parts[0] + ":" + parts[1];

            //                }

            //              }

            //               }


            //              if (!found)

            //             {

            //                if (newinventory == "")

            //                 {

            //                    newinventory = playerjob + ":" + Convert.ToString(actualskill);

            //                 }

            //                 else

            //                  {

            //                     newinventory =

            //                     newinventory + "," + playerjob + ":" + Convert.ToString(actualskill);

            //                 }

            //              }

            //          }

            //          else

            //         {

            //             newinventory = playerjob + ":" + Convert.ToString(actualskill);

            //      }

            //   iPlayer.job_skills[0] = newinventory;
        }



    }
}