using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform billboardElement;

    void LateUpdate()
    {
        billboardElement.LookAt(transform.position + Camera.main.transform.forward);
    }
}
