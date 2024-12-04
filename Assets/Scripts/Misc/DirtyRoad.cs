using UnityEngine;

public class DirtyRoad : MonoBehaviour
{
    bool allIsGround = false;
    // Start is called before the first frame update
    private void Start()
    {
        UpdateLayer(gameObject);
    }

    private static void UpdateLayer(GameObject go)
    {
        go.layer = LayerMask.NameToLayer("Ground");
        foreach (Transform child in go.transform)
        {
            UpdateLayer(child.gameObject);
        }
    }
    
    private void CheckAll(GameObject go)
    {
        if (go.layer != LayerMask.NameToLayer("Ground"))
        {
            allIsGround = false;
            return;
        }
        foreach (Transform child in go.transform)
        {
            CheckAll(child.gameObject);
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        //if (allIsGround) return;
        
        UpdateLayer(gameObject);
            
        allIsGround = true;
        CheckAll(gameObject);
    }
}
