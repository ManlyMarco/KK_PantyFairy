﻿using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_PantyFairy.Data;
using KK_PantyFairy.Events;
using KKAPI;
using KKAPI.MainGame;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;

namespace KK_PantyFairy
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    public partial class PantyFairyPlugin : BaseUnityPlugin
    {
        internal static PantyFairyPlugin Instance;
        public static new ManualLogSource Logger { get; set; }

        private static ConfigEntry<bool> _relaxedStatChecks;
        private static ConfigEntry<bool> _alwaysUnlockPowers;

        internal static bool IsHentStatBelow(int belowThis) => !_relaxedStatChecks.Value && EventUtils.GetPlayerData()?.hentai < belowThis;
        internal static bool IsSkillsForceUnlock() => _alwaysUnlockPowers.Value;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            _relaxedStatChecks = Config.Bind("Cheats", "Skip H stat checks", false,
                new ConfigDescription("Allow using all skills and progressing through the side story without leveling up the H stat.", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
            
            _alwaysUnlockPowers = Config.Bind("Cheats", "Always unlock all powers", false,
                new ConfigDescription("Allow using all skills without progressing the side story. WARNING: Might mess up the side story progression!", null, new ConfigurationManagerAttributes { IsAdvanced = true }));

#if !DEBUG
            GameAPI.RegisterExtraBehaviour<PantyFairyGameController>(GUID);
#else
            StartDebug();
#endif
        }

        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        internal static Sprite GetSprite(string texName)
        {
            _spriteCache.TryGetValue(texName, out var spr);
            if (spr == null)
            {
                spr = ResourceUtils.GetEmbeddedResource(texName, typeof(PantyFairyPlugin).Assembly).LoadTexture().ToSprite();
                _spriteCache[texName] = spr;
            }
            return spr;
        }

#if DEBUG
        private void StartDebug()
        {
            //_hi = Harmony.CreateAndPatchAll(typeof(Hooks));
            CustomEvents.Progress = StoryProgress.E1_Initial;
            CustomEvents.SaveData.PantiesStolenHeld = 0;
            if (!Manager.Game.Instance.actScene.isEventNow)
            {
                // Console.WriteLine("starting scene");
                // CustomEvents.StartE6(Disposable.Empty);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(100, 70, 600, 600),
                $"{CustomEvents.Progress} / {CustomEvents.SaveData.EventProgress}\nHeld={CustomEvents.SaveData.PantiesStolenHeld} Total={CustomEvents.SaveData.PantiesStolenTotal}  Uni={CustomEvents.SaveData.UniformsStolenTotal}");
            if (GUI.Button(new Rect(70, 70, 25, 25), "P+"))
                CustomEvents.Progress++;
            if (GUI.Button(new Rect(40, 70, 25, 25), "H+"))
                CustomEvents.SaveData.PantiesStolenHeld++;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                CustomEvents.StartE3_2(Disposable.Empty);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                CustomEvents.StartE3(Disposable.Empty);
        }

        private static Harmony _hi;

        private void OnDestroy()
        {
            _hi?.UnpatchSelf();
            CustomEvents.Progress = StoryProgress.Unknown;

            foreach (var sprite in _spriteCache) Destroy(sprite.Value);
        }
#endif
    }
}
