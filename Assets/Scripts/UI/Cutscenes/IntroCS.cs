using UnityEngine;
using UnityEngine.Playables;

namespace UI.Cutscenes
{
    public class IntroCs : CutscenesManager
    {
        [SerializeField] private ControlsPanel panel;
        [SerializeField] private GameObject parentHolder;
        [SerializeField] private GameObject scrollView;
        [SerializeField] private GameObject background;
        
        private new void Start()
        {
            base.Start();
            
            // Hide the controls panel
            parentHolder.SetActive(false);
            panel.gameObject.SetActive(false);
            
            playableDirector.stopped += CutsceneEnded;
        }
        
        private new void CutsceneEnded(PlayableDirector obj)
        {
            background.SetActive(true);
            parentHolder.SetActive(true);
            
            // Show the controls panel
            panel.gameObject.SetActive(true);
            panel.ToggleSelected(true);
            
            base.CutsceneEnded(obj);
            
            skipButton.onClick.AddListener(ShowTutorial);
        }

        private void ShowTutorial()
        {
            panel.ToggleSelected(false);
            scrollView.SetActive(true);
            
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(LoadLoadingScreen);
        }
    }
}