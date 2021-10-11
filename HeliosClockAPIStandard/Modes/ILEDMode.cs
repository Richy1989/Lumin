using LuminCommon.Interfaces;
using LuminCommon.LedCommon;
using System.Threading;
using System.Threading.Tasks;

namespace LuminClientControlAPI.Modes
{
    public interface ILEDMode
    {
        //Runs the LED mode
        Task RunMode(ILuminManager manager, CancellationToken cancellationToken);
    }
}
