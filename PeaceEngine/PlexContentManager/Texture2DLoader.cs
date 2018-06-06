﻿using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace Plex.Engine.PlexContentManager
{
    public class Texture2DLoader : ILoader<Texture2D>
    {
        [Dependency]
        GameLoop plex;
        public IEnumerable<string> Extensions => new[] { ".BMP", ".GIF", ".JPG", ".JPEG", ".PNG", ".TIF", ".TIFF", ".DDS" };

        public Texture2D Load(Stream fobj)
        {
            return Texture2D.FromStream(plex.GraphicsDevice, fobj);
        }
    }
}
