using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Players.Windows;
using GVRP.Module.Guenther;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> GuentherClub(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return false;

            if(dbPlayer.Player.Dimension == 0)
            {
                if (true)
                {
                    dbPlayer.SendNewNotification("Der Club ist scheinbar weitergezogen.");
                    return false;
                }
                if (dbPlayer.Player.Position.DistanceTo(GuentherModule.Outside) > 3.0f)
                {
                    dbPlayer.SendNewNotification("Diese Karte muss am Eingang von Johanns Club mit einem zugehörigen Passwort vorgezeigt werden...", duration: 30000);
                    dbPlayer.SendNewNotification("Das Passwort erhälst du von einem der anderen fünf Mitglieder von Johanns Club.", duration: 30000);
                    dbPlayer.SendNewNotification("Einer arbeitet bei einer großen Versicherung. Ein anderer ist ein Fischliebhaber. Ein weiterer " +
                        "ist am Hafen beschäftigt. Der vierte ist ein begabter Sportler und der fünfte ist im Finanzwesen beschäftigt.", duration: 30000);
                    return false;
                }

                dbPlayer.Character.Clothes.TryGetValue(6,out uint shoe);
                dbPlayer.Character.Clothes.TryGetValue(4,out uint leg);
                dbPlayer.Character.Clothes.TryGetValue(11,out uint top);
                if(shoe == 0 || leg == 0 || top == 0)
                {
                    dbPlayer.SendNewNotification("In Johanns Club kommst du nur mit einem seriösen Outfit rein");
                    return false;
                }
                List<uint> allowedShoes;
                List<uint> allowedLegs;
                List<uint> allowedTops;
                if (dbPlayer.IsMale())
                {
                    allowedShoes = new List<uint> { //6
                        243, 10131, 10327, 11442, 13580, //Variation 10
                        1115, 1116, 1117, 1118, 1119, 1120, 1121, 1122, 4186, 4187, 4188, 4189, 10100, 10101, 10102, //Variation 20
                        1123, 1124, 1125, 1126, 1127, 1128, 4190, 4191, 4192, 4193, 4194, 4195 //Variation 21
                    };
                    allowedLegs = new List<uint> { //4
                        4427, 4428, 4429, 10130,                     //Variation 10
                        627, 628, 629, 630, 631, 4426, 11441, 12989, //Variaton 24
                        632, 4138, 4139, 4140, 4141, 4142, 4566, 4880, 10124, 10194, 10195, 11589, 11590, 11603, 12988, //Variation 25
                        653, 655, 656, 658, 659, 661, 4430, 4431, 4432, 4433, 4434, 4435, 13674, //Variation 28
                        746, 747, 748, 4568, 4569, //Variation 48
                        749, 750, 751, //Variation 49
                        752, 753, 754, 755, //Variation 50
                        756, //Variation 51
                        758, //Variation 51

                    };
                    allowedTops = new List<uint> { //11
                        867, 868, 871, 874, 875, 10128, 12994, //Variation 4
                        923, 924, 925, //Variation 10
                        25, 4420, 4421, 4423, //Variation 20
                        75, 76, 4031, //Variation 23
                        77, 78, 79, 80, 4032, 4436, 4437, //Variation 24
                        82, 4033, 4034, //Variation 28
                        83, 84, 85, 4035, 4036, 4037, 4038, 4039, //Variatiom 29
                        4040, 4041, 4042, 4043, 4044, 4045, 4046, 4047, //Variation 30
                        244, 245, 249, 250, 252, 254, 257, 259, 11571, 13662, //Variation 31
                        264, 266, 267, 268, 269, 271, 272, 273, 11570, //Variation 32
                        4964, 4965, 4966, 4967, 10212, //Variation 72
                        598, 599, 600, 601, 602, 4969, 4970, 4971, 4972, //Variation 76
                        606, 607, 608, 610, 4958, 4959, 4962, 4963, //Variation 77
                        1348, //Variation 99
                        1424, //Variation 100
                        678, 679, 680, //Variation 101
                        686, 688, 689, //Variation 102
                        693, //Variation 103
                        695, //Variation 104
                        1386, //Variation 112
                        12460, 12461, 12642, 12463, 12464, 12465, 12466, 12467, 12468, 12469, 12470, 12471, 12472, 12473,
                        12474, 12475, 12476, 12477, 12478, 12479, 12480, 12481, 12482, 12483, 12484, 12485, //Variation 292
                        12434, 12435, 12436, 12437, 12438, 12439, 12440, 12441, 12442, 12443, 12444, 12445,
                        12446, 12447, 12448, 12449, 12450, 12451, 12452, 12453, 12454, 12455, 12456, 12457, 12458, //Variation 293
                        11988, 11989, 11990, 11993, 11994, 11995, 11996, 11997, 11998, 11999, 12000, 12001, 12002,
                        12003, 12004, 12005, 12006, 12007, 12008, 12009, 12010, 12011, 12012, 12013, 12014, //Variation 311
                        12016, 12017, 12018, 12019, 12020, 12021, 12022, 12023, 12024, 12025, 12026, 12027, 12028,
                        12029, 12030, 12031, 12032, 12033, 12034, 12035, 12036, 12037, 12038, 12039, 12040, 12041, 12042, 12043 //Variation 312
                    };
                    if (!(allowedShoes.Contains(shoe) && allowedLegs.Contains(leg) && allowedTops.Contains(top)))
                    {
                        dbPlayer.SendNewNotification("In Johanns Club kommst du nur mit einem seriösen Outfit rein");
                        return false;
                    }
                }
                else
                {
                    allowedTops = new List<uint>
                    {
                        137, 138, 4465, 4466, 141, 142, 127, 128, 130, 133, 134, 1232 ,1233, 1234, 1235, 1236, 1237, 1238, 1239, 1240, 1241,
                        1242, 1245, 1246, 1350, 1352, 1353, 1356, 1357, 1453, 1524, 10685, 10679, 10680, 10681, 1517, 4517, 4518, 4519, 4520
                    };
                    allowedLegs = new List<uint>
                    {
                        490, 4209, 4210, 492, 10654, 10655, 395, 396, 401, 402, 403, 404, 405, 406, 407, 713, 714, 715, 4285, 4286, 4287,
                        11281, 10769, 10771, 10774, 10775, 494, 497, 1581, 1582
                    };
                    allowedShoes = new List<uint>
                    {
                        10931, 10932, 10933, 10934, 539, 540, 587, 593, 597, 10147, 10148, 10162, 10150, 1441, 1443, 1445, 530, 532, 4188, 1121
                    };
                    if (!(allowedShoes.Contains(shoe) && allowedLegs.Contains(leg) && allowedTops.Contains(top)))
                    {
                        dbPlayer.SendNewNotification("In Johanns Club kommst du nur mit einem seriösen Outfit rein");
                        return false;
                    }
                }

                await Task.Delay(1);

                GTANetworkAPI.NAPI.Task.Run(() => ComponentManager.Get<TextInputBoxWindow>().Show()(
                dbPlayer, new TextInputBoxWindowObject() { Title = "Zugangskontrolle", Callback = "CheckGuentherPassword", Message = "Gib ein Passwort ein" }));
                return false;
            }
            return false;
        }
    }
}
