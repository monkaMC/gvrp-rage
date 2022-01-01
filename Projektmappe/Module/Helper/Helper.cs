using System;
using System.Collections.Generic;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Helper
{
    public class Tuning
    {
        public uint ID { get; }
        public string Name { get; }
        public int MaxIndex { get; }
        public int StartIndex { get; }

        public Tuning(uint p_ID, string p_Name, int p_StartIndex = -1, int p_MaxIndex = 20)
        {
            ID = p_ID;
            Name = p_Name;
            MaxIndex = p_MaxIndex;
            StartIndex = p_StartIndex;
        }
    }
    public static class Helper
    {
        /*
         * https://wiki.rage.mp/index.php?title=Vehicle_Mods#Plate_Types
         */
        public static Dictionary<int, Tuning> m_Mods = new Dictionary<int, Tuning>
        {
            {1, new Tuning(0, "Spoiler", p_MaxIndex: 100)},
            {2, new Tuning(1, "Front Bumper", p_MaxIndex: 100)},
            {3, new Tuning(2, "Rear Bumper", p_MaxIndex: 100)},
            {4, new Tuning(3, "Side Skirt", p_MaxIndex: 100)},
            {5, new Tuning(4, "Exhaust", p_MaxIndex: 100)},
            {6, new Tuning(5, "Frame")},
            {7, new Tuning(6, "Grille")},
            {8, new Tuning(7, "Hood", p_MaxIndex: 100)},
            {9, new Tuning(8, "Fender")},
            {10, new Tuning(9, "Right Fender")},
            {11, new Tuning(10, "Roof")},
            {12, new Tuning(11, "Engine", p_MaxIndex: 3)},
            {13, new Tuning(12, "Brakes", p_MaxIndex: 2)},
            {14, new Tuning(13, "Transmission", p_MaxIndex: 2)},
            {15, new Tuning(14, "Horn", p_MaxIndex: 52)},
            {16, new Tuning(15, "Suspension", p_MaxIndex: 3)},
            {17, new Tuning(16, "Armor", p_MaxIndex: 4)},
            {18, new Tuning(18, "Turbo", p_MaxIndex: 0)},
            {19, new Tuning(22, "Xenon", p_MaxIndex: 0)},
            {20, new Tuning(23, "Front Wheels", p_MaxIndex: 250)},
            {21, new Tuning(24, "Back Wheels", p_MaxIndex: 250)},
            {22, new Tuning(27, "Trim Design")},
            {23, new Tuning(30, "Dials")},
            {24, new Tuning(33, "Steering Wheel")},
            {25, new Tuning(34, "Shift Lever")},
            {26, new Tuning(38, "Hydraulics")},
            {27, new Tuning(48, "Livery", p_MaxIndex: 100)},
            {28, new Tuning(46, "Window Tint", p_StartIndex: 0, p_MaxIndex: 5)},
            {29, new Tuning(80, "Headlight Color", p_MaxIndex: 12)},
            {30, new Tuning(81, "Numberplate")},
            {31, new Tuning(95, "Tire SmokeR")},
            {32, new Tuning(96, "Tire SmokeG")},
            {33, new Tuning(97, "Tire SmokeB")},
            {34, new Tuning(98, "Pearllack")},
            {35, new Tuning(99, "Felgenfarbe")},
        };
        
        
        public static int GetTimestamp(DateTime date)
        {
            return (int) date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string ComplainPlayerDataInt(int[] playerVar, string dbField)
        {
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string GetWeapons(DbPlayer p_Player)
        {
            string l_String = "";
            var l_JsonOutput = GTANetworkAPI.NAPI.Util.ToJson(p_Player.Weapons);

            l_String = "`weapons` = '" + l_JsonOutput + "'";

            return l_String;
        }

        public static bool CheckPlayerData(DbPlayer dbPlayer, dynamic playerData, DbPlayer.Value value,
            out string query)
        {
            var valueUInt = (uint) value;
            if (dbPlayer.DbValues[valueUInt] != playerData)
            {
                string stringData;
                switch (playerData)
                {
                    case bool _:
                        stringData = playerData ? "1" : "0";
                        break;
                    case Enum _:
                        stringData = Convert.ToString(Convert.ChangeType(playerData,
                            Enum.GetUnderlyingType(playerData.GetType())));
                        break;
                    default:
                        stringData = Convert.ToString(playerData);
                        break;
                }

                query = "`" + DbPlayer.DbColumns[valueUInt] + "` = '" + stringData + "'";
                dbPlayer.DbValues[valueUInt] = playerData;
                return true;
            }

            query = null;
            return false;
        }

        public static string ComplainPlayerDataInt(uint[] playerVar, string dbField)
        {
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string ComplainPlayerData(DimensionType[] playerVar, string dbField)
        {
            return (int) playerVar[0] != (int) playerVar[1] ? $"`{dbField}` = '{(int) playerVar[0]}'" : null;
        }
        
        public static string ComplainPlayerDataFloat(float[] playerVar, string dbField)
        {
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string ComplainPlayerDataString(string[] playerVar, string dbField)
        {
            if (playerVar[0] == null) return "";
            string str = "";
            if (playerVar[0] != playerVar[1])
            {
                str = "`" + dbField + "` = '" + Convert.ToString(playerVar[0]) + "'";
            }

            return str;
        }

        public static string GetLetter(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var num = random.Next(0, chars.Length - 1);
            return "" + chars[num];
        }
    }

    public class ResponseData
    {
        public int m_Code { get; set; }
        public string m_Type { get; set; }
        public string m_Reason { get; set; }
    }

    public class WhitelistData
    {
        public string m_Key { get; set; }
        public string m_AccountData { get; set; }
        public string m_Hash { get; set; }
    }

    public class ClearWhitelistData
    {
        public string api_token { get; private set; }

        public ClearWhitelistData()
        {
            api_token = Configurations.Configuration.Instance.CLEAR_API_KEY;
        }
    }

    public class ClearWhitelistDataAnswer
    {
        public bool success { get; set; }
    }

    /// <summary>
    /// Von Gameserver ausgehendes Paket für das Löschen eines Spielers aus der Launcher-Whitelist
    /// </summary>
    public class ResetWhitelistData
    {
        public string auth_token { get; private set; }
        public string forum_id { get; private set; }

        public ResetWhitelistData(string p_ForumID)
        {
            auth_token = Configurations.Configuration.Instance.RESET_API_KEY;
            forum_id = p_ForumID;
        }
    }

    /// <summary>
    /// Von API Ausgehendes Paket nach Löschen eines Spielers aus der Launcher Whitelist
    /// </summary>
    public class ResetWhitelistDataAnswer
    {
        public bool success { get; set; }
    }

    /// <summary>
    /// Von API Ausgehendes Paket nach Whitelist Statis Abfrage
    /// </summary>
    public class WhitelistDataAnswer
    {
        public bool success { get; set; }
        public string reason { get; set; }
    }
}