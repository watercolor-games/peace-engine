using Microsoft.Xna.Framework.Graphics;
using OpenWheels.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GraphicsSubsystem
{
    public sealed class MgBatcher : Batcher
    {
        private readonly Dictionary<string, int> _textureIds;
        private readonly SerenityRenderer _renderer;

        public MgBatcher(GraphicsDevice gd)
        {
            _textureIds = new Dictionary<string, int>();

            _renderer = new SerenityRenderer(gd);
            Renderer = _renderer;
        }

        public void RegisterTexture(Texture2D texture)
        {
            RegisterTexture(texture, texture.Name);
        }

        public void RegisterTexture(Texture2D texture, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (_textureIds.ContainsKey(name))
            {
                if (_renderer.GetTexture(_textureIds[name]) == texture)
                    return;
                _textureIds.Remove(name);
            }
            var id = _renderer.AddTexture(texture);
            _textureIds.Add(name, id);
        }

        public void SetTexture(string texture)
        {
            var id = _textureIds[texture];
            SetTexture(id);
        }

        public void ClearTextures()
        {
            while(_textureIds.Count>0)
            {
                var first = _textureIds.First();
                _renderer.RemoveTexture(first.Value);
                _textureIds.Remove(first.Key);
            }
        }
    }
}
