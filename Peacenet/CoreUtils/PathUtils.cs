﻿using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Filesystem;
using Plex.Engine;
using Peacenet.Applications;
using Plex.Engine.GUI;
using Microsoft.Xna.Framework.Graphics;

namespace Peacenet.CoreUtils
{
    public class FileUtils : IEngineComponent
    {

        [Dependency]
        private Plexgate _plexgate = null;

        [Dependency]
        private FSManager _fs = null;

        public int DrawIndex
        {
            get
            {
                return -1;
            }
        }

        public string GetNameFromPath(string path)
        {
            while (path.EndsWith("/"))
                path = path.Remove(path.Length - 1, 1);
            return path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        /// <summary>
        /// Resolve any "up-one" and "current" directory names in the specified path to an absolute path. I.E: /home/alkaline/../Documents/./.. -> /home/Documents
        /// </summary>
        /// <param name="path">The path to resolve</param>
        /// <returns>The resolved path</returns>
        public string Resolve(string path)
        {
            Stack<string> pathParts = new Stack<string>();
            string[] split = path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in split)
            {
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    if (pathParts.Count > 0)
                        pathParts.Pop(); //remove the parent directory entry from the path.
                    continue;
                }
                if (part == "~")
                {
                    pathParts.Push("home");
                    continue;
                }
                pathParts.Push(part);
            }
            string[] parts = new string[pathParts.Count];
            for (int i = parts.Length-1; i >= 0; i--)
            {
                parts[i] = pathParts.Pop();
            }
            string absolute = "";
            foreach (var part in parts)
                absolute += "/" + part;
            if (string.IsNullOrWhiteSpace(absolute))
                absolute = "/";
            return absolute;
        }

        /// <summary>
        /// Gets the MIME type of a specified file name via its extension.
        /// </summary>
        /// <param name="filename">The file name to look up</param>
        /// <returns>The MIME type for the file</returns>
        public string GetMimeType(string filename)
        {
            if (!filename.Contains("."))
                return "unknown";
            int last = filename.LastIndexOf(".");
            int len = filename.Length - last;
            string ext = filename.Substring(last, len);
            var types = MimeTypeMap.List.MimeTypeMap.GetMimeType(ext);
            return (types.Count == 0) ? "unknown" : types.First();
        }

        public Texture2D GetMimeIcon(string mimetype)
        {
            switch (mimetype)
            {
                case "text/plain":
                    return _plexgate.Content.Load<Texture2D>("UIIcons/FileIcons/file-text");
                default:
                    return _plexgate.Content.Load<Texture2D>("UIIcons/FileIcons/unknown");
            }
        }

        public void Initiate()
        {
        }

        public void OnFrameDraw(GameTime time, GraphicsContext ctx)
        {
        }

        public void OnGameUpdate(GameTime time)
        {
        }

        public void OnKeyboardEvent(KeyboardEventArgs e)
        {
        }

        public void Unload()
        {
        }
    }

    public class GUIUtils : IEngineComponent
    {
        [Dependency]
        private WindowSystem _winsys = null;

        public void Initiate()
        {
        }

        public int DrawIndex
        {
            get
            {
                return -1;
            }
        }

        public void OnFrameDraw(GameTime time, GraphicsContext ctx)
        {
        }

        public void OnGameUpdate(GameTime time)
        {
        }

        public void OnKeyboardEvent(KeyboardEventArgs e)
        {
        }

        public void Unload()
        {
        }

        public void AskForFile(bool saving, Action<string> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var fs = new FileManager(_winsys);
            fs.SetDialogCallback(callback, saving);
            fs.SetWindowStyle(Plex.Engine.GUI.WindowStyle.Dialog);
            fs.Show();
        }
    }
}
