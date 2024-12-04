using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Shop
{
    public class SlotLine : MonoBehaviour
    {
        [SerializeField] private SlotFace slotFace1;     // Reference to the SlotFace component for displaying a prize

        public SlotPrize FinalPrize;
        private IList<SlotPrize> _slotPrizes;                // Array of slot prizes
        private int _currentPrizeIndex;                 // Index of the current prize

        private float _initialSpinSpeed;            // Initial speed of the reel spin
        private float _finalSpinSpeed;               // Slowest speed when reel stops
        private int _spinAmount;                        // Amount of spins
        private float _spinDuration;                  // Total duration of the spin
        public bool isSpinning;                        // Flag to check if the reel is currently spinning

        private const float LocalStartHeight = 1.76f;
        private const float LocalEndHeight = -1.76f;

        private void Start()
        {
            _currentPrizeIndex = 0;
            
            slotFace1.gameObject.SetActive(true);
        }
        
        // Set the slot prizes
        public void SetSlotPrizes(SlotPrize[] slotPrizes, Sprite initialSlotFaceSprite)
        {
            _slotPrizes = slotPrizes;
            
            // Set the slot face sprites
            slotFace1.SetSprite(initialSlotFaceSprite);
            NextSlotFaces();
        }
        
        public void SetSpinSpeed(float initialSpinSpeed, float finalSpinSpeed, float spinDuration, int spinAmount)
        {
            _initialSpinSpeed = initialSpinSpeed;
            _finalSpinSpeed = finalSpinSpeed;
            _spinDuration = spinDuration;
            _spinAmount = spinAmount;
        }
        
        public void StartSpin()
        {
            if (isSpinning) return;    
            
            isSpinning = true;
            StartCoroutine(SpinReel());
        }

        private IEnumerator SpinReel()
        {
            int spinCount = 0;
            isSpinning = true;

            while (spinCount < _spinAmount)
            {
                float elapsedTime = 0f;

                // Loop until the slot face moves from LocalStartHeight to LocalEndHeight
                while (elapsedTime < _spinDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float currentSpinSpeed = Mathf.Lerp(_initialSpinSpeed, _finalSpinSpeed, elapsedTime / _spinDuration);

                    // Move the slot face
                    slotFace1.transform.localPosition += new Vector3(0, -currentSpinSpeed * Time.deltaTime, 0);

                    // Check if the slot face is out of bounds
                    if (slotFace1.transform.localPosition.y <= LocalEndHeight)
                    {
                        // Reset position and increment spin count
                        slotFace1.transform.localPosition = new Vector3(slotFace1.transform.localPosition.x, LocalStartHeight, slotFace1.transform.localPosition.z);
                        NextSlotFaces();
                        spinCount++;

                        // Break out of inner loop to reset elapsed time and start next spin
                        break;
                    }

                    yield return null; // Wait until the next frame
                }
            }
            
            _currentPrizeIndex = Random.Range(0, _slotPrizes.Count);
            
            // Set the final prize
            slotFace1.SetSprite(_slotPrizes[_currentPrizeIndex].PrizeSprite);
            
            // Lerp to 0 to make sure the slot face is at the correct position
            slotFace1.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBounce);
            
            FinalPrize = _slotPrizes[_currentPrizeIndex];
            
            isSpinning = false;
        }
        
        // Set the slot face sprites
        private void NextSlotFaces()
        {
            // Set the sprite of the slot face
            slotFace1.SetSprite(_slotPrizes[_currentPrizeIndex].PrizeSprite);
            
            // Increment the current prize index
            _currentPrizeIndex = (_currentPrizeIndex == _slotPrizes.Count - 1) ? 0 : _currentPrizeIndex + 1;
        }
    }
}
