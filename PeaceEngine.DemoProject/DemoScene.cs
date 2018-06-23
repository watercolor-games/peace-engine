using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            _button.Text = "Click me!";
            _button.ToolTip = "What's this do? Click it and find out.";
            _button.Click += (o, a) =>
            {
                _game.Exit();
            };
            _button.AutoSize = true;
        
            //Peace Engine will also now handle UI element tool tips.
            _uiPanel.ToolTip = @"This is a tool tip, used to convey contextual info about a UI element.

You are currently hovering over a Panel, which is a blank control you can place other controls inside.";
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

        }
    }
}
