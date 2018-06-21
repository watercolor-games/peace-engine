﻿using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using System.IO;
using Plex.Objects;

namespace Plex.Engine.Cutscene
{
    /// <summary>
    /// Provides an engine component capable of handling playback of <see cref="Cutscene"/> objects. 
    /// </summary>
    public class CutsceneManager : IEngineComponent, IDisposable
    {


        [Dependency]
        private GameLoop _GameLoop = null;

        private List<Cutscene> _cutscenes = null;
        private Cutscene _current = null;

        public Cutscene[] Cutscenes
        {
            get
            {
                return _cutscenes.ToArray();
            }
        }

        /// <summary>
        /// Retrieves whether a cutscene is playing.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (_current == null)
                    return false;
                return !_current.IsFinished;
            }
        }

        private Action _callback = null;

        /// <summary>
        /// Stop playing a cutscene.
        /// </summary>
        /// <param name="runCallback">Whether the cutscene end callback should be fired.</param>
        public void Stop(bool runCallback = true)
        {
            if(_current != null)
            {
                _current.IsFinished = true;
                _current.OnFinish();
                _GameLoop.GetLayer(LayerType.Foreground).RemoveEntity(_current);
                if (runCallback)
                    _callback?.Invoke();
                _callback = null;
                _current = null;
            }
        }

        /// <summary>
        /// Play a cutscene.
        /// </summary>
        /// <param name="name">The name of the cutscene to play.</param>
        /// <param name="callback">A callback function to run when the cutscene ends.</param>
        /// <returns>Whether the cutscene was able to start playing.</returns>
        public bool Play(string name, Action callback = null)
        {
            var cs = _cutscenes.FirstOrDefault(x => x.Name == name);
            if (cs == null)
                return false;
            Stop(false);
            _callback = callback;
            cs.IsFinished = false;
            _current = cs;
            _current.OnPlay();
            _GameLoop.GetLayer(LayerType.Foreground).AddEntity(_current);
            return true;
        }

        /// <inheritdoc/>
        public void Initiate()
        {
            _cutscenes = new List<Cutscene>();
            Logger.Log("Looking for coded cutscenes...");
            foreach (var type in ReflectMan.Types.Where(x=>x.BaseType == typeof(Cutscene)))
            {
                var cs = (Cutscene)Activator.CreateInstance(type, null);
                Logger.Log($"Found: {cs.Name}");
                if (_cutscenes.FirstOrDefault(x => x.Name == cs.Name)!=null)
                {
                    Logger.Log("...but it's a duplicate.");
                    continue;
                }
                _GameLoop.Inject(cs);
                cs.Load(_GameLoop.Content);
                _cutscenes.Add(cs);
            }
            Logger.Log($"{_cutscenes.Count} cutscenes loaded.");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            while(_cutscenes.Count > 0)
            {
                var cs = _cutscenes[0];
                cs.Dispose();
                _cutscenes.RemoveAt(0);
                cs = null;
            }
            _cutscenes = null;
        }
    }
}
