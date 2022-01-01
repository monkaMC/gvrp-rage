using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Logging;

namespace GVRP.Handler
{
    public static class StartupScripts
    {
        public static void OnStartup()
        {
            // Update Users Count on Server
            MySQLHandler.Execute($"UPDATE configuration SET `value` = '0' WHERE `key` = 'Users_1';");

            MySQLHandler.Execute($"UPDATE player SET `online` = '0';");

            MySQLHandler.Execute("DELETE FROM sms_konversations_messages WHERE sms_konversation_id NOT IN (SELECT id FROM sms_konversations)");

            // DELETE Key FROM banned player
            MySQLHandler.Execute("DELETE `pv` FROM player_to_vehicle pv LEFT JOIN vehicles v ON pv.vehicleID = v.id LEFT JOIN player p ON p.id = v.owner WHERE p.warns = 3;");

            // Delete Vehicles without valid owner
            MySQLHandler.Execute("DELETE FROM vehicles WHERE owner NOT IN (SELECT id FROM player);");

            MigrateContainers();
            VehicleGarage();
            RemoveOldDB();
            RemoveOldMarketplaceOffers();
            BusinessMembers();
            RemoveOldSms();
            CorrectVehicleModels();
            ClearLogs();
            BlacklistRemove();
            NSARemove();
        }

        public static void ClearLogs()
        {
            MySQLHandler.Execute("DELETE FROM log_crashes WHERE time < NOW() - INTERVAL 1 WEEK");
        }

        public static void BusinessMembers()
        {
            // Correct Biz Datas
            MySQLHandler.Execute("DELETE FROM business_members WHERE business_id NOT IN (SELECT id FROM business)");
        }

        public static void CorrectVehicleModels()
        {
            // Correct
            MySQLHandler.Execute("DELETE FROM vehicles WHERE model NOT IN (SELECT id FROM vehicledata)");
            MySQLHandler.Execute("DELETE FROM fvehicles WHERE model NOT IN (SELECT id FROM vehicledata)");
        }

        public static void RemoveOldSms()
        {
            MySQLHandler.Execute("DELETE FROM sms_konversations WHERE `last_updated` < (NOW() - INTERVAL 7 DAY);");
            MySQLHandler.Execute("DELETE FROM sms_konversations_messages WHERE `timestamp` < (NOW() - INTERVAL 7 DAY);");
        }

        public static void BlacklistRemove()
        {
            MySQLHandler.Execute("DELETE FROM team_blacklist WHERE `entry_date` < (NOW() - INTERVAL 30 DAY);");
        }
        public static void NSARemove()
        {
            MySQLHandler.Execute("DELETE FROM nsa_observation_players WHERE `added` < (NOW() - INTERVAL 7 DAY);");
        }

        public static void VehicleGarage()
        {
            MySQLHandler.Execute($"UPDATE vehicles SET `inGarage` = '1' WHERE `pos_x` = '0' OR `pos_y` = '0';");
            MySQLHandler.Execute($"UPDATE fvehicles SET `inGarage` = '1' WHERE `pos_x` = '0' OR `pos_y` = '0';");

            // Fahrzeuge NOT IN vGarage
            MySQLHandler.Execute($"UPDATE vehicles SET `inGarage` = '1', `pos_x` = '0', `pos_y` = '0', `pos_z` = '0' WHERE `lastUpdate` < (NOW() - INTERVAL 3 HOUR) AND `vgarage` = '{(int)VirtualGarageStatus.IN_WORLD}';");
            MySQLHandler.Execute($"UPDATE fvehicles SET `inGarage` = '1', `pos_x` = '0', `pos_y` = '0', `pos_z` = '0' WHERE `lastUpdate` < (NOW() - INTERVAL 3 HOUR) AND `vgarage` = '{(int)VirtualGarageStatus.IN_WORLD}' AND (`team` != '31' OR `team` != '32' OR `team` != '33');");

            // Fahrzeuge in vGarage Older than 24h
            MySQLHandler.Execute($"UPDATE vehicles SET `inGarage` = '1', `pos_x` = '0', `pos_y` = '0', `pos_z` = '0' WHERE `lastUpdate` < (NOW() - INTERVAL 24 HOUR) AND `vgarage` != '{(int)VirtualGarageStatus.IN_WORLD}';");
            MySQLHandler.Execute($"UPDATE fvehicles SET `inGarage` = '1', `pos_x` = '0', `pos_y` = '0', `pos_z` = '0' WHERE `lastUpdate` < (NOW() - INTERVAL 24 HOUR) AND `vgarage` != '{(int)VirtualGarageStatus.IN_WORLD}';");

            // Fahrzeuge ohne gültige letzte Garage (Frakfahrzeuge)
            MySQLHandler.Execute($"UPDATE fvehicles SET `inGarage` = '1', `pos_x` = '0', `pos_y` = '0', `pos_z` = '0' WHERE `lastGarage`=0 AND `inGarage`=0;");

            // Update garage not found failures
            MySQLHandler.Execute($"UPDATE vehicles SET garage_id = 1 WHERE garage_id NOT IN(SELECT id FROM garages)");
        }

