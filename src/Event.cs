using System;

namespace GaiaLabs
{
    public static class Event
    {
        public static event Action<RomMap> Loaded;
        public static event Action<object> Selected;
        public static event Action<string> Warning;

        public static readonly Action<string> TriggerWarning = (x) => { Warning?.Invoke(x); };
        public static readonly Action<object> TriggerSelected = (x) => { Selected?.Invoke(x); };
        public static readonly Action<RomMap> TriggerLoaded = (x) => { Loaded?.Invoke(x); };
    }
}
