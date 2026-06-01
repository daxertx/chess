using chess.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chess.connection
{
    public class server
    {
        public TcpListener listener;
        public TcpClient client;
        public NetworkStream stream;

        private StreamReader reader;
        private StreamWriter writer;

        public Action<string> on_message;
        public Action on_disconnect;
        public Action on_connect;

        public bool connected
        {
            get
            {
                try
                {
                    return this.client != null && this.client.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        public server(int port)
        {
            this.listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task start()
        {
            this.listener.Start();

            this.client = await this.listener.AcceptTcpClientAsync();

            this.stream = this.client.GetStream();
            this.reader = new StreamReader(this.stream, Encoding.UTF8);

            this.writer = new StreamWriter(this.stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

            this.on_connect?.Invoke();

            _ = listen();
        }
        public void send_promotion(piece.PieceType new_type, int x, int y)
        {
            //col,row
            if (!this.connected)
                return;
            string message = $"PROMOTION:{new_type}{x}{y}";
            this.writer.WriteLine(message);
        }
        public async Task listen()
        {
            try
            {
                while (this.connected)
                {
                    string message = await this.reader.ReadLineAsync();

                    if (message == null)
                        break;

                    this.on_message?.Invoke(message);
                }
            }
            catch
            {

            }

            this.on_disconnect?.Invoke();

            stop();
        }
        public async Task send(string message)
        {
            if (!this.connected)
                return;

            await this.writer.WriteLineAsync(message);
        }
        public void send_move(Move move)
        {
            if (!this.connected)
                return;

            this.writer.WriteLine(move.ToString()); // sync: ensures message is fully sent before next one
        }
        public void stop()
        {
            try
            {
                this.reader?.Close();
                this.writer?.Close();
                this.stream?.Close();
                this.client?.Close();
                this.listener?.Stop();
            }
            catch
            {

            }
        }
    }
}
