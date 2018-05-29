using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Plex.Engine.GraphicsSubsystem
{
    public class SerenityRenderer : IRenderer
    {
        private GraphicsDevice device;

        private VertexBuffer _vbo = null;
        private IndexBuffer _ibo = null;

        
        public SerenityRenderer(GraphicsDevice gfx)
        {
            device = gfx;
            _vbo = new VertexBuffer(device, typeof(VertexPositionColorTexture), 2048, BufferUsage.WriteOnly);
            _ibo = new IndexBuffer(device, typeof(int), 4096, BufferUsage.WriteOnly);
        }

        public void BeginRender()
        {
        }

        public void DrawBatch(GraphicsState state, VertexPositionColorTexture[] vertexBuffer, int[] indexBuffer, int startIndex, int indexCount, object batchUserData)
        {
            if (state.UseScissorRect)
                device.ScissorRectangle = state.ScissorRect;
            else
                device.ScissorRectangle = GetViewport();

            var rasterizer = new RasterizerState();
            rasterizer.CullMode = CullMode.None;
            rasterizer.FillMode = FillMode.Solid;
            rasterizer.MultiSampleAntiAlias = true;
            rasterizer.ScissorTestEnable = state.UseScissorRect;

            device.BlendState = state.BlendState;
            device.SamplerStates[0] = state.SamplerState;

            var _effect = new BasicEffect(device);
            _effect.LightingEnabled = false;
            _effect.VertexColorEnabled = true;

            var tex = state.Texture;
            _effect.TextureEnabled = tex != null;
            _effect.Texture = (Texture2D)tex;

            _effect.Projection = Matrix.CreateOrthographicOffCenter(device.Viewport.Bounds, device.Viewport.MinDepth, device.Viewport.MaxDepth);
            _effect.View = Matrix.Identity;
            _effect.World = Matrix.Identity;

            _vbo.SetData(vertexBuffer);

            device.SetVertexBuffer(_vbo);
            _ibo.SetData(indexBuffer);

            device.Indices = _ibo;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, startIndex, indexCount/3);
            }
        }

        

        public void EndRender()
        {
        }

        public Point GetTextureSize(Texture2D texture)
        {
            if (texture == null)
                return new Point(1, 1);
            return new Point(texture.Width, texture.Height);
        }

        public Rectangle GetViewport()
        {
            return device.Viewport.Bounds;
        }
    }
}
