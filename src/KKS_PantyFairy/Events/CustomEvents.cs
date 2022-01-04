using System;
using System.Collections.Generic;
using System.Linq;
using ADV;
using KK_PantyFairy.Data;
using KK_PantyFairy.Functions;
using KKAPI.MainGame;
using KKAPI.Utilities;
using Manager;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KK_PantyFairy.Events
{
    public static class CustomEvents
    {
        public static readonly List<CustomEventData> EventData;

        private static readonly Vector3 _mainEventActionIconPosition = new Vector3(-15.5f, 0.0f, -36.2f);
        private static readonly Vector3 _mainEventPosition = new Vector3(-15.5f, 0.0f, -36.2f);
        private static readonly int _mainEventMapNo = 32; // clothing store

        private const string UnknownName = "？？？";
        private const string FairyName = "Panty Fairy";

        // How many to collect for events
        internal const int PantyRequiredCount = 5;

        private static PantyFairySaveData _saveData;
        public static PantyFairySaveData SaveData
        {
            get => _saveData ?? (_saveData = new PantyFairySaveData());
            set
            {
                if (_saveData == value) return;
                _saveData = value ?? new PantyFairySaveData();
                OnProgressChanged();
            }
        }

        public static StoryProgress Progress
        {
            get => SaveData.Progress;
            set
            {
                if (SaveData.Progress != value)
                {
                    SaveData.Progress = value;
                    SaveData.EventProgress = 0;
                    OnProgressChanged();
                }
            }
        }

        public static bool LockersEnabled => Progress >= StoryProgress.E4_SecondRaid && Progress < StoryProgress.Complete || PantyFairyPlugin.IsSkillsForceUnlock();
        public static bool StealingEnabled => Progress >= StoryProgress.E5_Steal || PantyFairyPlugin.IsSkillsForceUnlock();
        public static bool WindEnabled => Progress >= StoryProgress.E6_GustOfWind || PantyFairyPlugin.IsSkillsForceUnlock();
        public static bool StealingClothesEnabled => Progress >= StoryProgress.E7_Uniform || PantyFairyPlugin.IsSkillsForceUnlock();

        private static void OnProgressChanged()
        {
            var progress = Progress;
            PantyFairyPlugin.Logger.LogDebug("Progress changed to " + progress);
            var anyRunning = false;
            foreach (var eventData in EventData)
            {
                var running = progress == eventData.Index;
                if (running) anyRunning = true;
                eventData.SetRunning(running);
            }
            if (progress != StoryProgress.Complete && !anyRunning)
                PantyFairyPlugin.Logger.LogWarning("EVENT DATA MISSING FOR " + progress);

            LockerRaidFeat.Enabled = LockersEnabled;
            PantyStealFeat.Enabled = StealingEnabled;
            PantyStealFeat.ClothesEnabled = StealingClothesEnabled;
            GustOfWindFeat.Enabled = WindEnabled;
        }

        private static IDisposable MakeActIcon(Action<IDisposable> startEvent, string iconName, int mapNo, Vector3 position, Color color)
        {
            // Todo add a proper event icon
            if (iconName == "action_event") iconName = "action_point";

            IDisposable icon = null;
            icon = GameAPI.AddActionIcon(mapNo, position,
                PantyFairyPlugin.GetTexture(iconName + ".png"), color,
                "Start event",
                () => startEvent(icon),
                delayed: true, immediate: true);
            return icon;
        }

        static CustomEvents()
        {
            EventData = new List<CustomEventData>();

            // 2 - in front of training center
            // 17 - hotel changing room 
            // 33 - beach changing room 

            EventData.Add(new CustomEventData(StoryProgress.E1_Initial, () => MakeActIcon(StartE1, "action_question", 2, new Vector3(29.2f, 0.0f, -114.2f), Color.white))); // near garbage bags

            var eventColor = new Color(255 / 255f, 160 / 255f, 175 / 255f);

            EventData.Add(new CustomEventData(StoryProgress.E2_Meeting, () => MakeActIcon(StartE2, "action_question", _mainEventMapNo, _mainEventActionIconPosition, eventColor)));

            EventData.Add(new CustomEventData(StoryProgress.E3_FirstRaid, () => new CompositeDisposable(
                MakeActIcon(StartE3, "action_event", _mainEventMapNo, _mainEventActionIconPosition, eventColor),
                MakeActIcon(StartE3_2, "action_point", 17, new Vector3(-92.62f, 0.00f, -93.78f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 17, new Vector3(-93.19f, 0.00f, -91.89f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 17, new Vector3(-93.26f, 0.00f, -90.28f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 17, new Vector3(-94.00f, 0.00f, -88.41f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 33, new Vector3(-120.92f, 0.00f, -53.55f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 33, new Vector3(-122.83f, 0.00f, -53.55f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 33, new Vector3(-123.48f, 0.00f, -56.33f), Color.white),
                MakeActIcon(StartE3_2, "action_point", 33, new Vector3(-120.34f, 0.00f, -56.33f), Color.white)
                )));

            EventData.Add(new CustomEventData(StoryProgress.E4_SecondRaid, () => MakeActIcon(StartE4, "action_event", _mainEventMapNo, _mainEventActionIconPosition, eventColor)));

            EventData.Add(new CustomEventData(StoryProgress.E5_Steal, () => MakeActIcon(StartE5, "action_event", _mainEventMapNo, _mainEventActionIconPosition, eventColor)));

            EventData.Add(new CustomEventData(StoryProgress.E6_GustOfWind, () => MakeActIcon(StartE6, "action_event", _mainEventMapNo, _mainEventActionIconPosition, eventColor)));

            EventData.Add(new CustomEventData(StoryProgress.E7_Uniform, () => MakeActIcon(StartE7, "action_event", _mainEventMapNo, _mainEventActionIconPosition, eventColor)));
        }

        public static void StartE1(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent(true);
            list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hmm?"));
            list.Add(Program.Transfer.Text(EventApi.Narrator, "A folded piece of paper is laying on the floor with shoe marks all over it. Looks like something is written on it."));
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
            list.Add(Program.Transfer.Text(UnknownName, "\"There's a ghost in the clothing shop's changing booth, I swear! I was trying on some underwear yesterday and heard strange noises and groans!\""));
            list.Add(Program.Transfer.Text(UnknownName, "\"I asked staff to check, but they said there was nothing there... and that I shouldn't spread fake rumors!"));
            list.Add(Program.Transfer.Text(UnknownName, "\"I swear it's real! And I'm not lying, that's slander! I'm just well trained at noticing people when I'm alone!\""));
            list.Add(Program.Transfer.Text(UnknownName, "\"Can you go and see? I'm too afraid to go there by myself now. What if I got pinned to the ground and ravaged?\""));
            list.Add(Program.Transfer.Text(UnknownName, "\"You're the only person I can trust with this. It's my favourite rubbing spot so this is really imporant to me.\""));
            list.Add(Program.Transfer.Text(EventApi.PlayerName, "...Rubbing spot? Didn't she mean to say shopping spot?"));
            list.Add(Program.Transfer.Text(EventApi.Narrator, "Well trained at noticing people when alone, pinned to the ground and ravaged, rubbing spot... Sounds like she's well cultured."));
            list.Add(Program.Transfer.Text(EventApi.Narrator, "Still, what would make noises in the changing room? It wasn't me and other girls wouldn't peep, maybe some pervert infiltrated the island?"));
            list.Add(Program.Transfer.Text(EventApi.Narrator, "She wrote it was in the clothing shop. I guess I should go check it out in case it's something dangerous."));
            list.Add(Program.Transfer.Close());

            StartMonologueEventAndDisposeIcon(list, icon, () => Progress = StoryProgress.E2_Meeting);
        }

        public static void StartE2(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent();
            var progress = false;
            list.Add(Program.Transfer.Create(true, Command.SceneFade, "out"));
            if (PantyFairyPlugin.IsTotalPointsBelow(75))
            {
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Looks like this is the place that the cultured girl wrote about."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I can't hear anything, as expected. The noises probably came from the staff room behind the wall..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Something rustled up above. A person? No way, there are no floors above in this building."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hey, is anyone up there?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "... No response"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hm?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "What is this smell? I seems familiar, but I can't put my finger on what it is."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "It smells... kind of lewd. Maybe I could tell if I did some more Koikatsu club research."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(You need to have gained at least 75 Koikatsu points in total to continue)"));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", FairyName));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Looks like this is the place the horny girl wrote about."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I can't hear anything out of the ordinary. Did I get pranked? The note did seem pretty weird..."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hm?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Wait a minute! This smell... It smells like..."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Freshly worn panties?!"));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_04_22_004"));
                list.Add(Program.Transfer.Text(UnknownName, "Ufufu!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Who's there?"));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_04_22_003")); // heee
                list.Add(Program.Transfer.Text(UnknownName, "You're quite a naughty one to notice me, aren't you " + EventApi.PlayerName + "? The gossips weren't wrong."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "A feminine voice echoes within the room. Where is the voice coming from? There's no one in the shop, even staff is on a break."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Where are you? Show yourself!"));
                list.Add(Program.Transfer.Text(UnknownName, "Relax... You must think I'm suspicious, but I'm really not! I'm right in front of you, I just can't be seen at the moment."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Indeed, the voice is coming from the middle of the changing booth."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Who are you? I sure hope you're a student, or else I'll have to call for the janitor to clean you up."));
                list.Add(Program.Transfer.Text(UnknownName, "Is that a threat? That's rude. Yes, I am a student, technically speaking. At least when I was still around."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "She does sound like a girl. Actually, she sounds like she could be my type!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "So you're telling me that you are a ghost?"));
                list.Add(Program.Transfer.Text(UnknownName, "You catch on pretty fast, but wrong! I'm actually a fairy, Panty Fairy!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Panty Fairy? So basically a pervert?"));
                list.Add(Program.Transfer.Text(FairyName, "You really are rude, you know. I gather panties to restore my energy, I don't do this just because I like it."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Balls of light appeared where the voice was coming from. They had a warm glow and circled around randomly."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Wha..."));
                list.Add(Program.Transfer.Text(FairyName, "This is the best I can do with what I gathered so far, do you believe me now?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Well... I guess you might be a real ghost... I still think you're just some perverted old dude though."));
                list.Add(Program.Transfer.Text(FairyName, "Sigh. Just so you know, I'm quite the beauty. My human body will be restored once I regain my power. I'll have you eat your words then."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Yeah, sure. So why are you talking to me?"));
                list.Add(Program.Transfer.Text(FairyName, "I want you to help me collect panties of course."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Ugh, do you want me to get expelled? Why not ask the girls directly?"));
                list.Add(Program.Transfer.Text(FairyName, "I tried, but they all ignored me! I wish I could just go get their panties myself, but they have to be given to me."));
                list.Add(Program.Transfer.Text(FairyName, "Ah, don't worry about getting expelled! I'll use my powers to protect you. As long as you keep bringing me panties that is."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Use your powers? What can you do?"));
                list.Add(Program.Transfer.Text(FairyName, "Glad you asked! I can manipulate perceptions, make people not notice certain things. Like, for example, their panties missing."));
                list.Add(Program.Transfer.Text(FairyName, "With this you'll be able to steal the panties with no risk, and use them as you wish. This should be a pretty good deal for you."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "So you'll give me protection from being found out in exchange for panties? Who do you think I am?"));
                list.Add(Program.Transfer.Text(FairyName, "A pervert?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I hate that you're right. So, what should I do?"));
                list.Add(Program.Transfer.Text(FairyName, "My powers are very limited at the moment, so we need to be careful. For now, bring me any panties you can find."));
                list.Add(Program.Transfer.Text(FairyName, "The safest place to get them would be in the changing rooms. Sneak in there when no one's around and grab a pair from one of the drawers."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Won't the owner notice their panties missing?"));
                list.Add(Program.Transfer.Text(FairyName, "Not with my blessing. They will believe in something harmless, like forgetting their panties or throwing them away."));
                list.Add(Program.Transfer.Text(FairyName, "Just don't overdo it, I can only do so much at the moment."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright, fine, I'll do it. For purely academic purposes."));
                list.Add(Program.Transfer.Text(FairyName, "Yes, of course. No lewd motives here, fufu. I'll wait for you here, have a safe trip!"));

                progress = true;
            }

            list.Add(Program.Transfer.Create(false, Command.SceneFade, "in"));
            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(
                EventApi.StartAdvEvent(list, _mainEventPosition, Quaternion.identity, extraData: new Program.OpenDataProc
                {
                    onLoad = () =>
                    {
                        if (progress)
                        {
                            icon.Dispose();
                            Progress = StoryProgress.E3_FirstRaid;
                        }
                    }
                }));
        }

        public static void StartE3(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent();
            list.Add(Program.Transfer.Create(true, Command.SceneFade, "out"));
            list.Add(Program.Transfer.Create(true, Command.FontColor, "Color2", FairyName));
            var progress = false;
            if (SaveData.PantiesStolenTotal == 0)
            {
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventApi.PlayerName + "! Did you get the panties?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "No, not yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002"));
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "As if! It's not easy to do this, just you know."));
                list.Add(Program.Transfer.Text(FairyName, "Don't worry, with my blessing you should be fine. Just sneak into either the hotel or beach locker room and grab a pair."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I'm going to regret this, aren't I."));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventApi.PlayerName + "! Did you get the fresh panties?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Looks like no one in the shop is paying me any attention. I close the courtain and take out the spoils."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Here."));
                list.Add(Program.Transfer.Text(FairyName, "Oh? Oooh～!"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Balls of light appear out of thin air and float around the panties. The panties float up and disappear in a flash."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Whoa."));
                list.Add(Program.Transfer.Text(FairyName, "Hmm..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Something's wrong?"));
                list.Add(Program.Transfer.Text(FairyName, "Ah, sorry, I was trying them out."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Pervert."));
                list.Add(Program.Transfer.Text(FairyName, "Rude! I meant in a spiritual way! I don't even have my body back yet."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "..."));
                list.Add(Program.Transfer.Text(FairyName, "Why are you giving me the silent stare?! You're not even looking directly at me."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I try to adjust my glare towards the source of the voice, but she keeps moving around and eventually I give up."));
                list.Add(Program.Transfer.Text(FairyName, "Well, anyways! I gained much less energy than I expected from these panties. The quality might be too low."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Quality? Of the fabric?"));
                list.Add(Program.Transfer.Text(FairyName, "No, that's not important. I need something more freshly worn. The fresher the better."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "And how am I supposed to get that?! I already risked my skin for this one."));
                list.Add(Program.Transfer.Text(FairyName, "Relax. My powers got a little stronger, and it's not like you have to pull them off the person."));
                list.Add(Program.Transfer.Text(FairyName, "Just hide in one of the changing rooms and wait. After someone gets changed you swoop in, grab the goods, and flee. Easy peasy."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "What if I refuse?"));
                list.Add(Program.Transfer.Text(FairyName, "Let's see... I could haunt you to the grave... Ah, I could also spread rumors about you stealing panties."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Great, so I never had a choice."));
                list.Add(Program.Transfer.Text(FairyName, "Hey, I don't want to do any of this! I want us to have a friendly working relationship. Keeping you safe is a part of that."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Sounds sketchy, but I guess I have no choice but to trust you. I'll see what I can do."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I guess I'll hide in the locker room immediately after the next period starts and hope for the best."));
                progress = true;
            }

            list.Add(Program.Transfer.Create(false, Command.SceneFade, "in"));
            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(
                EventApi.StartAdvEvent(list, _mainEventPosition, Quaternion.identity, extraData: new Program.OpenDataProc
                {
                    onLoad = () =>
                    {
                        if (progress)
                        {
                            icon.Dispose();
                            Progress = StoryProgress.E4_SecondRaid;
                        }
                    }
                }));
        }

        public static void StartE3_2(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent(true);

            switch (SaveData.EventProgress)
            {
                case 0:
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "I think this is the locker of that cheeky girl from class next door. She's cute, but the personality..."));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "The door doesn't budge, it's locked."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "So much for that."));
                    SaveData.EventProgress++;
                    break;
                case 1:
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "The locker looks beat up. Pulling on the door makes it creak and squirm, but it doesn't open."));
                    SaveData.EventProgress++;
                    break;
                case 2:
                    var topInt = Game.HeroineList.FirstOrDefault(x => x.relation >= 3);
                    if (topInt == null)
                    {
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "\"Chikarin\" is scribbled on the side of the locker. It's locked tight."));
                    }
                    else
                    {
                        var fullname = topInt.charFile.parameter.fullname;
                        if (TranslationHelper.TryTranslate(fullname, out var tlName)) fullname = tlName;
                        list.Add(Program.Transfer.Create(true, Command.Replace, "Other", fullname));
                        list.Add(Program.Transfer.Text(EventApi.PlayerName, "This locker... It belongs to [Other]!"));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "It's... It's unlocked! What is she thinking?! And her panties are right there, in full view!"));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "Ugh, should I take them? I feel bad for her..."));

                        list.Add(Program.Transfer.Create(true, Command.Choice, "true", "Take them.,yes", "Look for another pair.,no"));

                        list.Add(Program.Transfer.Create(true, Command.Tag, "yes"));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "It feels wrong, but I can't pass on this opportunity!"));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "I put hope in the fairy's blessing and took the panties from the locker. They seem to be brand new, but that's fine too."));
                        list.Add(Program.Transfer.VAR("string", "Took", "yes"));
                        list.Add(Program.Transfer.Close());

                        list.Add(Program.Transfer.Create(true, Command.Tag, "no"));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "I'd rather have a proper relationship, where I can ask for her panties without worry."));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "I paid my respects to the locker and closed the door without taking anything."));
                        list.Add(Program.Transfer.Close());
                    }
                    SaveData.EventProgress++;
                    break;
                case 3:
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "This one's open! Let's see..."));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "After digging for a while I found a neatly folded pair of pure white panties. They look brand new."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "Nice."));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "Let's get out of here before somebody sees me."));
                    SaveData.EventProgress = 5;
                    SaveData.PantiesStolenTotal++;
                    break;
                default:
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "I've already got what I came for. I should go back to the fairy in the clothing shop."));
                    break;
            }

            list.Add(Program.Transfer.Close());

            StartMonologueEventAndDisposeIcon(list, icon, () =>
            {
                var scenarioVars = ActionScene.instance.AdvScene.Scenario.Vars;
                scenarioVars.TryGetValue("Took", out var val);
                if (val?.o?.ToString() == "yes")
                {
                    PantyFairyPlugin.Logger.LogDebug("Found the Took var");
                    SaveData.EventProgress = 5;
                    SaveData.PantiesStolenTotal++;
                    scenarioVars.Remove("Took");
                }
            });
        }

        private static void StartE4(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent();
            list.Add(Program.Transfer.Create(true, Command.SceneFade, "out"));
            list.Add(Program.Transfer.Create(true, Command.FontColor, "Color2", FairyName));
            var progress = false;
            if (SaveData.EventProgress < 5)
            {
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventApi.PlayerName + "! Did you get the fresh panties?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "No, not yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002"));
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "As if! It's not easy to do this, just you know."));
                list.Add(Program.Transfer.Text(FairyName, "Don't worry, you'll be fine. Just sneak into the locker room, wait for someone to change, and grab the used pair."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright..."));
            }
            else
            {
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventApi.PlayerName + "! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I reach inside a pocket in my pants and pull it out."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Right here, freshly harvested."));
                list.Add(Program.Transfer.Text(FairyName, "Ooh～! Let me see, let me see!"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Balls of light gather around the panties and quickly absorb them."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(FairyName, "Yes, this is much better! I can feel my power growing, just like the thing in your pants."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Pervert～ How shameless～"));
                list.Add(Program.Transfer.Text(FairyName, "Don't act shy now! I bet you put them in that pocket so you could rub against them!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Wait, what? I didn't do that, how did you even come up with that scenario?!"));
                list.Add(Program.Transfer.Text(FairyName, "I swear, once I get my body back I'll bonk your head so hard that I'll fix your IQ loss from horniness."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I'm looking forward to seeing you try. My horniness is absolute."));
                list.Add(Program.Transfer.Text(FairyName, "Your wha... No, whatever, let's leave it at that."));
                list.Add(Program.Transfer.Text(FairyName, "I have some bad news and some good news."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright, give me the bad news first."));
                list.Add(Program.Transfer.Text(FairyName, $"The bad news is, I'll need around {PantyRequiredCount} more panties."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I have to do this " + PantyRequiredCount + " more times? Give me a break. What's the good news then?"));
                list.Add(Program.Transfer.Text(FairyName, "You're going to love this. I gathered enough energy to give you a new power."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "A new power? You made me harder to notice before, so some sort of stealth?"));
                list.Add(Program.Transfer.Text(FairyName, "Not even close. I'll give you a technique that can instantly teleport a nearby pair of panties into your hand."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "That sounds ridiculous yet amazing. How do I use this skill?"));
                list.Add(Program.Transfer.Text(FairyName, "It's pretty simple! Reach out with your hand and yell \"Hippity hoppity your pantsu is now my property!\""));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Are you kidding me?! I'll either die from embarassment or get expelled, probably both. Isn't there some other way?"));
                list.Add(Program.Transfer.Text(FairyName, "You've been good to me, so I guess I should help you out. You can also yell \"Steal!\" to the same effect."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "That's not much better, do I really have to yell?"));
                list.Add(Program.Transfer.Text(FairyName, "Well, I guess a whisper would work too, but that's boring! My blessing will protect you from getting caught anyways, so why not let loose?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "No thanks, I'm fine with being tight. Is there anything else I should know?"));
                list.Add(Program.Transfer.Text(FairyName, "Let me think... Stealing might fail if the girl is too cautious of you, it will work better on friends and lovers."));
                list.Add(Program.Transfer.Text(FairyName, "Also, the skill uses horniness as fuel, so you might have to do some training in that."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, $"Got it. I'll see you once I collect {PantyRequiredCount} panties."));
                list.Add(Program.Transfer.Text(FairyName, "Alrgiht, have fun!"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(To steal panties first talk to a girl, then click on the panty icon at top right.)"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(For now, only attempt stealing if they are wearing a skirt or pants. Stealing bikini bottoms is too risky with current power level.)"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(You can also steal from lockers after someone changes like before.)"));

                progress = true;
                SaveData.PantiesStolenHeld = 0;
                SaveData.EventProgress = 0;
            }

            list.Add(Program.Transfer.Create(false, Command.SceneFade, "in"));
            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(
                EventApi.StartAdvEvent(list, _mainEventPosition, Quaternion.identity, extraData: new Program.OpenDataProc
                {
                    onLoad = () =>
                    {
                        if (progress)
                        {
                            icon.Dispose();
                            Progress = StoryProgress.E5_Steal;
                        }
                    }
                }));
        }

        public static void StartE4_2(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent(true);
            if (Progress == StoryProgress.E4_SecondRaid)
            {
                switch (SaveData.EventProgress)
                {
                    case 0:
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "The locker door is bulging out. It pops open the moment I touch it."));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "It's full to the brim with a ball of warm compressed clothes. What a mess."));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "I dig through the clothes and only find a box of half-melted chocolates. No panties in sight."));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "I guess I have to try again."));
                        SaveData.EventProgress++;
                        break;
                    case 1:
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "The door of the locker is slightly ajar. A neatly folded set of clothes fills most of the insides."));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "A warm pair of light pink panties is hidden under of the skirt."));
                        list.Add(Program.Transfer.Text(EventApi.PlayerName, "Sorry, I'll be taking that."));
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "Score! Time to report back to the Panty Fairy."));
                        SaveData.EventProgress = 5;
                        SaveData.PantiesStolenTotal++;
                        SaveData.PantiesStolenHeld++;
                        break;
                    default:
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "I already found a pair of panties. I should report back to Panty Fairy in the clothing shop before they get cold."));
                        break;
                }
            }
            else
            {
                if (Random.value <= 0.2f)
                {
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "The locker is locked, I can't get in."));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "Unlucky, I'll have to try again."));
                }
                else
                {
                    if (Random.value <= 0.5f)
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "The door of the locker is slightly ajar. A neatly folded set of clothes fills most of the insides."));
                    else
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "The locker is unlocked. The clothes inside are left in a complete disarray."));

                    if (Random.value <= 0.2f)
                    {
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "There are no panties in sight. Looks like I'll have to try again."));
                    }
                    else
                    {
                        list.Add(Program.Transfer.Text(EventApi.Narrator, "A pair of cute panties is hidden under other clothes. The panties are still warm, owner couldn't have gone far."));
                        list.Add(Program.Transfer.Text(EventApi.PlayerName, "Sorry, I'll be taking that."));
                        SaveData.PantiesStolenTotal++;
                        SaveData.PantiesStolenHeld++;
                    }
                }
            }

            list.Add(Program.Transfer.Close());

            StartMonologueEventAndDisposeIcon(list, icon, () => { });
        }

        private static void StartE5(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent();
            list.Add(Program.Transfer.Create(true, Command.SceneFade, "out"));
            list.Add(Program.Transfer.Create(true, Command.FontColor, "Color2", FairyName));
            var progress = false;

            if (SaveData.PantiesStolenHeld < PantyRequiredCount)
            {
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventApi.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, $"No, I didn't get all {PantyRequiredCount} pairs yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002"));
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "As if! It's not easy to do this, just you know."));
                list.Add(Program.Transfer.Text(FairyName, "Don't worry, you'll be fine. Just talk to someone that wears more than just a bikini and press the steal button at top right."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright..."));
            }
            else
            {
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventApi.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I puff my chest out and bump it with a fist."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Of course! I'm a bit of a technician if I do say so myself."));
                list.Add(Program.Transfer.Text(FairyName, "Heh～? Are you sure it's not all because of my power?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "It's not that easy to use, just so you know. You need to be in the right mindset."));
                list.Add(Program.Transfer.Text(FairyName, "A perverted mindset."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Shush. So, is this enough for you?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, $"I close the courtain and hand over {PantyRequiredCount} of the panties I collected. Small lights quickly gather around my hands and swallow the goods."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(FairyName, "Oh! Oh～! This is really good!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "So can I finally see you?"));
                list.Add(Program.Transfer.Text(FairyName, "No, not yet! This will restore a good chunk of my power but I still need more."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Ugh. Will this ever end, or am I your lifetime dealer now?"));
                list.Add(Program.Transfer.Text(FairyName, "Don't be so impatient, just enjoy the panties. I know, I'll give you a reward!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "What kind of a reward?"));
                list.Add(Program.Transfer.Text(FairyName, "A power of perverted wind! You can blow someone's skirt upwards and peep all you want. Bonus points if you steal the panties first, hehehe."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "That sounds... interesting. How do I use it?"));
                list.Add(Program.Transfer.Text(FairyName, "Just imagine wind blowing upwards, it's not that hard. You might need to train your imagination a bit though."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Power of perversion, got it. Still, how did you know I would like this?"));
                list.Add(Program.Transfer.Text(FairyName, "I just felt like I should give you something like this. Glad to hear you like it."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Why does this feel like talking with a close guy friend? I really missed this since transferring here."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "You know, if you weren't a guy I'd probably want to date you."));
                list.Add(Program.Transfer.Text(FairyName, "Like I said, I'm not a guy! I'll friendzone you just as if I was a guy though."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "That's harsh, at least give me a chance."));
                list.Add(Program.Transfer.Text(FairyName, $"Then stop being rude. Alright, back to business. I need you to get me {PantyRequiredCount} more panties."));
                list.Add(Program.Transfer.Text(FairyName, "This should be the last time. I'll finally have enough power to go back to my original form. Then I can gather energy by myself. Any questions?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I really hope you're not an ugly old pervert."));
                list.Add(Program.Transfer.Text(FairyName, "Rude! I'm a beauty! Get going already or I'll smite you!"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(To use the wind power, talk to a girl in a skirt and click on the wind icon at top right. You need at least 150 total Koikatsu points.)"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, $"(You don't have to use the wind power to progress, you only need to collect {PantyRequiredCount} pairs of panties.)"));

                SaveData.PantiesStolenHeld -= PantyRequiredCount;
                progress = true;
            }

            list.Add(Program.Transfer.Create(false, Command.SceneFade, "in"));
            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(
                EventApi.StartAdvEvent(list, _mainEventPosition, Quaternion.identity, extraData: new Program.OpenDataProc
                {
                    onLoad = () =>
                    {
                        if (progress)
                        {
                            icon.Dispose();
                            Progress = StoryProgress.E6_GustOfWind;
                        }
                    }
                }));
        }

        public static void StartE6(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent();
            list.Add(Program.Transfer.Create(true, Command.SceneFade, "out"));
            list.Add(Program.Transfer.Create(true, Command.FontColor, "Color2", FairyName));
            var progress = false;
            if (SaveData.PantiesStolenHeld < PantyRequiredCount)
            {
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventApi.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, $"No, I didn't get all {PantyRequiredCount} pairs yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002")); // hontounano?
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Of course not, I'm a pure boy unlike you."));
                list.Add(Program.Transfer.Text(FairyName, "Rude! I'm a pure maiden at heart too! Just go and steal them from someone already!"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Looks like I hit a nerve, heh."));
            }
            else
            {
                list.Add(Program.Transfer.Create(true, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventApi.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Of course, it was a piece of cake."));
                list.Add(Program.Transfer.Text(FairyName, "Nice! Hurry, give them to me. Come on! This should be all I need, I can't wait to go back!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright, alright, here you go."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "The moment I hold the panties up they disappear in a ball of light."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(EventApi.Narrator, "I close the courtain just in case someone enters the shop."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "So, what happens now?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hey, are you there?"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "There's no sign of the fairy. No balls of light, no perverted presence."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Did she fulfill her lingering desire and go to heaven or something? I thought it was something that movies made up."));
                list.Add(Program.Transfer.Create(false, Command.TextClear));
                list.Add(Program.Transfer.Create(false, Command.Wait, "5"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "There are no signs of life. Is she's actually gone? We were getting along so well too..."));

                list.Add(Program.Transfer.Create(false, Command.TextClear));
                list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));

                var scenario = ActionScene.instance.AdvScene.Scenario;
                scenario.heroineList = new List<SaveData.Heroine> { LoadFairyCard() };

                list.Add(Program.Transfer.Create(false, Command.CharaCreate, "0", "-2")); // Spawn the heroine ID -2 as adv character ID 0
                list.Add(Program.Transfer.Create(true, Command.CharaActive, "0", "true", "center")); // Enable the chracter 0 and move it to center
                list.Add(Program.Transfer.Create(true, Command.CharaChange, "0", "true")); // Set VARs from the character 0 (name, stats, etc)
                list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "0")); // eyes forward, not focues on camera
                list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "1", "1")); // neck slightly down
                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "8", "7", "0", "0", "", "", "", "", "", ""));
                list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_05_00"));

                list.Add(Program.Transfer.Create(false, Command.CharaClothState, "0", "top", "3"));
                list.Add(Program.Transfer.Create(false, Command.CharaClothState, "0", "bot", "3"));
                list.Add(Program.Transfer.Create(false, Command.CharaClothState, "0", "bra", "3"));
                list.Add(Program.Transfer.Create(false, Command.CharaClothState, "0", "shorts", "3"));
                list.Add(Program.Transfer.Create(false, Command.CharaClothState, "0", "socks", "3"));
                list.Add(Program.Transfer.Create(false, Command.CharaClothState, "0", "shoes_inner", "3"));

                list.Add(Program.Transfer.Create(false, Command.Fade, "out", "2", "white", "back", "TRUE")); // fade out to white with a sliding gradient

                list.Add(Program.Transfer.Create(false, Command.Wait, "0.5"));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Wha..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "This is... This is amazing! I need to burn this image into my brain while I still can!"));

                list.Add(Program.Transfer.Create(false, Command.TextClear));
                list.Add(Program.Transfer.Create(false, Command.Wait, "2.5"));
                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "", "", "3", "", "0.9", "", "", "", "", "", ""));

                list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "0", "0")); // Make neck follow camera
                list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "1")); // Make eyes follow camera
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_00_22_007"));
                list.Add(Program.Transfer.Text(UnknownName, "Ah."));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Is that you, fairy?"));

                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_03_22_010"));
                list.Add(Program.Transfer.Text(UnknownName,
                    $"Yeah, I'm finally back to being a human! You look different in person, {EventApi.PlayerName}."));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Umm, I'm not complaining, but why are you naked?"));

                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "2", "8", "", "0.8", "", "", "0.4", "", "", ""));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_02_22_005"));
                list.Add(Program.Transfer.Text(FairyName, "Hm?"));

                list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_Oth_08"));
                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "2", "9", "", "1", "", "", "1", "", "1", ""));
                list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "3", "0.5"));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_00_22_000"));
                list.Add(Program.Transfer.Text(FairyName, "Kya!"));

                list.Add(Program.Transfer.Text(FairyName, "What happened to my clothes?!"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "She looks around the changing booth nervously, then takes a deep breath and focuses on me."));
                list.Add(Program.Transfer.Text(FairyName, "Umm, could you not stare so much?! This is really embarassing!"));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Even if you tell me that..."));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "Ooh, bare butt!"));

                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "8", "6", "", "0.7", "", "", "", "", "", ""));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_03_22_016"));
                list.Add(Program.Transfer.Text(FairyName, "Aaaaa, stop it! I've had enough, farewell!"));

                list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));
                list.Add(Program.Transfer.Create(false, Command.CharaActive, "0", "false"));
                list.Add(Program.Transfer.Create(false, Command.Fade, "out", "0.5", "white", "back", "TRUE"));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Aw..."));

                list.Add(Program.Transfer.Text(FairyName, "Don't act so disappointed! Your eyes were way too lewd!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "You're actually a beauty, of course I'd stare in that situation!"));
                list.Add(Program.Transfer.Text(FairyName, "A-A beauty? Eheheh. See? You were wrong to doubt me."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I was wrong, I'm sorry. Long hair, emerald eyes, fair skin... You're totally my type! Please go out with me!"));
                list.Add(Program.Transfer.Text(FairyName, "Wait wait wait, what's with this heel turn?! You don't even know my name!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I'm sorry! I just assumed you were some old creepy dude lying to me. I prepared myself for the worst."));
                list.Add(Program.Transfer.Text(FairyName, "You thought I was some random creep, and yet you still stole panties for me? You're a pervert through and through."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hey, you said you would expose me if I didn't comply!"));
                list.Add(Program.Transfer.Text(FairyName, "I did? How would I even do that when only you can hear me?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I... I didn't think of that. Well, anyways! There's something much more important I want to ask."));
                list.Add(Program.Transfer.Text(FairyName, "Let me guess, my name?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Bingo, we really do think alike."));
                list.Add(Program.Transfer.Text(FairyName, "Ugh, listen now. Can you stop acting like this? Don't treat me nicely all of a sudden just because of my looks."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Oh, sorry, I didn't mean it like that. I'm just really relieved that you were telling the truth."));
                list.Add(Program.Transfer.Text(FairyName, $"If you say so. By the way, it's {EventApi.HeroineName}."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Hmm?"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, $"My name. I'm {EventApi.HeroineFullName}. Nice to meet you."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, $"{EventApi.HeroineName}... That's a really nice name, I like it. Can I call you that from now on? Calling you Panty Fairy is a bit..."));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "Yes, of course, go ahead. The Panty Fairy thing was a joke. I didn't think you would keep calling me that."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Oh, did you expect me to ask for your real name?"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "Of course! No girl wants to be called a \"Panty Fairy.\""));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I'm sorry. Are you mad?"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "I'm not mad, and stop apologizing. I'd rather talk about my clothes, for some reason I've lost all of my belongings."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Can't you wear one of the panties I brought you at least?"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "I don't have them anymore, I converted them all into energy. But good thinking, I could use someone's clothes instead."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Wait, you want someone else's clothes? We're in a clothing store, I can buy you something if you don't have any money."));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "The clothes here are no good, there's no energy in them so they won't work well with my body."));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright... So what do I have to do? Grab some clothes from one of the lockers in the changing rooms?"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "No, that's too slow. Now that my power is restored, I have a much faster way of doing it."));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "Go up to a girl, like when stealing her panties, except this time imagine stealing everything."));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Everything?! Are you serious?"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "Of course, consider it a reward for helping me out. Don't worry about getting caught, my blessing got much stronger. She won't even notice."));

                list.Add(Program.Transfer.Text(EventApi.PlayerName, "(I wish you didn't notice not having clothes...)"));

                list.Add(Program.Transfer.Text(EventApi.HeroineName, "Did you say something?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "No, nothing! I'll get going then, see you in a bit!"));

                list.Add(Program.Transfer.Text(EventApi.Narrator, "(To steal clothes, talk to a fully clothed girl and click on the new clothing icon at top right. You need at least 200 total Koikatsu points.)"));
                list.Add(Program.Transfer.Text(EventApi.Narrator, "(If the girl has her guard up then it's harder to successfully steal. In this case try befriending them first.)"));

                SaveData.PantiesStolenHeld -= PantyRequiredCount;
                progress = true;
            }

            list.Add(Program.Transfer.Create(false, Command.SceneFade, "in"));
            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(
                EventApi.StartAdvEvent(list, _mainEventPosition, Quaternion.identity, extraData: new Program.OpenDataProc
                {
                    onLoad = () =>
                    {
                        if (progress)
                        {
                            icon.Dispose();
                            Progress = StoryProgress.E7_Uniform;
                        }
                    }
                }));
        }

        public static void StartE7(IDisposable icon)
        {
            var list = EventApi.CreateNewEvent();
            list.Add(Program.Transfer.Create(true, Command.SceneFade, "out"));
            var progress = false;

            var scenario = ActionScene.instance.AdvScene.Scenario;
            var fairyCard = LoadFairyCard();
            scenario.heroineList = new List<SaveData.Heroine> { fairyCard };
            list.Add(Program.Transfer.Create(true, Command.CharaChange, "-2", "true"));

            if (SaveData.UniformsStolenTotal < 1)
            {
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "You're finally here! Do you have the uniform?"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "Sorry, I didn't find any uniforms for you yet."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_03_22_003"));
                list.Add(Program.Transfer.Text(EventApi.HeroineName, "Then hurry up and get one, I can't wait to return to my body!"));
                list.Add(Program.Transfer.Text(EventApi.PlayerName, "I can't wait either!"));
            }
            else
            {
                var saveData = Game.saveData;
                var classNumber = saveData.player.schoolClass;
                var freeSlot = saveData.GetFreeClassSlots(classNumber).FirstOrDefault() ??
                               saveData.GetFreeClassSlots(classNumber + 1).FirstOrDefault() ??
                               saveData.GetFreeClassSlots(classNumber - 1).FirstOrDefault();
                if (freeSlot == null)
                {
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "(You need a free guest room in any of the lodgings to continue. Free one up in the Booking menu in your room.)"));
                }
                else
                {
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "I have the uniform, here you go!"));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "Finally! Thank you!"));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "I hold the uniform up and it disappears in a ball of light."));
                    AddFlashOfLight(list);
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "Hmm... I'll have to make some adjustments but it should work. Give me a minute."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright, I can wait."));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "..."));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "I'm done! Behold!"));
                    list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));
                    list.Add(Program.Transfer.Create(false, Command.CharaCreate, "0", "-2")); // Spawn the heroine ID -2 as adv character ID 0
                    list.Add(Program.Transfer.Create(true, Command.CharaActive, "0", "true", "center")); // Enable the chracter 0 and move it to center
                    list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "0", "0")); // Make neck follow camera
                    list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "1")); // Make eyes follow camera
                    list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "8", "19", "0", "1", "", "", "", "", "", ""));
                    list.Add(Program.Transfer.Create(false, Command.CharaMotion, "0", "Stand_02_00"));
                    list.Add(Program.Transfer.Create(false, Command.Fade, "out", "1", "white", "back", "TRUE")); // fade out to white with a sliding gradient
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, $"It is I, {EventApi.HeroineName}!"));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "Oooh! Cute!"));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "*Clap* *Clap* *Clap*"));
                    list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_02_22_001"));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "Ahaha, I didn't expect an ovation. You really are serious about this, aren't you."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "About what, becoming lovers? Of course I am! Our tastes match, and you're totally my type!"));
                    list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "1", "1"));
                    list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "1"));
                    list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_04_00"));
                    list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "", "", "7", "", "", "", "", "", "", "", ""));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "But we don't even know each other all that well."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "That can be fixed. Are you free after this?"));
                    list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_22_tanon_01"));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "Sigh... Alright, listen. From now on I'll be blending in with other students, so please act like I'm just a normal girl."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "Like a normal girl? Sure, if you want to."));
                    list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_01_00"));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName,
                        $"Umm, {EventApi.PlayerName}... Thank you for helping me. I would be stuck here for who knows how long without your help."));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "Also... I might seem cold sometimes, but I really like talking with you. I think you're a good friend, but I don't know about being lovers."));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "Wait, does this mean that she's giving me a chance? Alright, I'll definitely make you fall in love with me!"));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "So we're starting as friends, this works for me. What will you do now?"));
                    list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_07_00"));
                    list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "", "", "7", "", "", "", "", "", "", "", ""));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "Hmm... I think I'll take a stroll outside around the island, see what changed in the village."));
                    list.Add(Program.Transfer.Text(EventApi.PlayerName, "Alright, stay safe out there. See you later!"));
                    list.Add(Program.Transfer.Create(true, Command.CharaActive, "0", "false"));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "She walked to the door and waved to me before leaving the store."));
                    list.Add(Program.Transfer.Text(EventApi.HeroineName, "See you later!"));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, $"({EventApi.HeroineName} has been added to the roster. After this day ends, she will appear on the island as a normal student.)"));
                    list.Add(Program.Transfer.Text(EventApi.Narrator, "(Congratulations on finishing this side quest! This was PantyFairy by ManlyMarco.)"));

                    freeSlot.SetCharFile(fairyCard.charFile);
                    freeSlot.charFileInitialized = true;
                    // Set already met
                    freeSlot.talkEvent.Add(0);
                    freeSlot.talkEvent.Add(1);
                    // Set is friend
                    freeSlot.isFriend = true;
                    saveData.heroineList.Add(freeSlot);

                    progress = true;
                }
            }

            list.Add(Program.Transfer.Create(false, Command.SceneFade, "in"));
            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(
                EventApi.StartAdvEvent(list, _mainEventPosition, Quaternion.identity, extraData: new Program.OpenDataProc
                {
                    onLoad = () =>
                    {
                        if (progress)
                        {
                            icon.Dispose();
                            Progress = StoryProgress.Complete;
                            Game.saveData.player.koikatsuPoint += 50;
                        }
                    }
                }));
        }

        private static SaveData.Heroine LoadFairyCard()
        {
            var card = ResourceUtils.GetEmbeddedResource("fairy_card.png", typeof(PantyFairyPlugin).Assembly);
            if (card == null) throw new ArgumentNullException(nameof(card));
            var cfc = new ChaFileControl();
            cfc.LoadFromBytes(card, true, true);
            var h = new SaveData.Heroine(cfc, false);
            return h;
        }

        private static void AddFlashOfLight(List<Program.Transfer> list)
        {
            list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.Fade, "out", "0.5", "white", "back", "TRUE"));
        }

        private static void StartMonologueEventAndDisposeIcon(List<Program.Transfer> list, IDisposable icon, Action endAction)
        {
            // Let the icon disappearing effect finish before destroying it
            PantyFairyPlugin.Instance.StartCoroutine(CoroutineUtils.CreateCoroutine(new WaitForSeconds(4), icon.Dispose));
            PantyFairyPlugin.Instance.StartCoroutine(EventApi.StartMonologueEvent(list).AppendCo(endAction, icon.Dispose));
        }
    }
}