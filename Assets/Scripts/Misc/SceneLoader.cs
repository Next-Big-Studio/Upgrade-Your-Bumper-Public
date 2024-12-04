using UnityEngine;
using UnityEngine.SceneManagement;

namespace Misc
{
    public class SceneLoader : MonoBehaviour
    {
        public static void LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        public static void LoadSceneAsync(int sceneIndex)
        {
            SceneManager.LoadSceneAsync(sceneIndex);
        }
        
        public static void LoadSceneAsync(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
        
        public static void QuitGame()
        {
            Application.Quit();
        }
    }
}