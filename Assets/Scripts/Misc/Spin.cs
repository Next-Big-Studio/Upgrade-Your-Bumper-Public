using UnityEngine;

namespace Misc
{
    public class Spin : MonoBehaviour
    {
        public float speed = 10f;   // The speed at which the object spins
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(0, speed * Time.deltaTime, 0);
        }
    }
}
