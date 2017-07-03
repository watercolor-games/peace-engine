﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShiftOS.Engine;
using System.Drawing.Imaging;

namespace ShiftOS.Frontend.GUI
{
    public class PictureBox : Control
    {
        private System.Drawing.Image img = null;
        private ImageLayout _layout = ImageLayout.Fit;

        public ImageLayout ImageLayout
        {
            get
            {
                return _layout;
            }
            set
            {
                _layout = value;
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return img;
            }
            set
            {
                if (img != null)
                    img.Dispose();
                img = value;
            }
        }

        protected override void OnLayout()
        {
            if (AutoSize)
            {
                Width = (img == null) ? 0 : img.Width;
                Height = (img == null) ? 0 : img.Height;
            }
        }

        public override void Paint(Graphics gfx)
        {
            if(img != null)
                switch (_layout)
                {
                    case ImageLayout.None:
                        //Just draw the image.
                        gfx.DrawImage(img, new PointF(0, 0));
                        break;
                    case ImageLayout.Stretch:
                        //Stretch the image, with no regard for aspect ratio.
                        var stretched = ResizeImage(img, Width, Height);
                        gfx.DrawImage(stretched, 0, 0);
                        break;
                    case ImageLayout.Fit:
                        //Resize image to fit the control but keep aspect ratio.
                        var fitted = FixedSize(img, Width, Height);
                        gfx.DrawImage(fitted, 0, 0);
                        break;
                    case ImageLayout.Tile:
                        //Keep original size but tile the image.

                        for(int x = 0; x < Width; x += img.Width)
                        {
                            for (int y = 0; y < Height; y += img.Height)
                            {
                                gfx.DrawImage(img, x, y);
                            }
                        }

                        break;
                }
        }

        //Again, thanks StackOverflow
        static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(SkinEngine.LoadedSkin.ControlColor);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }

    public enum ImageLayout
    {
        None,
        Stretch,
        Tile,
        Fit,
    }
}