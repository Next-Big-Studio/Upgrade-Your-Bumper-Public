using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace UI.Cutscenes
{
    public class LoseCS : CutscenesManager
    {
        
        private new void Start()
        {
            base.Start();
            
            playableDirector.stopped += CutsceneEnded;
        }
        
        private new void CutsceneEnded(PlayableDirector obj)
        {
            
            base.CutsceneEnded(obj);
            
            skipButton.onClick.AddListener(LoadLoadingScreen);
        }
    }
}