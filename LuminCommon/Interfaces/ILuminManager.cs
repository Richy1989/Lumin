using LuminCommon.Enumerations;
using LuminCommon.EventArgs;
using LuminCommon.LedCommon;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace LuminCommon.Interfaces
{
    public interface ILuminManager : IDisposable
    {
        /// <summary>Occurs when [notify controller].</summary>
        event EventHandler<NotifyControllerEventArgs> NotifyController;

        /// <summary>Gets or sets the brightness.</summary>
        /// <value>The brightness.</value>
        int Brightness { get; set; }

        /// <summary>Gets a value indicating whether this instance is running.</summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        bool IsRunning { get; }

        /// <summary>Gets or sets the led controller.</summary>
        /// <value>The led controller.</value>
        ILedController LedController { get; set; }

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        Color StartColor { get; set; }

        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        Color EndColor { get; set; }

        /// <summary>Gets or sets the automatic off time in [ms].</summary>
        /// <value>The automatic off time.</value>
        double AutoOffTime { get; set; }

        /// <summary>Gets or sets the refresh speed.</summary>
        /// <value>The refresh speed.</value>
        int RefreshSpeed { get; set; }

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        Task SetOnOff(PowerOnOff onOff, LedSide side, Color color);

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        Task SetOnOff(PowerOnOff onOff, LedSide side, Color color, bool ignoreTimer);

        /// <summary>Runs the led mode.</summary>
        /// <param name="mode">The mode.</param>
        Task RunLedMode(LedMode mode);

        /// <summary>Stops the led mode.</summary>
        Task StopLedMode();

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <returns></returns>
        Task SetColor(Color startColor, Color endColor, ColorInterpolationMode interpolationMode);

        /// <summary>Sets the random color.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task SetRandomColor();

        /// <summary>Refreshes the screen.</summary>
        Task RefreshScreen();

        /// <summary>Notifies the controllers.</summary>
        void NotifyControllers();
    }
}
