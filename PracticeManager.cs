using HarmonyLib;
using System;
using System.Collections.Generic;
using TootTallyCore;
using TootTallyCore.Graphics;
using TootTallyCore.Graphics.Animations;
using TootTallyCore.Utils.TootTallyGlobals;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TootTallyPractice
{
    public static class PracticeManager
    {
        private static LevelSelectController _currentInstance;
        private static Button _practiceButton;
        private static AudioSource _btnClickSfx;
        private static PracticePanel _practicePanel;
        public static float StartTime;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void OnLevelSelectControllerStartPostfixAddPracticeButton(LevelSelectController __instance)
        {
            _currentInstance = __instance;
            TootTallyGlobalVariables.isPracticing = false;
            StartTime = 0;
            _practicePanel = new PracticePanel();

            //Practice Button Setup
            _btnClickSfx = __instance.hoversfx;
            _practiceButton = GameObject.Instantiate(__instance.playbtn, __instance.fullpanel.transform);
            _practiceButton.name = "PRACTICE";
            var rect = _practiceButton.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(461, -58);
            var txts = _practiceButton.GetComponentsInChildren<Text>();
            foreach (var txt in txts)
            {
                if (txt.TryGetComponent<LocalizeStringEvent>(out var localStringEvent))
                    GameObject.DestroyImmediate(localStringEvent);
                txt.text = "<i>Practice</i>";
            }
            _practiceButton.onClick = new Button.ButtonClickedEvent();
            _practiceButton.onClick.AddListener(OnPracticeButtonClick);
            var btnTriggers = _practiceButton.GetComponent<EventTrigger>();
            btnTriggers.triggers.Clear();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener(OnPracticeButtonHover);
            btnTriggers.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerLeaveEvent = new EventTrigger.Entry();
            pointerLeaveEvent.eventID = EventTriggerType.PointerExit;
            pointerLeaveEvent.callback.AddListener(OnPracticeButtonUnhover);
            btnTriggers.triggers.Add(pointerLeaveEvent);
        }

        public static void OnPracticeButtonHover(BaseEventData _)
        {
            _btnClickSfx.Play();
            _practiceButton.transform.Find("playBackground").GetComponent<Image>().color = Theme.colors.playButton.backgroundOver;
            _practiceButton.transform.Find("playOutline").GetComponent<Image>().color = Theme.colors.playButton.outlineOver;
            _practiceButton.transform.Find("txt-play/txt-play-front").GetComponent<Text>().color = Theme.colors.playButton.textOver;
            _practiceButton.transform.Find("playShadow").GetComponent<Image>().color = Theme.colors.playButton.shadowOver;
        }

        public static void OnPracticeButtonUnhover(BaseEventData _)
        {
            _practiceButton.transform.Find("playBackground").GetComponent<Image>().color = Theme.colors.playButton.background;
            _practiceButton.transform.Find("playOutline").GetComponent<Image>().color = Theme.colors.playButton.outline;
            _practiceButton.transform.Find("txt-play/txt-play-front").GetComponent<Text>().color = Theme.colors.playButton.text;
            _practiceButton.transform.Find("playShadow").GetComponent<Image>().color = Theme.colors.playButton.shadow;
        }

        public static void OnPracticeButtonClick()
        {
            if (_practicePanel.IsVisible)
                _practicePanel.Hide();
            else
                _practicePanel.Show(_currentInstance.alltrackslist[_currentInstance.songindex].trackref);
        }

        public static void StartSongWithPractice()
        {
            TootTallyGlobalVariables.isPracticing = true;
            _currentInstance.clickPlay();

        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
        [HarmonyPostfix]
        public static void OnClickPlayAnimatePracticeButton() => TootTallyAnimationManager.AddNewPositionAnimation(_practiceButton.gameObject, new Vector2(661, -58), .35f, new SecondDegreeDynamicsAnimation(2.15f, 1f, 1f));

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPrefix]
        public static bool SkipAdvanceSongsIfPracticePanelVisible() => !_practicePanel.IsVisible;

        [HarmonyPatch(typeof(GameController), nameof(GameController.playsong))]
        [HarmonyPrefix]
        public static void SkipToStartTimeOnSongStart(GameController __instance)
        {
            if (!TootTallyGlobalVariables.isPracticing) return;

            if (StartTime > __instance.musictrack.clip.length - 1) StartTime = __instance.musictrack.clip.length - 1;
            __instance.musictrack.time = StartTime;
            __instance.resync_timer = 5;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.buildNotes))]
        [HarmonyPrefix]
        public static void DeletePastNotesFromLevelData(GameController __instance)
        {
            if (!TootTallyGlobalVariables.isPracticing || StartTime <= 2) return;
            var index = __instance.leveldata.FindIndex(x => BeatToSeconds2(x[0], __instance.tempo) >= StartTime + 2);
            if (index > 0)
                __instance.leveldata = __instance.leveldata.GetRange(index, __instance.leveldata.Count - index);
        }

        public static float BeatToSeconds2(float beat, float bpm) => 60f / bpm * beat;
    }
}
