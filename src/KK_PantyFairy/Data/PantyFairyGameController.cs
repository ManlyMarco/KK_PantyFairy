using ActionGame;
using KK_PantyFairy.Events;
using KK_PantyFairy.Functions;
using KKAPI.MainGame;

namespace KK_PantyFairy.Data
{
    public class PantyFairyGameController : GameCustomFunctionController
    {
        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            var data = GetExtendedData();
            CustomEvents.SaveData = PantyFairySaveData.Deserialize(data);
        }

        protected override void OnGameSave(GameSaveLoadEventArgs args)
        {
            var data = CustomEvents.SaveData.Serialize();
            SetExtendedData(data);
        }

        protected override void OnDayChange(Cycle.Week day)
        {
            PantyStealFeat.ClearDepantified();
        }
    }
}