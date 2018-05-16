using System;

namespace WatercolorGames.Utilities
{
    public static class TerrainGen
    {
		// TODO get these constants out of the code serialised onto disk
		const int Width = 1000;
		const int Height = 1000;
		const int MinFeatures = 200;
		const int MaxFeatures = 300;
		const double MinHeight = 0.5;
		const double MaxHeight = 1.0;
		const int MinRadius = 60;
		const int MaxRadius = 140;
		const double WaterLevel = 2.75;
        public static double[] GenMap(int seed)
        {
            var map = new double[Width * Height];
            var r = new Random(seed);
            var nfeatures = r.Next(MinFeatures, MaxFeatures + 1);
            for (int i = 0; i < nfeatures; i++)
            {
                var height = r.NextDouble() * (MaxHeight - MinHeight) + MinHeight;
                var radius = r.Next(MinRadius, MaxRadius + 1);
                var cx = r.Next(0, Width);
                var cy = r.Next(0, Height);
                for (var y = Math.Max(0, cy - radius); y < Math.Min(cy + radius, Height); y++)
                    for (var x = Math.Max(0, cx - radius); x < Math.Min(cx + radius, Width); x++)
                        map[y * Width + x] += height * Math.Max(0, (radius - Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2))) / radius);
            }
            return map;
        }
    }
}

