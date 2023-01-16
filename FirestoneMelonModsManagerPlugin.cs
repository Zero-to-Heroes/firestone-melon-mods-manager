using MelonLoader;
using HarmonyLib;
using System.Linq;
using System;
using UnityEngine;
using MelonLoader.TinyJSON;
using Newtonsoft.Json;

namespace FirestoneMelonModsManager
{
    public class FirestoneMelonModsManagerPlugin : MelonPlugin
    {
        public static MelonLogger.Instance SharedLogger;

        public static string currentBobSkin = null;

        private WebSocketServerPlugin wsServer;

        public override void OnPreInitialization()
        {
            FirestoneMelonModsManagerPlugin.SharedLogger = LoggerInstance;
            var harmony = this.HarmonyInstance;

            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Creating websocket");
            this.wsServer = new WebSocketServerPlugin();
            int port = 9977;
            var wsLocation = this.wsServer.OpenServer(9978, new Action(PublishModsInfo), new Action<string>(OnMessage));
            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Websocket server created, listening on port {port} at location {wsLocation}");
        }

        private void PublishModsInfo()
        {
            var loadedMelons = MelonAssembly.LoadedAssemblies.SelectMany(a => a.LoadedMelons).ToList();
            var stringified = JsonConvert.SerializeObject(loadedMelons.Select(m => new
            {
                Name = m.Info.Name,
                Registered = m.Registered,
                Version = m.Info.Version,
                DownloadLink = m.Info.DownloadLink,
            }));
            //FirestoneMelonModsManagerPlugin.SharedLogger.Msg(stringified);
            this.wsServer.Broadcast($"{{ \"type\": \"mods-info\", \"data\": {stringified} }}");
        }

        private void OnMessage(string message)
        {
            dynamic msg = JsonConvert.DeserializeObject(message);
            if (msg.type == "toggle-mod")
            { 
                string modName = msg.modName;
                var loadedMelons = MelonAssembly.LoadedAssemblies.SelectMany(a => a.LoadedMelons).ToList();
                var mod = loadedMelons.Find(m => m.Info.Name == modName);
                if (mod == null)
                {
                    FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Couldn't find loaded melon {modName} {message}");
                    return;
                }
                var isRegistered = mod.Registered;
                if (isRegistered)
                {
                    FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Unregistering {modName}");
                    mod.Unregister();
                } 
                else
                { 
                    FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Registering {modName}");
                    mod.Register();
                }
                PublishModsInfo();
            }
        }

        public override void OnApplicationEarlyStart()
        {
            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"OnApplicationEarlyStart");
        }

        public override void OnPreModsLoaded()
        {
            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"OnPreModsLoaded");
        }

        public override void OnPreSupportModule()
        {
            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"OnPreSupportModule");
        }

        public override void OnApplicationStarted()
        {
            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"OnApplicationStarted");
            //GetAllMods();
        }
    }
}
