using LuminCommon.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LuminClientControlAPI.Modes
{
    public class DiscoMode : ILEDMode
    {
        /// <summary>Spins the LEDs.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunMode(ILuminManager manager, CancellationToken cancellationToken)
        {
            var ledController = manager.LedController;
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await manager.SetRandomColor().ConfigureAwait(false);
                    await Task.Delay(manager.RefreshSpeed, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken);
        }
    }
}
