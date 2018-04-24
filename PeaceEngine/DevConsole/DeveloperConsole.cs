#define DEBUG
#if DEBUG
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;
using Plex.Objects;
using Plex.Objects.Pty;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.Debug
{
    public class DeveloperConsoleComponent : IEngineComponent
    {
        [Dependency]
        private Plexgate _plexgate = null;

        private DevConsole _console = null;

        public void Initiate()
        {
            _console = _plexgate.New<DevConsole>();
            _plexgate.GetLayer(LayerType.Foreground).AddEntity(_console);
        }

        private class DevConsole : IEntity, ILoadable
        {
            [Dependency]
            private Themes.ThemeManager _theme = null;

            [Dependency]
            private Plexgate _plexgate = null;

            [Dependency]
            private UIManager _ui = null;

            private int _consoleTextAvailableWidth = 0;

            private int _consoleStartY = 0;

            private PseudoTerminal _master = null;
            private PseudoTerminal _slave = null;
            private StreamReader _stdin = null;
            private StreamWriter _stdout = null;

            private int _charSizeX = 0;
            private int _charSizeY = 0;

            private GlobalsType _globals = null;

            private ScriptOptions _options = null;

            private bool _consoleOpen = false;
            private float _slideAnim = 0;

            private Queue<ScriptRunner<object>> _runners = new Queue<ScriptRunner<object>>();

            private string _consoleText = ""; //This must scare literally everyone good with memory management.

            private bool _shellActive = false;

            public void Draw(GameTime time, GraphicsContext gfx)
            {
                _consoleTextAvailableWidth = gfx.Width - 30;

                int consoleHeight = (int)MathHelper.Lerp(0, gfx.Height, _slideAnim);
                gfx.BeginDraw();
                gfx.DrawRectangle(0, 0, gfx.Width, consoleHeight, Color.Black * 0.5F);

                var headFont = _theme.Theme.GetFont(Themes.TextFontStyle.Header3); //The font used by the heading text.
                var headColor = new Color(64, 128, 255); //The color used by the heading text - don't use the theme because I want this to be theme-independent.
                var headText = "Peace Engine Developer Console"; //The header text itself
                var headMeasure = headFont.MeasureString(headText); //We need to know how big the heading text is so we can calculate the console cull Y below
                var consoleTextCullY = 30 + headMeasure.Y; //This is the location on the Y axis where we'll stop rendering console text.
                var consoleFont = _theme.Theme.GetFont(Themes.TextFontStyle.Mono); //The font used by the actual console output.
                var consoleTextColor = Color.White; //Console text color
                var consoleTextShadowColor = Color.Black; //In case the background is ontop of something bright, like a sky, we want to put a drop-shadow on the console text.
                if (_consoleOpen)
                {
                    gfx.DrawString(headText, new Vector2(15, 15), headColor, headFont);
                    gfx.DrawString(headText, new Vector2(17, 17), consoleTextShadowColor, headFont);

                    int charX = 0;
                    int charY = 0;

                    int consoleTextHeight = ((consoleHeight) - 15) - (_consoleStartY * _charSizeY);


                    foreach (char c in _consoleText)
                    {
                        switch (c)
                        {
                            case '\r':
                            case '\t':
                            case '\v':
                            case '\0':
                            case (char)0x02:
                            case (char)0x1B:
                                continue;
                            case '\n':
                                charX = 0;
                                charY++;
                                continue;
                            default:
                                int pixelCharY = consoleTextHeight + (charY * _charSizeY);
                                int pixelCharX = 15 + (charX * _charSizeX);
                                if (pixelCharY > consoleTextCullY)
                                {
                                    gfx.DrawString(c.ToString(), new Vector2(pixelCharX - 2, pixelCharY - 2), consoleTextShadowColor, consoleFont);
                                    gfx.DrawString(c.ToString(), new Vector2(pixelCharX, pixelCharY), consoleTextColor, consoleFont);
                                }
                                if (_charSizeX + (charX + 1) > _consoleTextAvailableWidth)
                                {
                                    charX = 0;
                                    charY++;
                                }
                                else
                                {
                                    charX++;
                                }
                                break;
                        }
                    }
                    int cursorY = consoleTextHeight + (charY * _charSizeY);
                    int cursorX = 15 + (charX * _charSizeX);
                    gfx.DrawRectangle(cursorX, cursorY, _charSizeX, _charSizeY, headColor);
                }

                gfx.EndDraw();
            }

            public void Load(ContentManager content)
            {
                var charMeasure = _theme.Theme.GetFont(Themes.TextFontStyle.Mono).MeasureString("#");
                _charSizeX = (int)charMeasure.X;
                _charSizeY = (int)charMeasure.Y;


                var options = new TerminalOptions();

                options.LFlag = PtyConstants.ICANON | PtyConstants.ECHO;
                options.C_cc[PtyConstants.VERASE] = (byte)'\b';
                options.C_cc[PtyConstants.VEOL] = (byte)'\r';
                options.C_cc[PtyConstants.VEOL2] = (byte)'\n';

                PseudoTerminal.CreatePair(out _master, out _slave, options);

                _stdout = new StreamWriter(_master);
                _stdin = new StreamReader(_master);
                _stdout.AutoFlush = true;

                Logger.Log("Debug Console is pre-loading .NET assemblies.");
                _options = ScriptOptions.Default.WithReferences(_plexgate.GetAllComponents().Select(x => x.GetType().Assembly).ToArray().Concat(new[] { typeof(Game).Assembly, typeof(RuntimeBinderException).Assembly, typeof(Logger).Assembly })).WithImports("System", "Microsoft.Xna.Framework", "Plex.Engine", "Plex.Engine.Interfaces", "Plex.Engine.GraphicsSubsystem", "Plex.Engine.GUI");
                _globals = new GlobalsType(_stdout, _plexgate.GetAllComponents());
                Logger.Log("Done.");

                _ui.FocusGained += () =>
                {
                    _consoleOpen = false;
                };
            }

            private int CalculateStartY()
            {
                int charX = 0;
                int charY = 0;

                foreach(char c in _consoleText)
                {
                    switch(c)
                    {
                        case '\r':
                        case '\t':
                        case '\v':
                        case '\0':
                        case (char)0x02:
                        case (char)0x1B:
                            break;
                        case '\n':
                            charX = 0;
                            charY++;
                            break;
                        default:
                            if(_charSizeX + (charX + 1) > _consoleTextAvailableWidth)
                            {
                                charX = 0;
                                charY++;
                            }
                            else
                            {
                                charX++;
                            }
                            break;
                    }
                }
                return charY;
            }

            public void OnGameExit()
            {
            }

            public void OnKeyEvent(KeyboardEventArgs e)
            {
                if(e.Key == Keys.Escape)
                {
                    _consoleOpen = !_consoleOpen;
                }
                if (_consoleOpen && _shellActive)
                {
                    if (e.Modifiers.HasFlag(KeyboardModifiers.Control) && e.Key == Microsoft.Xna.Framework.Input.Keys.V)
                    {
                        if (System.Windows.Forms.Clipboard.ContainsText())
                        {
                            foreach (char c in System.Windows.Forms.Clipboard.GetText())
                            {
                                _slave.WriteByte((byte)c);
                                _consoleStartY = CalculateStartY();
                            }
                        }
                        return;
                    }

                    if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                    {
                        _slave.WriteByte((byte)'\r');
                        _slave.WriteByte((byte)'\n');
                        _consoleStartY = CalculateStartY();
                        return;
                    }

                    if (e.Character != null)
                    {
                        _slave.WriteByte((byte)e.Character);
                        _consoleStartY = CalculateStartY();
                    }
                }
            }

            public void OnMouseUpdate(MouseState mouse)
            {
            }

            private void Compile(string code)
            {

            }


            public void Update(GameTime time)
            {
                if(_consoleOpen == true)
                {
                    _slideAnim = MathHelper.Clamp(_slideAnim + (float)time.ElapsedGameTime.TotalSeconds*4, 0, 1);
                    if(_slideAnim==1)
                    {
                        if (_shellActive == false)
                        {
                            _shellActive = true;
                            //Disable UI keyboard focus.
                            _ui.SetFocus(null);
                            //Start a Task which runs the console itself.
                            Task.Run(() =>
                            {
                                _stdout.WriteLine("Welcome to the Peace Engine Developer Console.");
                                _stdout.WriteLine("This console allows you to run arbitrary C# code in Peace Engine.");
                                _stdout.WriteLine("/!\\ WARNING: THIS FEATURE IS FOR PEOPLE WHO KNOW WHAT THEY'RE DOING. One wrong move and you can VERY EASILY break or crash the game. Think before you type, and with great power comes great responsibility.");
                                _stdout.WriteLine("Press ESC again to exit the console. Focusing a UI element will also close the console.");
                                _stdout.WriteLine("");
                                while (_shellActive)
                                {
                                    _stdout.Write("> ");
                                    string code = _stdin.ReadLine();
                                    if (!string.IsNullOrWhiteSpace(code) || _shellActive == true) 
                                    {
                                        var script = CSharpScript.Create(code: code, options: _options, globalsType: typeof(GlobalsType));
                                        try
                                        {
                                            var runner = script.CreateDelegate();
                                            _runners.Enqueue(runner);
                                        }
                                        catch (Exception ex)
                                        {
                                            _stdout.WriteLine(ex.ToString());
                                        }
                                    }
                                }

                            });
                        }

                        var ch = _slave.ReadByte();
                        if (ch != -1)
                        {
                            while (ch != -1)
                            {
                                if ((char)ch == '\b')
                                {
                                    if (_consoleText.Length > 0)
                                        _consoleText = _consoleText.Remove(_consoleText.Length - 1, 1);
                                }
                                else
                                {
                                    _consoleText += (char)ch;
                                }
                                ch = _slave.ReadByte();
                            }
                            _consoleStartY = CalculateStartY();
                        }

                        while(_runners.Count > 0)
                        {
                            var runner = _runners.Dequeue();
                            runner.Invoke(_globals);
                        }
                    }
                }
                else
                {
                    _consoleText = "";
                    _consoleStartY = 0;
                    if (_shellActive == true)
                    {
                        _shellActive = false;
                        _slave.WriteByte((byte)'\r');
                        _slave.WriteByte((byte)'\n');
                    }
                    _slideAnim = MathHelper.Clamp(_slideAnim - (float)time.ElapsedGameTime.TotalSeconds*4, 0, 1);
                }
            }
        }
    }

    public class GlobalsType
    {
        private ExpandoObject _obj = null;
        private StreamWriter _out = null;
        private ExpandoObject _varBag = new ExpandoObject();

        public GlobalsType(StreamWriter output, params object[] objects)
        {
            _out = output;
            _obj = new ExpandoObject();
            var dict = _obj as IDictionary<string, object>;
            foreach (var obj in objects)
                dict[obj.GetType().Name] = obj;
        }

        public dynamic Components
        {
            get
            {
                return _obj;
            }
        }

        /// <summary>
        /// A place to store variables.
        /// </summary>
        public dynamic VarBag
        {
            get
            {
                return _varBag;
            }
        }

        public void ListComponents()
        {
            foreach (var component in Components)
                _out.WriteLine($"Components.{component.Key}");
        }
    }

}
#endif
