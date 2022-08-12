using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDeformationSystem : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;

    [SerializeField] private MeshCollider meshCollider;

    [Header("Max distance between vertex and collision point")]
    [SerializeField] private float maxDamageDistance = 0.75f;

    [Header("Damage power")]
    [SerializeField] private float damageCoefficient = 1.5f;

    [Header("Body resistance to damage")]
    [Range(1f, 100f)]
    [SerializeField] private float damageOpposition = 1f;

    [SerializeField] private DeformationQuality deformationQuality;

    [Header("Degrades performance")]
    [SerializeField] private bool recalculateMeshCollider = true;

    private void Awake() => meshFilter.mesh.MarkDynamic();

    private void OnCollisionEnter(Collision collision){
        Vector3 localContactPoint = transform.InverseTransformPoint(collision.contacts[0].point);
        Vector3 localForce = transform.InverseTransformDirection(collision.impulse * 0.000035f);
        Deformation(collision, localContactPoint, localForce);
    }

    public void Deformation(Collision collision, Vector3 localContactPoint, Vector3 localForce){
        Vector3[] targetVertices = meshFilter.mesh.vertices;
        for (int i = 0; i < targetVertices.Length; i++)
        {
            float distance = (localContactPoint - targetVertices[i]).magnitude;
            if(distance <= maxDamageDistance){
                switch(deformationQuality){
                    case DeformationQuality.Quality:
                    targetVertices[i] += localForce * (maxDamageDistance - distance) * damageCoefficient;
                    break;

                    case DeformationQuality.Performance:
                    targetVertices[i] += localForce / damageOpposition * (damageCoefficient - distance);
                    break;
                }
            }
        }
        meshFilter.mesh.vertices = targetVertices;
        if(recalculateMeshCollider) meshCollider.sharedMesh = meshFilter.mesh;
        RecalculateMesh();
    }

    public void RecalculateMesh(){
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
    }
}
public enum DeformationQuality{ Quality, Performance }
