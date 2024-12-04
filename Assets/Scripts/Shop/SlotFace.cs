using UnityEngine;

namespace Shop
{
    public class SlotFace : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer spriteRenderer;
        
        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}