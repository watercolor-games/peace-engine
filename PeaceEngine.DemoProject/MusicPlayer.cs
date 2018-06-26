using Microsoft.Xna.Framework;
using PeaceEngine.DemoProject.Themes;
using Plex.Engine.GameComponents;
using Plex.Engine.GameComponents.Audio;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaceEngine.DemoProject
{
    public class MusicPlayer : GameScene
    {
        [ChildComponent]
        private UserInterface _ui = null;

        [AutoLoad]
        private Button _selectFile = null;

        [AutoLoad]
        private ProgressBar _playProgress = null;

        [AutoLoad]
        private Label _album = null;

        [AutoLoad]
        private Label _artist = null;

        [AutoLoad]
        private Label _title = null;

        [AutoLoad]
        private Lomont.LomontFFT _fft = null;

        private AdvancedAudioPlayer _player = null;

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
            _ui.Theme.DrawPanel(gfx, PanelStyles.Dark);

            if(_player != null)
            {
                var samps = _player.Samples;
                int w = gfx.Width / (samps.Length / 4);
                int y = gfx.Height / 2;
                for (int i = 0; i < samps.Length; i+=4)
                {
                    int h = (int)MathHelper.Lerp(0, gfx.Height / 2, samps[i]);
                    int x = w * (i / 4);
                    gfx.FillRectangle(x, y, w, h, Color.Yellow);
                }
            }
        }

        protected override void OnLoad()
        {
            _ui.Theme = New<UIDemoTheme>();
            _ui.Controls.Add(_selectFile);
            _ui.Controls.Add(_album);
            _ui.Controls.Add(_artist);
            _ui.Controls.Add(_title);

            _album.AutoSize = true;
            _artist.AutoSize = true;
            _title.AutoSize = true;

            _album.TextStyle = Plex.Engine.GameComponents.UI.Themes.TextStyle.Heading3;
            _title.TextStyle = Plex.Engine.GameComponents.UI.Themes.TextStyle.Heading1;

            _selectFile.Text = "Open file";

            _selectFile.Click += (o, a) =>
            {
                var opener = new System.Windows.Forms.OpenFileDialog();
                opener.Filter = "Ogg Vorbis files|*.ogg";
                opener.Title = "Open Audio File";
                if (opener.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _player = new AdvancedAudioPlayer(opener.FileName, false);
                    System.Threading.Thread.Sleep(100);
                }
            };

            _ui.Controls.Add(_playProgress);
        }

        protected override void OnUnload()
        {
        }

        protected override void OnUpdate(GameTime time)
        {
            
            _selectFile.X = 15;
            _selectFile.Y = 15;

            _playProgress.X = 15;
            _playProgress.Y = (Height - _playProgress.Height) - 15;
            _playProgress.Width = (Width - 30);

            if(_player != null)
            {
                if (_player.State != Microsoft.Xna.Framework.Audio.SoundState.Playing)
                    _player.Play();
                var pos = _player.Position.TotalMilliseconds;
                var len = _player.Duration.TotalMilliseconds;
                _playProgress.Value = (float)(pos / len);
                _album.Text = _player.Album ?? "Unknown album";
                _title.Text = _player.Title ?? "Unknown song";

                string artist = _player.Artist ?? "Unknown";
                string composer = _player.Composer ?? "Unknown";

                _artist.Text = $"Artist: {artist}    Composer: {composer}    Year: {_player.Year}";
            }
            else
            {
                _playProgress.Value = 0;
                _album.Text = "";
                _artist.Text = "Not playing";
                _title.Text = "Select a song to play.";
            }

            _artist.X = 15;
            _artist.Y = _playProgress.Y - _artist.Height - 30;
            _title.X = 15;
            _title.Y = _artist.Y - _title.Height - 7;
            _album.X = 15;
            _album.Y = _title.Y - _album.Height - 7;
        }
    }
}
