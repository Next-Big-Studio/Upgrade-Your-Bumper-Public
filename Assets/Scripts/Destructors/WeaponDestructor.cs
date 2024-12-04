using UnityEngine;

namespace Destructors
{
    public class WeaponDestructor : Destructor
    {
        // equivalent to bodyStrength
        public float weaponDurability;
        // how much IN TOTAL the weapon can be deformed before it stops working.
        // calculated by summing the total deformation of each vertex
        public float maximumDisplacementBeforeFailure;

        public override void TakeDamageAtPoint(Vector3 point, Vector3 direction, float damage)
        {
            TakeDamageAtPoint(point, direction, damage, weaponDurability);
        }

        private void OnCollisionEnter(Collision collision)
        {
            foreach (ContactPoint point in collision.contacts)
            {
                print("ouchies!");
                TakeDamageAtPoint(point.point, collision.impulse.normalized, collision.impulse.magnitude, weaponDurability);
            }
        }

        // Added meshes 
        public void AddWeapon(MeshFilter weapon, bool isMainMesh)
        {
            DestructableMeshInformation mesh = new DestructableMeshInformation();
            mesh.meshFilter = weapon;
            mesh.mesh = mesh.meshFilter.mesh;
            mesh.normalVertices = mesh.mesh.vertices; // keep a copy of the original
            mesh.modifiedVertices = mesh.mesh.vertices;
            mesh.objectTransform = mesh.meshFilter.transform;
            if (isMainMesh)
            {
                mesh.isMainMesh = true;
                mainMesh = mesh;
            }
            if(mesh.meshFilter.GetComponent<MeshCollider>())
            {
                mesh.meshCollider = mesh.meshFilter.GetComponent<MeshCollider>();
            }
            meshes.Add(mesh);
        }
    }
}