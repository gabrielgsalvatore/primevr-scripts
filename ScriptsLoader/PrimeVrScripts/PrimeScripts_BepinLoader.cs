#if !DEBUG
using BepInEx;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Reflection;

namespace PrimeVrScripts
{
    [BepInPlugin("h3vr.primevr.PrimeScripts", "PrimeScripts BepinEx Loader", "1.0.0")]
    class PrimeScripts_BepinLoader : BaseUnityPlugin
    {
        public static string PluginPath;

        public static List<string> LoadedPluginPaths = new List<string>();
        public PrimeScripts_BepinLoader()
        {
            PluginPath = this.Info.Location;
            Logger.LogInfo($"PluginPath: {PluginPath}");

            string pluginName = Path.GetFileName(PluginPath);
            Logger.LogInfo($"pluginName: {pluginName}");
            string pluginFolder = Path.GetDirectoryName(PluginPath);
            Logger.LogInfo($"pluginFolder: {pluginFolder}");

            DirectoryInfo directoryInfo = new DirectoryInfo(pluginFolder);
            FileInfo[] filesInDir = directoryInfo.GetFiles("*.dll");

            foreach (FileInfo file in filesInDir)
            {
                Logger.LogInfo($"file.FullName: {file.FullName}");
                if (file.FullName == PluginPath) continue;
                Assembly loadedAssembly = Assembly.LoadFrom(file.FullName);
                Harmony.CreateAndPatchAll(loadedAssembly);
                LoadedPluginPaths.Add(file.FullName);
            }
            
            Logger.LogInfo("PrimeScripts loaded!");
        }

    }
}
#endif