using System;
using System.Collections;
using System.Collections.Generic;
using ADV;
using HarmonyLib;
using KK_PantyFairy.Data;
using KK_PantyFairy.Events;
using KKAPI.MainGame;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace KK_PantyFairy.Functions
{
    internal static class PantyStealFeat
    {
        private static IDisposable _dispose, _disposeClothes;
        private static bool _applied;
        public static bool Enabled
        {
            get => _applied;
            set
            {
                if (_applied == value) return;
                _applied = value;

                PantyFairyPlugin.Logger.LogDebug("PantySteal/panty enabled: " + value);

                if (value)
                {
                    var hi = Harmony.CreateAndPatchAll(typeof(PantyStealFeat), typeof(PantyStealFeat).FullName);
                    var ico = GameAPI.AddTouchIcon(PantyFairyPlugin.GetSprite("touch_icon_pantsu.png"), button => button.onClick.AddListener(OnIconClick), 1, 691);
                    _dispose = Disposable.Create(() =>
                    {
                        hi.UnpatchSelf();
                        ico.Dispose();
                    });
                }
                else
                {
                    ClothesEnabled = false;
                    _dispose?.Dispose();
                }
            }
        }

        public static bool ClothesEnabled
        {
            get => _clothesEnabled;
            set
            {
                if (_clothesEnabled == value) return;
                _clothesEnabled = value;

                PantyFairyPlugin.Logger.LogDebug("PantySteal/clothes enabled: " + value);

                if (value)
                {
                    Enabled = true;
                    var ico = GameAPI.AddTouchIcon(PantyFairyPlugin.GetSprite("touch_icon_clothes.png"), button => button.onClick.AddListener(OnIconClickClothes), 1, 692);
                    _disposeClothes = ico;
                }
                else
                {
                    _disposeClothes?.Dispose();
                }
            }
        }

        private static void OnIconClick()
        {
            PantyFairyPlugin.Instance.StartCoroutine(StealPanty(Object.FindObjectOfType<TalkScene>()));
        }

        private static void OnIconClickClothes()
        {
            PantyFairyPlugin.Instance.StartCoroutine(StealClothes(Object.FindObjectOfType<TalkScene>()));
        }

        private static readonly HashSet<SaveData.Heroine> _depantified = new HashSet<SaveData.Heroine>();
        private static readonly List<SaveData.Heroine> _declothified = new List<SaveData.Heroine>();
        private static bool _clothesEnabled;

        public static bool IsDepantified(ChaControl instance)
        {
            var he = instance.GetHeroine();
            return he != null && _depantified.Contains(he);
        }

        public static void Depantify(ChaControl instance)
        {
            // todo separate between coordinates?
            var he = instance.GetHeroine();
            if (he != null) _depantified.Add(he);
        }

        public static bool IsDeclothified(ChaControl instance)
        {
            var he = instance.GetHeroine();
            return he != null && _declothified.Contains(he);
        }

        public static void Declothify(ChaControl instance)
        {
            var he = instance.GetHeroine();
            if (he != null) _declothified.Add(he);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState), typeof(int), typeof(byte), typeof(bool))]
        private static void SetClothesStateHook(ChaControl __instance, int clothesKind, ref byte state)
        {
            var kind = (ChaFileDefine.ClothesKind)clothesKind;
            if (kind == ChaFileDefine.ClothesKind.shorts)
            {
                if (Enabled && IsDepantified(__instance))
                    state = 3;
            }
            else
            {
                if (ClothesEnabled && IsDeclothified(__instance))
                    state = 3;
            }
        }

        private static IEnumerator StealClothes(TalkScene talkScene)
        {
            var targetGirl = talkScene.targetHeroine;

            //todo split general parts and specifics?

            var list = EventUtils.CreateNewEvent();
            if (!Enabled || !CustomEvents.StealingEnabled || targetGirl == null)
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Uhh, what was I doing again? My head is blank."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            if (targetGirl.isTeacher || targetGirl.schoolClass == -1)
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I can't steal clothes from her, it's way too risky."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            var chaCtrl = targetGirl.chaCtrl;
            var currentCoordinateType = chaCtrl.fileStatus.coordinateType;
            var currentCoord = targetGirl.charFile.coordinate[currentCoordinateType];

            var topPart = currentCoord.clothes.parts[0];
            var topListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_top, topPart.id);
            var notHasTop = topPart.id == 0 || topListInfo == null || chaCtrl.fileStatus.clothesState[0] == 3;

            var botPart = currentCoord.clothes.parts[1];
            var botListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_bot, botPart.id);
            var notHasBot = botPart.id == 0 || botListInfo == null || chaCtrl.fileStatus.clothesState[1] == 3;

            //var braPart = currentCoord.clothes.parts[2];
            //var braListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_bra, braPart.id);
            //var notHasBra = braPart.id == 0 || braListInfo == null || chaCtrl.fileStatus.clothesState[2] == 3;
            //
            //var pantsPart = currentCoord.clothes.parts[3];
            //var pantsListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_shorts, pantsPart.id);
            //var notHasPan = pantsPart.id == 0 || pantsListInfo == null || chaCtrl.fileStatus.clothesState[3] == 3;

            if (notHasTop || IsDeclothified(chaCtrl))
            {
                if (notHasBot)
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Hmm... Looks like there's nothing I could steal."));
                else
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Hmm... I don't think her clothes are stealable."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I should find someone wearing a full uniform."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            if (PantyFairyPlugin.IsHentStatBelow(80))
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Is something wrong?"));
                if (Random.value > 0.5f)
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Just practicing my Jojo poses, how fabulous do I look?"));
                else
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ah? No, nothing, my hand just cramped!"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hmm..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I feel like something is holding me back. Could it be I'm not perverted enough?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(You need at least 80 Sensuality/H points to use this skill)"));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            var successProbability = GetStealProbability(targetGirl, true);

            if (Random.value > successProbability)
            {
                // Failed to get them
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hmm?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I could feel the spell working, but it got dispelled. Did she notice?"));

            retry:
                if (Random.value > 0.8f)
                {
                    // Failed the roll twice. Time to get angry, get the stock get angry event and spice it up
                    var senarioData = TalkSceneUtils.GetSenarioData(targetGirl, "42"); // 42 - angry event
                    if (senarioData == null) goto retry; // todo make this not dumb, probably not relevant since this shouldn't happen
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hey..."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Oh crap, she noticed!"));
                    foreach (var item in senarioData) list.Add(Program.Transfer.Create(item.Multi, item.Command, item.Args));
                    list.Insert(list.Count - 3, Program.Transfer.Text(EventUtils.PlayerName, "I'm sorry, I didn't mean to!"));
                    list.Add(Program.Transfer.Close());
                    targetGirl.isAnger = true;
                    targetGirl.anger = Mathf.Min(100, targetGirl.anger + 50);
                    return EventUtils.StartTextSceneEvent(talkScene, list, true, true);
                }
                else
                {
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Is something wrong?"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ah? No, nothing, just practicing my Jojo poses!"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Ehh? Are all boys like that?"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Um, probably! Please don't mind me."));
                    list.Add(Program.Transfer.Close());
                    return EventUtils.StartTextSceneEvent(talkScene, list, decreaseTalkTime: true);
                }
            }
            else
            {
                // Succeeded at getting them
                Declothify(chaCtrl);
                chaCtrl.SetClothesStateAll(0);
                // Apply to the overworld npc as well
                PantyFairyPlugin.Instance.StartCoroutine(new WaitUntil(() => targetGirl.chaCtrl != chaCtrl).AppendCo(() => targetGirl.chaCtrl.SetClothesStateAll(0)));

                var noticed = Random.value > 0.3f;
                if (noticed)
                {
                    talkScene.Touch(TalkSceneUtils.TouchLocation.MuneL, TalkSceneUtils.TouchKind.Touch, Vector3.zero); // todo add a way to run commands in the list, maybe set a var and check it every frame
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Kya?!"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Clothes appear in my hands as she jumps, startled."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Is something wrong?"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Umm, it feels like... No... Never mind..."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "If you say so."));
                }
                else
                {
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, $"Clothes appear in my hands while she's completely oblivious."));
                }

                CustomEvents.SaveData.UniformsStolenTotal++;

                list.Add(Program.Transfer.Close());
                var evt = EventUtils.StartTextSceneEvent(talkScene, list, decreaseTalkTime: true);
                if (noticed) evt = evt.AppendCo(() => targetGirl.lewdness = Mathf.Min(100, targetGirl.lewdness + 50));
                return evt;
            }
        }

        private static IEnumerator StealPanty(TalkScene talkScene)
        {
            var targetGirl = talkScene.targetHeroine;

            var list = EventUtils.CreateNewEvent();
            if (!Enabled || !CustomEvents.StealingEnabled || targetGirl == null)
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Uhh, what was I doing again? My head is blank."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            if (targetGirl.isTeacher || targetGirl.schoolClass == -1)
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I can't steal panties from her, it's way too risky."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            if (PantyFairyPlugin.IsHentStatBelow(65))
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Is something wrong?"));
                if (Random.value > 0.5f)
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Just practicing my Jojo poses, how fabulous do I look?"));
                else
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ah? No, nothing, my hand just cramped!"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hmm..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I feel like something is holding me back. Could it be I'm not perverted enough?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(You need at least 65 Sensuality/H points to use this skill)"));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            var chaCtrl = targetGirl.chaCtrl;
            var currentCoordinateType = chaCtrl.fileStatus.coordinateType;
            const int pantsPartIndex = 3;
            var currentCoord = targetGirl.charFile.coordinate[currentCoordinateType];
            var pantsPart = currentCoord.clothes.parts[pantsPartIndex];
            var pantsId = pantsPart.id;
            var pantsListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_shorts, pantsId);
            var noPan = pantsId == 0 || pantsListInfo == null || chaCtrl.fileStatus.clothesState[pantsPartIndex] == 3;

            var botPart = currentCoord.clothes.parts[1];
            var botListInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_bot, botPart.id);
            var notHasBot = botPart.id == 0 || botListInfo == null || chaCtrl.fileStatus.clothesState[1] == 3;

            if (IsDepantified(chaCtrl))
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I already stole her panties, I should try again tomorrow."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }
            else if (noPan && notHasBot)
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Looks like she's not wearing any panties?"));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }
            else if (notHasBot && CustomEvents.Progress < StoryProgress.E7_Uniform && !PantyFairyPlugin.IsSkillsForceUnlock())
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "If I stole her panties, she wouldn't be covered at all down there. It would definitely get noticed."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I should wait until the fairy's power is stronger before attempting this level of degeneracy."));
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }
            else if (noPan)
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hmm?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "The spell went off, but nothing happened. Could it be that she's not wearing any panties?"));
                if (Random.value > 0.8f)
                {
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Is something wrong?"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ah? No, nothing, my hand just cramped!"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Really? Are you okay?"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Yeah, don't worry about it!"));
                }
                list.Add(Program.Transfer.Close());
                return EventUtils.StartTextSceneEvent(talkScene, list);
            }

            var successProbability = GetStealProbability(targetGirl, false);

            if (Random.value > successProbability)
            {
                // Failed to get them
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hmm?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I could feel the spell working, but it got dispelled. Did she notice?"));

            retry:
                if (Random.value > 0.8f)
                {
                    // Failed the roll twice. Time to get angry, get the stock get angry event and spice it up
                    var senarioData = TalkSceneUtils.GetSenarioData(targetGirl, "42"); // 42 - angry event
                    if (senarioData == null) goto retry; // todo make this not dumb, probably not relevant since this shouldn't happen
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hey..."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Oh crap, she noticed!"));
                    foreach (var item in senarioData) list.Add(Program.Transfer.Create(item.Multi, item.Command, item.Args));
                    list.Insert(list.Count - 3, Program.Transfer.Text(EventUtils.PlayerName, "I'm sorry, I didn't mean to!"));
                    list.Add(Program.Transfer.Close());
                    targetGirl.isAnger = true;
                    targetGirl.anger = Mathf.Min(100, targetGirl.anger + 50);
                    return EventUtils.StartTextSceneEvent(talkScene, list, true, true);
                }
                else
                {
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Is something wrong?"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ah? No, nothing, just practicing my Jojo poses!"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Ehh? Are all boys like that?"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Um, probably! Please don't mind me."));
                    list.Add(Program.Transfer.Close());
                    return EventUtils.StartTextSceneEvent(talkScene, list, false, true);
                }
            }
            else
            {
                // Succeeded at getting them
                Depantify(chaCtrl);
                chaCtrl.SetClothesState(pantsPartIndex, 3);
                // Apply to the overworld npc as well
                PantyFairyPlugin.Instance.StartCoroutine(new WaitUntil(() => targetGirl.chaCtrl != chaCtrl).AppendCo(() => targetGirl.chaCtrl.SetClothesState(pantsPartIndex, 3)));

                var noticed = Random.value > 0.7f;
                if (noticed)
                {
                    talkScene.Touch(TalkSceneUtils.TouchLocation.MuneL, TalkSceneUtils.TouchKind.Touch, Vector3.zero); // todo add a way to run commands in the list, maybe set a var and check it every frame
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Kya?!"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Panties appear in my hand as she jumps, startled."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Is something wrong?"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Umm, it feels like... No... Never mind..."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "If you say so."));
                }
                else
                {
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach out with my hand and whisper \"Steal!\""));
                    string panName = TranslationHelper.TryTranslate(pantsListInfo.Name, out panName) ? panName : pantsListInfo.Name;
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, $"Panties appear in my hand while she's completely oblivious. Looks like it's {panName}!"));
                }

                CustomEvents.SaveData.PantiesStolenHeld++;
                CustomEvents.SaveData.PantiesStolenTotal++;

                if (CustomEvents.SaveData.PantiesStolenHeld >= 2)
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, $"(I've got {CustomEvents.SaveData.PantiesStolenHeld} pairs now)"));

                list.Add(Program.Transfer.Close());
                var evt = EventUtils.StartTextSceneEvent(talkScene, list, decreaseTalkTime: true);
                if (noticed) evt = evt.AppendCo(() => targetGirl.lewdness = Mathf.Min(100, targetGirl.lewdness + 30));
                return evt;
            }
        }

        public static float GetStealProbability(SaveData.Heroine targetGirl, bool clothes)
        {
            float successProbability;
            if (clothes)
            {
                if (!CustomEvents.StealingClothesEnabled) return 0f;

                var lvls = CustomEvents.SaveData.Progress - StoryProgress.E7_Uniform;
                successProbability = Mathf.Clamp01(lvls * 0.2f);
            }
            else
            {
                if (!CustomEvents.StealingEnabled)
                    return 0f;

                if (CustomEvents.SaveData.PantiesStolenHeld == 0)
                    return 1f;

                var lvls = CustomEvents.SaveData.Progress - StoryProgress.E5_Steal;
                successProbability = Mathf.Clamp01(lvls * 0.1f);
            }

            if (!targetGirl.isAnger)
            {
                if (targetGirl.isGirlfriend || targetGirl.HExperience == SaveData.Heroine.HExperienceKind.淫乱) successProbability += 0.8f;
                else if (targetGirl.HExperience == SaveData.Heroine.HExperienceKind.慣れ) successProbability += 0.65f;
                else if (targetGirl.HExperience == SaveData.Heroine.HExperienceKind.不慣れ) successProbability += 0.5f;
                else successProbability += 0.35f;

                if (targetGirl.parameter.attribute.bitch) successProbability += 0.1f;
                if (targetGirl.parameter.attribute.choroi) successProbability += 0.1f;
                if (targetGirl.parameter.attribute.donkan) successProbability += 0.1f;
                if (targetGirl.parameter.attribute.kireizuki) successProbability -= 0.1f;
                if (targetGirl.parameter.attribute.majime) successProbability -= 0.1f;

                switch (targetGirl.personality)
                {
                    case 0: //sexy
                    case 11: //motherly
                    case 13: //gyaru
                    case 18: //jinxed
                    case 19: //bookish
                    case 24: //yandere
                    case 33: //glamorous
                        successProbability += 0.1f;
                        break;
                    case 2: //snobby
                    case 12: //bigsis
                    case 14: //deliquent
                    case 15: //wild
                    case 17: //reluctant
                    case 36: //sadistic
                        successProbability -= 0.1f;
                        break;
                }
            }

            return successProbability;
        }

        public static void ClearDepantified()
        {
            _depantified.Clear();
            _declothified.Clear();
        }
    }
}