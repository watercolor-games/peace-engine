using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents
{
    /// <summary>
    /// Marks a field within a <see cref="GameScene"/> as a field with should automatically be created on scene load. For <see cref="GameComponent"/>, please use <see cref="ChildComponentAttribute"/> instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoLoadAttribute : Attribute
    {
    }
}
