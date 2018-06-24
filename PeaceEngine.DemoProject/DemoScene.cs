using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PeaceEngine.DemoProject.Themes;
using Plex.Engine;
using Plex.Engine.GameComponents;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaceEngine.DemoProject
{
    //This is a Peace Engine game scene.
    //
    //Game scenes are components that act as a top-level component for your game.
    //
    //Scenes can contain child components, and have their own content managers.
    //Scenes can also act as their own components, with an update and draw method.
    public class DemoScene : GameScene
    {
        //UserInterface lets us host interactive UI elements similar to Windows Forms. UserInterface will take up the entire screen.
        [ChildComponent]
        private UserInterface _ui = null;

        //Yes, scenes and game components can have dependencies too!
        [Dependency]
        private GameLoop _game = null;

        //This is a top-level UI element that we will position in the center of the screen.
        private Panel _uiPanel = null;

        //Buttons are UI elements that can be clicked.
        private Button _button = null;

        //This Label will act as our "title" text.
        private Label _headingLabel = null;

        //This label will tell the player a bit about the engine.
        private Label _bodyLabel = null;

        [AutoLoad]
        private Button _frameDemo = null;

        //This is where your scene is able to render content to the screen.
        //Treat this as a place to render your backdrop, since child components will render on-top of it.
        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
            gfx.Clear(Color.CornflowerBlue); //In true MonoGame fashion.
        }

        //This is called once the scene is created. You can load your content here.
        //Also, by this point, all [Dependency] fields have been populated.
        protected override void OnLoad()
        {
            _uiPanel = _game.New<Panel>();
            _ui.Controls.Add(_uiPanel); //Add the panel to our scene's UI.

            _button = _game.New<Button>();
            _uiPanel.Children.Add(_button);

            _uiPanel.Children.Add(_frameDemo);

            _headingLabel = _game.New<Label>();
            _uiPanel.Children.Add(_headingLabel);

            _bodyLabel = _game.New<Label>();
            _uiPanel.Children.Add(_bodyLabel);

            _bodyLabel.AutoSize = false; //The UI engine will wrap text based on the Label's width, not its max width.
            _bodyLabel.Text = @"Peace Engine is a modular and awesome game engine built on top of MonoGame.

It is built with both modularity and ease of use in mind. Peace Engine is comprised of several different components that work together to make your game possible.

1. Game Components - the meat and bones of your game, where you can write all your logic and rendering code.
2. Engine Modules - mini libraries that are managed by the engine and can be used and depended on within other components.
3. Game Services - game components that are treated like engine modules and exist across scenes and levels.

We have coded a little demo project you can use to see how these components work to build an effective workflow and amazing 2D games.";

            _button.Text = "UI demo";
            _button.ToolTip = "Load the UI demo scene. This scene will show you all the different things you can do with our UI system.";
            _button.Click += (o, a) =>
            {
                LoadScene<UIDemoScene>();
            };
            _button.AutoSize = true;
        
            //Peace Engine will also now handle UI element tool tips.
            _uiPanel.ToolTip = @"This is a tool tip, used to convey contextual info about a UI element.

You are currently hovering over a Panel, which is a blank control you can place other controls inside.";

            _headingLabel.Text = "Welcome to Peace Engine!";

            _headingLabel.TextStyle = Plex.Engine.GameComponents.UI.Themes.TextStyle.Heading1;

            _ui.Theme = _game.New<UIDemoTheme>();

            _frameDemo.Text = "Frame Demo";
            _frameDemo.ToolTip = "Switch to the Frame Demo scene.";
            _frameDemo.Click += (o, a) =>
            {
                LoadScene<FrameDemoScene>();
            };
        }

        //This is called when the game exits or a new scene is about to be created.
        //This is where you're able to unload all your content, detach events, etc.
        protected override void OnUnload()
        {
            //We don't have any non-ContentManager resources, so we don't need to do anything.
            //Peace Engine will automatically unload everything we load through the Content.Load method.
        }

        //This is where you are able to update your scene and child components.
        protected override void OnUpdate(GameTime time)
        {
            //Make the panel half of our screen size and in the center of the screen.
            _uiPanel.Width = Width / 2;
            _uiPanel.Height = Height / 2;
            _uiPanel.X = (Width - _uiPanel.Width) / 2;
            _uiPanel.Y = (Height - _uiPanel.Height) / 2;

            //Child UI elements' coordinates are relative to their parent.
            _button.X = 15;
            _button.Y = _uiPanel.Height - _button.Height - 15;

            //Labels can be auto-sized.
            _headingLabel.AutoSize = true;
            _headingLabel.AutoSizeMaxWidth = _uiPanel.Width - 30;
            _headingLabel.X = 15;
            _headingLabel.Y = 15;

            //Now we lay-out the body.
            _bodyLabel.X = 15;
            _bodyLabel.Y = _headingLabel.Y + _headingLabel.Height + 7;
            _bodyLabel.Width = _headingLabel.AutoSizeMaxWidth;
            _bodyLabel.Height = (_button.Y - 7) - _bodyLabel.Y;

            _frameDemo.X = _button.X + _button.Width + 7;
            _frameDemo.Y = _button.Y;
        }
    }
}
