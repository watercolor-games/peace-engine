using Microsoft.Xna.Framework;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI
{
    public class GridListView : ListView
    {
        private const int _padH = 7;
        private const int _padV = 7;
        private const int _itemTextMaxWidth = 100;
        private const int _itemImageSize = 48;
        private const int _itemImageMargin = 6;
        private const int _itemHorizontalSpacing = 5;
        private const int _itemVerticalSpacing = 3;
        private const int _itemTextMargin = 4;
        private const int _itemHighlightPadH = 6;
        private const int _itemHighlightPadV = 3;

        public GridFlow GridFlow { get; set; } = GridFlow.Horizontal;

        protected override Rectangle[] GetItemRects()
        {
            Rectangle[] rects = new Rectangle[Items.Count];

            int x = _padH;
            int y = _padV;

            int lineheight = 0;

            var font = Theme.GetFont(Themes.TextStyle.ListItem);

            for (int i = 0; i < rects.Length; i++)
            {
                var item = Items[i];

                var textSize = TextRenderer.MeasureText(item.Text, font, _itemTextMaxWidth, TextRenderers.WrapMode.Words);
                int width = (_itemHighlightPadH * 2) + _itemTextMaxWidth;
                int height = _itemImageMargin + _itemImageSize + _itemTextMargin + (_itemHighlightPadV * 2) + (int)textSize.Y;

                if (GridFlow == GridFlow.Horizontal)
                {
                    if (x + width + _itemHorizontalSpacing >= Width - _padH)
                    {
                        x = _padH;
                        y += lineheight + _itemVerticalSpacing;
                        lineheight = 0;
                    }
                    lineheight = Math.Max(lineheight, height);
                }
                else
                {
                    if (y + height + _itemVerticalSpacing >= Height - _padV)
                    {
                        y = _padV;
                        x += lineheight + _itemHorizontalSpacing;
                        lineheight = 0;
                    }
                    lineheight = Math.Max(lineheight, width);
                }

                rects[i] = new Rectangle(x, y, width, height);

                if (GridFlow == GridFlow.Horizontal)
                {
                    x += width + _itemHorizontalSpacing;
                }
                else
                {
                    y += height + _itemVerticalSpacing;
                }
            }

            return rects;
        }

        protected override void CalculateSize()
        {
            Rectangle[] rects = GetItemRects();
            if (rects.Length == 0)
                return;

            switch (GridFlow)
            {
                case GridFlow.Horizontal:
                    var lowestItem = rects.OrderBy(x => x.Height).ThenBy(x => x.Y).Last();
                    Height = lowestItem.Y + lowestItem.Height + _padV;
                    break;
                case GridFlow.Vertical:
                    var rightmostItem = rects.OrderBy(x => x.Width).ThenBy(x => x.X).Last();
                    Width = rightmostItem.X + rightmostItem.Width + _padH;
                    break;
            }
        }

        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            base.OnPaint(time, gfx);

            var rects = GetItemRects();

            for (int i = 0; i < rects.Length; i++)
            {
                var item = Items[i];
                var rect = rects[i];

                string text = item.Text;
                var image = GetImage(item.ImageKey);

                if (image != null)
                {
                    var imageTint = Theme.GetTextColor(Themes.TextStyle.ListItem);
                    gfx.FillRectangle(new Rectangle(rect.X + ((rect.Width - _itemImageSize) / 2), rect.Y + _itemImageMargin, _itemImageSize, _itemImageSize), image, imageTint);
                }


                var highlightRect = new Rectangle(rect.X, rect.Y + _itemImageMargin + _itemImageSize + _itemTextMargin, rect.Width, rect.Height - (_itemImageMargin + _itemImageSize + _itemTextMargin));
                if(i == SelectedIndex)
                {
                    Theme.DrawSelectedHighlight(gfx, highlightRect);
                }
                else if(i == TrackedIndex)
                {
                    Theme.DrawHoveredHighlight(gfx, highlightRect);
                }

                int textX = rect.X + ((rect.Width - _itemTextMaxWidth) / 2);
                int textY = rect.Y + _itemImageMargin + _itemImageSize + _itemTextMargin + _itemHighlightPadV;

                gfx.DrawString(text, new Vector2(textX, textY), Theme.GetTextColor(Themes.TextStyle.ListItem), Theme.GetFont(Themes.TextStyle.ListItem), TextAlignment.Center, _itemTextMaxWidth, TextRenderers.WrapMode.Words);

            }
        }
    }

    public enum GridFlow
    {
        Horizontal,
        Vertical
    }
}
