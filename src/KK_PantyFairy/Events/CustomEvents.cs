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

        private static readonly Vector3 _toiletActionIconPosition = new Vector3(-0.53f, 0.3f, -0.90f);
        private static readonly Vector3 _toiletEventPosition = new Vector3(-0.70f, 0, 0.51f);
        private static readonly Quaternion _toiletEventRotation = Quaternion.Euler(0, 358.60f, 0);

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

        public static bool LockersEnabled => Progress >= StoryProgress.E4_SecondRaid || PantyFairyPlugin.IsSkillsForceUnlock();
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

        private static IDisposable MakeActIcon(Action<IDisposable> startEvent, string iconName, int mapNo, Vector3 position)
        {
            IDisposable icon = null;
            icon = GameAPI.AddActionIcon(mapNo, position,
                PantyFairyPlugin.GetSprite(iconName + "_on.png"),
                PantyFairyPlugin.GetSprite(iconName + "_off.png"),
                () => startEvent(icon));
            return icon;
        }

        static CustomEvents()
        {
            EventData = new List<CustomEventData>();

            // 2 - in the 1f hallway
            // 18 - male toilet
            // 46 - changing room

            EventData.Add(new CustomEventData(StoryProgress.E1_Initial, () => MakeActIcon(StartE1, "action_question", 2, new Vector3(15.11f, -0.8f, -1.42f))));

            EventData.Add(new CustomEventData(StoryProgress.E2_Meeting, () => MakeActIcon(StartE2, "action_question", 18, _toiletActionIconPosition)));

            EventData.Add(new CustomEventData(StoryProgress.E3_FirstRaid, () => new CompositeDisposable(
                MakeActIcon(StartE3, "action_event", 18, _toiletActionIconPosition),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(1.31f, 0, 0.70f)),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(-0.88f, 0, 0.70f)),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(-2.38f, 0, 2.55f)),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(-0.27f, 0, 2.55f)),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(-0.83f, 0, -0.75f)),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(3.48f, 0, -2.63f)),
                MakeActIcon(StartE3_2, "action_point", 46, new Vector3(-0.29f, 0, -2.63f)))));

            EventData.Add(new CustomEventData(StoryProgress.E4_SecondRaid, () => MakeActIcon(StartE4, "action_event", 18, _toiletActionIconPosition)));

            EventData.Add(new CustomEventData(StoryProgress.E5_Steal, () => MakeActIcon(StartE5, "action_event", 18, _toiletActionIconPosition)));

            EventData.Add(new CustomEventData(StoryProgress.E6_GustOfWind, () => MakeActIcon(StartE6, "action_event", 18, _toiletActionIconPosition)));

            EventData.Add(new CustomEventData(StoryProgress.E7_Uniform, () => MakeActIcon(StartE7, "action_event", 18, _toiletActionIconPosition)));
        }

        public static void StartE1(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hmm?"));
            list.Add(Program.Transfer.Text(EventUtils.Narrator, "A folded piece of paper is laying on the floor with shoe marks all over it. Looks like something is written on it."));
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
            list.Add(Program.Transfer.Text(UnknownName, "\"There's a ghost in the men's bathroom, I swear!\nI snuck in there yesterday and heard strange noises and groans!\""));
            list.Add(Program.Transfer.Text(UnknownName, "\"I asked others to check, but they said there was nothing there... and that I'm delusional from being too horny!"));
            list.Add(Program.Transfer.Text(UnknownName, "\"I swear it's real! And I'm not horny, that's slander! I'm just unusually sensual!\""));
            list.Add(Program.Transfer.Text(UnknownName, "\"Can you go and see? It should be around the furthest stall.  I'm too afraid to go there by myself now. What if I got pinned to the ground and ravaged?\""));
            list.Add(Program.Transfer.Text(UnknownName, "\"You're the only person I can trust with this. It's my favourite rubbing spot so this is really imporant to me.\""));
            list.Add(Program.Transfer.Text(EventUtils.PlayerName, "...Rubbing spot?"));
            list.Add(Program.Transfer.Text(EventUtils.Narrator, "She sure sounds horny. Still, what would make noises in the male bathroom? There are no male teachers or students other than me."));
            list.Add(Program.Transfer.Text(EventUtils.Narrator, "Was it the janitor, or maybe some old pervert infiltrated the school? Not much of a difference I guess."));
            list.Add(Program.Transfer.Text(EventUtils.Narrator, "She said it was at the back of the men's bathroom. I guess I should go check it out in case it's someone dangerous."));
            list.Add(Program.Transfer.Close());

            var playerData = EventUtils.GetPlayerData();
            PantyFairyPlugin.Instance.StartCoroutine(EventUtils.StartAdvEvent(list, true, playerData.transform.position, playerData.transform.rotation).AppendCo(() =>
             {
                 icon.Dispose();
                 Progress = StoryProgress.E2_Meeting;
             }));
        }

        public static void StartE2(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            var progress = false;
            if (PantyFairyPlugin.IsHentStatBelow(50))
            {
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Looks like this is the place that the horny girl wrote about."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I can't hear anything, as expected. The noises probably came from outside the window..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Something rustled around the window. A person? No way, this is the second floor."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hey, is anyone there?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "... No response"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hm?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "What is this smell? I seems familiar, but I can't put my finger on what it is."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "It smells... kind of lewd. Maybe I could tell if I did some more \"sensual\" research in the club."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(Increase your Sensuality/H stat to at least 50 to continue)"));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", FairyName));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Looks like this is the place the horny girl wrote about."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I can't hear anything out of the ordinary. Did I get pranked? The note did seem pretty weird..."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hm?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Wait a minute! This smell... It smells like..."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Freshly worn panties?!"));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_04_22_004"));
                list.Add(Program.Transfer.Text(UnknownName, "Ufufu!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Who's there?"));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_04_22_003")); // heee
                list.Add(Program.Transfer.Text(UnknownName, "You're quite a naughty one to notice me, aren't you " + EventUtils.PlayerName + "? The gossips weren't wrong."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "A feminine voice echoes within the room. Where is the voice coming from? There's no one inside the stalls."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Where are you? Show yourself!"));
                list.Add(Program.Transfer.Text(UnknownName, "Relax... You must think I'm suspicious, but I'm really not! I'm right in front of you, I just can't be seen at the moment."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Indeed, the voice is coming from around here. Not from the window or the stalls."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Who are you? I sure hope you're a student, or else I'll have to call for the janitor to clean you up."));
                list.Add(Program.Transfer.Text(UnknownName, "Is that a threat? That's rude. Yes, I am a student, technically speaking. At least when I was still around."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "She does sound like a girl. Actually, she sounds like she could be my type!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "So you're telling me that you are a ghost?"));
                list.Add(Program.Transfer.Text(UnknownName, "You catch on pretty fast, but wrong! I'm actually a fairy, Panty Fairy!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Panty Fairy? So basically a pervert?"));
                list.Add(Program.Transfer.Text(FairyName, "You really are rude, you know. I gather panties to restore my energy, I don't do this just because I like it."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Balls of light appeared where the voice was coming from. They had a warm glow and circled around randomly."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Wha..."));
                list.Add(Program.Transfer.Text(FairyName, "This is the best I can do with what I gathered so far, do you believe me now?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Well... I guess you might be a real ghost... I still think you're just some perverted old dude though."));
                list.Add(Program.Transfer.Text(FairyName, "Sigh. Just so you know, I'm quite the beauty. My human body will be restored once I regain my power. I'll have you eat your words then."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Yeah, sure. So why are you talking to me?"));
                list.Add(Program.Transfer.Text(FairyName, "I want you to help me collect panties of course."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ugh, do you want me to get expelled? Why not ask the girls directly?"));
                list.Add(Program.Transfer.Text(FairyName, "I tried! But they all ran away or ignored me. I wish I could just go get their panties myself, but they have to be given to me."));
                list.Add(Program.Transfer.Text(FairyName, "Ah, don't worry about getting expelled! I'll use my powers to protect you. As long as you keep bringing me panties that is."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Use your powers? What can you do?"));
                list.Add(Program.Transfer.Text(FairyName, "Glad you asked! I can manipulate perceptions, make people not notice certain things. Like, for example, their panties missing."));
                list.Add(Program.Transfer.Text(FairyName, "With this you'll be able to steal the panties with no risk, and use them as you wish. This should be a pretty good deal for you."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "So you'll give me protection from being found out in exchange for panties? Who do you think I am?"));
                list.Add(Program.Transfer.Text(FairyName, "A pervert?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I hate that you're right. So, what should I do?"));
                list.Add(Program.Transfer.Text(FairyName, "My powers are very limited at the moment, so we need to be careful. For now, bring me any panties you can find."));
                list.Add(Program.Transfer.Text(FairyName, "The safest place to get them would be in the changing room. Sneak in there when no one's around and grab a pair from one of the drawers."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Won't the owner notice their panties missing?"));
                list.Add(Program.Transfer.Text(FairyName, "Not with my blessing. They will believe in something harmless, like forgetting their panties or throwing them away."));
                list.Add(Program.Transfer.Text(FairyName, "Just don't overdo it, I can only do so much at the moment."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright, fine, I'll do it. For purely academic purposes."));
                list.Add(Program.Transfer.Text(FairyName, "Yes, of course. No lewd motives here, fufu. I'll wait for you here, have a safe trip!"));

                progress = true;
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, false, _toiletEventPosition, _toiletEventRotation);

            if (progress) eventCo = eventCo.AppendCo(() =>
            {
                icon.Dispose();
                Progress = StoryProgress.E3_FirstRaid;
            });//todo generic way?

            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
        }

        public static void StartE3(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", FairyName));
            var progress = false;
            if (SaveData.PantiesStolenTotal == 0)
            {
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventUtils.PlayerName + "! Did you get the panties?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "No, not yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002"));
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "As if! It's not easy to do this, just you know."));
                list.Add(Program.Transfer.Text(FairyName, "Don't worry, with my blessing you should be fine. Just sneak into the locker room and grab a pair."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I'm going to regret this, aren't I."));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventUtils.PlayerName + "! Did you get the fresh panties?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I make sure that there is no one around and take out the spoils."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Here."));
                list.Add(Program.Transfer.Text(FairyName, "Oh? Oooh～!"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Balls of light appear out of thin air and float around the panties. The panties float up and disappear in a flash."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Whoa."));
                list.Add(Program.Transfer.Text(FairyName, "Hmm..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Something's wrong?"));
                list.Add(Program.Transfer.Text(FairyName, "Ah, sorry, I was trying them out."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Pervert."));
                list.Add(Program.Transfer.Text(FairyName, "Rude! I meant in a spiritual way! I don't even have my body back yet."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "..."));
                list.Add(Program.Transfer.Text(FairyName, "Why are you giving me the silent stare?! You're not even looking directly at me."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I try to adjust my glare towards the source of the voice, but she keeps moving around and eventually I give up."));
                list.Add(Program.Transfer.Text(FairyName, "Well, anyways! I gained much less energy than I expected from these panties. The quality might be too low."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Quality? Of the fabric?"));
                list.Add(Program.Transfer.Text(FairyName, "No, that's not important. I need something more freshly worn. The fresher the better."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "And how am I supposed to get that?! I already risked my skin for this one."));
                list.Add(Program.Transfer.Text(FairyName, "Relax. My powers got a little stronger, and it's not like you have to pull them off the person."));
                list.Add(Program.Transfer.Text(FairyName, "Just hide in the locker room and wait until someone gets changed. After they are gone you swoop in, grab the goods, and flee. Easy peasy."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "What if I refuse?"));
                list.Add(Program.Transfer.Text(FairyName, "Let's see... I could haunt you to the grave... Ah, I could also spread rumors about you stealing panties."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Great, so I never had a choice."));
                list.Add(Program.Transfer.Text(FairyName, "Hey, I don't want to do any of this! I want us to have a friendly working relationship. Keeping you safe is a part of that."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Sounds sketchy, but I guess I have no choice but to trust you. I'll see what I can do."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I guess I'll hide in the locker room immediately after the next period starts and hope for the best."));
                progress = true;
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, false, _toiletEventPosition, _toiletEventRotation);

            if (progress) eventCo = eventCo.AppendCo(() =>
            {
                icon.Dispose();
                Progress = StoryProgress.E4_SecondRaid;
            });

            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
        }

        public static void StartE3_2(IDisposable icon)
        {
            icon.Dispose();

            var list = EventUtils.CreateNewEvent(true);

            switch (SaveData.EventProgress)
            {
                case 0:
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I think this is the locker of that cheeky girl from class next door. She's cute, but the personality..."));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "The door doesn't budge, it's locked."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "So much for that."));
                    SaveData.EventProgress++;
                    break;
                case 1:
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "The locker looks beat up. Pulling on the door makes it creak and squirm, but it doesn't open."));
                    SaveData.EventProgress++;
                    break;
                case 2:
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "\"Chikarin\" is scribbled on the side of the locker. It's locked tight."));
                    SaveData.EventProgress++;
                    break;
                case 3:
                    var topInt = Game.Instance.HeroineList.Where(x => x.intimacy >= 50).OrderByDescending(x => x.intimacy).FirstOrDefault();
                    if (topInt == null)
                    {
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "It's empty."));
                    }
                    else
                    {
                        var fullname = topInt.charFile.parameter.fullname;
                        if (TranslationHelper.TryTranslate(fullname, out var tlName)) fullname = tlName;
                        list.Add(Program.Transfer.Create(true, Command.Replace, "Other", fullname));
                        list.Add(Program.Transfer.Text(EventUtils.PlayerName, "This locker... It belongs to [Other]!"));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "It's... It's unlocked! What is she thinking?! And her panties are right there, in full view!"));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "Ugh, should I take them? I feel bad for her..."));

                        list.Add(Program.Transfer.Create(true, Command.Choice, "true", "Take them.,yes", "Look for another pair.,no"));

                        list.Add(Program.Transfer.Create(true, Command.Tag, "yes"));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "It feels wrong, but I can't pass on this opportunity!"));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "I put hope in the fairy's blessing and took the panties from the locker. They seem to be brand new, but that's fine too."));
                        list.Add(Program.Transfer.VAR("string", "Took", "yes"));
                        list.Add(Program.Transfer.Close());

                        list.Add(Program.Transfer.Create(true, Command.Tag, "no"));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "I'd rather have a proper relationship, where I can ask for her panties without worry."));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "I paid my respects to the locker and closed the door without taking anything."));
                        list.Add(Program.Transfer.Close());
                    }
                    SaveData.EventProgress++;
                    break;
                case 4:
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "It's empty."));
                    SaveData.EventProgress++;
                    break;
                case 5:
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "This one's open! Let's see..."));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "After digging for a while I found a neatly folded pair of pure white panties. They look brand new."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Nice."));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Let's get out of here before somebody sees me."));
                    SaveData.EventProgress++;
                    SaveData.PantiesStolenTotal++;
                    break;
                default:
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "I already got what I came for. I should go back to the fairy in the toilet."));
                    break;
            }

            list.Add(Program.Transfer.Close());

            PantyFairyPlugin.Instance.StartCoroutine(EventUtils.StartAdvEvent(list, true, EventUtils.GetPlayerData().transform.position, EventUtils.GetPlayerData().transform.rotation).AppendCo(() =>
             {
                 var scenarioVars = Singleton<Game>.Instance.actScene.AdvScene.Scenario.Vars;
                 scenarioVars.TryGetValue("Took", out var val);
                 if (val?.o?.ToString() == "yes")
                 {
                     PantyFairyPlugin.Logger.LogDebug("Found the Took var");
                     SaveData.EventProgress = 6;
                     SaveData.PantiesStolenTotal++;
                     scenarioVars.Remove("Took");
                 }
             }));
        }

        private static void StartE4(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", FairyName));
            var progress = false;
            if (SaveData.EventProgress < 2)
            {
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventUtils.PlayerName + "! Did you get the fresh panties?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "No, not yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002"));
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "As if! It's not easy to do this, just you know."));
                list.Add(Program.Transfer.Text(FairyName, "Don't worry, you'll be fine. Just sneak into the locker room, wait for someone to change, and grab the used pair."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright..."));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Text(FairyName, "Welcome back, " + EventUtils.PlayerName + "! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I reach inside a pocket in my pants and pull it out."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Right here, freshly harvested."));
                list.Add(Program.Transfer.Text(FairyName, "Ooh～! Let me see, let me see!"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Balls of light gather around the panties and quickly absorb them."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(FairyName, "Yes, this is much better! I can feel my power growing, just like the thing in your pants."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Pervert～ How shameless～"));
                list.Add(Program.Transfer.Text(FairyName, "Don't act shy now! I bet you put them in that pocket so you could rub against them!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Wait, what? I didn't do that, how did you even come up with that scenario?!"));
                list.Add(Program.Transfer.Text(FairyName, "I swear, once I get my body back I'll bonk your head so hard that I'll fix your IQ loss from horniness."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I'm looking forward to seeing you try. My horniness is absolute."));
                list.Add(Program.Transfer.Text(FairyName, "Your wha... No, whatever, let's leave it at that."));
                list.Add(Program.Transfer.Text(FairyName, "I have some bad news and some good news."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright, give me the bad news first."));
                list.Add(Program.Transfer.Text(FairyName, $"The bad news is, I'll need around {PantyRequiredCount} more panties."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I have to do this " + PantyRequiredCount + " more times? Give me a break. What's the good news then?"));
                list.Add(Program.Transfer.Text(FairyName, "You're going to love this. I gathered enough energy to give you a new power."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "A new power? You made me harder to notice before, so some sort of stealth?"));
                list.Add(Program.Transfer.Text(FairyName, "Not even close. I'll give you a technique that can instantly teleport a nearby pair of panties into your hand."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "That sounds ridiculous yet amazing. How do I use this skill?"));
                list.Add(Program.Transfer.Text(FairyName, "It's pretty simple! Reach out with your hand and yell \"Hippity hoppity your pantsu is now my property!\""));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Are you kidding me?! I'll either die from embarassment or get expelled, probably both. Isn't there some other way?"));
                list.Add(Program.Transfer.Text(FairyName, "You've been good to me, so I guess I should help you out. You can also yell \"Steal!\" to the same effect."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "That's not much better, do I really have to yell?"));
                list.Add(Program.Transfer.Text(FairyName, "Well, I guess a whisper would work too, but that's boring! My blessing will protect you from getting caught anyways, so why not let loose?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, $"No thanks, I'm fine with being tight. Is there anything else I should know?"));
                list.Add(Program.Transfer.Text(FairyName, "Let me think... Stealing might fail if the girl is too cautious of you, it will work better on friends and lovers."));
                list.Add(Program.Transfer.Text(FairyName, "Also, the skill uses horniness as fuel, so you might have to do some training in that."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, $"Got it. I'll see you once I collect {PantyRequiredCount} panties."));
                list.Add(Program.Transfer.Text(FairyName, "Alrgiht, have fun!"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(To steal panties first talk to a girl, then click on the panty icon at top right. You need at least 65 Sensuality/H points)"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(You can also steal from lockers after someone changes like before)"));

                progress = true;
                SaveData.PantiesStolenHeld = 0;
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, false, _toiletEventPosition, _toiletEventRotation);

            if (progress) eventCo = eventCo.AppendCo(() =>
            {
                icon.Dispose();
                Progress = StoryProgress.E5_Steal;
            });

            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
        }

        public static void StartE4_2(IDisposable icon)
        {
            icon.Dispose();

            var list = EventUtils.CreateNewEvent(true);
            if (Progress == StoryProgress.E4_SecondRaid)
            {
                switch (SaveData.EventProgress)
                {
                    case 0:
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "The locker door is bulging out. It pops open the moment I touch it."));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "It's full to the brim with a ball of warm compressed clothes. What a mess."));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "I dig through the clothes and only find a box of half-melted chocolates. No panties in sight."));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "I guess I have to try again."));
                        SaveData.EventProgress++;
                        break;
                    case 1:
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "The door of the locker is slightly ajar. A neatly folded uniform fills most of the insides."));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "A warm pair of light pink panties is hidden under of the uniform."));
                        list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Sorry, I'll be taking that."));
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "Score! Time to report back to the Panty Fairy."));
                        SaveData.EventProgress++;
                        SaveData.PantiesStolenTotal++;
                        break;
                    default:
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "I already found a pair of panties. I should report back to Panty Fairy in men's bathroom before they get cold."));
                        break;
                }
            }
            else
            {
                if (Random.value <= 0.2f)
                {
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "The locker is locked, I can't get in."));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Unlucky, I'll have to try again."));
                }
                else
                {
                    if (Random.value <= 0.5f)
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "The door of the locker is slightly ajar. A neatly folded uniform fills most of the insides."));
                    else
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "The locker is unlocked. The clothes inside are left in a complete disarray."));

                    if (Random.value <= 0.2f)
                    {
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "There are no panties in sight. Looks like I'll have to try again."));
                    }
                    else
                    {
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, "A pair of cute panties is hidden under other clothes. The panties are still warm, owner couldn't have gone far."));
                        list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Sorry, I'll be taking that."));
                        SaveData.PantiesStolenTotal++;
                        SaveData.PantiesStolenHeld++;
                    }
                }
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, true, EventUtils.GetPlayerData().transform.position, EventUtils.GetPlayerData().transform.rotation);
            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
        }

        private static void StartE5(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", FairyName));
            var progress = false;

            if (SaveData.PantiesStolenHeld < PantyRequiredCount)
            {
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventUtils.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, $"No, I didn't get all {PantyRequiredCount} pairs yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002"));
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "As if! It's not easy to do this, just you know."));
                list.Add(Program.Transfer.Text(FairyName, "Don't worry, you'll be fine. Just talk to someone and press the steal button at top right."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright..."));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventUtils.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "I puff my chest out and bump it with a fist."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Of course! I'm a bit of a technician if I do say so myself."));
                list.Add(Program.Transfer.Text(FairyName, "Heh～? Are you sure it's not all because of my power?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "It's not that easy to use, just so you know. You need to be in the right mindset."));
                list.Add(Program.Transfer.Text(FairyName, "A perverted mindset."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Shush. So, is this enough for you?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, $"I hand over {PantyRequiredCount} of the panties I collected. Small lights quickly gather around my hands and swallow the goods."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(FairyName, "Oh! Oh～! This is really good!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "So can I finally see you?"));
                list.Add(Program.Transfer.Text(FairyName, "No, not yet! This will restore a good chunk of my power but I still need more."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Ugh. Will this ever end, or am I your lifetime dealer now?"));
                list.Add(Program.Transfer.Text(FairyName, "Don't be so impatient, just enjoy the panties. I know, I'll give you a reward!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "What kind of a reward?"));
                list.Add(Program.Transfer.Text(FairyName, "A power of prerverted wind! You can blow someone's skirt upwards and peep all you want. Bonus points if you steal the panties first, hehehe."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "That sounds... interesting. How do I use it?"));
                list.Add(Program.Transfer.Text(FairyName, "Just imagine wind blowing upwards, it's not that hard. You might need to train your imagination a bit though."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Power of perversion, got it. Still, how did you know I would like this?"));
                list.Add(Program.Transfer.Text(FairyName, "I just felt like I should give you something like this. Glad to hear you like it."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Why does this feel like talking with a close guy friend? I really missed this since transferring here."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "You know, if you weren't a guy I'd probably want to date you."));
                list.Add(Program.Transfer.Text(FairyName, "Like I said, I'm not a guy! I'll friendzone as if I was one though."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "That's harsh, at least give me a chance."));
                list.Add(Program.Transfer.Text(FairyName, $"Then stop being rude. Alright, back to business. I need you to get me {PantyRequiredCount} more panties."));
                list.Add(Program.Transfer.Text(FairyName, "This should be the last time. I'll finally have enough power to go back to my original form. Then I can gather energy by myself. Any questions?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I really hope you're not an ugly old pervert."));
                list.Add(Program.Transfer.Text(FairyName, "Rude! I'm a beauty! Get going already or I'll smite you!"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(To use the wind power, talk to a girl in a skirt and click on the wind icon at top right. You need at least 70 Sensuality/H points)"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, $"(You don't have to use the wind power to progress, you only need to collect {PantyRequiredCount} pairs of panties)"));

                SaveData.PantiesStolenHeld -= PantyRequiredCount;
                progress = true;
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, false, _toiletEventPosition, _toiletEventRotation);

            if (progress) eventCo = eventCo.AppendCo(() =>
            {
                icon.Dispose();
                Progress = StoryProgress.E6_GustOfWind;
            });

            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
        }

        public static void StartE6(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", FairyName));
            var progress = false;
            if (SaveData.PantiesStolenHeld < PantyRequiredCount)
            {
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventUtils.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, $"No, I didn't get all {PantyRequiredCount} pairs yet. Sorry."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_01_22_002")); // hontounano?
                list.Add(Program.Transfer.Text(FairyName, "Really? Could it be that you still want to \"use\" them for something?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Of course not, I'm a pure boy unlike you."));
                list.Add(Program.Transfer.Text(FairyName, "Rude! I'm a pure maiden at heart too! Just go and steal them from someone already!"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Looks like I hit a nerve, heh."));
            }
            else
            {
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", UnknownName));
                list.Add(Program.Transfer.Text(FairyName, "Oh, " + EventUtils.PlayerName + "! You're back! Do you have the goods?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Of course, it was a piece of cake."));
                list.Add(Program.Transfer.Text(FairyName, "Nice! Hurry, give them to me. Come on! This should be all I need, I can't wait to go back!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright, alright, here you go."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "The moment I hold the panties up they disappear in a ball of light."));
                AddFlashOfLight(list);
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "So, what happens now?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hey, you there?"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "There's no sign of the fairy. No balls of light, no perverted presence."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Did she fulfill her lingering desire go to heaven or something? I thought it was something that movies made up."));
                list.Add(Program.Transfer.Create(false, Command.TextClear));
                list.Add(Program.Transfer.Create(false, Command.Wait, "5"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "There are no signs of life. Is she's actually gone? We were getting along so well too..."));

                list.Add(Program.Transfer.Create(false, Command.TextClear));
                list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));

                var scenario = Game.Instance.actScene.AdvScene.Scenario;
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

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Wha..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "This is... This is amazing! I need to burn this image into my brain while I still can!"));

                list.Add(Program.Transfer.Create(false, Command.TextClear));
                list.Add(Program.Transfer.Create(false, Command.Wait, "2.5"));
                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "", "", "3", "", "0.9", "", "", "", "", "", ""));

                list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "0", "0")); // Make neck follow camera
                list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "1")); // Make eyes follow camera
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_00_22_007"));
                list.Add(Program.Transfer.Text(UnknownName, "Ah."));

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Is that you, fairy?"));

                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_03_22_010"));
                list.Add(Program.Transfer.Text(UnknownName,
                    $"Yeah, I'm finally back to being a human! You look different in person, {EventUtils.PlayerName}."));

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Umm, I'm complaining, but why are you naked?"));

                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "2", "8", "", "0.8", "", "", "0.4", "", "", ""));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_02_22_005"));
                list.Add(Program.Transfer.Text(FairyName, "Hm?"));

                list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_Oth_08"));
                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "2", "9", "", "1", "", "", "1", "", "1", ""));
                list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "3", "0.5"));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_00_22_000"));
                list.Add(Program.Transfer.Text(FairyName, "Kya!"));

                list.Add(Program.Transfer.Text(FairyName, "What happened to my clothes?!"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "She looks around the bathroom nervously, then takes a deep breath and focuses on me."));
                list.Add(Program.Transfer.Text(FairyName, "Umm, could you not stare so much?! This is really embarassing!"));

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Even if you tell me that..."));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "Ooh, bare butt!"));

                list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "8", "6", "", "0.7", "", "", "", "", "", ""));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_03_22_016"));
                list.Add(Program.Transfer.Text(FairyName, "Aaaaa, stop it! I've had enough, farewell!"));

                list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));
                list.Add(Program.Transfer.Create(false, Command.CharaActive, "0", "false"));
                list.Add(Program.Transfer.Create(false, Command.Fade, "out", "0.5", "white", "back", "TRUE"));

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Aw..."));

                list.Add(Program.Transfer.Text(FairyName, "Don't act so disappointed! Your eyes were way too lewd!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "You're actually a beauty, of course I'd stare in that situation!"));
                list.Add(Program.Transfer.Text(FairyName, "A-A beauty? Eheheh. See? You were wrong to doubt me."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I was wrong, I'm sorry. Long hair, emerald eyes, fair skin... You're totally my type! Please go out with me!"));
                list.Add(Program.Transfer.Text(FairyName, "Wait wait wait, what's with this heel turn?! You don't even know my name!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I'm sorry! I just assumed you were some old creepy dude lying to me. I prepared myself for the worst."));
                list.Add(Program.Transfer.Text(FairyName, "You thought I was some random creep, and yet you still stole panties for me? You're a pervert through and through."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hey, you said you would expose me if I didn't comply!"));
                list.Add(Program.Transfer.Text(FairyName, "I did? How would I even do that when only you can hear me?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I... I didn't think of that. Well, anyways! There's something much more important I want to ask."));
                list.Add(Program.Transfer.Text(FairyName, "Let me guess, my name?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Bingo, we really do think alike."));
                list.Add(Program.Transfer.Text(FairyName, "Ugh, listen now. Can you stop acting like this? Don't treat me nicely all of a sudden just because of my looks."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Oh, sorry, I didn't mean it like that. I'm just really relieved that you were telling the truth."));
                list.Add(Program.Transfer.Text(FairyName, $"If you say so. By the way, it's {EventUtils.HeroineName}."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Hmm?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, $"My name. I'm {EventUtils.HeroineFullName}. Nice to meet you."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, $"{EventUtils.HeroineName}... That's a really nice name, I like it. Can I call you that from now on? Calling you Panty Fairy is a bit..."));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Yes, of course, go ahead. The Panty Fairy thing was a joke. I didn't think you would keep calling me that."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Oh, did you expect me to ask for your real name?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Of course! No girl wants to be called a \"Panty Fairy.\""));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I'm sorry. Are you mad?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "I'm not mad, and stop apologizing. I'd rather talk about my clothes, for some reason I've lost all of my belongings."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Can't you wear one of the panties I brought you at least?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "I don't have them anymore, I converted them all into energy. But your idea is solid, I could use someone's uniform."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Wait, you're fine with someone else's clothes?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Not a problem, I can purify and adjust them easily."));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright... So what do I have to do? Grab a uniform from one of the lockers?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "No, that's too slow. Now that my power is restored, I have a much faster way of doing it."));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Go up to a girl, like when stealing her panties, except this time imagine stealing everything."));

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Everything?! Are you serious?"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Of course, consider it a reward for helping me out. Don't worry about getting caught, my blessing got much stronger. She won't even notice."));

                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "(I wish you didn't notice not having clothes...)"));

                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Did you say something?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "No, nothing! I'll get going then, see you in a bit!"));

                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(To steal clothes, talk to a girl in a uniform and click on the clothing icon at top right. You need at least 80 Sensuality/H points)"));
                list.Add(Program.Transfer.Text(EventUtils.Narrator, "(If the girl has her guard up then it's harder to successfully steal. In this case try befriending them first)"));

                SaveData.PantiesStolenHeld -= PantyRequiredCount;
                progress = true;
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, false, _toiletEventPosition, _toiletEventRotation);//, _toiletAdvCameraData);

            if (progress) eventCo = eventCo.AppendCo(() =>
            {
                icon.Dispose();
                Progress = StoryProgress.E7_Uniform;
            });

            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
        }

        public static void StartE7(IDisposable icon)
        {
            var list = EventUtils.CreateNewEvent(true);
            var progress = false;

            var scenario = Game.Instance.actScene.AdvScene.Scenario;
            var fairyCard = LoadFairyCard();
            scenario.heroineList = new List<SaveData.Heroine> { fairyCard };
            list.Add(Program.Transfer.Create(true, Command.CharaChange, "-2", "true"));

            if (SaveData.UniformsStolenTotal < 1)
            {
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "You're finally here! Do you have the uniform?"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Sorry, I didn't find any uniforms for you yet."));
                list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_03_22_003"));
                list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Then hurry up and get one, I can't wait to return to my body!"));
                list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I can't wait either!"));
            }
            else
            {
                var saveData = Game.Instance.saveData;
                var classNumber = saveData.player.schoolClass;
                var freeSlot = saveData.GetFreeClassSlots(classNumber).FirstOrDefault() ??
                               saveData.GetFreeClassSlots(classNumber + 1).FirstOrDefault() ??
                               saveData.GetFreeClassSlots(classNumber - 1).FirstOrDefault();
                if (freeSlot == null)
                {
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "(You need to free up one seat in any of the classes to continue)"));
                }
                else
                {
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I have the uniform, here you go!"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Finally! Thank you!"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "I hold the uniform up and it disappears in a ball of light."));
                    AddFlashOfLight(list);
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hmm... I'll have to make some adjustments but it should work. Give me a minute."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright, I can wait."));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "..."));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "I'm done! Behold!"));
                    list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0.5", "white", "back", "TRUE"));
                    list.Add(Program.Transfer.Create(false, Command.CharaCreate, "0", "-2")); // Spawn the heroine ID -2 as adv character ID 0
                    list.Add(Program.Transfer.Create(true, Command.CharaActive, "0", "true", "center")); // Enable the chracter 0 and move it to center
                    list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "0", "0")); // Make neck follow camera
                    list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "1")); // Make eyes follow camera
                    list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "0", "8", "19", "0", "1", "", "", "", "", "", ""));
                    list.Add(Program.Transfer.Create(false, Command.CharaMotion, "0", "Stand_02_00"));
                    list.Add(Program.Transfer.Create(false, Command.Fade, "out", "1", "white", "back", "TRUE")); // fade out to white with a sliding gradient
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, $"It is I, {EventUtils.HeroineName}!"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Oooh! Cute!"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "*Clap* *Clap* *Clap*"));
                    list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_02_22_001"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Ahaha, I didn't expect an ovation. You really are serious about this, aren't you."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "About what, becoming lovers? Of course I am! Our tastes match, and you're totally my type!"));
                    list.Add(Program.Transfer.Create(true, Command.CharaLookNeck, "0", "1", "1"));
                    list.Add(Program.Transfer.Create(true, Command.CharaLookEyes, "0", "1"));
                    list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_04_00"));
                    list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "", "", "7", "", "", "", "", "", "", "", ""));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "But we don't even know each other all that well."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "That can be fixed. Are you free after this?"));
                    list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c22/adm/00.unity3d", "adm_22_tanon_01"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Sigh... Alright, listen. From now on I'll be blending in with other students, so please act like I'm just a normal girl."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Like a normal girl? Sure, if you want to."));
                    list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_01_00"));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName,
                        $"Umm, {EventUtils.PlayerName}... Thank you for helping me. I would be stuck here for who knows how long without your help."));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Also... I might seem cold sometimes, but I really like talking with you. I think you're a good friend, but I don't know about being lovers."));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "Wait, does this mean that she's giving me a chance? Alright, I'll definitely make you fall in love with me!"));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "So we're starting as friends, this works for me. What will you do now?"));
                    list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stand_07_00"));
                    list.Add(Program.Transfer.Create(true, Command.CharaExpression, "0", "", "", "7", "", "", "", "", "", "", "", ""));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hmm... I think I'll take a stroll outside of the school, see what changed in the city. I'll come back to school after that."));
                    list.Add(Program.Transfer.Text(EventUtils.PlayerName, "Alright, stay safe out there. See you later!"));
                    list.Add(Program.Transfer.Create(true, Command.CharaActive, "0", "false"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "She walked to the door and waved to me before leaving the bathroom."));
                    list.Add(Program.Transfer.Text(EventUtils.HeroineName, "See you later!"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, $"({EventUtils.HeroineName} has been added to the student roster. After this day ends, she will appear in school as a normal student)"));
                    list.Add(Program.Transfer.Text(EventUtils.Narrator, "(Congratulations on finishing this side quest! This was KK_PantyFairy by ManlyMarco)"));

                    freeSlot.SetCharFile(fairyCard.charFile);
                    freeSlot.charFileInitialized = true;
                    // Set already met
                    freeSlot.talkEvent.Add(0);
                    freeSlot.talkEvent.Add(1);
                    // Set is friend
                    freeSlot.talkEvent.Add(2);
                    saveData.heroineList.Add(freeSlot);

                    progress = true;
                }
            }

            list.Add(Program.Transfer.Close());

            var eventCo = EventUtils.StartAdvEvent(list, false, _toiletEventPosition, _toiletEventRotation);//, _toiletAdvCameraData);

            if (progress) eventCo = eventCo.AppendCo(() =>
            {
                icon.Dispose();
                Progress = StoryProgress.Complete;
            });

            PantyFairyPlugin.Instance.StartCoroutine(eventCo);
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
    }
}