using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.PlexContentManager
{
    public class JsonLoader<T> : ILoader<T>
    {
        public IEnumerable<string> Extensions
        {
            get
            {
                yield return ".JSON";
            }
        }

        public T Load(Stream fobj)
        {
            using (var reader = new StreamReader(fobj))
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }
    }
}
