using UnityEngine;

namespace Car
{
    [CreateAssetMenu(fileName = "Car", menuName = "CarScriptableObjects/Car", order = 1)]
    public class CarInfo : ScriptableObject
    {
        // info for code
        public GameObject carPrefab;

        // info for player
        public new string name;
        public string description;
    
        // for saving
        public int id;
    
    }
}
