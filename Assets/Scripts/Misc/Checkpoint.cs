using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Misc
{
    public class Checkpoint : MonoBehaviour
    {
        public bool isFinishLine;
        public int checkpointNumber;

        public static implicit operator Transform(Checkpoint x)
        {
            return x.transform;
        }
    }
}
