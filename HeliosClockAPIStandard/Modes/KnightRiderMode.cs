using LuminCommon.Helper;
using LuminCommon.Interfaces;
using LuminCommon.LedCommon;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace LuminClientControlAPI.Modes
{
    public class KnightRiderMode : ILEDMode
    {
        /// <summary>Starts Knights Rider mode.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunMode(ILuminManager manager, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                int ledCount = manager.LedController.LedCount;

                //20% of LEDs used for Knight rider mode
                int knightCount = (int)Math.Round(((double)ledCount / 100.0 * 20.0), 0);

                var leds = new LedScreen(manager.LedController);

                int colorCount = 1;
                int startIndex = 0;

                bool isClockwise = true;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var colors = await ColorHelpers.DimColor(manager.StartColor, knightCount, true).ConfigureAwait(false);

                    for (int i = 0; i < ledCount; i++)
                    {
                        int index = isClockwise ? i : ledCount - i - 1;

                        if (i >= startIndex && i < startIndex + colorCount)
                            leds.SetPixel(ref index, colors[i - startIndex]);
                        else
                            leds.SetPixel(ref index, Color.Black);

                    }

                    if (colorCount < knightCount)
                        colorCount++;

                    await manager.LedController.SendPixels(leds.pixels).ConfigureAwait(false);

                    startIndex++;

                    if (startIndex >= ledCount)
                    {
                        colorCount = 1;
                        startIndex = 0;
                        isClockwise = !isClockwise;
                    }

                    await Task.Delay(manager.RefreshSpeed, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}