using LuminCommon.Interfaces;
using LuminCommon.LedCommon;
using System.Threading;
using System.Threading.Tasks;

namespace LuminClientControlAPI.Modes
{
    public class SpinLEDsMode : ILEDMode
    {
        /// <summary>Spins the LEDs.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunMode(ILuminManager manager, CancellationToken cancellationToken)
        {
            var ledController = manager.LedController;
            var oldOffest = ledController.PixelOffset;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ledController.PixelOffset++;
                    await Task.Delay(manager.RefreshSpeed, cancellationToken).ConfigureAwait(false);
                    if (ledController.PixelOffset >= ledController.LedCount)
                    {
                        ledController.PixelOffset = 0;
                    }
                    await ledController.Repaint().ConfigureAwait(false);
                }
            }
            catch
            {
            }
            finally
            {
                ledController.PixelOffset = oldOffest;
                await ledController.Repaint().ConfigureAwait(false);
            }
        }
    }
}
