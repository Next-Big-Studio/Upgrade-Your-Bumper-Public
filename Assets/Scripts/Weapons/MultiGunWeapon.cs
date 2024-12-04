namespace Weapons
{
    public class MultiGunWeapon : GunWeapon
    {
        // this gun has multiple firerers
        // that's all
        public IWeaponFirerer[] Firerers;
        int numShot;

        private void Start()
        {
            Firerers = GetComponentsInChildren<IWeaponFirerer>();
            print(GetComponentsInChildren<HitScanWeaponFirerer>().Length);
        }

        public override void Fire()
        {
            print("base fire");
            Firerer = Firerers[numShot];
            numShot++;
            if (numShot >= Firerers.Length)
                numShot = 0;
            base.Fire();
        }
    
    }
}
