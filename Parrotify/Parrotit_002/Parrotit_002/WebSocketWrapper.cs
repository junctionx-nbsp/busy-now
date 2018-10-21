﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
    public class WebSocketWrapper : IDisposable
    {
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        public readonly ClientWebSocket Client;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        public event Action<WebSocketWrapper> OnConnected;
        public event Action<WebSocketWrapper, string> OnMessage;
        public event Action<WebSocketWrapper> OnDisconnected;

        protected WebSocketWrapper(Uri uri)
        {
            Client = new ClientWebSocket();
            Client.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = uri;
            _cancellationToken = _cancellationTokenSource.Token;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static WebSocketWrapper Create(Uri uri)
        {
            return new WebSocketWrapper(uri);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public WebSocketWrapper Connect()
        {
            ConnectAsync();
            return this;
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal", CancellationToken.None);
        }
        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(string message)
        {
            SendMessageAsync(message);
        }

        private async void SendMessageAsync(string message)
        {
            if (Client.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }

                await Client.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken);
            }
        }

        private async void ConnectAsync()
        {
            try
            {
                await Client.ConnectAsync(_uri, _cancellationToken);

            }
            catch (Exception)
            {
                // TODO
            }
            CallOnConnected();
            StartListen();
        }

        private async void StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (Client.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();


                    WebSocketReceiveResult result;
                    do
                    {
                        result = await Client.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await
                                Client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            CallOnDisconnected();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    CallOnMessage(stringResult);

                }
            }
            catch (Exception)
            {
                CallOnDisconnected();
            }
            finally
            {
                Client.Dispose();
            }
        }

        private void CallOnMessage(StringBuilder stringResult)
        {
            if (OnMessage != null)
                RunInTask(() => OnMessage(this, stringResult.ToString()));
        }

        private void CallOnDisconnected()
        {
            if (OnDisconnected != null)
                RunInTask(() => OnDisconnected(this));
        }

        private void CallOnConnected()
        {
            if (OnConnected != null)
                RunInTask(() => OnConnected(this));
        }

        private static void RunInTask(Action action)
        {
            Task.Factory.StartNew(action);
        }

        public void Dispose()
        {
            if (Client != null)
            {
                Client.Dispose();
            }
        }
    }
}