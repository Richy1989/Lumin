using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LuminCommon.GlobalEvents
{
    public class GlobalEventManager : IGlobalEventManager
    {
        private readonly ILogger<GlobalEventManager> logger;
        private static Dictionary<Guid, (GlobalEvents globalEvent, Action callback)> eventDictionary = new Dictionary<Guid, (GlobalEvents, Action)>();

        /// <summary>Initializes a new instance of the <see cref="GlobalEventManager"/> class.</summary>
        /// <param name="logger">The logger.</param>
        public GlobalEventManager(ILogger<GlobalEventManager> logger)
        {
            this.logger = logger;
        }

        /// <summary>Registers the specified global event.</summary>
        /// <param name="globalEvent">The global event.</param>
        /// <param name="callback">The callback.</param>
        public void Register(GlobalEvents globalEvent, Action callback)
        {
            logger.LogDebug("Register for global event listening: {0}", globalEvent);
            eventDictionary.Add(Guid.NewGuid(), (globalEvent, callback));
        }

        /// <summary>Throws the global event.</summary>
        /// <param name="globalEvent">The global event.</param>
        public void ThrowGlobalEvent(GlobalEvents globalEvent)
        {
            foreach (var keyValPair in eventDictionary.Where(s => s.Value.globalEvent == globalEvent))
            {
                //Check if callback is not null
                if (keyValPair.Value.callback == null)
                    continue;

                try
                {
                    //Invoke all callbacks in threads
                    Task.Run(() => keyValPair.Value.callback.Invoke());
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Error invoking glob event: {0}. Message: {1}", keyValPair.Value.globalEvent, ex.Message);
                }
            }

            //Remove null values from dictionary, cause the unregister function is not mandatory
            //Note: we do this after executing the event to not delay the invoking of the global events
            eventDictionary = eventDictionary.Where(s => s.Value.callback != null).ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
