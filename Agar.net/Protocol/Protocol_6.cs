using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agar.Utils
{
    public sealed class Protocol_6 : Protocol
    {
        private const byte VERSION = 6;
        public override byte Version => VERSION;

        protected override World CurrentWorld => throw new NotImplementedException();

        public override void RequestSpectate()
        {
            throw new NotImplementedException();
        }

        public override void SendEject()
        {
            throw new NotImplementedException();
        }

        public override void SendMouseMove<T>(T x, T y)
        {
            throw new NotImplementedException();
        }

        public override void SendPlay(string name)
        {
            throw new NotImplementedException();
        }

        public override void SendSplit()
        {
            throw new NotImplementedException();
        }

        protected override void OnMessage(DataReader reader)
        {

            uint opcode = reader.ReadByte();
            switch (opcode)
            {
                case 16: // Node update
                    handleUpdateCells(reader);
                    break;

                case 17: // Update camera in spectator mode
                    handleSpectateCameraMove(reader);
                    break;

                case 32: // Add Client cell
                    handleSpawnCell(reader);
                    break;

                case 49: // (FFA) Leaderboard Update
                    handleFFALeaderboardUpdate(reader);
                    break;

                case 50: // (Team) Leaderboard Update
                    handleTeamLeaderboardUpdate(reader);
                    break;

                case 64: // World size message
                    handleWorldInfo(reader);
                    break;

                default:
                    Console.WriteLine("Unknown opcode : " + opcode);

                    break;
            }

        }
    }
}
