#define DO_RENDEROFFSETS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = Plex.Engine.RectangleF;
using Plex.Engine.TextRenderers;

namespace Plex.Engine.GraphicsSubsystem
{
    /// <summary>
    /// Encapsulates a <see cref="GraphicsDevice"/> and <see cref="SpriteBatch"/> and contains methods for easily rendering various objects using those encapsulated objects. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     <para>The <see cref="GraphicsContext"/> class employs scissor testing in all of its draw calls. This makes it so that any data rendering outside the scissor rectangle (defined by the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/> and <see cref="Height"/> properties) will be clipped and not rendered to the screen.</para>
    ///     <para>Also, apart from the <see cref="X"/> and <see cref="Y"/> properties of the graphics context, any X/Y coordinate pairs are relative to the coordinates of the scissor rectangle. So, the coordinates (0,5) refer to <see cref="X"/>+0,<see cref="Y"/>+5.</para>
    /// </remarks>
    /// <seealso cref="RasterizerState.ScissorTestEnable"/>
    /// <seealso cref="GraphicsDevice.ScissorRectangle"/>
    /// <seealso cref="GraphicsDevice"/>
    /// <seealso cref="SpriteBatch"/>
    /// <threadsafety static="true" instance="false"/>
    public sealed class GraphicsContext
    {
        private GraphicsDevice _device = null;
        private MgBatcher _batcher = null;
        private SpriteBatch _spriteBatch = null;
        
        public Rectangle ScissorRectangle { get => _batcher.ScissorRect.ToMg(); set => _batcher.ScissorRect = value.ToOw(); }

        public int X => ScissorRectangle.X;
        public int Y => ScissorRectangle.Y;
        public int Width => (ScissorRectangle == Rectangle.Empty) ? _device.Viewport.Width : ScissorRectangle.Width;
        public int Height => (ScissorRectangle == Rectangle.Empty) ? _device.Viewport.Height : ScissorRectangle.Height;

#if DO_RENDEROFFSETS
        public float RenderOffsetX { get; set; }
        public float RenderOffsetY { get; set; }
#else
        private float _killme, _killyou = 0;

            public float RenderOffsetX { get => 0; set => _killme = value; }
        public float RenderOffsetY { get => 0; set => _killyou = value; }

#endif

        private Texture2D _white = null;
        private string _whiteUid = null;

        private Dictionary<string, int> _textureIDMap = new Dictionary<string, int>();

        public GraphicsContext(GraphicsDevice device)
        {
            _spriteBatch = new SpriteBatch(device);
            _device = device;
            _batcher = new MgBatcher(_device);
            _white = CreateTexture(1, 1);
            _white.SetData(new uint[] { 0xffffffff });
            _white.Name = $"white_{Guid.NewGuid().ToString()}";
            _whiteUid = _white.Name;
            _batcher.RegisterTexture(_white);
        }

        private string getID(Texture2D tex)
        {
            if (tex == null)
                return _whiteUid;
            if (string.IsNullOrWhiteSpace(tex.Name))
                tex.Name = Guid.NewGuid().ToString();
            _batcher.RegisterTexture(tex);
            return tex.Name;
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            _batcher.Finish();
            _device.SetRenderTarget(target);
            if (target == null)
                ScissorRectangle = OpenWheels.Rectangle.Unit.ToMg();
            else
                ScissorRectangle = new Rectangle(0, 0, target.Width, target.Height);
            _batcher.Start();
        }

        internal void StartFrame(BlendState blendState, SamplerState samplerState)
        {
            _batcher.SamplerState = samplerState.ToOw();
            _batcher.BlendState = blendState.ToOw();
            _batcher.Start();
        }

        internal void EndFrame()
        {
            _batcher.Finish();
        }

        public RenderTarget2D CreateRenderTarget(int width, int height)
        {
            return new RenderTarget2D(_device, width, height, false, _device.PresentationParameters.BackBufferFormat, _device.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
        }

        public void DrawLine(Vector2 a, Vector2 b, float width, Color color, Texture2D texture = null)
        {
            _batcher.SetTexture(getID(texture));
            _batcher.DrawLine(a.ToNum() - new Vector2(RenderOffsetX,RenderOffsetY).ToNum(), b.ToNum() - new Vector2(RenderOffsetX,RenderOffsetY).ToNum(), color.ToOw(), width);
        }

        public Texture2D CreateTexture(int w, int h)
        {
            return new Texture2D(_device, w, h);
        }

        public void Clear(Color color)
        {
            FillRectangle(new RectangleF(0, 0, Width, Height), color);
        }

        public void FillRectangle(float x, float y, float w, float h, Texture2D texture, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(x, y, w, h, Color.White, texture, layout);
        }

        public void DrawCircle(Vector2 center, float radius, Color color, Texture2D texture = null)
        {
            _batcher.SetTexture(getID(texture));
            _batcher.FillCircle(center.ToNum() - new Vector2(RenderOffsetX, RenderOffsetY).ToNum(), radius, color.ToOw(), 180);
        }

        public void FillRectangle(RectangleF rect, Texture2D texture)
        {
            FillRectangle(rect, texture);
        }

        public void FillRectangle(Vector2 pos, Vector2 size, Color color, Texture2D texture = null, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(pos.X, pos.Y, size.X, size.Y), color, texture, layout);
        }

