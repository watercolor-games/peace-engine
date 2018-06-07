using System;
using Plex.Engine.GraphicsSubsystem;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Plex.Engine.Cutscenes
{
    public class PNV : IVideoFormat, IDisposable
    {
        private Texture2D _frameTexture = null;
        private Stream _fobj;
        private BinaryReader _read;
        private uint[] _frame;
        public PNV(Stream fobj)
        {
            this._fobj = new GZipStream(fobj, CompressionMode.Decompress, true);
            this._read = new BinaryReader(this._fobj, Encoding.UTF8, true);
            if (_read.ReadUInt32() != 0x56654E50)
                throw new InvalidDataException("This is not a PNV file.");
            Length = _read.ReadInt32();
            FlicksPerFrame = _read.ReadInt32();
            w = _read.ReadInt32();
            h = _read.ReadInt32();
            _frame = new uint[w * h];
        }

        public int Length { get; private set; }
        public int FlicksPerFrame { get; private set; }

        int w, h;

        public void Dispose()
        {
            _read?.Dispose();
            _fobj?.Dispose();
            _frame = null;
            _frameTexture?.Dispose();
        }

        public VideoFrame NextFrame(GraphicsContext gfx)
        {
            if (_frameTexture == null)
                _frameTexture = gfx.CreateTexture(w, h);
            VideoFrame ret;
            int p = 0;
            while (p < _frame.Length)
            {
                uint inst = _read.ReadUInt32();
                uint l = inst >> 24;
                for (uint i = 0; i < l; i++)
                {
                    _frame[p] ^= inst;
                    p++;
                }
            }
            ret.sound = null;
            ret.picture = _frameTexture;
            ret.picture.SetData(_frame);
            return ret;
        }
    }
}
