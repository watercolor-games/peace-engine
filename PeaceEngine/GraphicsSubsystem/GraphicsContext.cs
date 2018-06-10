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
using Plex.Engine.Config;

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
        [Dependency]
        private ConfigManager _config = null;

        private GraphicsDevice _device = null;
        private MgBatcher _batcher = null;
        private SpriteBatch _spriteBatch = null;
        
        public Rectangle ScissorRectangle { get => _batcher.ScissorRect.ToMg(); set => _batcher.ScissorRect = value.ToOw(); }

        public int X => ScissorRectangle.X;
        public int Y => ScissorRectangle.Y;
        public int Width => (_batcher.ScissorRect == OpenWheels.Rectangle.Empty) ? _device.Viewport.Width : ScissorRectangle.Width;
        public int Height => (_batcher.ScissorRect == OpenWheels.Rectangle.Empty) ? _device.Viewport.Height : ScissorRectangle.Height;

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

        private float _circleDefaultStepSize = 0;

        public GraphicsContext(GraphicsDevice device)
        {
            GameLoop.GetInstance().Inject(this);

            _spriteBatch = new SpriteBatch(device);
            _device = device;
            _batcher = new MgBatcher(_device);
            _white = CreateTexture(1, 1);
            _white.SetData(new uint[] { 0xffffffff });
            _white.Name = $"white_{Guid.NewGuid().ToString()}";
            _whiteUid = _white.Name;
            _batcher.RegisterTexture(_white);
            _circleDefaultStepSize = _batcher.GetCircleStepSize(OpenWheels.Rendering.Batcher.RightStartAngle, OpenWheels.Rendering.Batcher.RightEndAngle, 180);
        }



        private string getID(Texture2D tex)
        {
            if (tex == null)
                throw new ArgumentNullException(nameof(tex));
            if (string.IsNullOrWhiteSpace(tex.Name))
                tex.Name = Guid.NewGuid().ToString();
            _batcher.RegisterTexture(tex);
            return tex.Name;
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            _batcher.Finish();
            _device.SetRenderTarget(target);
            ScissorRectangle = Rectangle.Empty;
            RenderOffsetX = 0;
            RenderOffsetY = 0;
            _batcher.Start();
        }

        private Vector2 ToBounds(Vector2 point)
        {
            return new Vector2((X+point.X) + RenderOffsetX, (Y+point.Y) + RenderOffsetY);
        }

        private Rectangle ToBounds(Rectangle rect)
        {
            return new Rectangle((X+rect.X) + (int)RenderOffsetX, (Y+rect.Y) + (int)RenderOffsetY, rect.Width, rect.Height);
        }

        private RectangleF ToBounds(RectangleF rect)
        {
            return new RectangleF((X+rect.X) + RenderOffsetX, (Y+rect.Y) + RenderOffsetY, rect.Width, rect.Height);
        }

        internal void StartFrame(BlendState blendState)
        {
            _batcher.ClearTextures();
            _sampler = _config.GetValue("anisotropicFiltering", true) ? SamplerState.AnisotropicClamp : SamplerState.LinearClamp;
            _batcher.SamplerState = _sampler.ToOw();
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

        public void DrawLine(Vector2 a, Vector2 b, float width, Color color)
        {
            DrawLine(a, b, width, _white, color);
        }

        public void DrawLine(Vector2 a, Vector2 b, float width, Texture2D texture, Color color)
        {
            if (texture == null)
                return;
            if (color.A == 0)
                return;
            _batcher.SetTexture(getID(texture));
            _batcher.DrawLine(ToBounds(a).ToNum(), ToBounds(b).ToNum(), color.ToOw(), width);
        }

        public Texture2D CreateTexture(int w, int h)
        {
            return new Texture2D(_device, w, h);
        }

        public void Clear(Color color)
        {
            FillRectangle(new RectangleF(0, 0, Width, Height), color);
        }

        public void FillCircle(Vector2 center, float radius, Color color)
        {
            FillCircle(center, radius, _white, color);
        }

        public void FillCircle(Vector2 center, float radius, Texture2D texture, Color color)
        {
            if (texture == null)
                return;

            if (color.A == 0)
                return;

            _batcher.SetTexture(getID(texture));
            float circumference = (float)(Math.PI * (Math.Pow(radius, 2)));
            int triangles = (int)Math.Round(circumference * _circleDefaultStepSize);
            if (triangles < 1)
            {
                _batcher.FillRect(new RectangleF(ToBounds(new Vector2(center.X - radius, center.Y - radius)), new Vector2(radius * 2, radius * 2)).ToOw(), color.ToOw());
            }
            else
            {
                _batcher.FillCircle(ToBounds(center).ToNum(), radius, color.ToOw(), triangles);
            }
        }

        #region Rectangle -> Filled

        public void FillRectangle(RectangleF rect, Color color)
        {
            FillRectangle(rect, _white, color);
        }

        public void FillRectangle(Vector2 pos, Vector2 size, Color color)
        {
            FillRectangle(new RectangleF(pos, size), color);
        }

        public void FillRectangle(Vector2 pos, Vector2 size, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(pos, size), texture, color, layout);
        }

        public void FillRectangle(Rectangle rect, Color color)
        {
            FillRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), color);
        }

        public void FillRectangle(Rectangle rect, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), texture, color, layout); ;
        }

        public void FillRectangle(int x, int y, int w, int h, Color color)
        {
            FillRectangle(new RectangleF(x, y, w, h), color);
        }

        public void FillRectangle(float x, float y, float w, float h, Color color)
        {
            FillRectangle(new RectangleF(x, y, w, h), color);
        }

        public void FillRectangle(int x, int y, int w, int h, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(x, y, w, h), texture, color, layout);
        }

        public void FillRectangle(float x, float y, float w, float h, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRectangle(new RectangleF(x, y, w, h), texture, color, layout);
        }

        public void FillRectangle(RectangleF rect, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            if (texture == null)
                return;

            if (color.A == 0)
                return;


            _batcher.SetTexture(getID(texture));

            rect = ToBounds(rect);

            float tw = (texture == null) ? rect.Width : texture.Width;
            float th = (texture == null) ? rect.Height : texture.Height;

            switch (layout)
            {
                case ImageLayout.None:
                    _batcher.FillRect(new RectangleF(rect.X, rect.Y, (texture == null) ? rect.Width : texture.Width, (texture == null) ? rect.Height : texture.Height).ToOw(), color.ToOw());
                    break;
                case ImageLayout.Stretch:
                    _batcher.FillRect(new RectangleF(rect.X,rect.Y,rect.Width,rect.Height).ToOw(), color.ToOw());
                    break;
                case ImageLayout.Center:
                    _batcher.FillRect(new RectangleF(rect.X + ((rect.Width - tw) / 2), rect.Y + ((rect.Height - th) / 2), tw, th).ToOw(), color.ToOw());
                    break;
                case ImageLayout.Zoom:

                    float scale = Math.Min(rect.Width / tw, rect.Height / th);

                    var scaleWidth = (tw * scale);
                    var scaleHeight = (th * scale);

                    _batcher.FillRect(new RectangleF(rect.X + ((rect.Width - scaleWidth) / 2), rect.Y + ((rect.Height - scaleHeight) / 2), scaleWidth, scaleHeight).ToOw(), color.ToOw());
                    break;
            }
        }

        #endregion

        #region Rectangle -> Filled -> Rounded

        public void FillRoundedRectangle(RectangleF rect, float radius, Color color)
        {
            FillRoundedRectangle(rect, radius, _white, color);
        }

        public void FillRoundedRectangle(Vector2 pos, Vector2 size, float radius, Color color)
        {
            FillRoundedRectangle(new RectangleF(pos, size), radius, color);
        }

        public void FillRoundedRectangle(Vector2 pos, Vector2 size, float radius, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRoundedRectangle(new RectangleF(pos, size), radius, texture, color, layout);
        }

        public void FillRoundedRectangle(Rectangle rect, float radius, Color color)
        {
            FillRoundedRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), radius, color);
        }

        public void FillRoundedRectangle(Rectangle rect, float radius, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRoundedRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), radius, texture, color, layout); ;
        }

        public void FillRoundedRectangle(int x, int y, int w, int h, float radius, Color color)
        {
            FillRoundedRectangle(new RectangleF(x, y, w, h), radius, color);
        }

        public void FillRoundedRectangle(float x, float y, float w, float h, float radius, Color color)
        {
            FillRoundedRectangle(new RectangleF(x, y, w, h), radius, color);
        }

        public void FillRoundedRectangle(int x, int y, int w, int h, float radius, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRoundedRectangle(new RectangleF(x, y, w, h), radius, texture, color, layout);
        }

        public void FillRoundedRectangle(float x, float y, float w, float h, float radius, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            FillRoundedRectangle(new RectangleF(x, y, w, h), radius, texture, color, layout);
        }

        public void FillRoundedRectangle(RectangleF rect, float radius, Texture2D texture, Color color, ImageLayout layout = ImageLayout.Stretch)
        {
            if (texture == null)
                return;

            if (color.A == 0)
                return;

            _batcher.SetTexture(getID(texture));

            rect = ToBounds(rect);

            float tw = (texture == null) ? rect.Width : texture.Width;
            float th = (texture == null) ? rect.Height : texture.Height;

            switch (layout)
            {
                case ImageLayout.None:
                    _batcher.FillRoundedRect(new RectangleF(rect.X, rect.Y, (texture == null) ? rect.Width : texture.Width, (texture == null) ? rect.Height : texture.Height).ToOw(), radius, 32, color.ToOw());
                    break;
                case ImageLayout.Stretch:
                    _batcher.FillRoundedRect(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height).ToOw(), radius,32, color.ToOw());
                    break;
                case ImageLayout.Center:
                    _batcher.FillRoundedRect(new RectangleF(rect.X + ((rect.Width - tw) / 2), rect.Y + ((rect.Height - th) / 2), tw, th).ToOw(), radius, 32, color.ToOw());
                    break;
                case ImageLayout.Zoom:

                    float scale = Math.Min(rect.Width / tw, rect.Height / th);

                    var scaleWidth = (tw * scale);
                    var scaleHeight = (th * scale);

                    _batcher.FillRoundedRect(new RectangleF(rect.X + ((rect.Width - scaleWidth) / 2), rect.Y + ((rect.Height - scaleHeight) / 2), scaleWidth, scaleHeight).ToOw(), radius, 32, color.ToOw());
                    break;
            }
        }

        #endregion


        private SamplerState _sampler = null;

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

            StartSpriteBatch();

            var wrap = TextRenderer.WrapText(font, text, wrapWidth, mode).Split('\n');
            for(int i = 0; i < wrap.Length; i++)
            {
                string line = wrap[i];
                var measure = font.MeasureString(line);
                switch(alignment)
                {
                    case TextAlignment.Center:
                        DrawStringInternal(font, line, new Vector2(pos.X + ((wrapWidth - measure.X) / 2), pos.Y + (font.LineSpacing * i)), color);
                        break;
                    case TextAlignment.Left:
                        DrawStringInternal(font, line, new Vector2(pos.X, pos.Y + (font.LineSpacing * i)), color);
                        break;
                    case TextAlignment.Right:
                        DrawStringInternal(font, line, new Vector2(pos.X + (wrapWidth - measure.X), pos.Y + (font.LineSpacing * i)), color);
                        break;
                }
            }

            EndSpriteBatch();
        }

        private RasterizerState GetRasterizerState()
        {
            if(_batcher.ScissorRect == OpenWheels.Rectangle.Empty)
            {
                return (_batcher.Renderer as SerenityRenderer).NoScissor;
            }
            else
            {
                return (_batcher.Renderer as SerenityRenderer).Scissor;
            }
        }

        private Rectangle _tempScissor = Rectangle.Empty;

        private void StartSpriteBatch()
        {
            _batcher.Finish();

            _tempScissor = _device.ScissorRectangle;

            _device.ScissorRectangle = (_batcher.ScissorRect == OpenWheels.Rectangle.Empty) ? _device.Viewport.Bounds : ScissorRectangle;

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, _sampler, DepthStencilState.None, GetRasterizerState(), null);
        }

        private void EndSpriteBatch()
        {
            _spriteBatch.End();

            _device.ScissorRectangle = _tempScissor;

            _batcher.Start();
        }

        public void DrawStringInternal(SpriteFont font, string text, Vector2 position, Color color)
        {
            var pos = new Vector2((position.X + _device.ScissorRectangle.X) + RenderOffsetX, (position.Y + _device.ScissorRectangle.Y) + RenderOffsetY);

            _spriteBatch.DrawString(font, text, pos, color);
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, Color color)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            if (color.A == 0)
                return;

            StartSpriteBatch();

            DrawStringInternal(font, text, position, color);

            EndSpriteBatch();
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
