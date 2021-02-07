using LuminCommon.Configurator;

namespace HeliosClockAPIStandard.GpioService
{
    //Mapping GPIO pin to board pin
    public class GpioInputPin
    {
        /// <summary>Initializes a new instance of the <see cref="GpioInputPin"/> class.</summary>
        /// <param name="configuration">The configuration.</param>
        public GpioInputPin(ILuminConfiguration configuration)
        {
            LeftSide = configuration.GpioLeftPin;
            RightSide = configuration.GpioRightPin;
        }
        
        public int LeftSide { get; set; }
        public int RightSide { get; set; }
    }
}