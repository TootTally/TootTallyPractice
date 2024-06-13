using BaboonAPI.Hooks.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTallyCore.Graphics;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TootTallyPractice
{
    public class PracticePanel
    {
        private GameObject _practicePanel;
        private Text _title;
        private TMP_Text _startTimeLabel;
        private GameObject _panelContainer;
        private Slider _startTimeSlider;
        public bool IsPracticePanelVisible;
        private bool _isStarting;

        public PracticePanel()
        {
            //Panel content
            _practicePanel = GameObjectFactory.CreateOverlayPanel(null, Vector2.zero, new Vector2(1000, 350), 12f, "PracticePanel");
            _practicePanel.SetActive(false);
            var fgPanel = _practicePanel.transform.GetChild(0).GetChild(1);
            GameObject.DestroyImmediate(fgPanel.GetChild(1).gameObject);
            _title = fgPanel.GetChild(0).GetComponent<Text>();
            GameObject.DestroyImmediate(_title.GetComponent<LocalizeStringEvent>());
            _panelContainer = fgPanel.GetChild(1).gameObject;
            _startTimeLabel = GameObjectFactory.CreateSingleText(_panelContainer.transform, "StartTime", "Start Time: 0:00");
            _startTimeLabel.rectTransform.sizeDelta = new Vector2(600, 40);
            _startTimeSlider = GameObjectFactory.CreateSliderFromPrefab(_panelContainer.transform, "StartTimeSlider");
            _startTimeSlider.transform.localScale = Vector3.one;
            _startTimeSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 40);
            _startTimeSlider.handleRect.sizeDelta = new Vector2(40, 0);
            _startTimeSlider.gameObject.SetActive(true);
            _startTimeSlider.onValueChanged.AddListener(OnStartTimeSliderValueChange);

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

            IsPracticePanelVisible = false;
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
            _startTimeSlider.value = 0;
            IsPracticePanelVisible = true;
            _practicePanel.SetActive(true);
        }

        public void Hide()
        {
            IsPracticePanelVisible = false;
            _practicePanel.SetActive(false);
        }

        public void StartSong()
        {
            if (_isStarting) return;
            _isStarting = true;
            PracticeManager.StartSongWithPractice();
        }

        public void OnStartTimeSliderValueChange(float value)
        {
            PracticeManager.StartTime = value;
            _startTimeLabel.text = $"Start Time: {TimeSpan.FromSeconds(value):mm\\:ss} / {TimeSpan.FromSeconds(_startTimeSlider.maxValue):mm\\:ss}";
        }

        public void SetSliderMaxValue(float maxValue) => _startTimeSlider.maxValue = maxValue;
    }
}
