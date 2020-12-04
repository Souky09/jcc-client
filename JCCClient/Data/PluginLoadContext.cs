using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using EFT;
using SharedClasses;

namespace JCCClient.Data
{
    public class PluginLoadContext
    {
        public static IEFTPlugin ReadExtensions(IOptions<ConfigInfo> Config)
        {

            var pluginsLists = new List<IEFTPlugin>();
            var files = Directory.GetFiles("Extensions", "*.dll");
      
            foreach (var file in files)
            {
              
                var assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(Path.Combine(Directory.GetCurrentDirectory(), file))));
                
                var pluginsTypes = assembly.GetTypes().Where(t => typeof(IEFTPlugin).IsAssignableFrom(t) && !t.IsInterface).ToArray();
                if(pluginsTypes!=null)
                {
                    var x = pluginsTypes.FirstOrDefault();
                }
                foreach (var pluginType in pluginsTypes)
                {
                    var pluginInstance = Activator.CreateInstance(pluginType,Config ) as IEFTPlugin;
                    pluginsLists.Add(pluginInstance);
                }
            }
            return pluginsLists.FirstOrDefault();
        }

        public static IEFTPlugin GetTypeFromAssembly(string typeName, IOptions<ConfigInfo> Config)
        {
            var assemblyQName = GetAssemblyName(typeName);
            IEFTPlugin printerObject = null;
            if (!string.IsNullOrEmpty(assemblyQName))
            {
                var type = Type.GetType(assemblyQName);
                printerObject = Activator.CreateInstance(type, Config) as IEFTPlugin;
            }
            return printerObject;

        }

        public static string GetAssemblyName(string typeName)
        {
            var assemblyQName = "";
            var pluginsLists = new List<Type>();
            var files = Directory.GetFiles("Extensions", "*.dll");
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), file));
                var pluginsTypes = assembly.GetTypes().Where(t => typeof(IEFTPlugin).IsAssignableFrom(t) && !t.IsInterface).ToList();
                var found = pluginsTypes.FirstOrDefault(x => x.Name.Trim().ToLower() == typeName.Trim().ToLower());

                if (found != null)
                {
                    assemblyQName = found.AssemblyQualifiedName;
                    break;
                }
            }

            //Console.WriteLine("Quaified name assmbly from gettypeassembluy method =====" + assemblyQName);
            return assemblyQName;
        }

       
    }
}
