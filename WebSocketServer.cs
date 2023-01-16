using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirestoneMelonModsManager
{
    class WebSocketServerPlugin
    {
        private ConcurrentDictionary<Guid, IWebSocketConnection> sockets = new ConcurrentDictionary<Guid, IWebSocketConnection>();

        internal string OpenServer(int port, Action onClientConnect, Action<string> onMessage)
        {
            var server = new WebSocketServer($"ws://127.0.0.1:{port}/firestone-mods-manager");
            FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Websocket server element created ");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    sockets.TryAdd(socket.ConnectionInfo.Id, socket);
                    FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Client connected {socket.ConnectionInfo.Id}");
                    onClientConnect();
                };
                socket.OnClose = () => 
                {
                    sockets.TryRemove(socket.ConnectionInfo.Id, out IWebSocketConnection removedSocket);
                    FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Client disconnected {socket.ConnectionInfo.Id}");
                };
                socket.OnMessage = message =>
                {
                    FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"received message {message}");
                    onMessage(message);
                };
                FirestoneMelonModsManagerPlugin.SharedLogger.Msg($"Websocket server started");
            });
            return server.Location;
        }

        public void Broadcast(string message)
        {
            foreach (var socket in sockets)
            {
                if (socket.Value.IsAvailable)
                {
                    socket.Value.Send(message);
                }
            }
        }
    }
}
