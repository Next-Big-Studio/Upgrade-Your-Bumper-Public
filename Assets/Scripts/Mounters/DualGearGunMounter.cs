using UnityEngine;
using Weapons;

namespace Mounters
{
    public class DualGearGunMounter : MonoBehaviour, IMounter
    {
        public Transform leftGun;
        public Transform rightGun;
    
        public void Mount(string name)
        {
            Mounting mounter = transform.parent.GetComponent<Mounting>();
            mounter.MountWeapon(leftGun, mounter.leftSideMount, name);
            mounter.MountWeapon(rightGun, mounter.rightSideMount, name);
            leftGun.right = transform.parent.right;
            rightGun.right = transform.parent.right;
        }

        public void Dismount()
        {
            // TODO: THIS lol
            throw new System.NotImplementedException("josh is a lazy idiot");
        }
    }
}