        public void FillRectangle(Vector2 pos, Vector2 size, Texture2D texture, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(pos.X, pos.Y, size.X, size.Y), Color.White, texture, layout);
        }


        public void FillRectangle(RectangleF rect, Color color, Texture2D texture = null, ImageLayout layout = ImageLayout.Stretch)
        {
            _batcher.SetTexture(getID(texture));

            rect = new RectangleF(rect.X - RenderOffsetX, rect.Y - RenderOffsetY, rect.Width, rect.Height);

            float tw = (texture == null) ? rect.Width : texture.Width;
            float th = (texture == null) ? rect.Height : texture.Height;

            switch (layout)
            {
                case ImageLayout.None:
                    _batcher.FillRect(new RectangleF(rect.X+X, rect.Y+Y, (texture == null) ? rect.Width : texture.Width, (texture == null) ? rect.Height : texture.Height).ToOw(), color.ToOw());
                    break;
                case ImageLayout.Stretch:
                    _batcher.FillRect(new RectangleF(rect.X+X,rect.Y+Y,rect.Width,rect.Height).ToOw(), color.ToOw());
                    break;
                case ImageLayout.Center:
                    _batcher.FillRect(new RectangleF(X+rect.X + ((rect.Width - tw) / 2), Y+rect.Y + ((rect.Height - th) / 2), tw, th).ToOw(), color.ToOw());
                    break;
                case ImageLayout.Zoom:

                    float scale = Math.Min(rect.Width / tw, rect.Height / th);

                    var scaleWidth = (tw * scale);
                    var scaleHeight = (th * scale);

                    _batcher.FillRect(new RectangleF(X+rect.X + ((rect.Width - scaleWidth) / 2), Y+rect.Y + ((rect.Height - scaleHeight) / 2), scaleWidth, scaleHeight).ToOw(), color.ToOw());
                    break;
            }
        }

        public void FillRectangle(float x, float y, float w, float h, Color color, Texture2D texture = null, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(x, y, w, h), color, texture, layout);
        }

        public void DrawString(string text, float x, float y, Color color, SpriteFont font, TextAlignment alignment = TextAlignment.Left, int wrapWidth = 0, WrapMode mode = WrapMode.None)
        {
            DrawString(text, new Vector2(x, y), color, font, alignment, wrapWidth, mode);
        }

        public void DrawString(string text, Vector2 pos, Color color, SpriteFont font, TextAlignment alignment = TextAlignment.Left, int wrapWidth = 0, WrapMode mode = WrapMode.None)
        {
            if (string.IsNullOrWhiteSpace(text) || color.A == 0)
                return;
            if (mode == WrapMode.None || wrapWidth == 0)
            {
                DrawString(font, text, pos, color);
                return;
            }
            var wrap = TextRenderer.WrapText(font, text, wrapWidth, mode).Split('\n');
            for(int i = 0; i < wrap.Length; i++)
            {
                string line = wrap[i];
                var measure = font.MeasureString(line);
                switch(alignment)
                {
                    case TextAlignment.Center:
                        DrawString(font, line, new Vector2(pos.X + ((wrapWidth - measure.X) / 2), pos.Y + (font.LineSpacing * i)), color);
                        break;
                    case TextAlignment.Left:
                        DrawString(font, line, new Vector2(pos.X, pos.Y + (font.LineSpacing * i)), color);
                        break;
                    case TextAlignment.Right:
                        DrawString(font, line, new Vector2(pos.X + (wrapWidth - measure.X), pos.Y + (font.LineSpacing * i)), color);
                        break;
                }
            }
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, Color color)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            if (color.A == 0)
                return;

            _batcher.Finish();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, (_batcher.Renderer as SerenityRenderer).NoScissor, null);

            var pos = new Vector2((position.X + X) + RenderOffsetX, (position.Y + Y) + RenderOffsetY);

            _spriteBatch.DrawString(font, text, pos, color);

            _spriteBatch.End();

            _batcher.Start();
        }
    }

    public enum ImageLayout
    {
        None,
        Stretch,
        Zoom,
        Center,
        Tile
    }
}
