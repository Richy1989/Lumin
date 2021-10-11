﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LuminClientControlAPI.Modes;
using LuminCommon.Attributes;
using LuminCommon.Configurator;
using LuminCommon.Enumerations;
using LuminCommon.EventArgs;
using LuminCommon.GlobalEvents;
using LuminCommon.Helper;
using LuminCommon.Interfaces;
using LuminCommon.LedCommon;
using Microsoft.Extensions.Logging;

namespace HeliosClockAPIStandard
{
    public class LuminManager : ILuminManager
    {
        private System.Timers.Timer autoOffTmer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private readonly ILuminConfiguration luminConfiguration;
        public event EventHandler<NotifyControllerEventArgs> NotifyController;
        private readonly ILogger<LuminManager> logger;
        public ILedController LedController { get; set; }
        public int RefreshSpeed { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }

        /// <summary>Gets or sets the global brightness.</summary>
        /// <value>The global brightness.</value>
        public int Brightness
        {
            get { return LedController.Brightness; }
            set { LedController.Brightness = value; }
        }

        public bool IsRunning => autoOffTmer.Enabled;
        public bool IsModeRunning { get; set; }
        private LedMode runningLedMode = LedMode.None;

        /// <summary>Gets or sets the automatic off time in [ms].</summary>
        /// <value>The automatic off time.</value>
        public double AutoOffTime { get; set; }

        /// <summary>Initializes a new instance of the <see cref="HeliosManager"/> class.</summary>
        /// <param name="ledController">The led controller.</param>
        public LuminManager(ILedController ledController, ILuminConfiguration luminConfiguration, IGlobalEventManager globalEventManager, ILogger<LuminManager> logger)
        {
            RefreshSpeed = 100;
            LedController = ledController;
            this.luminConfiguration = luminConfiguration;
            this.logger = logger;

            CreateAutoOffTimer();

            //When Configuration Changes, reload AutoOffTimer
            luminConfiguration.OnConfigurationChanged += (s, e) =>
            {
                if (e.Args == nameof(luminConfiguration.AutoOffTime))
                    CreateAutoOffTimer();
            };

            logger.LogInformation("Lumin Manager Initialized ...");

            //////Wait 500ms and turn the LEDs to black. Enusre it is black on startup.
            ////Task.Run(async () =>
            ////{
            ////    await Task.Delay(500).ConfigureAwait(false);
            ////    await SetOnOff(PowerOnOff.Off, LedSide.Full, Color.Black, false).ConfigureAwait(false);
            ////    //autoOffTmer.Enabled = false;
            ////});
        }

        /// <summary>Creates the automatic off timer.</summary>
        private void CreateAutoOffTimer()
        {
            double timeInHours = luminConfiguration.AutoOffTime;
            AutoOffTime = timeInHours * 60.0 * 60.0 * 1000.0; // milliseconds

            if (autoOffTmer != null)
            {
                autoOffTmer.Elapsed -= AutoOffTmer_Elapsed;
                autoOffTmer.Stop();
                autoOffTmer.Dispose();
            }

            logger.LogDebug("Starting auto off timer with interval: {0} hour ...", timeInHours);
            autoOffTmer = new System.Timers.Timer(AutoOffTime);
            autoOffTmer.Elapsed += AutoOffTmer_Elapsed;
        }

        //Reset Timer
        private async Task ResetTimer(PowerOnOff onOff)
        {
            await StopLedMode().ConfigureAwait(false);
            autoOffTmer.Stop();

            if (onOff == PowerOnOff.On)
            {
                autoOffTmer.Start();
            }
        }

        /// <summary>Notifies the controllers.</summary>
        public void NotifyControllers()
        {
            NotifyController?.Invoke(this, new NotifyControllerEventArgs { StartColor = StartColor, EndColor = EndColor });
        }

        /// <summary>Refreshes the screen.</summary>
        public async Task RefreshScreen()
        {
            await LedController.Repaint().ConfigureAwait(false);
        }

        /// <summary>Handles the Elapsed event of the AutoOffTmer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private async void AutoOffTmer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (e != null)
                logger.LogDebug("AutoOff Timer Invoked ...");

            await SetOnOff(PowerOnOff.Off, LedSide.Full, Color.Black).ConfigureAwait(false);
            Brightness = 255;
        }

