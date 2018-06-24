using Microsoft.Xna.Framework;
using PeaceEngine.DemoProject.Themes;
using Plex.Engine.GameComponents;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GameComponents.UI.Themes;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaceEngine.DemoProject
{
    public class UIDemoScene : GameScene
    {
        [ChildComponent]
        private UserInterface _ui = null;

        [AutoLoad]
        private Label _heading = null;

        [AutoLoad]
        private Label _description = null;

        [AutoLoad]
        private Button _back = null;

        [AutoLoad]
        private Button _regularButton = null;

        [AutoLoad]
        private Button _primaryButton = null;

        [AutoLoad]
        private Button _successButton = null;

        [AutoLoad]
        private Button _warningButton = null;

        [AutoLoad]
        private Button _dangerButton = null;

        [AutoLoad]
        private CheckLabel _checkLabel = null;

        [AutoLoad]
        private TextBox _textBox = null;

        [AutoLoad]
        private TextEditor _editor = null;

        [AutoLoad]
        private ListBox _listBox = null;

        [AutoLoad]
        private GridListView _horizontalGrid = null;

        [AutoLoad]
        private GridListView _verticalGrid = null;

        [AutoLoad]
        private ProgressBar _progressBar = null;

        [AutoLoad]
        private SliderBar _sliderBar = null;

        [AutoLoad]
        private ScrollView _scrollView = null;

        [AutoLoad]
        private VStacker _verticalStacker = null;

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
        }

        protected override void OnLoad()
        {
            _ui.Theme = New<UIDemoTheme>();

            _ui.Controls.Add(_heading);
            _ui.Controls.Add(_description);

            _ui.Controls.Add(_regularButton);
            _ui.Controls.Add(_primaryButton);
            _ui.Controls.Add(_successButton);
            _ui.Controls.Add(_warningButton);
            _ui.Controls.Add(_dangerButton);
            _ui.Controls.Add(_back);
            _ui.Controls.Add(_textBox);
            _ui.Controls.Add(_editor);
            _ui.Controls.Add(_listBox);
            _ui.Controls.Add(_horizontalGrid);
            _ui.Controls.Add(_verticalGrid);
            _ui.Controls.Add(_progressBar);
            _ui.Controls.Add(_sliderBar);
            _ui.Controls.Add(_scrollView);

            _scrollView.Children.Add(_verticalStacker);

            _verticalStacker.AutoSize = true;
            
            foreach(TextStyle style in Enum.GetValues(typeof(TextStyle)))
            {
                var label = New<Label>();
                label.Text = style.ToString();
                label.TextStyle = style;
                label.AutoSize = true;
                _verticalStacker.Children.Add(label);
            }

            for (int i = 0; i < 10; i++)
            {
                _listBox.Items.Add(string.Format("Item {0}", i + 1));
                _horizontalGrid.Items.Add(new ListViewItem(string.Format("Item {0}", i + 1), null, null));
                _verticalGrid.Items.Add(new ListViewItem(string.Format("Item {0}", i + 1), null, null));
            }

            _horizontalGrid.AutoSize = false;
            _verticalGrid.AutoSize = false;

            _verticalGrid.GridFlow = GridFlow.Vertical;
            _horizontalGrid.GridFlow = GridFlow.Horizontal;

            _textBox.Label = "Try typing in this text box.";
            _textBox.ToolTip = "This is a text box. Click on it and start typing!";
            _textBox.Width = 175;

            _ui.Controls.Add(_checkLabel);

            _checkLabel.AutoWidth = true;
            _checkLabel.Text = "Check label";

            _heading.Text = "User Interface Demo";
            _heading.AutoSize = true;
            _heading.TextStyle = Plex.Engine.GameComponents.UI.Themes.TextStyle.Heading1;

            _description.Text = "Peace Engine has a robust UI system that is completely themable. Buttons, text inputs, panels, lists, menus, everything you need is supported. There's even a window system.";
            _description.AutoSize = true;

            _back.Text = "Back to main screen.";
            _back.AutoSize = true;

            _regularButton.Text = "Regular";
            _primaryButton.Text = "Primary";
            _successButton.Text = "Success";
            _dangerButton.Text = "Danger";
            _warningButton.Text = "Warning";

            _primaryButton.ButtonStyle = ButtonStyle.Primary;
            _successButton.ButtonStyle = ButtonStyle.Success;
            _warningButton.ButtonStyle = ButtonStyle.Warning;
            _dangerButton.ButtonStyle = ButtonStyle.Danger;

            _progressBar.ToolTip = "This is a progress bar. Progress bars display progress and can't be interacted with.";
            _sliderBar.ToolTip = "This is a Slider Bar. This is an interactive version of the Progress Bar which you can drag to change the value. Give it a try!";

            _back.Click += (o, a) =>
            {
                LoadScene<DemoScene>();
            };
        }

        protected override void OnUnload()
        {
        }

        protected override void OnUpdate(GameTime time)
        {
            _heading.X = 15;
            _heading.Y = 15;
            _description.X = 15;
            _description.Y = _heading.Y + _heading.Height + 7;
            _heading.AutoSizeMaxWidth = Width - 30;
            _description.AutoSizeMaxWidth = _heading.AutoSizeMaxWidth;

            _regularButton.X = 15;
            _regularButton.Y = _description.Y + _description.Height + 15;

            _primaryButton.Y = _regularButton.Y;
            _primaryButton.X = _regularButton.X + _regularButton.Width + 7;
            _successButton.Y = _regularButton.Y;
            _successButton.X = _primaryButton.X + _primaryButton.Width + 7;
            _warningButton.Y = _regularButton.Y;
            _warningButton.X = _successButton.X + _successButton.Width + 7;
            _dangerButton.Y = _regularButton.Y;
            _dangerButton.X = _warningButton.X + _warningButton.Width + 7;

            _checkLabel.X = 15;
            _checkLabel.Y = _regularButton.Y + _regularButton.Height + 15;

            _textBox.X = _checkLabel.X + _checkLabel.Width + 7;
            _textBox.Y = _checkLabel.Y;

            _editor.X = 15;
            _editor.Y = _textBox.Y + _textBox.Height + 15;
            _editor.Width = Width / 4;
            _editor.Height = Height / 3;

            _listBox.Y = _editor.Y;
            _listBox.X = _editor.X + _editor.Width + 15;
            _listBox.Width = _editor.Width;
            _listBox.Height = _editor.Height;

            _horizontalGrid.Y = _listBox.Y;
            _horizontalGrid.X = _listBox.X + _listBox.Width + 15;
            _horizontalGrid.Width = _listBox.Width;
            _horizontalGrid.Height = _editor.Height;

            _verticalGrid.Y = _listBox.Y;
            _verticalGrid.X = _horizontalGrid.X + _horizontalGrid.Width + 15;
            _verticalGrid.Width = _listBox.Width;
            _verticalGrid.Height = _editor.Height;

            _progressBar.Width = Width - 30;
            _progressBar.X = 15;
            _progressBar.Y = _editor.Y + _editor.Height + 15;

            _sliderBar.Width = Width - 30;
            _sliderBar.X = 15;
            _sliderBar.Y = _progressBar.Y + _progressBar.Height + 7;

            _progressBar.Value = MathHelper.Clamp(_progressBar.Value + ((float)time.ElapsedGameTime.TotalSeconds / 4), 0, 1);
            if (_progressBar.Value == 1)
                _progressBar.Value = 0;

            _back.X = 15;
            _back.Y = (Height - _back.Height) - 15;

            _scrollView.X = 15;
            _scrollView.Y = _sliderBar.Y + _sliderBar.Height + 15;
            _scrollView.Width = Width - 30;
            _scrollView.Height = (_back.Y - 15) - _scrollView.Y;

            _verticalStacker.X = 0;
            _verticalStacker.Y = 0;
            _verticalStacker.Width = _scrollView.Width;
        }
    }
}
