using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Module.Spawners;

namespace GVRP.Module.Jobs.Bus
{
    public class BusStop
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public bool Last { get; set; }
    }

    public class BusLine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public readonly Dictionary<int, BusStop> Stops;

        public BusLine()
        {
            Stops = new Dictionary<int, BusStop>();
        }

        public void AddStop(Vector3 position, bool last = false)
        {
            var busLine = new BusStop
            {
                Id = Stops.Count,
                Last = last,
                Position = position
            };
            Stops.Add(busLine.Id, busLine);
        }

        public BusStop GetStop(int id)
        {
            return Stops.ContainsKey(id) ? Stops[id] : null;
        }
    }

    public class BusJob
    {
        public static BusJob Instance { get; } = new BusJob();

        private readonly Dictionary<int, BusLine> lines;

        private BusJob()
        {
            lines = new Dictionary<int, BusLine>();
        }

        public void Load()
        {
            //Linie 1
            var line = new BusLine
            {
                Id = 0,
                Name = "Linie 1"
            };
            line.AddStop(new Vector3(306.367, -765.648, 28.767));
            line.AddStop(new Vector3(202.736, -1203.227, 28.738));
            line.AddStop(new Vector3(-108.753, -1687.792, 28.786));
            line.AddStop(new Vector3(-1033.442, -2730.845, 19.634));
            line.AddStop(new Vector3(-291.848, -1845.296, 25.606));
            line.AddStop(new Vector3(51.987, -1535.694, 28.748));
            line.AddStop(new Vector3(430.606, -643.400, 28.075), true);
            lines.Add(line.Id, line);

            //Linie 2
            var line2 = new BusLine
            {
                Id = 1,
                Name = "Linie 2"
            };
            line2.AddStop(new Vector3(306.483, -766.289, 28.778));
            line2.AddStop(new Vector3(273.885, -590.928, 42.722));
            line2.AddStop(new Vector3(-30.416, -111.083, 56.664));
            line2.AddStop(new Vector3(-499.982, 20.542, 44.778));
            line2.AddStop(new Vector3(-694.224, -6.525, 37.689));
            line2.AddStop(new Vector3(-930.447, -125.977, 37.159));
            line2.AddStop(new Vector3(-1273.410, -316.519, 36.394));
            line2.AddStop(new Vector3(-1526.257, -465.771, 34.850));
            line2.AddStop(new Vector3(-1564.656, -662.064, 28.514));
            line2.AddStop(new Vector3(-1143.279, -709.612, 20.688));
            line2.AddStop(new Vector3(-692.056, -668.061, 30.363));
            line2.AddStop(new Vector3(-504.565, -668.180, 32.551));
            line2.AddStop(new Vector3(-175.459, -707.023, 34.29));
            line2.AddStop(new Vector3(430.606, -643.400, 28.075), true);
            lines.Add(line2.Id, line2);

            //ehemals Linie 4
            //Linie 3
            var line3 = new BusLine
            {
                Id = 2,
                Name = "Linie 3"
            };
            line3.AddStop(new Vector3(495.6489, -1004.153, 28.64232));
            line3.AddStop(new Vector3(257.489, -1123.821, 28.78614));
            line3.AddStop(new Vector3(138.4006, -948.8051, 29.1783));
            line3.AddStop(new Vector3(273.8791, -590.9042, 42.72399));
            line3.AddStop(new Vector3(245.6438, -349.2294, 43.90818));
            line3.AddStop(new Vector3(-143.5916, -294.7251, 39.43053));
            line3.AddStop(new Vector3(-340.3819, -648.0403, 31.63549));
            line3.AddStop(new Vector3(-994.461, -678.8943, 21.82755));
            line3.AddStop(new Vector3(-808.293, -1065.319, 11.64871));
            line3.AddStop(new Vector3(-716.6667, -1212.105, 10.08242));
            line3.AddStop(new Vector3(-250.6907, -1150.525, 22.48399));
            line3.AddStop(new Vector3(105.596, -807.7036, 30.85506));
            line3.AddStop(new Vector3(430.606, -643.400, 28.075), true);
            lines.Add(line3.Id, line3);

            //ehemals Linie 3
            //Linie 4
            var line4 = new BusLine
            {
                Id = 3,
                Name = "Linie 4"
            };
            line4.AddStop(new Vector3(1559.68311, 864.535645, 76.41256));
            line4.AddStop(new Vector3(2550.524, 1535.87573, 29.5694885));
            line4.AddStop(new Vector3(2578.563, 2641.842, 37.127533));
            line4.AddStop(new Vector3(2892.53027, 4430.602, 47.3101044));
            line4.AddStop(new Vector3(1651.57654, 6419.79346, 27.7276382));
            line4.AddStop(new Vector3(57.0786629, 6460.081, 30.29412));
            line4.AddStop(new Vector3(-715.860046, 5545.92432, 36.24978));
            line4.AddStop(new Vector3(-1526.63147, 5002.70654, 61.1790466));
            line4.AddStop(new Vector3(-2275.65869, 4262.69238, 43.05827));
            line4.AddStop(new Vector3(-2733.23486, 2312.02637, 16.559864));
            line4.AddStop(new Vector3(-3127.544, 1128.9491, 19.6663113));
            line4.AddStop(new Vector3(-3005.64282, 428.130219, 14.0787745));
            line4.AddStop(new Vector3(-2175.14819, -366.095245, 12.0874767));
            line4.AddStop(new Vector3(-1565.295, -660.5519, 29.04856));
            line4.AddStop(new Vector3(-1478.558, -632.3318, 30.49643));
            line4.AddStop(new Vector3(-695.8966, -668.129, 30.70399));
            line4.AddStop(new Vector3(-508.6643, -668.4409, 33.02017));
            line4.AddStop(new Vector3(-175.8523, -707.1271, 34.28528));
            line4.AddStop(new Vector3(430.606, -643.400, 28.075), true);
            lines.Add(line4.Id, line4);

            // Bus stations
            /*ObjectSpawn.Create(1888204845, new Vector3(199.064865, -1201.474, 28.2633324),
                new Vector3(1.00178095E-05, 5.00895658E-06, 93.57354));
            ObjectSpawn.Create(2142033519, new Vector3(-1034.49072, -2732.804, 19.0355434),
                new Vector3(1.00177785E-05, 5.00895567E-06, 149.224045));
            ObjectSpawn.Create(1681727376, new Vector3(-291.608765, -1848.25977, 25.2099857),
                new Vector3(1.00193711E-05, 4.249998, 179.750412));
            ObjectSpawn.Create(1888204845, new Vector3(117.965439, -1645.14392, 28.291584),
                new Vector3(1.00178486E-05, -5.00895567E-06, -150.924469));
            ObjectSpawn.Create(2142033519, new Vector3(406.575378, -1479.35315, 28.336235),
                new Vector3(1.00178477E-05, -5.008956E-06, -150.524414));
            ObjectSpawn.Create(2142033519, new Vector3(276.390045, -592.0393, 42.27006),
                new Vector3(9.234591E-06, 1.50000012, -113.74984));
            ObjectSpawn.Create(2142033519, new Vector3(-29.78868, -108.546906, 56.1631622),
                new Vector3(8.19247E-06, 1.124968, -22.6249676));
            ObjectSpawn.Create(1681727376, new Vector3(-1275.1073, -314.51004, 35.84157),
                new Vector3(1.01394289E-05, -0.749813139, 28.2749119));
            ObjectSpawn.Create(1681727376, new Vector3(-1145.28552, -711.7196, 20.231142),
                new Vector3(1.31823417E-05, -1.04978132, 130.37468));
            ObjectSpawn.Create(1681727376, new Vector3(-811.35376, -1067.00232, 11.20321),
                new Vector3(1.37754905E-05, -1.99999714, 117.899727));
            ObjectSpawn.Create(1888204845, new Vector3(-563.605835, -1233.313, 14.4148254),
                new Vector3(8.784608E-06, 5.000035, -126.97477));
            ObjectSpawn.Create(2142033519, new Vector3(-250.862762, -1153.16309, 22.0529671),
                new Vector3(1.00178413E-05, 5.00895567E-06, 179.075745));
            ObjectSpawn.Create(2142033519, new Vector3(141.964127, -1028.8407, 28.3505936),
                new Vector3(1.00178322E-05, 5.00895567E-06, 161.475128));
            ObjectSpawn.Create(2142033519, new Vector3(-1566.37439, -665.1469, 27.94571),
                new Vector3(1.00178049E-05, 5.008955E-06, 140.625122));
            ObjectSpawn.Create(1681727376, new Vector3(-173.469467, -714.252441, 33.5234032),
                new Vector3(1.00178468E-05, 5.00895567E-06, 160.000381));
            ObjectSpawn.Create(1681727376, new Vector3(1559.68311, 864.535645, 76.41256),
                new Vector3(1.00178722E-05, -5.008956E-06, -118.7497));
            ObjectSpawn.Create(1681727376, new Vector3(2550.524, 1535.87573, 29.5694885),
                new Vector3(1.00178331E-05, 5.0089543E-06, -94.84895));
            ObjectSpawn.Create(1681727376, new Vector3(2578.563, 2641.842, 37.127533),
                new Vector3(1.56406386E-05, 1.57510483, -70.97405));
            ObjectSpawn.Create(1681727376, new Vector3(2892.53027, 4430.602, 47.3101044),
                new Vector3(9.875607E-06, -1.6001035, -159.99942));
            ObjectSpawn.Create(1681727376, new Vector3(1651.57654, 6419.79346, 27.7276382),
                new Vector3(9.772569E-06, -2.24999571, -15.0000048));
            ObjectSpawn.Create(1681727376, new Vector3(57.0786629, 6460.081, 30.29412),
                new Vector3(1.00178277E-05, -5.008955E-06, 42.574604));
            ObjectSpawn.Create(1681727376, new Vector3(-715.860046, 5545.92432, 36.24978),
                new Vector3(1.02193108E-05, -0.6748313, 33.74995));
            ObjectSpawn.Create(1681727376, new Vector3(-1526.63147, 5002.70654, 61.1790466),
                new Vector3(1.0017864E-05, -5.00895567E-06, 46.5498352));
            ObjectSpawn.Create(1681727376, new Vector3(-2275.65869, 4262.69238, 43.05827),
                new Vector3(2.16040389E-05, -3.75000072, 53.84959));
            ObjectSpawn.Create(1681727376, new Vector3(-2733.23486, 2312.02637, 16.559864),
                new Vector3(1.02515287E-05, 1.9999969, 63.7498322));
            ObjectSpawn.Create(1681727376, new Vector3(-3127.544, 1128.9491, 19.6663113),
                new Vector3(1.00178941E-05, 5.00895658E-06, 84.99978));
            ObjectSpawn.Create(1681727376, new Vector3(-3005.64282, 428.130219, 14.0787745),
                new Vector3(1.00178777E-05, -5.00895567E-06, 84.52487));
            ObjectSpawn.Create(1681727376, new Vector3(-2175.14819, -366.095245, 12.0874767),
                new Vector3(1.00178422E-05, -5.00895567E-06, 168.750214));

            // Linie 4
            ObjectSpawn.Create(2142033519, new Vector3(492.822937, -1008.00043, 26.9186592),
                new Vector3(0, 0, 88.2399292));
            ObjectSpawn.Create(1888204845, new Vector3(141.318024, -949.765442, 28.7920208),
                new Vector3(-0, 0, -110.144989));
            ObjectSpawn.Create(2142033519, new Vector3(246.46376, -346.404053, 43.4663773),
                new Vector3(0, 0, -18.0670795));
            ObjectSpawn.Create(-1022684418, new Vector3(-144.480972, -290.868011, 38.9585724),
                new Vector3(-0, 0, -170.751831));
            ObjectSpawn.Create(-1022684418, new Vector3(-146.534912, -297.216156, 38.4097977),
                new Vector3(1.04899425e-06, -1.01777744e-13, 55.3453026));  
            ObjectSpawn.Create(1888204845, new Vector3(-340.424316, -644.06897, 31.3517857),
                new Vector3(0, 0, 1.45468557));
            ObjectSpawn.Create(-1022684418, new Vector3(-991.560608, -674.46521, 21.3999825),
                new Vector3(-0, 0, -174.638138));
            ObjectSpawn.Create(-1022684418, new Vector3(-997.817932, -678.544861, 20.8819561),
                new Vector3(0, 0, 60.8097191));
            ObjectSpawn.Create(1681727376, new Vector3(-719.112427, -1214.05505, 9.6319809),
                new Vector3(-0, 0, 129.986191));
            ObjectSpawn.Create(2142033519, new Vector3(104.042793, -811.066772, 30.4160957),
                new Vector3(-0, 0, 160.124344));*/
        }

        public BusStop GetNearestStop(Vector3 position)
        {
            return (from line   in lines
                from stop in line.Value.Stops
                where stop.Value.Position.DistanceTo(position) <= 2.0f
                select stop.Value).FirstOrDefault();
        }

        public BusLine GetLine(int id)
        {
            return lines.ContainsKey(id) ? lines[id] : null;
        }
    }
}