using static System.Environment;

namespace LuminCommon.Defaults
{
    public class DefaultValues
    {
        public static string UnixSavePath = string.Format("{0}/.luminConfig/lumin.config", GetFolderPath(SpecialFolder.UserProfile));

        public const string BaseUrl = "/hubs/lumin";

        public static int SignalPortOne = 5000;

        public const string HubUrl = "http://{0}:{1}/hubs/lumin";

        public const string AllClients = "AllClient";

        public const string DefaultClientName = "Led Client";

        public static int DefaultLedCount = 58;

        public static int DefaultAutoOffTime = 1;

        public static int GpioLeftPin = 17;

        public static int GpioRightPin = 16;
    }
}
