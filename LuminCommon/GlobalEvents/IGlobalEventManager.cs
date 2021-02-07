using System;
using System.Collections.Generic;
using System.Text;

namespace LuminCommon.GlobalEvents
{
    public interface IGlobalEventManager
    {
        /// <summary>Registers the specified global event.</summary>
        /// <param name="globalEvent">The global event.</param>
        /// <param name="callback">The callback.</param>
        void Register(GlobalEvents globalEvent, Action callback);

        /// <summary>Throws the global event.</summary>
        /// <param name="globalEvent">The global event.</param>
        void ThrowGlobalEvent(GlobalEvents globalEvent);
    }
}
