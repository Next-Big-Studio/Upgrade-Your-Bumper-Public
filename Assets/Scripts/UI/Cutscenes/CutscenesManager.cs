using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace UI.Cutscenes
{
    public class CutscenesManager : MonoBehaviour
    {
        [SerializeField] protected Button skipButton;
        [SerializeField] protected PlayableDirector playableDirector;

        protected void Start()
        {
            skipButton.onClick.AddListener(Skipped);
            playableDirector.stopped += CutsceneEnded;
        }

        protected void CutsceneEnded(PlayableDirector obj)
        {
            // Change the skip button to a continue button
            skipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
            ColorBlock colors = skipButton.colors;
            colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            skipButton.colors = colors;
            
            // Change the skip button's action to load the loading screen
            skipButton.onClick.RemoveAllListeners();
        }

        private void Skipped()
        {
            playableDirector.Stop();
        }

        protected static void LoadLoadingScreen()
        {
            // Load the loading screen
            GameManager.Instance.LoadLoadingScreen();
        }
    }
}