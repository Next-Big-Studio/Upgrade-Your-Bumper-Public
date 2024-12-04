using Saving_System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Main_Menu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        // Start is called before the first frame update
        private void Start()
        {
            continueButton.onClick.AddListener(ContinueGame);
            newGameButton.onClick.AddListener(NewGame);
            statsButton.onClick.AddListener(Stats);
            quitButton.onClick.AddListener(Quit);
        }
        
        public void Setup(bool saveCreated)
        {
            continueButton.gameObject.SetActive(!saveCreated);
        }
        
        private static void ContinueGame()
        {
            SceneManager.LoadScene("Loading");
        }
        
        private static void NewGame()
        {
            SaveSystem.Instance.WipeSave();
            
            GameManager.Instance.LoadThisScene("Intro");
        }
        
        private static void Stats()
        {
            // TODO: Show the player's stats
        }
        
        private static void Quit()
        {
            // Quit the game
            GameManager.Instance.OnApplicationQuit();
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false; // This stops play mode in the editor
            #endif
        }
    }
}
