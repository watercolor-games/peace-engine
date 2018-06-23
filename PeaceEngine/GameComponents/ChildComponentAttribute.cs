using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents
{
    /// <summary>
    /// Marks a field within a <see cref="GameScene"/> as a child component that will automatically be created when the <see cref="GameScene"/> loads.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ChildComponentAttribute : Attribute
    {
    }
}
