using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using OpenWheels.Rendering;
using Vector2 = System.Numerics.Vector2;
using SamplerState = OpenWheels.Rendering.SamplerState;
using BlendState = OpenWheels.Rendering.BlendState;
using Matrix = Microsoft.Xna.Framework.Matrix;
using OpenWheels;
using Plex.Objects;

namespace Plex.Engine.GraphicsSubsystem
{
    public class SerenityRenderer : IRenderer
    {
        private int _nextTextureId;
        private int _nextFontId;

        public GraphicsDevice GraphicsDevice { get; }
        private readonly Dictionary<int, Texture2D> _textures;
        private readonly Dictionary<int, SpriteFont> _fonts;

        private readonly BasicEffect _effect;

        private GraphicsState _prevState = GraphicsState.Default;

        

        public readonly RasterizerState NoScissor = RasterizerState.CullNone;
        public readonly RasterizerState Scissor = new RasterizerState
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            ScissorTestEnable = true
        };


        public SerenityRenderer(GraphicsDevice gd)
        {
            GraphicsDevice = gd;
            _textures = new Dictionary<int, Texture2D>();
            _fonts = new Dictionary<int, SpriteFont>();

            // note that this effect already performs the 2D transformation
            // so we don't need it in the batcher
            _effect = new BasicEffect(gd);
        }

        public int AddTexture(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            var id = GetTextureId();
            _textures.Add(id, texture);
            return id;
        }

        private int GetTextureId()
        {
            var id = _nextTextureId;
            _nextTextureId++;
            return id;
        }

        public int AddFont(SpriteFont font)
        {
            var id = GetFontId();
            _fonts.Add(id, font);
            return id;
        }

        private int GetFontId()
        {
            var id = _nextFontId;
            _nextFontId++;
            return id;
        }

        #region IRenderer Implementation

        public Point2 GetTextureSize(int texture)
        {
            var tex = _textures[texture];
            return new Point2(tex.Width, tex.Height);
        }

        public Vector2 GetTextSize(string text, int font)
        {
            var sf = _fonts[font];
            var vec = sf.MeasureString(text);
            return new Vector2(vec.X, vec.Y);
        }

        public Rectangle GetViewport()
        {
            var rect = GraphicsDevice.Viewport.Bounds;
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void BeginRender()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
        }

        public void DrawBatch(GraphicsState state, Vertex[] vertexBuffer, int[] indexBuffer, int startIndex,
            int indexCount, object batchUserData)
        {
            SetGraphicsState(state);

            var vd = VertexPositionColorTexture.VertexDeclaration;
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexBuffer, 0, vertexBuffer.Length,
                    indexBuffer, startIndex, indexCount / 3, vd);
            }
        }

        public void EndRender()
        {
        }

        public void RemoveTexture(int id)
        {
            _textures.Remove(id);
        }

        public Texture2D GetTexture(int id)
        {
            return _textures[id];
        }

        public int GetTextureID(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            foreach(var key in _textures)
            {
                if (key.Value == texture)
                    return key.Key;
            }
            throw new ArgumentException("The texture was not found in the dictionary.");
        }

        #endregion

        #region Helper Methods

        private void SetGraphicsState(GraphicsState state)
        {
            switch (state.SamplerState)
            {
                case SamplerState.PointWrap:
                    GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.PointWrap;
                    break;
                case SamplerState.PointClamp:
                    GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp;
                    break;
                case SamplerState.LinearWrap:
                    GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.LinearWrap;
                    break;
                case SamplerState.LinearClamp:
                    GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp;
                    break;
                case SamplerState.AnisotropicWrap:
                    GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.AnisotropicWrap;
                    break;
                case SamplerState.AnisotropicClamp:
                    GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.AnisotropicClamp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (state.BlendState)
            {
                case BlendState.AlphaBlend:
                    GraphicsDevice.BlendState = Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend;
                    break;
                case BlendState.Opaque:
                    GraphicsDevice.BlendState = Microsoft.Xna.Framework.Graphics.BlendState.Opaque;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            GraphicsDevice.RasterizerState = (state.UseScissorRect) ? Scissor : NoScissor;

            if (state.UseScissorRect)
                GraphicsDevice.ScissorRectangle = state.ScissorRect.ToMg();

            if (state.Texture == -1)
                throw new InvalidOperationException("An attempt was made to render a polygon without a texture.");

            _effect.Alpha = 1f;
            _effect.FogEnabled = false;
            _effect.LightingEnabled = false;
            _effect.Projection = Matrix.CreateOrthographicOffCenter(GraphicsDevice.Viewport.Bounds, GraphicsDevice.Viewport.MinDepth, GraphicsDevice.Viewport.MaxDepth);
            _effect.View = Matrix.Identity;
            _effect.World = Matrix.Identity;
            _effect.TextureEnabled = true;
            _effect.Texture = _textures[state.Texture];
            _effect.VertexColorEnabled = true;


            _prevState = state;
        }

        #endregion
    }
}