        /// <summary>Sets the random color.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SetRandomColor()
        {
            Random rnd = new();
            Color startColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            rnd = new Random();
            Color endColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            await SetColor(startColor, endColor, ColorInterpolationMode.HueMode).ConfigureAwait(false);
        }

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="interpolationMode"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SetColor(Color startColor, Color endColor, ColorInterpolationMode interpolationMode)
        {
            autoOffTmer.Stop();
            autoOffTmer.Start();

            var leds = new LedScreen(LedController);

            StartColor = startColor;
            EndColor = endColor;

            bool useSmoothing = true;

            //If mode is running, let only the color object change, but do not transfer colors to screen.
            if (IsModeRunning)
            {
                var enumType = typeof(LedMode);
                var memberInfos = enumType.GetMember(runningLedMode.ToString());
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
                var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(LedModeAttribute), false);

                if (!((LedModeAttribute)valueAttributes[0]).CanSetColor)
                    return;

                useSmoothing = ((LedModeAttribute)valueAttributes[0]).UseSmoothing;
            }

            var colors = await ColorHelpers.ColorGradient(StartColor, EndColor, LedController.LedCount, interpolationMode).ConfigureAwait(false);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                leds.SetPixel(ref i, colors[i]);
            }

            LedController.IsSmoothing = useSmoothing;
            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
            LedController.IsSmoothing = false;
        }

        /// <summary>Sets the LEDs either On or Off.</summary>
        /// <param name="onOff">The on off.</param>
        /// <param name="side">The side.</param>
        /// <param name="onColor">Color of the on.</param>
        public async Task SetOnOff(PowerOnOff onOff, LedSide side, Color onColor)
        {
            await SetOnOff(onOff, side, onColor, false);
        }

        /// <summary>Sets the LEDs either On or Off.</summary>
        /// <param name="onOff">The on off.</param>
        /// <param name="side">The side.</param>
        /// <param name="onColor">Color of the on.</param>
        public async Task SetOnOff(PowerOnOff onOff, LedSide side, Color onColor, bool ignoreTimer)
        {
            await StopLedMode().ConfigureAwait(false);
            autoOffTmer.Stop();

            if (onOff == PowerOnOff.On)
            {
                autoOffTmer.Start();
            }
            
            ////else
            ////{
            ////    await StopLedMode().ConfigureAwait(false);       
            ////}

            //Ignore the timer on initialization Turn off from constructor.
            ////if (!ignoreTimer)
            ////{
            ////    await StopLedMode().ConfigureAwait(false);
            ////    autoOffTmer?.Stop();
            ////    autoOffTmer?.Start();
            ////}

            LedController.IsSmoothing = false;

            var leds = new LedScreen(LedController);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                if (side == LedSide.Full)
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? onColor : Color.Black);
                }
                else if (side == LedSide.Left && i < (int)Math.Round(LedController.LedCount / 2.0))
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? onColor : Color.Black);
                }
                else if (side == LedSide.Right && i >= (int)Math.Round(LedController.LedCount / 2.0))
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? onColor : Color.Black);
                }
                else
                {
                    if (LedController.ActualScreen != null)
                        leds.SetPixel(ref i, LedController.ActualScreen[i].LedColor);
                }
            }

            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);

            StartColor = LedController.ActualScreen[0].LedColor;
            EndColor = LedController.ActualScreen[LedController.ActualScreen.Length - 1].LedColor;
        }

        /// <summary>Stops the led mode.</summary>
        public async Task StopLedMode()
        {
            cancellationTokenSource?.Cancel();

            while (IsModeRunning && !cancellationToken.IsCancellationRequested)
                await Task.Delay(1).ConfigureAwait(false);

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        /// <summary>Runs the led mode.</summary>
        /// <param name="mode">The mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunLedMode(LedMode mode)
        {
            await StopLedMode().ConfigureAwait(false);
            autoOffTmer.Stop();
            autoOffTmer.Start();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            Task.Run(async () =>
            {
                IsModeRunning = true;
                runningLedMode = mode;
                ILEDMode ledMode = null;
                try
                {
                    switch (mode)
                    {
                        case LedMode.Spin:
                            ledMode = new SpinLEDsMode();
                            break;

                        case LedMode.KnightRider:
                            ledMode = new KnightRiderMode();
                            break;
                        case LedMode.Disco:
                            ledMode = new DiscoMode();
                            break;
                        default:
                            break;
                    }

                    await ledMode.RunMode(this, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                }
                finally
                {
                    IsModeRunning = false;
                    runningLedMode = LedMode.None;
                    NotifyControllers();
                }
            }, cancellationToken).ConfigureAwait(false);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>Disposes the specified disposing.</summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        protected virtual async void Dispose(bool disposing)
        {
            await StopLedMode().ConfigureAwait(false);

            logger?.LogInformation("Disposing Client. Turn off! ...");

            //Turning LEDs off.
            AutoOffTmer_Elapsed(this, null);
            autoOffTmer?.Stop();
            autoOffTmer?.Dispose();

            LedController?.Dispose();
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}