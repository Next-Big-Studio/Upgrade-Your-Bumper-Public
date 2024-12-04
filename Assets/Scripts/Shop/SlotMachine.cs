using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Shop
{
    public class SlotMachine : MonoBehaviour
    {
        [SerializeField] private SlotLine slotLine1;
        [SerializeField] private SlotLine slotLine2;
        [SerializeField] private SlotLine slotLine3;
        
        private SlotPrize[] _slotPrizes; // Array of slot prizes
        
        public float initialSpinSpeed = 100f; // Initial speed of the reel spin
        public float finalSpinSpeed = 1f; // Slowest speed when reel stops
        public float spinDelay = 1f; // Delay between each reel spin
        public float spinDuration = 3.0f; // Total duration of the spin
        public int spinAmount = 10; // Amount of spins
        public bool isSpinning; // Flag to check if the reel is currently spinning

        public void InitializeSlotPrizes(IList<SlotPrize> slotPrizes, Sprite initialSlotFaceSprite)
        {
            _slotPrizes = slotPrizes.ToArray();
            
            ShufflePrizes();
            slotLine1.SetSlotPrizes(_slotPrizes, initialSlotFaceSprite);
            
            ShufflePrizes();
            slotLine2.SetSlotPrizes(_slotPrizes, initialSlotFaceSprite);
            
            ShufflePrizes();
            slotLine3.SetSlotPrizes(_slotPrizes, initialSlotFaceSprite);
            
            slotLine1.SetSpinSpeed(initialSpinSpeed, finalSpinSpeed, spinDuration, spinAmount);
            slotLine2.SetSpinSpeed(initialSpinSpeed, finalSpinSpeed, spinDuration, spinAmount);
            slotLine3.SetSpinSpeed(initialSpinSpeed, finalSpinSpeed, spinDuration, spinAmount);
        }
        
        public void StartSpin()
        {
            if (isSpinning) return;
            
            isSpinning = true;
            StartCoroutine(SpinReels());
        }
        
        private IEnumerator SpinReels()
        {
            yield return new WaitForSeconds(spinDelay);
            slotLine1.StartSpin();
            
            yield return new WaitForSeconds(spinDelay);
            slotLine2.StartSpin();
            
            yield return new WaitForSeconds(spinDelay);
            slotLine3.StartSpin();
            
            while (slotLine1.isSpinning || slotLine2.isSpinning || slotLine3.isSpinning)
            {
                yield return null; // Wait for the next frame
            }
            
            isSpinning = false;
        }

        private void ShufflePrizes()
        {
            // New random seed
            Random.InitState(System.DateTime.Now.Millisecond);
            for (int i = 0; i < _slotPrizes.Length; i++)
            {
                int randomIndex = Random.Range(i, _slotPrizes.Length);
                (_slotPrizes[i], _slotPrizes[randomIndex]) = (_slotPrizes[randomIndex], _slotPrizes[i]);
            }
        }
        public SlotPrize GetPrizes()
        {
            // Get prizes from each slot line
            SlotPrize prize1 = slotLine1.FinalPrize;
            SlotPrize prize2 = slotLine2.FinalPrize;
            SlotPrize prize3 = slotLine3.FinalPrize;
    
            // Dictionary to track each prize and its count
            SlotPrize[] prizes = { prize1, prize2, prize3 };
            var prizeCount = new Dictionary<string, int>();
            
            // Count occurrences of each prize
            foreach (SlotPrize prize in prizes)
            {
                if (!prizeCount.TryAdd(prize.PrizeName, 1))
                    prizeCount[prize.PrizeName]++;
            }

            // Check for winning conditions
            foreach ((string prize, int count) in prizeCount)
            {
                SlotPrize newPrize = prizes.First(p => p.PrizeName == prize);
                switch (count)
                {
                    case 3:
                        // Triple match
                        newPrize.Amount = 3;
                        return newPrize;
                    case 2:
                        newPrize.Amount = 2;
                        return newPrize;
                }
            }

            // No winning combination
            return null;
        }

    }
}