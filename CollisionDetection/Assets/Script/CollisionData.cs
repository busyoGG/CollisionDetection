using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionData : MonoBehaviour
{
    public Vector3 max = Vector3.zero;
    public Vector3 min = Vector3.zero;
    public Vector3[] vertexts = new Vector3[8];
    public Vector3[] axes = new Vector3[3];
}
