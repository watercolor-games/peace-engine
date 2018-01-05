﻿using Peacenet.Objectives;
using Plex.Engine;
using Plex.Engine.Saves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacenet.Missions
{
    public class Prologue : Mission
    {
        [Dependency]
        private SaveManager _save = null;

        public override string Description
        {
            get
            {
                return "What exactly IS the Peacenet? Why are you here?";
            }
        }

        public override bool IsAvailable
        {
            get
            {
                return !_save.GetValue<bool>("mission.prologue.completed", false);
            }
        }

        public override bool IsComplete
        {
            get
            {
                return _save.GetValue<bool>("mission.prologue.completed", false);
            }
        }

        public override string Name
        {
            get
            {
                return "A Network of Serenity";
            }
        }

        public override Queue<Objective> Objectives
        {
            get
            {
                var ret = new Queue<Objective>();
                ret.Enqueue(new CutsceneObjective("Find your purpose.", "Find out what The Peacenet is, and how and why you're in it.", "intro_00"));
                return ret;
            }
        }

        public override void OnComplete()
        {
        }

        public override void OnStart()
        {
        }
    }
}
