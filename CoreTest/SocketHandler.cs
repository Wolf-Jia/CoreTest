using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTest
{
    public class SocketHandler
    {
        private static List<WebSocket> _sockets = new List<WebSocket>();
        public const int BUFFER_SIZE = 4096;
        public static object objLock = new object();

        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }

        static async Task Acceptor(HttpContext httpContext, Func<Task> n)
        {
            var socket = await httpContext.WebSockets.AcceptWebSocketAsync();

            if (_sockets.Count >= 100)
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "连接数超过限制", CancellationToken.None);
                return;
            }

            lock (objLock)
            {
                _sockets.Add(socket);
            }

            var buffer = new byte[BUFFER_SIZE];

            string username = httpContext.Request.Query["username"].ToString();

            var chatData = new ChatData() { info = username + "进入房间，当前在线" + _sockets.Count + "人" };

            await SendToWebSocketsAsync(_sockets, chatData);

            while (true)
            {
                try
                {
                    var incoming = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (incoming.MessageType == WebSocketMessageType.Close)
                    {
                        lock (objLock)
                        {
                            _sockets.Remove(socket);
                        }

                        chatData = new ChatData() { info = username + "离开房间，还剩" + _sockets.Count + "人" };

                        await SendToWebSocketsAsync(_sockets, chatData);

                        break;
                    }
                    var chatDataStr = await ArraySegmentToStringAsync(new ArraySegment<byte>(buffer, 0, incoming.Count));
                    if (chatDataStr == "heartbeat")
                    {
                        continue;
                    }

                    chatData = JsonConvert.DeserializeObject<ChatData>(chatDataStr);
                    chatData.time = DateTime.Now;
                    await SendToWebSocketsAsync(_sockets.Where(t => t != socket).ToList(), chatData);

                }
                catch (Exception ex)
                {

                }
            }
        }


        /// <summary>
        /// 发送消息到所有人
        /// </summary>
        /// <param name="sockets"></param>
        /// <param name="arraySegment"></param>
        /// <returns></returns>
        public async static Task SendToWebSocketsAsync(List<WebSocket> sockets, ChatData data)
        {
            var chatData = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(chatData);
            ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer);

            foreach (WebSocket socket in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <param name="arraySegment"></param>
        /// <returns></returns>
        static async Task<string> ArraySegmentToStringAsync(ArraySegment<byte> arraySegment)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
