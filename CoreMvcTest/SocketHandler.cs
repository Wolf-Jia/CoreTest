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

namespace CoreMvcTest
{
    public class SocketHandler
    {
        //private static List<WebSocket> _sockets = new List<WebSocket>();
        private static Dictionary<string,WebSocket> _sockets = new Dictionary<string, WebSocket>();
        public const int BUFFER_SIZE = 4096;
        public static object objLock = new object();
        public static List<ChatData> historyMessage = new List<ChatData>();

        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }

        static async Task Acceptor(HttpContext httpContext, Func<Task> n)
        {

            string username = httpContext.Request.Query["username"].ToString();

            var socket = await httpContext.WebSockets.AcceptWebSocketAsync();

            //验证连接数
            if (_sockets.Count >= 100)
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "人数已达上限", CancellationToken.None);
                return;
            }

            //验证用户名是否重复
            if (_sockets.ContainsKey(username))
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "用户名已存在", CancellationToken.None);
                return;
            }

            lock (objLock)
            {
                _sockets.Add(username, socket);
            }

            var chatData = new ChatData() { Content = username + "进入房间，当前在线" + _sockets.Count + "人" };

            await SendToWebSocketsAsync(_sockets.Values.ToList(), chatData);

            while (true)
            {
                try
                {

                    var buffer = new byte[BUFFER_SIZE];

                    var incoming = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (incoming.MessageType == WebSocketMessageType.Close)
                    {
                        lock (objLock)
                        {
                            _sockets.Remove(username);
                        }

                        chatData = new ChatData() { Time = DateTime.Now, Content = username + "离开房间，还剩" + _sockets.Count + "人" };

                        await SendToWebSocketsAsync(_sockets.Values.ToList(), chatData);

                        break;
                    }

                    var chatDataStr = await ArraySegmentToStringAsync(new ArraySegment<byte>(buffer, 0, incoming.Count));
                    //if (chatdatastr == "heartbeat")
                    //{
                    //    continue;
                    //}

                    chatData = JsonConvert.DeserializeObject<ChatData>(chatDataStr);
                    
                    await SendToWebSocketsAsync(_sockets.Values.ToList(), chatData);

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
            if (data.Type == "system")
            {
                data.Content = "(system) " + data.Content;
            }
            else if (data.Type == "message")
            {
                data.Content = data.Username + ": " + data.Content;
            }

            SavaHistoryMessage(data);

            var chatData = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(chatData);
            ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer);

            foreach (WebSocket socket in sockets)
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

        private static void SavaHistoryMessage(ChatData chatdata)
        {
            if (chatdata.Type == "message")
            {
                if (historyMessage.Count > 40)
                {
                    historyMessage.RemoveAt(0);
                }

                lock (historyMessage)
                {
                    historyMessage.Add(chatdata);
                }
            }
        }
    }
}
