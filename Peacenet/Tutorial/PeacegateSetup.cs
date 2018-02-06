﻿using Plex.Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Peacenet.Tutorial
{
    /// <summary>
    /// The user-interface for the Tutorial Setup screen.
    /// </summary>
    public class PeacegateSetup : Window
    {
        private float _welcomeAnim = 0f;
        private float _cornerAnim = 0f;

        private Label _setupTitle = new Label();
        private Label _setupMode = new Label();
        private Button _next = new Button();
        private Button _back = new Button();
        private float _uiAnim = 0;

        private int _uiState = 0;
        private int _animState = 0;

        private double _animRide = 0;

        private TutorialBgmEntity _tutorial = null;

        /// <inheritdoc/>
        public PeacegateSetup(WindowSystem _winsys, TutorialBgmEntity tutorial) : base(_winsys)
        {
            _tutorial = tutorial;
            SetWindowStyle(WindowStyle.NoBorder);
            Width = _winsys.Width;
            Height = _winsys.Height;
            AddChild(_setupTitle);
            AddChild(_setupMode);
            AddChild(_back);
            AddChild(_next);
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            switch(_animState)
            {
                case 0:
                    _welcomeAnim += (float)time.ElapsedGameTime.TotalSeconds * 2;
                    if(_welcomeAnim>=1.0f)
                    {
                        _welcomeAnim = 1;
                        _animState++;
                    }
                    break;
                case 1:
                    _animRide += time.ElapsedGameTime.TotalSeconds;
                    if(_animRide >= 0.5)
                    {
                        _animRide = 0;
                        _animState++;
                    }
                    break;
                case 2:
                    _cornerAnim += (float)time.ElapsedGameTime.TotalSeconds * 2;
                    if (_cornerAnim >= 1.0f)
                    {
                        _cornerAnim = 1;
                        _animState++;
                    }
                    break;
                case 3:
                    _uiAnim += (float)time.ElapsedGameTime.TotalSeconds * 2;
                    if (_uiAnim >= 1.0f)
                    {
                        _uiAnim = 1;
                        _animState++;
                    }
                    break;
                case 5:
                    _uiAnim -= (float)time.ElapsedGameTime.TotalSeconds * 2;
                    if (_uiAnim <= 0f)
                    {
                        _uiAnim = 0;
                        _animState = 3;
                    }
                    break;

            }
            Width = (int)MathHelper.Lerp(WindowSystem.Width - 50, WindowSystem.Width, _welcomeAnim);
            Height = (int)MathHelper.Lerp(WindowSystem.Height - 50, WindowSystem.Height, _welcomeAnim);
            Parent.X = (int)MathHelper.Lerp(25, 0, _welcomeAnim);
            Parent.Y = (int)MathHelper.Lerp(25, 0, _welcomeAnim);

            _setupTitle.FontStyle = Plex.Engine.Themes.TextFontStyle.Header1;
            _setupTitle.Text = "Peacegate OS Setup";
            _setupTitle.AutoSize = true;

            //first we calculate where the title should ACTUALLY BE
            var titleLocMax = new Vector2(15, 15);
            var titleLocMin = new Vector2((Width - _setupTitle.Width) / 2, (Height - _setupTitle.Height) / 2);
            var titleLoc = Vector2.Lerp(titleLocMin, titleLocMax, this._cornerAnim);

            //Next, we calculate the proper Y coordinate.
            int titleLocY = (int)MathHelper.Lerp(titleLoc.Y + (Width * 0.25F), titleLoc.Y, _welcomeAnim);
            _setupTitle.X = (int)titleLoc.X;
            _setupTitle.Y = titleLocY;
            _setupTitle.Opacity = _welcomeAnim;



            Opacity = _welcomeAnim;

            base.OnUpdate(time);
        }
    }
}