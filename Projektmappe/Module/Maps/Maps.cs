using GTANetworkAPI;
using GVRP.Module.Maps.Test;
using GVRP.Module.Spawners;

namespace GVRP.Module.Maps
{
    public static class Maps
    {
        public static void LoadAssets()
        {
            Church.Load(100);
            Townhall.Load(101);
            NewsInterior.Load(102);
            KHMountZonah.Load(103);
            KHSandyShores.Load(105);
            DPOS.Load(106);
            KHPaleto.Load(107);
            Fahrschule.Load(108);
            FrakInv.Load(0);
            Triaden.Load(0);
            Dealerhole.Load(110);
            //Basement.Load(0);
            //Aod.Load(111);
            //Bratwa.Load(113);
            //Grove.Load(114);
            LCN.Load(115);
            //Lost.Load(116);
            TriadenKeller.Load(117);
            UR.Load(118);
            Yakuza.Load(119);
            Marabunta.Load(120);
            Vagos.Load(121);
            Zulassungsstelle.Load(150);
            HaustierShop.Load(151);
            ShootingRange.Load(152);
            PoliceStationVespucci.Load(153);
            PoliceStationLaMesa.Load(154);
            AutohausPaletto.Load(155);
            AutohausGlass.Load(156);
            //MotorbikeSanders.Load(157);
            AutohausBenefactor.Load(158);

            //Tests
            SeatsTest.Load(1000);

            // Police Parkplatz
            /*ObjectSpawn.Create(-1789571019, new Vector3(-1389.19226, -588.123352, 30.47027),
                new Vector3(1.00178495E-05, 5.00895567E-06, 32.9998856));

            ObjectSpawn.Create(-463994753, new Vector3(411.72f, -990.64f, 28.39978f), new Vector3(1.001773E-05f, -5.008955E-06f, -91.75f));

            // Bahama Doors
            ObjectSpawn.Create(-1789571019, new Vector3(-1389.19226, -588.123352, 30.47027),
                new Vector3(1.00178495E-05, 5.00895567E-06, 32.9998856));
            ObjectSpawn.Create(-1716946115, new Vector3(-1387.035, -586.7082, 30.4682865),
                new Vector3(1.00178495E-05, 5.008957E-06, 33.1500244));
            ObjectSpawn.Create(-1857663329, new Vector3(-71.14074, 6265.96045, 31.7657852),
                new Vector3(1.0565851E-05, 0.5000057, 32.8999557));

            // AOD
            ObjectSpawn.Create(1890297615, new Vector3(981.23211700, -103.28078500, 74.99642940),
                new Vector3(0.00000283, 0.00000764, 42.77670290));
            ObjectSpawn.Create(-190780785, new Vector3(984.87341300, -130.97200000, 74.09638980),
                new Vector3(0.00000000, -0.00000000, -120.04914900));

            // Garagentor
            ObjectSpawn.Create(-577103870, new Vector3(-71.4949646, 6265.66748, 30.4858856),
                new Vector3(-0, 0, -146.884598));

            // Fahrschule
            ObjectSpawn.Create(637724453, new Vector3(-778.341248, -1300.30029, 4.00037956),
                new Vector3(0, -0, -130.99939));
            ObjectSpawn.Create(637724453, new Vector3(-774.418945, -1295.64673, 4.00038),
                new Vector3(0, -0, -129.999466));
            ObjectSpawn.Create(637724453, new Vector3(-770.505859, -1291.0697, 4.00038), new Vector3(0, -0, -130.499283));
            ObjectSpawn.Create(637724453, new Vector3(-766.3323, -1286.72668, 4.00038), new Vector3(0, -0, -133.899612));
            ObjectSpawn.Create(637724453, new Vector3(-761.195, -1283.65857, 4.00038), new Vector3(0, -0, -148.9996));
            ObjectSpawn.Create(-208600510, new Vector3(-757.7022, -1281.63855, 4.00038), new Vector3(0, -0, -149.9993));
            ObjectSpawn.Create(-856050416, new Vector3(-758.469238, -1282.08582, 4.00038), new Vector3(0, 0, 29.3999577));
            ObjectSpawn.Create(-870868698, new Vector3(-858.17, -1280.45, 4.15017939), new Vector3(0, -0, -159.599487));
            ObjectSpawn.Create(609684180, new Vector3(-791.2715, -1313.73059, 4.100109), new Vector3(0, -0, 128.999466));
            ObjectSpawn.Create(-105439435, new Vector3(-845.637451, -1316.80542, 3.73), new Vector3(0, 0, 19.9983826));
            ObjectSpawn.Create(-469694731, new Vector3(-849.309265, -1282.19861, 4.00018167),
                new Vector3(0, 0, -66.1998138));
            //Objects.Create(-46303329, new Vector3(-853.6985, -1284.40625, 4.10018158), new Vector3(0, 0, -70.2998));
            //Objects.Create(-46303329, new Vector3(-850.1775, -1293.92065, 4.10018063), new Vector3(0, 0, -70.2998));
            ObjectSpawn.Create(1843657781, new Vector3(-852.365, -1278.83032, 4.15018), new Vector3(0, -0, -160.999619));
            ObjectSpawn.Create(1821241621, new Vector3(-856.2217, -1280.30212, 4.10017967),
                new Vector3(0, -0, -162.66626));
            ObjectSpawn.Create(840050250, new Vector3(-798.774048, -1322.70288, 4.100109),
                new Vector3(0, -0, 126.999619));
            ObjectSpawn.Create(840050250, new Vector3(-820.1058, -1308.32434, 4.100109), new Vector3(0, -0, -119.99987));

            // Keller
            ObjectSpawn.Create(1890297615, new Vector3(2432.26587, 4960.76563, 46.9454575),
                new Vector3(9.31322519E-10, -4.16881613E-10, -45.470993));

            // KH Mount Zonah
            ObjectSpawn.Create(-505101878, new Vector3(-457.0258f, -290.4704f, 77.21f), new Vector3(0f, 0f, -67.00046f));

            //ObjectSpawn.Create(-1007599668, new Vector3(1972.77f, 3815.37f, 33.66f), new Vector3(0f, 0f, 29.99998f), 4294967295);
            ObjectSpawn.Create(132154435, new Vector3(1972.77f, 3815.37f, 33.66f), new Vector3(0f, 0f, 29.99998f));*/

            //Knast
            /* Objects.Create(733542368, new Vector3(1616.07349, 2526.33374, 45.83544), new Vector3(0, 0, -87.9996948));
             Objects.Create(733542368, new Vector3(1625.73035, 2566.06958, 45.6585426), new Vector3(0, -0, -176.999435));
             Objects.Create(733542368, new Vector3(1621.588, 2565.79468, 45.7325745), new Vector3(0, -0, -174.99942));
             Objects.Create(733542368, new Vector3(1617.46436, 2565.44775, 45.65626), new Vector3(0, -0, -170.999161));
             Objects.Create(733542368, new Vector3(1613.411, 2564.78931, 45.6873131), new Vector3(0, -0, -173.999557));
             Objects.Create(733542368, new Vector3(1609.2771, 2564.30518, 45.73751), new Vector3(0, -0, 168.300385));
             Objects.Create(-1900591032, new Vector3(1616.22107, 2531.76953, 44.5648766), new Vector3(0, 0, 45.1987648));
             Objects.Create(-1900591032, new Vector3(1618.32654, 2533.99341, 44.5648766), new Vector3(0, -0, 133.4993));
             Objects.Create(-1900591032, new Vector3(1616.36084, 2536.043, 44.5648766), new Vector3(0, -0, 135.99939));
             Objects.Create(-1900591032, new Vector3(1614.59692, 2537.694, 44.5648766), new Vector3(0, -0, 130.999359));
             Objects.Create(-1900591032, new Vector3(1610.50623, 2537.50952, 44.5648766), new Vector3(0, 0, 44.9999123));
             Objects.Create(-1900591032, new Vector3(1620.27637, 2518.66187, 44.5719681), new Vector3(0, 0, 6.999997));
             Objects.Create(-1900591032, new Vector3(1623.39734, 2519.084, 44.5648766), new Vector3(0, 0, -81.99976));
             Objects.Create(-1900591032, new Vector3(1623.72083, 2516.16138, 44.5648766), new Vector3(0, 0, -81.99982));
             Objects.Create(-1900591032, new Vector3(1624.16956, 2513.8103, 44.8165474), new Vector3(0, 0, -84.999794));
             Objects.Create(-1900591032, new Vector3(1620.95667, 2510.8894, 44.75694), new Vector3(0, 0, 3.99999261));
             Objects.Create(-1900591032, new Vector3(1654.01746, 2490.45142, 44.573822), new Vector3(0, -0, 94.99968));
             Objects.Create(-1900591032, new Vector3(1653.72485, 2493.749, 44.5648766), new Vector3(0, -0, -176.999527));
             Objects.Create(-1900591032, new Vector3(1650.84961, 2493.60913, 44.5648766), new Vector3(0, -0, -176.999649));
             Objects.Create(-1900591032, new Vector3(1648.41772, 2493.53345, 44.5648766), new Vector3(0, -0, -173.999557));
             Objects.Create(-1900591032, new Vector3(1645.70874, 2493.13281, 44.5648766), new Vector3(0, 0, -82.99971));
             Objects.Create(-1900591032, new Vector3(1676.47607, 2481.76367, 44.7903976), new Vector3(0, 0, 43.9999275));
             Objects.Create(-1900591032, new Vector3(1679.022, 2484.11768, 44.6655922), new Vector3(0, -0, 135));
             Objects.Create(-1900591032, new Vector3(1676.98657, 2486.13965, 44.6276932), new Vector3(0, -0, 138.9997));
             Objects.Create(-1900591032, new Vector3(1675.21484, 2487.53735, 44.5649452), new Vector3(0, -0, 135.999466));
             Objects.Create(-1900591032, new Vector3(1671.00085, 2487.45117, 44.5649452), new Vector3(0, 0, 45.99992));
             Objects.Create(-1900591032, new Vector3(1714.87244, 2487.42, 44.5649452), new Vector3(0, -0, 134.999832));
             Objects.Create(-1900591032, new Vector3(1712.69849, 2489.586, 44.5649452), new Vector3(0, -0, -139.999908));
             Objects.Create(-1900591032, new Vector3(1710.52563, 2487.70483, 44.5649452), new Vector3(0, -0, -139.000046));
             Objects.Create(-1900591032, new Vector3(1708.7644, 2486.14038, 44.5649452), new Vector3(0, -0, -134.999863));
             Objects.Create(-1900591032, new Vector3(1708.94348, 2481.87524, 44.5649452), new Vector3(0, -0, 134.999741));
             Objects.Create(-1900591032, new Vector3(1734.25769, 2504.55615, 44.57784), new Vector3(0, 0, 75.99974));
             Objects.Create(-1900591032, new Vector3(1735.00464, 2507.68066, 44.7825165), new Vector3(0, -0, 165.9994));
             Objects.Create(-1900591032, new Vector3(1732.23108, 2508.41333, 44.620285), new Vector3(0, -0, 165.999222));
             Objects.Create(-1900591032, new Vector3(1729.80261, 2508.97437, 44.5648766), new Vector3(0, -0, 165.999344));
             Objects.Create(-1900591032, new Vector3(1726.41406, 2506.39478, 44.6186523), new Vector3(0, 0, 77.99979));
             Objects.Create(-1900591032, new Vector3(1764.25684, 2528.42358, 44.5650024), new Vector3(0, -0, 163.999634));
             Objects.Create(-1900591032, new Vector3(1761.34473, 2529.219, 44.5650024), new Vector3(0, -0, -108.999954));
             Objects.Create(-1900591032, new Vector3(1760.46741, 2526.4978, 44.5650024), new Vector3(0, -0, -106.999733));
             Objects.Create(-1900591032, new Vector3(1759.71216, 2524.27051, 44.5650024), new Vector3(0, -0, -103.999596));
             Objects.Create(-1900591032, new Vector3(1762.0127, 2520.71777, 44.5650024), new Vector3(0, -0, 164.999725));
             Objects.Create(-1900591032, new Vector3(1736.29224, 2560.76416, 44.5650024), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1739.16846, 2560.79858, 44.5650024), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1744.12756, 2560.658, 44.68616), new Vector3(0, -0, 178.000854));
             Objects.Create(-1900591032, new Vector3(1744.17175, 2564.03564, 44.6460266), new Vector3(0, 0, -88.99981));
             Objects.Create(-1900591032, new Vector3(1700.54, 2563.00171, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1703.41284, 2562.99658, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1705.38416, 2563.00952, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1708.46545, 2566.39624, 44.67298), new Vector3(0, -0, -90.99978));
             Objects.Create(-1900591032, new Vector3(1673.31555, 2563.054, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1676.21216, 2563.08716, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1678.278, 2563.03442, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1900591032, new Vector3(1681.20569, 2566.39185, 44.5648766), new Vector3(0, 0, -89.99977));
             Objects.Create(266061667, new Vector3(1616.10376, 2526.3335, 46.947876), new Vector3(0, 0, -88.19971));
             Objects.Create(266061667, new Vector3(1616.10938, 2526.3335, 49.1446877), new Vector3(0, 0, -88.29966));
             Objects.Create(266061667, new Vector3(1616.21655, 2580.7915, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(266061667, new Vector3(1620.34033, 2580.85938, 44.5648766), new Vector3(0, 0, 3.260374E-05));
             Objects.Create(266061667, new Vector3(1624.46362, 2580.90137, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(1001693768, new Vector3(1616.0144, 2575.604, 44.5648766), new Vector3(0, 0, -42.9999046));
             Objects.Create(1001693768, new Vector3(1616.65039, 2571.43359, 44.5648766), new Vector3(0, 0, 47.9999123));
             Objects.Create(1001693768, new Vector3(1614.65613, 2569.32471, 44.5648766), new Vector3(0, 0, 46.9999161));
             Objects.Create(1001693768, new Vector3(1612.61731, 2567.29517, 44.5648766), new Vector3(0, 0, 44.9999237));
             Objects.Create(1001693768, new Vector3(1610.39087, 2569.146, 44.5648766), new Vector3(0, 0, -39.9999352));
             Objects.Create(1948414141, new Vector3(1625.67749, 2577.125, 47.22846), new Vector3(0, 0, 0.7990534));
             Objects.Create(-1215378248, new Vector3(1759.306, 2543.13721, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(-1053433850, new Vector3(1759.84363, 2541.9563, 44.5650024), new Vector3(0, 0, -14.9999895));
             Objects.Create(-1053433850, new Vector3(1742.21448, 2547.02832, 44.5650024), new Vector3(0, 0, 17.9999866));
             Objects.Create(-2041628332, new Vector3(1741.449, 2547.51343, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(2124667619, new Vector3(1744.83069, 2543.66284, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(725387438, new Vector3(1746.71643, 2544.7793, 44.5650024), new Vector3(0, -0, 100.9998));
             Objects.Create(2042668880, new Vector3(1752.64612, 2538.65942, 44.34), new Vector3(0, -0, 0));
             Objects.Create(-1625949270, new Vector3(1748.80737, 2545.15845, 44.5650024), new Vector3(0, 0, -24.9999752));
             Objects.Create(2055647880, new Vector3(1748.153, 2544.469, 44.5650024), new Vector3(0, 0, -57.9999161));
             Objects.Create(390804950, new Vector3(1753.82275, 2540.711, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(1797043157, new Vector3(1752.11755, 2541.106, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(-1204312266, new Vector3(1751.59448, 2540.35034, 44.5650024), new Vector3(0, 0, -79.99986));
             Objects.Create(-608997843, new Vector3(1754.19, 2540.51367, 45.32909), new Vector3(0, -0, 0));
             Objects.Create(-1601356591, new Vector3(1748.68042, 2541.77319, 44.34), new Vector3(0, -0, 119.999763));
             Objects.Create(-1968566005, new Vector3(1756.32385, 2546.04663, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(122030657, new Vector3(1757.243, 2541.42065, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(736590427, new Vector3(1753.28174, 2543.92017, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(-608997843, new Vector3(1755.153, 2544.95044, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(390804950, new Vector3(1753.23328, 2540.53418, 45.22156), new Vector3(0, -0, 0));
             Objects.Create(-1625949270, new Vector3(1756.20166, 2547.64624, 44.5650024), new Vector3(0, 0, 18.99999));
             Objects.Create(2042668880, new Vector3(1751.74573, 2551.3396, 43.52), new Vector3(0, 0, 49.9998779));
             Objects.Create(-1625949270, new Vector3(1750.25146, 2550.14868, 44.5650024), new Vector3(0, 0, -43.99981));
             Objects.Create(122030657, new Vector3(1750.68457, 2538.63135, 44.13), new Vector3(0, -0, 104.999786));
             Objects.Create(1430257647, new Vector3(1758.212, 2546.6543, 44.5650024), new Vector3(0, -0, 128.999924));
             Objects.Create(1430257647, new Vector3(1743.718, 2544.269, 44.5650024), new Vector3(0, -0, -111.9999));
             Objects.Create(1652026494, new Vector3(1744.618, 2553.41455, 44.85), new Vector3(0, 0, 32.0000763));
             Objects.Create(-260208501, new Vector3(1744.80139, 2554.68945, 44.8150024), new Vector3(0, 0, 30.9999657));
             Objects.Create(-350795026, new Vector3(1758.81165, 2538.80469, 44.5650024), new Vector3(0, -0, -117.999992));
             Objects.Create(145818549, new Vector3(1747.75525, 2554.08374, 44.5650024), new Vector3(0, 0, 3.99999428));
             Objects.Create(145818549, new Vector3(1744.99, 2536.73633, 44.5650024), new Vector3(0, -0, 131.999466));
             Objects.Create(260517631, new Vector3(1766.04663, 2565.75537, 44.5650024), new Vector3(0, -0, 0));
             Objects.Create(-1184096195, new Vector3(1767.06787, 2565.84668, 45.26), new Vector3(0, -0, -91.0000458));
             Objects.Create(260873931, new Vector3(1767.16, 2565.53, 45.67), new Vector3(-90, -4.593141E-06, 11.9999943));
             Objects.Create(260873931, new Vector3(1766.92554, 2566.0083, 45.67), new Vector3(89.9999542, -5.00895658E-06, -5.00895158E-06));
             Objects.Create(683570518, new Vector3(1767.24121, 2566.07935, 45.77), new Vector3(0, -0, 0));
             Objects.Create(683570518, new Vector3(1766.79, 2565.58, 45.69), new Vector3(-89.98021, -2.423771, 8.23370552));
             Objects.Create(-1198343923, new Vector3(1764.74316, 2565.98584, 44.8006744), new Vector3(0, 0, -79.99977));
             Objects.Create(-1920611843, new Vector3(1764.62036, 2565.75684, 45.57), new Vector3(0, 0, 48.9999428));
             Objects.Create(1758176010, new Vector3(1767.14661, 2566.07178, 44.94), new Vector3(0, -0, -162.000153));
             Objects.Create(1916770868, new Vector3(1656.85071, 2546.11914, 44.5648766), new Vector3(0, -0, 138.999573));
             Objects.Create(1916770868, new Vector3(1655.98059, 2545.07349, 44.5648766), new Vector3(0, -0, 138.999573));
             Objects.Create(1916770868, new Vector3(1655.99329, 2545.928, 46.0683327), new Vector3(0, -0, -128.0002));
             Objects.Create(1916770868, new Vector3(1650.14954, 2538.37915, 44.5648766), new Vector3(0, -0, 138.999573));
             Objects.Create(-1147461795, new Vector3(1651.45093, 2538.734, 44.5648766), new Vector3(0, 0, 38.9999275));
             Objects.Create(-1147461795, new Vector3(1658.32874, 2545.213, 44.5648766), new Vector3(0, 0, 49.9998665));
             Objects.Create(1916770868, new Vector3(1652.11377, 2537.37451, 44.5648766), new Vector3(0, 0, 50.99991));
             Objects.Create(1916770868, new Vector3(1651.30945, 2537.847, 46.0683327), new Vector3(0, 0, -18.0000935));
             Objects.Create(1195840658, new Vector3(1652.39819, 2536.182, 44.5648766), new Vector3(0, -0, 142.9997));
             Objects.Create(1195840658, new Vector3(1653.256, 2537.23828, 44.5648766), new Vector3(0, -0, 142.9997));
             Objects.Create(1195840658, new Vector3(1652.92163, 2536.58447, 45.16612), new Vector3(0, -0, 149.999573));
             Objects.Create(-1787668082, new Vector3(1653.48022, 2543.48877, 45.1146355), new Vector3(0, 0, 46.99995));
             Objects.Create(-1787668082, new Vector3(1651.80066, 2541.66333, 45.14), new Vector3(5.008955E-06, 2.23117986E-05, 49.9999962));
             Objects.Create(-1787668082, new Vector3(1657.60449, 2543.9585, 45.09498), new Vector3(0, 0, 47.9998245));
             Objects.Create(2139496847, new Vector3(1655.13171, 2544.45142, 44.822834), new Vector3(0, 0, 21.99998));
             Objects.Create(1835700637, new Vector3(1655.49109, 2543.99561, 44.5648766), new Vector3(0, 0, 21.99998));
             Objects.Create(1835700637, new Vector3(1653.73193, 2544.04834, 44.6464272), new Vector3(0, 0, 21.99998));
             Objects.Create(1835700637, new Vector3(1652.00854, 2542.221, 44.67179), new Vector3(0, 0, 21.99998));
             Objects.Create(1835700637, new Vector3(1651.56946, 2541.71826, 44.67179), new Vector3(0, 0, 34.9999542));
             Objects.Create(1835700637, new Vector3(1657.80359, 2544.42163, 44.62677), new Vector3(0, 0, 34.9999542));
             Objects.Create(-2120435199, new Vector3(1657.77014, 2543.8584, 44.62677), new Vector3(0, 0, 55.99992));
             Objects.Create(-2120435199, new Vector3(1653.31018, 2543.54785, 44.6464272), new Vector3(0, 0, 55.99992));
             Objects.Create(-2120435199, new Vector3(1653.47021, 2543.8418, 44.8494873), new Vector3(0, 0, 55.99992));
             Objects.Create(-2120435199, new Vector3(1651.79675, 2541.982, 44.9743652), new Vector3(0, -0, 94.99984));
             Objects.Create(-2120435199, new Vector3(1651.49084, 2540.90332, 44.67179), new Vector3(0, -0, -131.000214));
             Objects.Create(736590427, new Vector3(1650.98584, 2541.23535, 44.67179), new Vector3(0, -0, 0));
             Objects.Create(736590427, new Vector3(1653.73853, 2544.24731, 44.9734573), new Vector3(0, 0, 2.99999952));
             Objects.Create(736590427, new Vector3(1657.84839, 2544.56543, 45.00553), new Vector3(0, 0, 2.99999928));
             Objects.Create(736590427, new Vector3(1657.39917, 2544.19946, 44.62677), new Vector3(0, 0, 2.99999928));
             Objects.Create(736590427, new Vector3(1657.19043, 2543.37427, 44.62677), new Vector3(0, 0, 2.99999928));
             Objects.Create(2139496847, new Vector3(1652.10425, 2542.37427, 45.19801), new Vector3(0, 0, 53.99992));
             Objects.Create(2139496847, new Vector3(1650.80981, 2540.844, 44.5648766), new Vector3(0, 0, 53.9999123));
             Objects.Create(2139496847, new Vector3(1650.81592, 2540.84814, 44.81507), new Vector3(0, 0, 53.9999123));
             Objects.Create(2139496847, new Vector3(1651.07947, 2540.621, 44.6398773), new Vector3(0, 0, 18.9998741));
             Objects.Create(-1095320058, new Vector3(1752.52185, 2550.26367, 44.49), new Vector3(-22, -1.99999774, -147.000122));
             Objects.Create(1469496946, new Vector3(1710.5072, 2498.95044, 44.5648766), new Vector3(0, 0, 89.9998856));
             Objects.Create(1469496946, new Vector3(1710.53516, 2504.95459, 44.5648766), new Vector3(0, 0, 81.99982));
             Objects.Create(710800597, new Vector3(1710.59009, 2498.89, 44.5648766), new Vector3(0, -0, -129.999664));
             Objects.Create(1469496946, new Vector3(1709.22119, 2497.35132, 44.5648766), new Vector3(0, -0, 179.999435));
             Objects.Create(1469496946, new Vector3(1703.2196, 2497.33765, 44.5648766), new Vector3(0, -0, 179.999435));
             Objects.Create(1469496946, new Vector3(1697.22937, 2497.34839, 44.5648766), new Vector3(0, -0, 179.999435));
             Objects.Create(710800597, new Vector3(1711.44617, 2510.90576, 44.5648766), new Vector3(0, 0, 55.0006447));
             Objects.Create(710800597, new Vector3(1712.61194, 2512.55371, 44.5648766), new Vector3(0, 0, 45.00059));
             Objects.Create(1469496946, new Vector3(1647.00647, 2511.73682, 44.5648766), new Vector3(0, 0, -42.00027));
             Objects.Create(1469496946, new Vector3(1651.43469, 2507.77271, 44.5648766), new Vector3(0, 0, -42.00027));
             Objects.Create(1469496946, new Vector3(1655.904, 2503.74219, 44.5648766), new Vector3(0, 0, -42.00028));
             Objects.Create(344241399, new Vector3(1660.32507, 2499.7666, 44.5648766), new Vector3(0, 0, -40.9999275));
             Objects.Create(710800597, new Vector3(1663.30139, 2497.169, 44.5648766), new Vector3(0, 0, -16.99999));
             Objects.Create(344241399, new Vector3(1665.22412, 2496.55615, 44.5648766), new Vector3(0, 0, 0.000120162957));
             Objects.Create(344241399, new Vector3(1669.19568, 2496.57324, 44.5648766), new Vector3(0, 0, 0.000120162957));
             Objects.Create(344241399, new Vector3(1673.18677, 2496.542, 44.5648766), new Vector3(0, 0, 0.000120162957));
             Objects.Create(-1286880215, new Vector3(1678.8717, 2550.5354, 44.5648766), new Vector3(0, 0, 50.999897));
             Objects.Create(-1286880215, new Vector3(1667.885, 2537.46851, 44.5648766), new Vector3(0, 0, 50.9998932));
             Objects.Create(-1286880215, new Vector3(1655.96887, 2523.21045, 44.5648766), new Vector3(0, 0, 50.9998932));
             Objects.Create(-1286880215, new Vector3(1717.37244, 2557.6416, 44.5648766), new Vector3(0, 0, -1.00013328));
             Objects.Create(-1286880215, new Vector3(1701.31677, 2557.765, 44.5648766), new Vector3(0, 0, -1.00013316));
             Objects.Create(93871477, new Vector3(1738.80212, 2526.365, 44.5650024), new Vector3(0, -0, -112.999519));
             Objects.Create(93871477, new Vector3(1722.39539, 2557.91528, 44.5648766), new Vector3(0, -0, -112.999512));
             Objects.Create(1469496946, new Vector3(1742.93665, 2528.56, 44.5650024), new Vector3(0, 0, 27.4003315));
             Objects.Create(1469496946, new Vector3(1748.213, 2531.24, 44.6452942), new Vector3(0, 0, 27.4003277));
             Objects.Create(1469496946, new Vector3(1753.49329, 2533.91235, 44.68209), new Vector3(0, 0, 28.6003113));
             Objects.Create(1469496946, new Vector3(1758.71252, 2536.70239, 44.7062531), new Vector3(0, 0, 28.6003132));
             Objects.Create(710800597, new Vector3(1763.99878, 2539.47974, 44.7163277), new Vector3(0, 0, 27.8998814));
             Objects.Create(-1324470710, new Vector3(1709.31018, 2508.47925, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-2093428068, new Vector3(1708.77075, 2502.5647, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1658282356, new Vector3(1708.12158, 2503.1665, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1658282356, new Vector3(1708.84863, 2507.82886, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1656246279, new Vector3(1708.81982, 2502.1123, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1658282356, new Vector3(1701.18323, 2500.315, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(727229237, new Vector3(1701.98987, 2504.3623, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-2010456872, new Vector3(1709.73242, 2509.99585, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1658282356, new Vector3(1701.50647, 2504.27563, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(99244117, new Vector3(1695.71667, 2499.069, 44.5648766), new Vector3(0, 0, 0));
             Objects.Create(-1658282356, new Vector3(1696.28979, 2499.625, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(-1658282356, new Vector3(1694.98584, 2499.332, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(-1658282356, new Vector3(1702.76624, 2509.91553, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(-1658282356, new Vector3(1709.05249, 2509.55933, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(-1658282356, new Vector3(1708.283, 2508.74634, 44.5648766), new Vector3(0, -0, 0));
             Objects.Create(-1256343353, new Vector3(1705.51807, 2505.69971, 43.96487), new Vector3(0, 0, 0));
             Objects.Create(-1256343353, new Vector3(1703.008, 2499.2876, 43.96487), new Vector3(0, -0, 0));
             Objects.Create(-1636670283, new Vector3(1708.109, 2500.48438, 44.03987), new Vector3(0, 0, 0));
             Objects.Create(1948414141, new Vector3(1761.62427, 2518.46484, 47.1747665), new Vector3(0, 0, -14.2009439));
             Objects.Create(1948414141, new Vector3(1737.33276, 2503.47021, 47.19744), new Vector3(0, 0, 74.39896));
             Objects.Create(1948414141, new Vector3(1707.554, 2480.03931, 47.12312), new Vector3(0, 0, -45.50138));
             Objects.Create(1948414141, new Vector3(1678.96753, 2479.272, 47.1816559), new Vector3(0, 0, 44.99857));
             Objects.Create(1948414141, new Vector3(1643.68042, 2489.60571, 47.2348022), new Vector3(0, 0, -84.00128));
             Objects.Create(1948414141, new Vector3(1621.26465, 2507.377, 47.2355537), new Vector3(0, 0, 4.998745));
             Objects.Create(1948414141, new Vector3(1608.763, 2539.01514, 47.20741), new Vector3(0, -0, -134.601013));
             Objects.Create(1948414141, new Vector3(1608.38367, 2568.048, 47.21448), new Vector3(0, -0, 135.399048));
             Objects.Create(-250842784, new Vector3(1607.71082, 2512.23535, 44.5648766), new Vector3(0, -0, 114.999878));
             Objects.Create(-250842784, new Vector3(1605.22046, 2517.36426, 44.5648766), new Vector3(0, -0, 110.999832));
             Objects.Create(-250842784, new Vector3(1603.197, 2522.739, 44.5648766), new Vector3(0, -0, 96.9992142));
             Objects.Create(-250842784, new Vector3(1731.73389, 2478.08521, 44.5649452), new Vector3(0, 0, 12.9999933));
             Objects.Create(-250842784, new Vector3(1725.98511, 2477.2356, 44.5649452), new Vector3(0, 0, 7.999996));
             Objects.Create(-250842784, new Vector3(1725.87732, 2477.24268, 44.5649452), new Vector3(0, -0, 170.999725));
             Objects.Create(-250842784, new Vector3(1659.78467, 2477.09424, 44.5649452), new Vector3(0, 0, -5.99999666));
             Objects.Create(-250842784, new Vector3(1659.65991, 2477.07959, 44.5649452), new Vector3(0, -0, 165.999481));
             Objects.Create(-250842784, new Vector3(1653.965, 2478.49, 44.5649452), new Vector3(0, -0, 141.999588));
             Objects.Create(-250842784, new Vector3(1783.06445, 2525.966, 44.5650024), new Vector3(0, 0, 85.9999));
             Objects.Create(-250842784, new Vector3(1783.45251, 2531.68262, 44.5650024), new Vector3(0, 0, 83.99972));
             Objects.Create(-250842784, new Vector3(1784.10425, 2537.37939, 44.5650024), new Vector3(0, 0, 86.99991));
             Objects.Create(-250842784, new Vector3(1782.96777, 2516.31055, 44.5650024), new Vector3(0, -0, -145.999832));
             Objects.Create(-250842784, new Vector3(1791.48682, 2558.79346, 44.5650024), new Vector3(0, -0, 90.3996353));
             Objects.Create(733542368, new Vector3(1681.26917, 2563.02539, 45.8568268), new Vector3(0, 0, 89.9997));
             Objects.Create(733542368, new Vector3(1708.5282, 2563.00049, 45.7123642), new Vector3(0, 0, 88.99969));
             Objects.Create(733542368, new Vector3(1744.22742, 2560.72363, 45.7592926), new Vector3(0, 0, 89.99959));
             Objects.Create(733542368, new Vector3(1678.95752, 2484.122, 45.4933167), new Vector3(0, -0, -134.999924));
             Objects.Create(733542368, new Vector3(1624.35046, 2511.36572, 45.6978531), new Vector3(0, -0, -169.999588));
             Objects.Create(1278261455, new Vector3(1733.91479, 2504.38574, 48.44831), new Vector3(0, 0, 72.9998245));
             Objects.Create(1278261455, new Vector3(1734.05273, 2504.34888, 50.9083824), new Vector3(0, 0, 75.999794));
             Objects.Create(1278261455, new Vector3(1709.42065, 2481.77832, 48.2908974), new Vector3(0, -0, 135.999557));
             Objects.Create(1278261455, new Vector3(1709.46826, 2481.77173, 50.8878632), new Vector3(0, -0, 132.999756));
             Objects.Create(1278261455, new Vector3(1676.61914, 2481.62061, 48.3251762), new Vector3(0, 0, 47.9999046));
             Objects.Create(1278261455, new Vector3(1676.57727, 2481.66235, 50.6384277), new Vector3(0, 0, 45.9999542));
             Objects.Create(1278261455, new Vector3(1646.08728, 2489.72437, 48.36861), new Vector3(0, -0, 94.99981));
             Objects.Create(1278261455, new Vector3(1646.07471, 2489.689, 50.7237625), new Vector3(0, -0, 93.99969));
             Objects.Create(1278261455, new Vector3(1620.96326, 2510.81421, 48.4436531), new Vector3(0, 0, 3.00000024));
             Objects.Create(1278261455, new Vector3(1620.96863, 2510.75244, 50.68359), new Vector3(0, 0, 3.00000024));
             Objects.Create(1278261455, new Vector3(1610.37317, 2537.29126, 49.4368248), new Vector3(0, 0, 43.9999275));
             Objects.Create(1278261455, new Vector3(1610.39868, 2537.287, 48.4066467), new Vector3(0, 0, 44.9999275));
             Objects.Create(1278261455, new Vector3(1762.402, 2520.7334, 48.5598564), new Vector3(0, -0, 166.9993));
             Objects.Create(1278261455, new Vector3(1762.47546, 2520.86743, 50.9883347), new Vector3(0, -0, 167.999557));
             Objects.Create(1130240275, new Vector3(1674.98157, 2566.53833, 48.7885475), new Vector3(0, 0, -54.9995155));
             Objects.Create(1130240275, new Vector3(1701.60852, 2566.54834, 48.5110245), new Vector3(0, 0, -47.9998627));
             Objects.Create(1130240275, new Vector3(1737.068, 2564.19824, 47.64295), new Vector3(0, 0, -44.9999237));
             Objects.Create(1130240275, new Vector3(1763.99048, 2526.63965, 47.77891), new Vector3(0, -0, -161.00148));
             Objects.Create(1130240275, new Vector3(1733.15222, 2504.59, 48.2212868), new Vector3(0, -0, 115.999458));
             Objects.Create(1130240275, new Vector3(1764.02832, 2526.73, 48.65086), new Vector3(0, -0, -159.001053));
             Objects.Create(1130240275, new Vector3(1762.742, 2522.002, 48.305603), new Vector3(0, -0, 129.99971));
             Objects.Create(1130240275, new Vector3(1710.19958, 2482.55176, 48.3452072), new Vector3(0, -0, 97.99834));
             Objects.Create(1130240275, new Vector3(1675.65527, 2482.58447, 49.09255), new Vector3(0, 0, 80.9998856));
             Objects.Create(1130240275, new Vector3(1647.41223, 2489.825, 49.1647568), new Vector3(0, 0, 60.99989));
             Objects.Create(1130240275, new Vector3(1620.88184, 2511.74463, 48.70012), new Vector3(0, 0, 46.9999161));
             Objects.Create(1130240275, new Vector3(1611.30908, 2536.36475, 48.7229233), new Vector3(0, 0, 9.999991));*/
        }
    }
}