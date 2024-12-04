using UnityEngine;

namespace UI.Framework
{
    public class UIPanel : MonoBehaviour
    {
        public string panelName { get; protected set; }

        protected RectTransform RTransform;
        [SerializeField] public GameObject content;

        protected UIPanel() { }
        
        protected void Awake()
        {
            RTransform = GetComponent<RectTransform>();
            
            if (content == null)
            {
                content = gameObject;
            }
            
            if (string.IsNullOrEmpty(panelName))
            {
                gameObject.SetActive(false);
            }
        }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Refresh()
        {
            // Override this method in child classes
        }

        public virtual void ToggleSelected(bool selected)
        {
            if (selected)
            {
                Refresh();
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}