﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PraiseBase.Presenter.Persistence
{
    public static class SongFilePluginFactory
    {
        private static Dictionary<Type, ISongFilePlugin> _plugins = new Dictionary<Type, ISongFilePlugin>();

        private static Dictionary<String, HashSet<Type>> _supportedExtensionMapping = new Dictionary<string, HashSet<Type>>();

        /// <summary>
        /// A list of supported file name extensions
        /// </summary>
        public static List<String> SupportedExtensions 
        {
            get
            {
                return _supportedExtensionMapping.Keys.ToList();
            } 
        }

        static SongFilePluginFactory()
        {
            var interfaceType = typeof(ISongFilePlugin);
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(x => x.GetTypes())
              .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (var atype in allTypes)
            {
                ISongFilePlugin inst = (ISongFilePlugin)Activator.CreateInstance(atype);
                _plugins.Add(atype, inst);
                if (!_supportedExtensionMapping.Keys.Contains(inst.GetFileExtension()))
                {
                    _supportedExtensionMapping.Add(inst.GetFileExtension(), new HashSet<Type>(new[] { atype }));
                }
                else
                {
                    _supportedExtensionMapping[inst.GetFileExtension()].Add(atype);
                }
            }
        }

        /// <summary>
        /// Returns a SongFileReader based on the requested type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ISongFilePlugin Create(Type type)
        {
            if (_plugins[type] != null)
            {
                return _plugins[type];
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a ISongFilePlugin based on the file name
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ISongFilePlugin Create(string filename)
        {
            string ext = Path.GetExtension(filename);

            if (ext != null && _supportedExtensionMapping[ext] != null)
            {
                foreach (Type t in _supportedExtensionMapping[ext])
                {
                    var reader = Create(t);
                    if (reader.IsFileSupported(filename))
                    {
                        return reader;
                    }
                }
            }
            throw new NotImplementedException();
        }

        public static List<ISongFilePlugin> GetPlugins()
        {
            return new List<ISongFilePlugin>(_plugins.Values);
        }

        public static List<ISongFilePlugin> GetWriterPlugins()
        {
            return _plugins.Values.Where(p => p.IsWritingSupported()).ToList();
        }
    }
}
