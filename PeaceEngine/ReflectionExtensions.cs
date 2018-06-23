using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine
{
    public static class ReflectionExtensions
    {
        public static bool Inherits(this Type type, Type baseType)
        {
            while (type != null)
            {
                if (type == baseType)
                    return true;
                type = type.BaseType;
            }
            return false;
        }

    }
}
