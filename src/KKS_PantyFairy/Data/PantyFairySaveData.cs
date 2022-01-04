using System;
using System.Reflection;
using ExtensibleSaveFormat;

namespace KK_PantyFairy.Data
{
    public sealed class PantyFairySaveData
    {
        public StoryProgress Progress;
        public int EventProgress;
        public int PantiesStolenHeld;
        public int PantiesStolenTotal;
        public int UniformsStolenTotal;
        //public byte[] FairyHeroineData;

        #region SaveLoad

        private static readonly PantyFairySaveData _default = new PantyFairySaveData();
        private static readonly FieldInfo[] _serializedFields = typeof(PantyFairySaveData).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public static PantyFairySaveData Deserialize(PluginData data)
        {
            var result = new PantyFairySaveData();
            if (data != null)
            {
                foreach (var fieldInfo in _serializedFields)
                {
                    if (data.data.TryGetValue(fieldInfo.Name, out var val))
                    {
                        try
                        {
                            if (fieldInfo.FieldType.IsEnum) val = (int)val;
                            fieldInfo.SetValue(result, val);
                        }
                        catch (Exception ex)
                        {
                            PantyFairyPlugin.Logger.LogError($"Could not deserialize field {fieldInfo.Name} because of error: {ex.Message}");
                        }
                    }
                }

                if (result.Progress == StoryProgress.Unknown || !Enum.IsDefined(typeof(StoryProgress), result.Progress))
                {
                    result.Progress = StoryProgress.E1_Initial;
                    PantyFairyPlugin.Logger.LogInfo("Resetting Story Progress to E1_Initial from " + result.Progress);
                }
            }
            else
            {
                result.Progress = StoryProgress.E1_Initial;
            }

            return result;
        }

        public PluginData Serialize()
        {
            var result = new PluginData { version = 1 };
            foreach (var fieldInfo in _serializedFields)
            {
                var value = fieldInfo.GetValue(this);
                // Check if any value is different than default, if not then don't save any data
                var defaultValue = fieldInfo.GetValue(_default);
                if (!Equals(defaultValue, value))
                    result.data.Add(fieldInfo.Name, value);
            }

            return result.data.Count > 0 ? result : null;
        }

        #endregion
    }
}