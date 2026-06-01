using chess.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chess.connection
{
    public class client
    {
        public TcpClient tcp;
        public NetworkStream stream;

        private StreamReader reader;
        private StreamWriter writer;

        public Action<string> on_message;
        public Action on_disconnect;

        public bool connected
        {
            get
            {
                try
                {
                    return tcp != null && tcp.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        public client(string ip, int port)
        {
            this.tcp = new TcpClient();
            this.tcp.Connect(ip, port);

            this.stream = tcp.GetStream();

            this.reader = new StreamReader(stream, Encoding.UTF8);
            this.writer = new StreamWriter(stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

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
            close();
        }

        public async Task send(string message)
        {
            if (!this.connected)
                return;
        this.writer.WriteLine(message);
        }

        public void send_move(Move move)
        {
            if (!this.connected)
                return;

            this.writer.WriteLine(move.ToString()); // sync: ensures message is fully sent before next one
        }
        public void close()
        {
            try
            {
                this.reader?.Close();
                this.writer?.Close();
                this.stream?.Close();
                this.tcp?.Close();
            }
            catch
            {

            }
        }
    }
}

