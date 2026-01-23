namespace Nautilus
{
    public class Compat
    {
        private static bool? _riskOfOptions;
        public static bool RiskOfOptions
        {
            get
            {
                if (_riskOfOptions == null)
                {
                    _riskOfOptions = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                }
                return (bool)_riskOfOptions;
            }
        }
    }
}