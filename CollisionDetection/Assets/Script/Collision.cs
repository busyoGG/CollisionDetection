using Game;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum collision
{
    AABB,
    OBB
}

public class Collision : MonoBehaviour
{
    [SerializeField]
    private GameObject obj1;
    [SerializeField]
    private GameObject obj2;

    private CollisionData data1 = null;
    private CollisionData data2 = null;

    private LineDrawer line1 = null;
    private LineDrawer line2 = null;

    [SerializeField]
    collision _collision = collision.AABB;


    // Start is called before the first frame update
    void Start()
    {
        data1 = obj1.GetComponent<CollisionData>();
        data2 = obj2.GetComponent<CollisionData>();

        line1 = obj1.GetComponent<LineDrawer>();
        line2 = obj2.GetComponent<LineDrawer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (line1.changed || line2.changed)
        {
            ChangeCollision(_collision);
        }
    }


    public collision GetCollision()
    {
        return _collision;
    }

    private void ChangeCollision(collision type)
    {
        switch (type)
        {
            case collision.AABB:
                CollisionAABB();
                break;
            case collision.OBB:
                CollisionOBB();
                break;
        }
    }

    //----- AABB ----- start

    /// <summary>
    /// AABB碰撞
    /// </summary>
    private void CollisionAABB()
    {
        if (data1.max.x < data2.min.x || data1.max.y < data2.min.y || data1.max.z < data2.min.z ||
            data1.min.x > data2.max.x || data1.min.y > data2.max.y || data1.min.z > data2.max.z)
        {
            line1.Collided(false);
            line2.Collided(false);
        }
        else
        {
            line1.Collided(true);
            line2.Collided(true);
        }
    }

    //----- AABB ----- end

    //----- OBB ----- start

    /// <summary>
    /// SAT分离轴碰撞检测之OBB检测
    /// </summary>
    private void CollisionOBB()
    {
        //求两轴之间法平面轴
        int len1 = data1.axes.Length;
        int len2 = data2.axes.Length;
        Vector3[] axes = new Vector3[len1 + len2 + len1 * len2];
        int k = 0;
        int initJ = len2;
        for (int i = 0; i < len1; i++)
        {
            axes[k++] = data1.axes[i];
            for (int j = 0; j < len2; j++)
            {
                if (initJ > 0)
                {
                    initJ--;
                    axes[k++] = data2.axes[j];
                }
                axes[k++] = Vector3.Cross(data1.axes[i], data2.axes[j]);
            }
        }


        for (int i = 0, len = axes.Length; i < len; i++)
        {
            if (NotInteractiveObb(data1.vertexts, data2.vertexts, axes[i]))
            {
                //有一个不相交就退出
                line1.Collided(false);
                line2.Collided(false);
                return;
            }
        }
        line1.Collided(true);
        line2.Collided(true);
    }

    /// <summary>
    /// 计算投影是否相交
    /// </summary>
    /// <param name="vertexs1"></param>
    /// <param name="vertexs2"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private bool NotInteractiveObb(Vector3[] vertexs1, Vector3[] vertexs2, Vector3 axis)
    {
        float[] limit1 = GetProjectLimit(vertexs1, axis);
        float[] limit2 = GetProjectLimit(vertexs2, axis);
        return limit1[0] > limit2[1] || limit2[0] > limit1[1];
    }

    /// <summary>
    /// 计算顶点投影极限值
    /// </summary>
    /// <param name="vertexts"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private float[] GetProjectLimit(Vector3[] vertexts, Vector3 axis)
    {
        float[] result = new float[2] { float.MaxValue, float.MinValue };
        for (int i = 0, len = vertexts.Length; i < len; i++)
        {
            Vector3 vertext = vertexts[i];
            float dot = Vector3.Dot(vertext, axis);
            result[0] = Mathf.Min(dot, result[0]);
            result[1] = Mathf.Max(dot, result[1]);
        }
        return result;
    }
    //----- OBB ----- end

    //TODO 圆与圆

    //TODO 圆与AABB

    //TODO 圆与OBB

    //TODO 射线检测

    //TODO GJK检测
}
