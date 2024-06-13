using HarmonyLib;
using Mono.Security.Protocol.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyCore.Utils.TootTallyGlobals;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallyPractice
{
    public static class PracticeManager
    {
        private static Button _practiceButton;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void OnLevelSelectControllerStartPostfixAddPracticeButton(LevelSelectController __instance)
        {
            _practiceButton = GameObject.Instantiate(__instance.playbtn, __instance.fullpanel.transform);
            _practiceButton.name = "PRACTICE";
            var rect = _practiceButton.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(456, -58);
            var txtObj = _practiceButton.transform.Find("txt-play");
            txtObj.GetComponent<Text>().text = txtObj.transform.GetChild(0).GetComponent<Text>().text = "PRACTICE";
            
        }

        public static void OnPracticeButtonHover()
        {

        }

        public static void OnPracticeButtonUnhover()
        {

        }

    }
}
