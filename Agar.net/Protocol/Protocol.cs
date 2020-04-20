using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agar.Utils
{
    public abstract class Protocol
    {
        public abstract byte Version { get; }
        protected abstract World CurrentWorld { get; }
        protected abstract void OnMessage(DataReader reader);
        public abstract void SendMouseMove<T>(T x, T y) where T : unmanaged;
        public abstract void SendPlay(string name);
        public virtual void SendChat(string text) { }
        public abstract void SendSplit();
        public abstract void SendEject();
        public abstract void RequestSpectate();

    }
}
