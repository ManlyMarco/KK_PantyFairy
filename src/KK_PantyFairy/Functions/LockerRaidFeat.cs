using System;
using System.Collections;
using System.Collections.Generic;
using ActionGame;
using ActionGame.Chara;
using HarmonyLib;
using KK_PantyFairy.Events;
using KKAPI.MainGame;
using KKAPI.Utilities;
using Manager;
using UniRx;
using UnityEngine;

namespace KK_PantyFairy.Functions
{
    internal static class LockerRaidFeat
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
                
                PantyFairyPlugin.Logger.LogDebug("LockerSearch enabled: " + value);

                if (value)
                {
                    var hi = Harmony.CreateAndPatchAll(typeof(LockerRaidFeat), typeof(LockerRaidFeat).FullName);
                    _dispose = Disposable.Create(() =>
                    {
                        hi.UnpatchSelf();
                    });
                }
                else
                {
                    _dispose?.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a queue with last ten actions the AI has taken
        /// </summary>
        public static Queue<int> GetLastActions(this AI ai)
        {
            var scene = Traverse.Create(ai).Property("actScene").GetValue<ActionScene>();

            // Dictionary<SaveData.Heroine, ActionControl.DesireInfo>
            var dicTarget = Traverse.Create(scene.actCtrl).Field("dicTarget").GetValue<IDictionary>();

            var npc = GetNPC(ai);
            // ActionControl.DesireInfo
            var di = dicTarget[npc.heroine];
            return Traverse.Create(di).Field("_queueAction").GetValue<Queue<int>>();
        }

        /// <summary>
        /// Get the NPC object assigned to this AI
        /// </summary>
        public static NPC GetNPC(this AI ai)
        {
            return Traverse.Create(ai).Property("npc").GetValue<NPC>();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AI), "Result")]
        public static void AfterResult(AI __instance, ActionControl.ResultInfo result)
        {
            if (__instance.accesser.mapNo != 46 || Game.Instance.actScene.Player.mapNo != 46) return; // 46 = changing room

            if (!CustomEvents.LockersEnabled) return;

            var actionHistory = __instance.GetLastActions().ToArray();
            var actionCount = actionHistory.Length;

            if (actionCount < 2) return;

            // The result gives the action that is currently being carried out, same as this
            int currentAction = actionHistory[actionCount - 1];
            // This is the action that we just finished, this is the important one to compare against
            int previousAction = actionHistory[actionCount - 2];

            // 17 (change mind) seems to happen when interrupted, 23 is making them follow you, 25 is being embarassed
            // In all cases the original task was not finished and will be attempted again
            if (currentAction == 23 || currentAction == 17 || currentAction == 25) return;

            if (previousAction != currentAction && previousAction == 0) // 0 Change Clothes
            {
                //var npc = __instance.GetNPC();

                IDisposable icon = null;
                icon = GameAPI.AddActionIcon(46, __instance.position,
                    PantyFairyPlugin.GetSprite("action_point" + "_on.png"),
                    PantyFairyPlugin.GetSprite("action_point" + "_off.png"),
                    () => CustomEvents.StartE4_2(icon),
                    null, true, true);

                // Remove the icon after some time
                PantyFairyPlugin.Instance.StartCoroutine(CoroutineUtils.CreateCoroutine(new WaitForSeconds(30), icon.Dispose));
            }
        }
    }
}