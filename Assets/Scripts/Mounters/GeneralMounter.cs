using Destructors;
using Mounters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

public class GeneralMounter : MonoBehaviour, IMounter
{
    public MountSpot mountSpot;
    
    // puts the object on the car
    public void Mount(string name)
    {
        transform.parent.GetComponent<Mounting>().MountGeneral(transform, mountSpot, name);
    }

    // removes the object from the car
    public void Dismount()
    {
        transform.parent.GetComponent<CarDestruction>().DeleteMesh(GetComponent<MeshFilter>());
    }
}
