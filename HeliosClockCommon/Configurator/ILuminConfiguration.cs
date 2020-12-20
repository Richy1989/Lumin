﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HeliosClockCommon.Configurator
{
    public interface ILuminConfiguration
    {
        /// <summary>Occurs when [on configuration changed].</summary>
        event EventHandler<EventArgs<string>> OnConfigurationChanged;

        /// <summary>Gets or sets the name of the LED Client.</summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>Gets or sets the led count for the LED Controller.</summary>
        /// <value>The led count.</value>
        int LedCount { get; set; }

        /// <summary>Gets or sets the automatic off time of the LED controller.</summary>
        /// <value>The automatic off time.</value>
        double AutoOffTime { get; set; }

        /// <summary>Gets or sets the discovery port.</summary>
        /// <value>The discovery port.</value>
        int DiscoveryPort { get; set; }
    }
}
