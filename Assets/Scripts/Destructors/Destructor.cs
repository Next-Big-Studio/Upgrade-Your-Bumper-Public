using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Destructors
{
    // This class handles destroying ANYTHING.
    // Other classes extend it, such as CarDestruction
    // This class declares fields necessary for destruction and the method
    // "Take Damage At Point"

    public abstract class Destructor : MonoBehaviour
    {
        [Header("Config")] public float maxDeform = 0.05f; // the maximum amount each vertex can moves from its original spot

        public float deformationRadius = 5; // the "radius" of the affected hit. if the object is hit at a point, all of the vertices within this radius around will deform.

        [Range(0f, 1f)] public float deformationFallOff = 0.5f; // how quickly the vertices around the central hit stop being affected.
        // small falloff means a very square hole. a medium falloff means a round crater. high falloff means a very steep puncture.

        public float deformationPerImpulse = 0.001f; // how many units a vertex will be deformed per 1 Ns of impulse, assuming the vertex is right on the collision

        [Header("Meshes")] public List<DestructableMeshInformation> meshes;
        public List<DetachableMeshInformation> detachableMeshes;
        public DestructableMeshInformation mainMesh;

        [Header("Info")] 
        public float totalDeformation = 0f;
        public float maxDeformation = 1000f;

        protected virtual void Awake()
        {
            foreach (DestructableMeshInformation mesh in meshes)
            {
                mesh.mesh = mesh.meshFilter.mesh;
                mesh.normalVertices = mesh.mesh.vertices; // keep a copy of the original
                mesh.modifiedVertices = mesh.mesh.vertices;
                mesh.objectTransform = mesh.meshFilter.transform;

                // check if there already exists a main mesh
                if (mesh.isMainMesh)
                {
                    mainMesh = mesh;
                }

                if (mesh.meshFilter.GetComponent<MeshCollider>())
                {
                    mesh.meshCollider = mesh.meshFilter.GetComponent<MeshCollider>();
                }
            }

            foreach (DetachableMeshInformation detachableMesh in detachableMeshes)
            {
                detachableMesh.objectTransform = detachableMesh.meshFilter.transform;
                detachableMesh.meshCollider = detachableMesh.meshFilter.GetComponent<MeshCollider>();
                detachableMesh.AttachedVertices = new HashSet<int>();
                // figure out all of the vertices on the main mesh that are close to each of the vertices on this mesh
                for (int j = 0; j < mainMesh.normalVertices.Length; j++)
                {
                    Vector3 mainMeshVertex = mainMesh.normalVertices[j];
                    Vector3 mainMeshVertexWorldPoint = mainMesh.objectTransform.TransformPoint(mainMeshVertex);
                    foreach (Vector3 detachableMeshVertex in detachableMesh.meshFilter.mesh.vertices)
                    {
                        Vector3 detachableMeshVertexWorldPoint = detachableMesh.objectTransform.TransformPoint(detachableMeshVertex);
                        if (Vector3.Distance(detachableMeshVertexWorldPoint, mainMeshVertexWorldPoint) < 0.5f)
                        {
                            detachableMesh.AttachedVertices.Add(j);
                        }
                    }
                }

                detachableMesh.maxAttachedVertices = detachableMesh.AttachedVertices.Count;
            }
        }

        // this method is used when other classes are trying to damage this object (for example, weapons shooting cars)
        // since the weapon has no clue what the strength of the given target is, this method will be called instead.
        // it is the job of this method to call the 4 parameter overload and pass in the bodystrength
        // this must be implemented in each subclass.

        public abstract void TakeDamageAtPoint(Vector3 point, Vector3 direction, float damage);

        // if a collision reference is passed, instead of using one direction to determine offset, it will use collisions to 

        public virtual void TakeDamageAtPoint(Vector3 point, Vector3 direction, float damage, float bodyStrength)
        {
            for (int j = 0; j < meshes.Count; j++)
            {
                DestructableMeshInformation meshInfo = meshes[j];
                Vector3[] modifiedVertices = meshInfo.modifiedVertices;
                Vector3[] normalVertices = meshInfo.normalVertices;

                //print("Working on " + meshInfo.objectTransform.name);
                int numPointsModified = 0;
                for (int i = 0; i < modifiedVertices.Length; i++)
                {
                    Vector3 originalVertex = normalVertices[i];
                    Vector3 currentVertex = modifiedVertices[i];

                    float distanceToPoint = Vector3.Distance(currentVertex, meshInfo.objectTransform.InverseTransformPoint(point));
                    Vector3 collisionImpulseDirection = meshInfo.objectTransform.InverseTransformDirection(direction);


                    if (distanceToPoint < deformationRadius)
                    {
                        //print(i + " dp " + distanceToPoint);
                        numPointsModified++;
                        float amountToDeform = 1 - (distanceToPoint * deformationFallOff / deformationRadius);
                        amountToDeform *= deformationPerImpulse * damage / bodyStrength;

                        // print(amountToDeform);

                        Vector3 deformationPosition = collisionImpulseDirection * amountToDeform + currentVertex;

                        //Debug.DrawRay(transform.TransformPoint(currentVertex), collisionImpulseDirection * 0.9f * amountToDeform, Color.green, 30f);
                        //Debug.DrawRay(transform.TransformPoint(currentVertex) + collisionImpulseDirection * 0.9f * amountToDeform, collision.impulse.normalized * amountToDeform * 0.1f, Color.red, 30f);

                        float totalVertexDeform = Vector3.Distance(deformationPosition, originalVertex);


                        if (totalVertexDeform > maxDeform)
                        {
                            deformationPosition = collisionImpulseDirection * maxDeform + originalVertex;
                        }

                        float deltaDeform = Vector3.Distance(deformationPosition, modifiedVertices[i]);
                        totalDeformation += deltaDeform;
                        meshInfo.totalDeformation += deltaDeform;

                        // print("Mesh" + meshInfo.objectTransform.name + " vertex " + i + " was deformed + " + deltaDeform);

                        modifiedVertices[i] = deformationPosition;
                        if (!meshInfo.isMainMesh) continue;
                        
                        // Keep this a for loop, not a foreach loop, because we are modifying the collection
                        for (int k = 0; k < detachableMeshes.Count; k++)
                        {
                            DetachableMeshInformation detMeshInfo = detachableMeshes[k];
                            if (!(totalVertexDeform > detMeshInfo.deformNeededToLoseVertex)) continue;
                            
                            detMeshInfo.AttachedVertices.Remove(i);
                            detMeshInfo.currentVertices = detMeshInfo.AttachedVertices.Count;
                            if (detMeshInfo.currentVertices < detMeshInfo.maxAttachedVertices * detMeshInfo.percentVerticesNeeded)
                            {
                                LoseMesh(detMeshInfo);
                            }
                        }
                    }
                }

                meshInfo.mesh.vertices = modifiedVertices;
                //if (meshInfo.meshCollider)
                  //  meshInfo.meshCollider.sharedMesh = meshInfo.mesh;

                //print("Modified " + numPointsModified + " verticies");
            }
        }

        // goes through all of the destructible / detachable mesh info and removes them from their lists.
        // then destroys the object
        public void DeleteMesh(MeshFilter mesh)
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                if (meshes[i].meshFilter == mesh)
                {
                    meshes.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < detachableMeshes.Count; i++)
            {
                if (detachableMeshes[i].meshFilter == mesh)
                {
                    detachableMeshes.RemoveAt(i);
                    break;
                }
            }

            if (mesh)
            {
                Destroy(mesh.gameObject);
            }
        }

        // loses a detachable mesh

        protected virtual void LoseMesh(DetachableMeshInformation mesh)
        {
            mesh.objectTransform.parent = null;
            DestructableMeshInformation meshInfo = FindDmiFromMeshFilter(mesh.meshFilter);
            if (mesh.objectTransform.GetComponent<MeshCollider>())
            {
                Destroy(mesh.objectTransform.GetComponent<MeshCollider>());
            }

            mesh.objectTransform.AddComponent<MeshCollider>().convex = true;
            mesh.objectTransform.AddComponent<Rigidbody>();
            detachableMeshes.Remove(mesh);
            if (meshInfo != null)
            {
                meshes.Remove(meshInfo);
            }
        }

        private DestructableMeshInformation FindDmiFromMeshFilter(MeshFilter meshFilter)
        {
            return meshes.FirstOrDefault(mesh => mesh.meshFilter == meshFilter);
        }
    }

    [System.Serializable]
    public class DestructableMeshInformation
    {
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;
        public Mesh mesh;
        public Vector3[] normalVertices;
        public Vector3[] modifiedVertices;
        public Transform objectTransform;

        public float totalDeformation;

        // the main mesh will determine the connectivity of other meshes
        // for examples, wheels will be connected to the main mesh of the car and the
        // connection vertices will only be from that mesh.
        public bool isMainMesh;

        // if it can detach, then it will fall off if the areas around it are damaged;
        public bool isDetachable;

        public void ResetMesh()
        {
            mesh.vertices = normalVertices;
            modifiedVertices = mesh.vertices;
            meshCollider.sharedMesh = mesh;
        }
    }

    [System.Serializable]
    public class DetachableMeshInformation
    {
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;

        public HashSet<int> AttachedVertices;

        // how much each vertex needs to be deformed for it to be "lost"
        public float deformNeededToLoseVertex;

        // what percentage of vertices need to remain attached for this mesh to not detach
        // 0.5 = 50%
        public float percentVerticesNeeded;
        public int maxAttachedVertices;

        public Transform objectTransform;
        public float currentVertices;
    }
}