        public static void RemoveUnusedDatabaseEntrys()
        {
            MySQLHandler.Execute("DELETE FROM garages_spawns WHERE garage_id NOT IN (SELECT id FROM garages)");
            MySQLHandler.Execute("DELETE FROM farm_positions WHERE farm_id NOT IN (SELECT id FROM farms)");
        }

        public static void RemoveOldDB()
        {
            // Clear Bankhistory
            MySQLHandler.Execute("DELETE FROM player_bankhistory WHERE DATE_SUB(CURDATE(),INTERVAL 7 DAY) >= date");
            MySQLHandler.Execute("DELETE FROM team_bankhistory WHERE DATE_SUB(CURDATE(),INTERVAL 7 DAY) >= date");
            MySQLHandler.Execute("DELETE FROM business_bankhistory WHERE DATE_SUB(CURDATE(),INTERVAL 7 DAY) >= date");

            MySQLHandler.Execute("DELETE FROM player_crime_history WHERE DATE_SUB(CURDATE(),INTERVAL 3 DAY) >= date");
            MySQLHandler.Execute("DELETE FROM log_item WHERE DATE_SUB(CURDATE(),INTERVAL 14 DAY) >= timestamp");
            MySQLHandler.Execute("DELETE FROM container_heap");
        }

        public static void RemoveOldMarketplaceOffers()
        {
            MySQLHandler.Execute("DELETE FROM marketplace_offers WHERE DATE_SUB(CURDATE(),INTERVAL 2 DAY) >= date");

        }

        public static void MigrateContainers()
        {
            Logger.Print("MIGRATING CONTAINERS...");

            // Update container to vehicledata
            MySQLHandler.Execute($"UPDATE `container_vehicle` SET `max_weight` = (SELECT `inv_weight` FROM `vehicledata` WHERE `ID` IN (SELECT `model` FROM `vehicles` WHERE `vehicles`.`id` = `container_vehicle`.`id`))");

            // Update slots to vehicledata
            MySQLHandler.Execute($"UPDATE `container_vehicle` SET `max_slots` = (SELECT `inv_size` FROM `vehicledata` WHERE `ID` IN (SELECT `model` FROM `vehicles` WHERE `vehicles`.`id` = `container_vehicle`.`id`))");

            // Update fcontainer to vehicledata
            MySQLHandler.Execute($"UPDATE `container_fvehicle` SET `max_weight` = (SELECT `inv_weight` FROM `vehicledata` WHERE `ID` IN (SELECT `model` FROM `fvehicles` WHERE `fvehicles`.`id` = `container_fvehicle`.`id`))");

            // Update fslots to vehicledata
            MySQLHandler.Execute($"UPDATE `container_fvehicle` SET `max_slots` = (SELECT `inv_size` FROM `vehicledata` WHERE `ID` IN (SELECT `model` FROM `fvehicles` WHERE `fvehicles`.`id` = `container_fvehicle`.`id`))");

            // Update Meetr. Container
            MySQLHandler.Execute($"UPDATE `container_house_meertraeubel` SET `max_slots` = '24', `max_weight` = '200000'");

            MySQLHandler.Execute($"UPDATE `container_nightclub` SET `max_slots` = '48', `max_weight` = '2500000'");


            Logger.Print("FINISH MIGRATING CONTAINERS");
        }
    }
}
