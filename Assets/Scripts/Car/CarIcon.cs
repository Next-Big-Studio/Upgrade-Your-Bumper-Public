using UnityEngine;

namespace Car
{
    public class CarIcon : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
            transform.up = Vector3.forward;
        }

        public void SetColor(Color color)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = color;
            meshRenderer.material.SetColor("_EmissionColor", color);
        }
    }
}
