using BaboonAPI.Hooks.Tracks;
using System;
using TMPro;
using TootTallyCore.Graphics;
using TootTallyCore.Graphics.Animations;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TootTallyPractice
{
    public class PracticePanel
    {
        private GameObject _practicePanel, _fgPanel;
        private TootTallyAnimation _panelAnimation;
        private Text _title;
        private TMP_Text _startTimeLabel;
        private GameObject _panelContainer;
        private Slider _startTimeSlider;
        public bool IsVisible;
        private bool _isStarting;

        public PracticePanel()
        {
            //Panel content
            _practicePanel = GameObjectFactory.CreateOverlayPanel(null, Vector2.zero, new Vector2(1000, 350), 12f, "PracticePanel");
            _practicePanel.SetActive(false);
            _fgPanel = _practicePanel.transform.GetChild(0).GetChild(1).gameObject;
            _fgPanel.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, .01f);
            _fgPanel.transform.parent.localScale = Vector2.zero;

            GameObject.DestroyImmediate(_fgPanel.transform.GetChild(1).gameObject);
            _title = _fgPanel.transform.GetChild(0).GetComponent<Text>();
            GameObject.DestroyImmediate(_title.GetComponent<LocalizeStringEvent>());
            _panelContainer = _fgPanel.transform.GetChild(1).gameObject;
            _startTimeLabel = GameObjectFactory.CreateSingleText(_panelContainer.transform, "StartTime", "Start Time: 0:00");
            _startTimeLabel.rectTransform.sizeDelta = new Vector2(600, 40);
            _startTimeSlider = GameObjectFactory.CreateSliderFromPrefab(_panelContainer.transform, "StartTimeSlider");
            _startTimeSlider.transform.localScale = Vector3.one;
            _startTimeSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 40);
            _startTimeSlider.handleRect.sizeDelta = new Vector2(40, 0);
            _startTimeSlider.gameObject.SetActive(true);
            _startTimeSlider.onValueChanged.AddListener(OnStartTimeSliderValueChange);
            _startTimeSlider.minValue = 2;

            EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
            pointerUpEvent.eventID = EventTriggerType.PointerUp;
            pointerUpEvent.callback.AddListener(delegate { PracticeManager.SetAudioClipTime(); });
            _startTimeSlider.gameObject.AddComponent<EventTrigger>().triggers.Add(pointerUpEvent);

            GameObjectFactory.CreateCustomButton(_panelContainer.transform, Vector2.zero, new Vector2(120, 40), "Back", "PracticeBackBtn", Hide);
            GameObjectFactory.CreateCustomButton(_panelContainer.transform, Vector2.zero, new Vector2(120, 40), "Start", "PracticeStartBtn", StartSong);

            //Panel setup
            _title.text = "Practice Mode";
            var panelRect = _practicePanel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = Vector2.one / 2f;
            var containerRect = _panelContainer.GetComponent<RectTransform>();
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(1000, 150);
            var containerLayout = _panelContainer.GetComponent<VerticalLayoutGroup>();
            containerLayout.childAlignment = TextAnchor.MiddleCenter;
            containerLayout.padding = new RectOffset();

            IsVisible = false;
        }

        public void Show(string trackref)
        {
            var track = TrackLookup.tryLookup(trackref);
            if (track == null)
            {
                TootTallyNotifManager.DisplayNotif("Track Incompatible with practice mode");
                return;
            }

            SetSliderMaxValue(track.Value.length - 1);
            _startTimeSlider.value = _startTimeSlider.value > track.Value.length - 1 ? 0 : _startTimeSlider.value; //Cheesy way to trigger the OnValueChange event lol
            IsVisible = true;

            _practicePanel.SetActive(true);
            _panelAnimation?.Dispose();
            _panelAnimation = TootTallyAnimationManager.AddNewScaleAnimation(_fgPanel.transform.parent.gameObject, Vector3.one, 1f, new SecondDegreeDynamicsAnimation(2.75f, 1f, 0f));
        }

        public void Hide()
        {
            IsVisible = false;
            _panelAnimation?.Dispose();
            _panelAnimation = TootTallyAnimationManager.AddNewScaleAnimation(_fgPanel.transform.parent.gameObject, Vector2.zero, .45f, new SecondDegreeDynamicsAnimation(3.25f, 1f, .25f),
                delegate { _practicePanel.SetActive(false); });
            if (!GlobalVariables.menu_music)
                PracticeManager.StopAudioClip();
        }

        public void StartSong()
        {
            if (_isStarting) return;
            _isStarting = true;
            _panelAnimation?.Dispose();
            TootTallyAnimationManager.AddNewPositionAnimation(_fgPanel.transform.parent.gameObject, new Vector3(2000, 0, 0), .45f, new SecondDegreeDynamicsAnimation(3.25f, 1f, .15f));
            PracticeManager.StartSongWithPractice();
        }

        public void OnStartTimeSliderValueChange(float value)
        {
            PracticeManager.StartTime = value;
            _startTimeLabel.text = $"Start Time: {TimeSpan.FromSeconds(PracticeManager.StartTime):mm\\:ss} / {TimeSpan.FromSeconds(_startTimeSlider.maxValue):mm\\:ss}";
        }

        public void SetSliderMaxValue(float maxValue) => _startTimeSlider.maxValue = maxValue;
    }
}
