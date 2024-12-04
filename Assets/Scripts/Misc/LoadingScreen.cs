using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Misc
{
    public class LoadingScreen : MonoBehaviour
    {
        public GameObject background;
        public GameObject tipsText;
        public GameObject loadingText;

        private string[] _allTips;
    
        public List<Sprite> backgrounds;
    
        // Start is called before the first frame update
        private void Start()
        {
            // We could do this more efficiently by loading it in the start of the game but this is just a loading screen so it's fine
            StreamReader reader = new(Application.streamingAssetsPath + "/Tips.txt"); // Load CarNames.txt from streaming assets
            _allTips = reader.ReadToEnd().Split('\n'); // Split the text into an array of tips
        
            background.GetComponent<Image>().sprite = backgrounds[Random.Range(0, backgrounds.Count)]; // Random background
            tipsText.GetComponent<TextMeshProUGUI>().text = _allTips[Random.Range(0, _allTips.Length)]; // Random tip
        
            StartCoroutine(LoadingText()); // Start the loading text
        }
    
        // This is so bad, but it's just a loading screen so it's fine
        private IEnumerator LoadingText()
        {
            while (true)
            {
                loadingText.GetComponent<TextMeshProUGUI>().text = "Loading";
                yield return new WaitForSeconds(0.5f);
                loadingText.GetComponent<TextMeshProUGUI>().text = "Loading.";
                yield return new WaitForSeconds(0.5f);
                loadingText.GetComponent<TextMeshProUGUI>().text = "Loading..";
                yield return new WaitForSeconds(0.5f);
                loadingText.GetComponent<TextMeshProUGUI>().text = "Loading...";
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
