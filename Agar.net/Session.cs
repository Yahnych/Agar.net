using System;
using SFML;
using SFML.Graphics;
using SFML.Window;
using System.Net;
using System.IO;
using System.Text;
using WebSocketSharp;
using System.Collections.Generic;
using SFML.System;
using System.Threading;
using Agar.Utils;
using System.Diagnostics;

namespace Agar
{
    
    class Session 
    {
        public const byte PROTOCOL_VERSION = 6;
        private WebSocket _ws;
        private bool _open;
        private World _world;

        private Queue<byte[]> _dataQueue;
        //private Mutex _dataMutex;


        public Session(World world)
        {
            _world = world;
            _dataQueue = new Queue<byte[]>();
            //_dataMutex = new Mutex();
        }

        public void ConnectToServer(string url)
        {
            Console.WriteLine("Attempting to connect to " + url + " - ");
            _ws = new WebSocket("ws://" + url);
            _ws.Log.Level = LogLevel.Fatal;
            _ws.OnMessage += (sender, e) => {
                //_dataMutex.WaitOne();
                Console.WriteLine("en");
                _dataQueue.Enqueue(e.RawData);
               // _dataMutex.ReleaseMutex();
            };

            _ws.OnError += (sender, e) => {
                Console.WriteLine(e.Message);
            };

            _ws.OnOpen += (sender, e) =>
            {
                _open = true;
                Console.WriteLine("Connection opened to " + url + " - ");
                SendHandShake();
            };

            _ws.Connect();
        }

        private void SendHandShake()
        {
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);

                writer.Write((byte)254);
                writer.Write((uint)PROTOCOL_VERSION);
                _ws.Send(ms.ToArray());
            }
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);

                writer.Write((byte)255);
                writer.Write(1);
                _ws.Send(ms.ToArray());
            }
            Spawn("hi");
        }

        public void Update()
        {
            if(_open)

            //_dataMutex.WaitOne();
            while(_dataQueue.Count != 0)
            {
                Process(_dataQueue.Dequeue());
            }
            //_dataMutex.ReleaseMutex();
        }

        public bool IsOpen()
        {
            return _open;
        }



        private void Send(byte[] data)
        {
            if (!_open)
                return;

            _ws.Send(data);
            
        }



        // handlers
        private void Process(byte[] data)
        {
            Console.WriteLine("length: " + data.Length);
            if (data.Length == 0)
                return;
            DataReader reader = new DataReader(data);

            byte opcode = reader.ReadByte();
            switch (opcode)
            {
                case 16: // Node update
                    HandleCellUpdates(reader);
                    break;

                case 17: // Update camera in spectator mode
                    UpdateSpectateView(reader);
                    break;

                case 32: // Add Client cell
                    HandledSpawnedCell(reader);
                    break;

                case 49: // (FFA) Leaderboard Update
                    FFALeaderboardUpdate(reader);
                    break;

                case 50: // (Team) Leaderboard Update
                    TeamLeaderboardUpdate(reader);
                    break;

                case 64: // World size message
                    UpdateWorldInfo(reader);
                    break;

                default:
                    Console.WriteLine("Unknown opcode : " + opcode);
                    break;
            }

        }
        private void UpdateSpectateView(DataReader reader)
        {
            float x = reader.Read<float>();
            float y = reader.Read<float>();
            float s = reader.Read<float>();
            _world.SetView(x, y, s);
        }
        public void UpdateWorldInfo(DataReader reader)
        {
            float left = (float)reader.Read<double>();
            float top = (float)reader.Read<double>();
            float right = (float)reader.Read<double>();
            float bottom = (float)reader.Read<double>();
            _world.SetPosition(new Vector2f(((left + right) / 2), ((top + bottom) / 2)));
            _world.SetSize(new Vector2f(right - left, bottom - top));

            //Console.WriteLine("World info : # " + worldX + " # " + worldY + " # " + worldW + " # " + worldH);
        }
        private void FFALeaderboardUpdate(DataReader data)
        {
            /*
            uint32 count = data.getInt();
            for (uint32 i = 0; i < count; ++i)
            {
                uint32 score = data.getInt();
                std::string name = data.getUTF16String();
            }
            */
        }

        private void TeamLeaderboardUpdate(DataReader data)
        {
            /*
            uint32 teamCount = data.getInt();
            std::vector<float> score;
            for (uint32 i = 0; i < teamCount; ++i)
                score.push_back(data.getFloat());
            t0 = score[0];
            t1 = score[1];
            t2 = score[2];
            */
        }
        private void HandledSpawnedCell(DataReader reader)
        {
            _world.playing = true;
            _world.AddOwnedCell(reader.Read<uint>());
        }
        private void HandleCellUpdates(DataReader reader)
        {
            ushort consumeCount = reader.Read<ushort>();
            for (int i = 0; i < consumeCount; i++)
            {
                uint eatenById = reader.Read<uint>();
                uint victimId = reader.Read<uint>();
                _world.RemoveCell(victimId);
            }
            uint cellId;
            while ((cellId = reader.Read<uint>()) != 0)
            {
                int x = reader.Read<int>();
                int y = reader.Read<int>();
                ushort size = reader.Read<ushort>();
                byte flags = reader.ReadByte();
                bool isSpiked = (flags & 0x01) != 0;
                bool newColor = (flags & 0x02) != 0;
                bool newSkin = (flags & 0x04) != 0;
                bool newName = (flags & 0x08) != 0;
                bool isAgitated = (flags & 0x10) != 0;
                bool isEjectedCell = (flags & 0x20) != 0;
                Cell cell = _world.GetCell(cellId);
                if (cell == null)
                    cell = _world.AddCell(cellId);
                if (newColor)
                {
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();
                    cell.Color = new Color(r, g, b, 255);
                }
                if (newSkin)
                    reader.ReadUTF8String();
                if (newName)
                    cell.Name = reader.ReadUTF8String();
                cell.Mass = size;
                cell.Position = new Vector2i(x, y);
            }
            ushort deathCount = reader.Read<ushort>();
            for (int i = 0; i < deathCount; i++)
            {
                uint victimId = reader.Read<uint>();
                _world.RemoveCell(victimId);
            }
        }

        public void Spawn(string name = "")
        {
            if (!_open)
                return;

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write((byte)0);
            writer.Write(Encoding.Unicode.GetBytes(name));
            _ws.Send(ms.ToArray());
        }

        public void Spectate()
        {
            if (!_open)
                return;

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write((byte)1);
            _ws.Send(ms.ToArray());
        }

        public void SendAim(double x, double y)
        {
            if (!_open)
                return;

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write((byte)16);
            writer.Write(x);
            writer.Write(y);
            writer.Write(0);
            _ws.Send(ms.ToArray());
        }

        public void SendEjectMass()
        {
            if (!_open)
                return;

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write((byte)21);
            _ws.Send(ms.ToArray());
        }

        public void SendSplit()
        {
            if (!_open)
                return;

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write((byte)17);
            _ws.Send(ms.ToArray());
        }


    }
}
