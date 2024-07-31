/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

namespace Socks5.Core.Plugin
{
    public static class PluginLoader
    {
        //load plugin statically.

        private static readonly List<Type> _pluginTypes = new List<Type>()
    {
        typeof(LoginHandler), typeof(DataHandler), typeof(ConnectHandler), typeof(ClientConnectedHandler),
        typeof(ConnectSocketOverrideHandler)
    };

        private static bool _loaded;
        public static bool LoadPluginsFromDisk { get; set; }

        public static List<object> GetPlugins { get; } = new List<object>();

        public static void LoadPlugins()
        {
            if (_loaded) return;
            try
            {
                try
                {
                    foreach (var f in Assembly.GetExecutingAssembly().GetTypes())
                        try
                        {
                            if (!CheckType(f))
                            {
                                var type = Activator.CreateInstance(f);
                                if (type != null)
                                {
                                    GetPlugins.Push(type);
                                }
#if DEBUG
                                Console.WriteLine("Loaded Embedded Plugin {0}.", f.FullName);
#endif
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    foreach (var f in (Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly().GetTypes() : new Type[0]))
                    {
                        try
                        {
                            if (CheckType(f))
                            {
                                continue;
                            }

                            //Console.WriteLine("Loaded type {0}.", f.ToString());
                            var type = Activator.CreateInstance(f);
                            if (type != null)
                            {
                                GetPlugins.Push(type);
                            }
#if DEBUG
        Console.WriteLine("Loaded Plugin {0}.", f.FullName);
#endif
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                //load plugins from disk?
                if (LoadPluginsFromDisk)
                {
                    var pluginPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
                    if (!Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
                    foreach (var filename in Directory.GetFiles(pluginPath))
                    {
                        if (!filename.EndsWith(".dll"))
                        {
                            continue;
                        }

                        //Initialize unpacker.
                        var g = Assembly.Load(File.ReadAllBytes(filename));
                        //Test to see if it's a module.
                        foreach (var f in g.GetTypes())
                        {
                            if (CheckType(f))
                            {
                                continue;
                            }

                            var plug = Activator.CreateInstance(f);
                            if (plug !=null)
                            {
                                GetPlugins.Push(plug);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var p in e.LoaderExceptions)
                {
                    Console.WriteLine(p?.ToString());
                }
            }

            _loaded = true;
        }

        public static bool LoadCustomPlugin(Type f)
        {
            try
            {
                if (!CheckType(f))
                {
                    //Console.WriteLine("Loaded type {0}.", f.ToString());
                    var type = Activator.CreateInstance(f);
                    if (type != null)
                    {
                        GetPlugins.Push(type);
                    }
#if DEBUG
                    Console.WriteLine("Loaded Plugin {0}.", f.FullName);
#endif
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        private static bool CheckType(Type p)
        {
            return _pluginTypes.All(x => !x.IsAssignableFrom(p) || p == x);
        }

        public static List<object> LoadPlugin(Type assemblyType)
        {
            // Make sure plugins are loaded.
            var list = new List<object>();
            foreach (var x in GetPlugins)
            {
                if (assemblyType.IsInstanceOfType(x))
                {
                    var plugin = (IGenericPlugin)x;
                    if (plugin.OnStart() && plugin.Enabled)
                    {
                        list.Add(x);
                    }
                }
            }
            return list;
        }

        public static void ChangePluginStatus(bool enabled, Type pluginType)
        {
            foreach (var x in GetPlugins)
            {
                if (x.GetType() == pluginType)
                {
                    // Cast to generic type.
                    ((IGenericPlugin)x).Enabled = enabled;
                    break;
                }
            }
        }

    }
}

