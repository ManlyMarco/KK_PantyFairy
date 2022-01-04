using BepInEx;
using KKAPI;

namespace KK_PantyFairy
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class PantyFairyPlugin : BaseUnityPlugin
    {
        public const string GUID = "PantyFairy";
        public const string PluginName = "PantyFairy";
        public const string Version = "1.0.1";
    }
}