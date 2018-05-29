using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Plex.Engine.GraphicsSubsystem
{
    public static class MathHelpers
    {
        public static float InvLerp(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        
        public static Vector2 InvLerp(Vector2 a, Vector2 b, Vector2 v)
        {
            return new Vector2(InvLerp(a.X, b.X, v.X), InvLerp(a.Y, b.Y, v.Y));
        }

        public static float LinearMap(float value, float aFrom, float bFrom, float aTo, float bTo)
        {
            return MathHelper.Lerp(aTo, bTo, InvLerp(aFrom, bFrom, value));
        }

        public static Vector2 LinearMap(Vector2 value, RectangleF from, RectangleF to)
        {
            var normalized = (value.X - from.Left) / from.Width;
            var normalized1 = (value.Y - from.Top) / from.Height;
            return new Vector2(
                normalized * to.Width + to.Left,
                normalized1 * to.Height + to.Top);
        }

        public static RectangleF LinearMap(RectangleF value, RectangleF from, RectangleF to)
        {
            var tl = LinearMap(value.TopLeft, from, to);
            var br = LinearMap(value.BottomRight, from, to);
            return RectangleF.FromExtremes(tl.X, tl.Y, br.X, br.Y);
        }

        public static Matrix ToMonoGame(this System.Numerics.Matrix4x4 matrix)
        {
            return new Matrix(
                new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14),
                new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24),
                new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34),
                new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44));
        }
    }

    
}
