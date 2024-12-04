using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Video;

namespace Misc
{
    public class DynamicLookAt : MonoBehaviour
    {
    
        [SerializeField] private CinemachineVirtualCamera vcam;
        private GameObject _lookAtTarget;
        [SerializeField] private GameObject lookAtGameObject;
        [SerializeField] private GameObject player;
        private CinemachineTrackedDolly _body;
        [SerializeField] private PostProcessProfile postProcessProfile;
        private bool isDone;
        private bool videoPlaying;
        [SerializeField] private VideoPlayer videoPlayer;
        
        private void Start()
        {
            _lookAtTarget = player;
            vcam.LookAt = lookAtGameObject.transform;
            _body = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
        }

        private void Update()
        {
            if (!isDone)
            {
                // Move the camera along the track
                _body.m_PathPosition += Time.deltaTime * 0.005f;
                
                // Lerp the lookAtGameObject to the lookAtTarget
                lookAtGameObject.transform.position = Vector3.Lerp(lookAtGameObject.transform.position, _lookAtTarget.transform.position, Time.deltaTime);

                if (_body.m_PathPosition >= 1)
                {
                    isDone = true;
                }
                
                return;
            }
            
            postProcessProfile.GetSetting<DepthOfField>().focusDistance.value = 0.5f;
            postProcessProfile.GetSetting<DepthOfField>().aperture.value = 0.1f;
            
            // Lerp alpha of the video player 
            videoPlayer.targetCameraAlpha = Mathf.Lerp(videoPlayer.targetCameraAlpha, 1, Time.deltaTime/4);

            if (!(videoPlayer.targetCameraAlpha >= 0.99f) && (videoPlaying)) return;
            
            videoPlayer.Play();
            videoPlaying = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("LookAt")) return;
        
            _lookAtTarget = other.gameObject;
        }
    
        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag("LookAt")) return;
        
            _lookAtTarget = player;
        }
    }
}
