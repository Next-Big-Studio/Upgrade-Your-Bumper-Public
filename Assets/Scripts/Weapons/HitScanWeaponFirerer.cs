using System.Collections;
using Destructors;
using Misc;
using Mounters;
using UnityEngine;

namespace Weapons
{
    public class HitScanWeaponFirerer : MonoBehaviour, IWeaponFirerer
    {
        // defined as a maximum difference in angle on both axes
        // so if inaccuracy = 3, then the bullet can be anywhere from -3 to 3 degrees
        // inaccurate on its two axes of freedom
        // if you want a better explanation, dm me later, it's 11 pm and im eepy :(

        public Transform weaponStartPoint;
        public GameObject bulletPrefab;
        public bool destroyed = false;
        public IMounter Mounter;
        public Rigidbody carRB;

        public AudioSource gunShotSound;

		private void Start()
		{
            try
            {
			    carRB = transform.parent.GetComponent<CarDestruction>().carSystem.GetComponent<Rigidbody>();
            } catch
            {
                // ignored
            }
		}

		private void Update()
		{
			if (!carRB)
            {
                try
                {
                    carRB = transform.parent.GetComponent<CarDestruction>().carSystem.GetComponent<Rigidbody>();
                }
                catch
                {
                    // ignored
                }
			}
		}

		public void Fire(Vector3 direction, WeaponInfo weaponInfo)
        {
            // this weapon shoots out a ray
            // if it hits near its max range, hooray!
            // if not, boo!!

            // it'll draw a line renderer too as an effect. i'll probably have to change this out though.
            RaycastHit hit;
            Physics.Raycast(weaponStartPoint.position, direction, out hit, weaponInfo.range);

            if (hit.transform && hit.transform.GetComponent<Destructor>())
            {
                CarDestruction carDestruction = hit.transform.GetComponent<CarDestruction>();
                if (carDestruction != null) { carDestruction.damageTracker.AddDamage(true, weaponInfo.damage); }
                foreach (Destructor destructor in hit.transform.GetComponents<Destructor>())
                {
                    destructor.TakeDamageAtPoint(hit.point, direction, weaponInfo.damage);
                }
            }
            gunShotSound.Play();
            ShootBullet(direction);
        }

        void ShootBullet(Vector3 direction)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, weaponStartPoint.position, Quaternion.identity);
            bulletInstance.GetComponent<BulletTravel>().StartMoving(direction * 240);
        }
    }
}
