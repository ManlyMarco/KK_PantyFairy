using System;
using System.Collections;
using System.Linq;
using ADV;
using HarmonyLib;
using KKAPI.MainGame;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace KK_PantyFairy.Functions
{
    public static class GustOfWindFeat
    {
        private static IDisposable _dispose;
        private static bool _applied;
        public static bool Enabled
        {
            get => _applied;
            set
            {
                if (_applied == value) return;
                _applied = value;

                PantyFairyPlugin.Logger.LogDebug("GustOfWind enabled: " + value);

                if (value)
                {
                    var hi = Harmony.CreateAndPatchAll(typeof(GustOfWindFeat), typeof(GustOfWindFeat).FullName);
                    var ico = GameAPI.AddTouchIcon(PantyFairyPlugin.GetTexture("touch_icon_wind.png").ToSprite(), button => button.onClick.AddListener(Start), 1, 690);
                    _dispose = Disposable.Create(() =>
                    {
                        hi.UnpatchSelf();
                        ico.Dispose();
                    });
                }
                else
                {
                    _dispose?.Dispose();
                }
            }
        }

        private static void Start()
        {
            PantyFairyPlugin.Instance.StartCoroutine(StartCo(Object.FindObjectOfType<TalkScene>()));
        }

        private static IEnumerator StartCo(TalkScene talkScene)
        {
            if (talkScene == null) throw new ArgumentNullException(nameof(talkScene), "Not inside of a TalkScenee");

            var heroine = talkScene.targetHeroine;

            if (!Enabled || heroine == null)
            {
                var list = EventApi.CreateNewEvent();
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Uhh, what was I doing again? My head is blank."));
                list.Add(Program.Transfer.Close());
                yield return EventApi.StartTextSceneEvent(talkScene, list, false);
                yield break;
            }

            var chaCtrl = heroine.chaCtrl;
            var currentCoordinateType = chaCtrl.fileStatus.coordinateType;
            var currentCoord = heroine.charFile.coordinate[currentCoordinateType];
            //const int pantsPartIndex = 3;
            //var pantsPart = currentCoord.clothes.parts[pantsPartIndex];
            //var pantsId = pantsPart.id;
            //var pantsListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_shorts, pantsId);
            //var noPan = pantsId == 0 || pantsListInfo == null || chaCtrl.fileStatus.clothesState[pantsPartIndex] == 3;

            var botPart = currentCoord.clothes.parts[1];
            var botListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_bot, botPart.id);
            var notHasBot = botPart.id == 0 || botListInfo == null || chaCtrl.fileStatus.clothesState[1] == 3;

            if (PantyFairyPlugin.IsTotalPointsBelow(150))
            {
                var list = EventApi.CreateNewEvent();
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Argh, I can feel the power but it's hard to focus it. Looks like I need to improve my imagination some more."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(You need to have earned at least 150 Koikatsu points in total to use this skill)"));
                list.Add(Program.Transfer.Close());
                yield return EventApi.StartTextSceneEvent(talkScene, list, false);
                yield break;
            }

            var bones = heroine.chaCtrl.GetComponentsInChildren<DynamicBone>();
            var origForces = bones.Select(x => x.m_Force).ToArray();
            void ApplyForce(Vector3 vector3)
            {
                for (var i = 0; i < bones.Length; i++)
                    bones[i].m_Force = origForces[i] + vector3;
            }

            var notCares = PantyStealFeat.GetStealProbability(heroine, false) > Random.Range(0.2f, 0.6f);
            var touchKind = notCares ? TalkSceneUtils.TouchKind.Look : TalkSceneUtils.TouchKind.Touch;

            var endTime = Time.time + 3;
            var noticeTime = Time.time + 1;
            // Dont notice if not wearing skirt or not caring
            if (notHasBot) noticeTime += 10;
            var noticed = false;
            while (endTime > Time.time)
            {
                // skirt flips fully at 0.005f Y
                var extraForce = new Vector3(0, Random.Range(0.003f, 0.008f), 0);
                ApplyForce(extraForce);

                if (!noticed && noticeTime < Time.time)
                {
                    noticed = true;
                    talkScene.Touch(TalkSceneUtils.TouchLocation.MuneL, touchKind);
                }

                yield return new WaitForSeconds(0.4f);
            }

            heroine.lewdness = Mathf.Min(100, heroine.lewdness + (PantyStealFeat.IsDepantified(heroine.chaCtrl) ? 30 : 10));

            // Restore original forces
            ApplyForce(Vector3.zero);
            
            var list2 = EventApi.CreateNewEvent();
            list2.Add(Program.Transfer.Close());
            yield return EventApi.StartTextSceneEvent(talkScene, list2, true);
        }
    }
}