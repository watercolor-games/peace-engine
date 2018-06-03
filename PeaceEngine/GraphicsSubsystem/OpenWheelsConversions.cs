using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenWheels;
using OpenWheels.Rendering;

namespace Plex.Engine.GraphicsSubsystem
{
    public static class OpenWheelsConversions
    {
        public static Vector2 ToNum(this Microsoft.Xna.Framework.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static OpenWheels.RectangleF ToOw(this Plex.Engine.RectangleF rect)
        {
            return new OpenWheels.RectangleF(rect.TopLeft.ToNum(), rect.Size.ToNum());
        }

        public static Plex.Engine.RectangleF ToPe(this OpenWheels.RectangleF rect)
        {
            return new Plex.Engine.RectangleF(rect.TopLeft.ToMg(), rect.Size.ToMg());
        }

        public static Microsoft.Xna.Framework.Vector2 ToMg(this System.Numerics.Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Vector2(vector.X, vector.Y);
        }

        public static BlendState ToOw(this Microsoft.Xna.Framework.Graphics.BlendState bullshit)
        {
            if (bullshit == Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend)
                return BlendState.AlphaBlend;
            else
                return BlendState.Opaque;
        }

        public static SamplerState ToOw(this Microsoft.Xna.Framework.Graphics.SamplerState shitshit)
        {
            if (shitshit == Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp)
                return SamplerState.PointClamp;
            if (shitshit == Microsoft.Xna.Framework.Graphics.SamplerState.PointWrap)
                return SamplerState.PointWrap;
            if (shitshit == Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp)
                return SamplerState.LinearClamp;
            if (shitshit == Microsoft.Xna.Framework.Graphics.SamplerState.LinearWrap)
                return SamplerState.LinearWrap;
            if (shitshit == Microsoft.Xna.Framework.Graphics.SamplerState.AnisotropicClamp)
                return SamplerState.AnisotropicClamp;
            if (shitshit == Microsoft.Xna.Framework.Graphics.SamplerState.AnisotropicWrap)
                return SamplerState.AnisotropicWrap;
            throw new ArgumentException("This SamplerState is not supported by the OpenWheels rendering backend.");
        }

        public static Vector3 ToNum(this Microsoft.Xna.Framework.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Rectangle ToOw(this Microsoft.Xna.Framework.Rectangle rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Microsoft.Xna.Framework.Rectangle ToMg(this Rectangle rect)
        {
            return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Color ToOw(this Microsoft.Xna.Framework.Color col)
        {
            return new Color(col.PackedValue);
        }
    }

}
