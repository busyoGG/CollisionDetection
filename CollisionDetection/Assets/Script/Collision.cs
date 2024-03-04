using Game;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum collision
{
    AABB,
    OBB,
    Circle,
    Circle2AABB,
    Circle2OBB,
    Line2Circle,
    Line2AABB,
    Line2OBB,
    Capsule
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
            case collision.Circle:
                CollisionCircle();
                break;
            case collision.Circle2AABB:
                CollisionCircle2AABB();
                break;
            case collision.Circle2OBB:
                CollisionCircle2OBB();
                break;
            case collision.Line2Circle:
                CollisionRay2Circle();
                break;
            case collision.Line2AABB:
                CollisionRay2AABB();
                break;
            case collision.Line2OBB:
                CollisionRay2OBB();
                break;
            case collision.Capsule:
                CollisionCapsule();
                break;
        }
    }

    //----- AABB ----- start

    /// <summary>
    /// AABB检测
    /// </summary>
    private void CollisionAABB()
    {
        //包围盒1的最小值比包围盒2的最大值还大 或 包围盒1的最大值比包围盒2的最小值还小 则不碰撞
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
        //求两个OBB包围盒之间两两坐标轴的法平面轴 共9个
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
            if (NotInteractiveOBB(data1.vertexts, data2.vertexts, axes[i]))
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
    /// 计算投影是否不相交
    /// </summary>
    /// <param name="vertexs1"></param>
    /// <param name="vertexs2"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private bool NotInteractiveOBB(Vector3[] vertexs1, Vector3[] vertexs2, Vector3 axis)
    {
        //计算OBB包围盒在分离轴上的投影极限值
        float[] limit1 = GetProjectionLimit(vertexs1, axis);
        float[] limit2 = GetProjectionLimit(vertexs2, axis);
        //两个包围盒极限值不相交，则不碰撞
        return limit1[0] > limit2[1] || limit2[0] > limit1[1];
    }

    /// <summary>
    /// 计算顶点投影极限值
    /// </summary>
    /// <param name="vertexts"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private float[] GetProjectionLimit(Vector3[] vertexts, Vector3 axis)
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

    //----- Circle ----- start
    /// <summary>
    /// 球与球检测
    /// </summary>
    private void CollisionCircle()
    {
        //求两个球半径和
        float totalRadius = Mathf.Pow(data1.radius + data2.radius, 2);
        //球两个球心之间的距离
        float distance = (data1.center - data2.center).sqrMagnitude;
        //距离小于等于半径和则碰撞
        if (distance <= totalRadius)
        {
            line1.Collided(true);
            line2.Collided(true);
        }
        else
        {
            line1.Collided(false);
            line2.Collided(false);
        }
    }

    /// <summary>
    /// 球与AABB检测
    /// </summary>
    private void CollisionCircle2AABB()
    {
        //求出最近点
        Vector3 center = data1.center;
        Vector3 nearP = GetClosestPointAABB();
        //求出最近点与球心的距离
        float distance = (nearP - center).sqrMagnitude;
        float radius = Mathf.Pow(data1.radius, 2);
        //距离小于半径则碰撞
        if (distance <= radius)
        {
            line1.Collided(true);
            line2.Collided(true);
        }
        else
        {
            line1.Collided(false);
            line2.Collided(false);
        }
    }

    /// <summary>
    /// 获得一点到AABB最近点
    /// </summary>
    /// <returns></returns>
    private Vector3 GetClosestPointAABB()
    {
        Vector3 center = data1.center;
        Vector3 nearP = Vector3.zero;
        nearP.x = Mathf.Clamp(center.x, data2.min.x, data2.max.x);
        nearP.y = Mathf.Clamp(center.y, data2.min.y, data2.max.y);
        nearP.z = Mathf.Clamp(center.z, data2.min.z, data2.max.z);
        return nearP;
    }

    /// <summary>
    /// 球与OBB检测
    /// </summary>
    private void CollisionCircle2OBB()
    {
        //求最近点
        Vector3 nearP = GetClosestPointOBB();
        //与AABB检测原理相同
        float distance = (nearP - data1.center).sqrMagnitude;
        float radius = Mathf.Pow(data1.radius, 2);
        if (distance <= radius)
        {
            line1.Collided(true);
            line2.Collided(true);
        }
        else
        {
            line1.Collided(false);
            line2.Collided(false);
        }
    }

    /// <summary>
    /// 获取一点到OBB的最近点
    /// </summary>
    /// <returns></returns>
    private Vector3 GetClosestPointOBB()
    {
        Vector3 nearP = data2.center;
        //求球心与OBB中心的距离向量 从OBB中心指向球心
        Vector3 center1 = data1.center;
        Vector3 center2 = data2.center;
        Vector3 dist = center1 - center2;

        float[] extents = new float[3] { data2.extents.x, data2.extents.y, data2.extents.z };
        Vector3[] axes = data2.axes;

        for (int i = 0; i < 3; i++)
        {
            //计算距离向量到OBB坐标轴的投影长度 即距离向量在OBB坐标系中的对应坐标轴的长度
            float distance = Vector3.Dot(dist, axes[i]);
            distance = Mathf.Clamp(distance, -extents[i], extents[i]);
            //还原到世界坐标
            nearP.x += distance * axes[i].x;
            nearP.y += distance * axes[i].y;
            nearP.z += distance * axes[i].z;
        }
        return nearP;
    }

    //----- Circle ----- end

    //----- Ray ----- start

    /// <summary>
    /// 射线和球检测
    /// </summary>
    private void CollisionRay2Circle()
    {

        Vector3 centerDis = data2.center - data1.center;
        Vector3 direction = data1.direction;

        float projection = Vector3.Dot(centerDis, direction);
        float r2 = Mathf.Pow(data2.radius, 2);
        float f = Mathf.Pow(projection, 2) + r2 - centerDis.sqrMagnitude;

        //方向相反
        bool checkDirection = projection < 0;
        //射线过短
        bool checkDistance = centerDis.sqrMagnitude > Mathf.Pow(data1.radius + data2.radius, 2);
        //射线起点在球内部
        bool checkNotInside = centerDis.sqrMagnitude > r2;
        //不相交
        bool checkNotCollide = f < 0;

        if (checkNotInside && (checkDirection || checkDistance || checkNotCollide))
        {
            line1.Collided(false);
            line2.Collided(false);
            return;
        }

        line1.Collided(true);
        line2.Collided(true);

        float dis = projection - Mathf.Sqrt(f) * (checkNotInside ? 1 : -1);
        Vector3 point = data1.center + data1.direction * dis;
        ConsoleUtils.Log("碰撞点", point);
    }

    private void CollisionRay2AABB()
    {
        //判断是否不在AABB内
        bool checkNotInside = data1.center.x > data2.max.x || data1.center.x < data2.min.x ||
            data1.center.y > data2.max.y || data1.center.y < data2.min.y ||
            data1.center.z > data2.max.z || data1.center.z < data2.min.z;
        //判断反向情况
        bool checkForawd = Vector3.Dot(data2.center - data1.center, data1.direction) < 0;
        if (checkNotInside && checkForawd)
        {
            line1.Collided(false);
            line2.Collided(false);
            return;
        }

        //判断是否相交
        Vector3 min = data2.min - data1.center;
        Vector3 max = data2.max - data1.center;

        Vector3 projection = new Vector3(1 / data1.direction.x, 1 / data1.direction.y, 1 / data1.direction.z);

        Vector3 pMin = Vector3.Scale(min, projection);
        Vector3 pMax = Vector3.Scale(max, projection);

        if (data1.direction.x < 0) Swap(ref pMin.x, ref pMax.x);
        if (data1.direction.y < 0) Swap(ref pMin.y, ref pMax.y);
        if (data1.direction.z < 0) Swap(ref pMin.z, ref pMax.z);

        float n = Mathf.Max(pMin.x, pMin.y, pMin.z);
        float f = Mathf.Min(pMax.x, pMax.y, pMax.z);

        if (!checkNotInside)
        {
            line1.Collided(true);
            line2.Collided(true);
            Vector3 point = data1.center + data1.direction * f;

            ConsoleUtils.Log("碰撞点", point);
        }
        else
        {
            if (n < f && data1.radius >= n)
            {
                line1.Collided(true);
                line2.Collided(true);
            }
            else
            {
                line1.Collided(false);
                line2.Collided(false);
                return;
            }

            Vector3 point = data1.center + data1.direction * n;

            ConsoleUtils.Log("碰撞点", point);
        }
    }

    private void CollisionRay2OBB()
    {
        //判断不在OBB内
        Vector3 centerDis = data1.center - data2.center;
        float ray2ObbX = Vector3.Dot(centerDis, data2.axes[0]);
        float ray2ObbY = Vector3.Dot(centerDis, data2.axes[1]);
        float ray2ObbZ = Vector3.Dot(centerDis, data2.axes[2]);
        bool checkNotInside = ray2ObbX < -data2.extents[0] || ray2ObbX > data2.extents[0] ||
            ray2ObbY < -data2.extents[1] || ray2ObbY > data2.extents[1] ||
            ray2ObbZ < -data2.extents[2] || ray2ObbZ > data2.extents[2];
        //判断反向情况
        bool checkFoward = Vector3.Dot(data2.center - data1.center, data1.direction) < 0;
        if (checkNotInside && checkFoward)
        {
            line1.Collided(false);
            line2.Collided(false);
            return;
        }

        //判断是否相交
        Vector3 min = Vector3.zero;
        Vector3 minP = data2.vertexts[4] - data1.center;
        min.x = Vector3.Dot(minP, data2.axes[0]);
        min.y = Vector3.Dot(minP, data2.axes[1]);
        min.z = Vector3.Dot(minP, data2.axes[2]);

        Vector3 max = Vector3.zero;
        Vector3 maxP = data2.vertexts[2] - data1.center;
        max.x = Vector3.Dot(maxP, data2.axes[0]);
        max.y = Vector3.Dot(maxP, data2.axes[1]);
        max.z = Vector3.Dot(maxP, data2.axes[2]);


        Vector3 projection = Vector3.zero;
        projection.x = 1 / Vector3.Dot(data1.direction, data2.axes[0]);
        projection.y = 1 / Vector3.Dot(data1.direction, data2.axes[1]);
        projection.z = 1 / Vector3.Dot(data1.direction, data2.axes[2]);

        Vector3 pMin = Vector3.Scale(min, projection);
        Vector3 pMax = Vector3.Scale(max, projection);

        if (projection.x < 0) Swap(ref pMin.x, ref pMax.x);
        if (projection.y < 0) Swap(ref pMin.y, ref pMax.y);
        if (projection.z < 0) Swap(ref pMin.z, ref pMax.z);


        float n = Mathf.Max(pMin.x, pMin.y, pMin.z);
        float f = Mathf.Min(pMax.x, pMax.y, pMax.z);

        Debug.Log(n + " " + f);
        Debug.Log(pMin + " " + pMax);
        Debug.Log(projection);

        if (!checkNotInside)
        {
            line1.Collided(true);
            line2.Collided(true);
            Vector3 point = data1.center + data1.direction * f;

            ConsoleUtils.Log("碰撞点", point);
        }
        else
        {
            if (n < f && data1.radius >= n)
            {
                line1.Collided(true);
                line2.Collided(true);
            }
            else
            {
                line1.Collided(false);
                line2.Collided(false);
                return;
            }

            Vector3 point = data1.center + data1.direction * n;

            ConsoleUtils.Log("碰撞点", point);
        }
    }

    //----- Ray ----- end

    //----- Capsule ----- start

    private void CollisionCapsule()
    {
        //计算头尾点最值
        Vector3 pointA1 = data1.center + data1.direction * data1.extents.y;
        Vector3 pointA2 = data1.center - data1.direction * data1.extents.y;

        Vector3 pointB1 = data2.center + data2.direction * data2.extents.y;
        Vector3 pointB2 = data2.center - data2.direction * data2.extents.y;

        Vector3 closest1;

        if ((pointA1 - data2.center).magnitude <= (pointA2 - data2.center).magnitude)
        {
            closest1 = pointA1;
        }
        else
        {
            closest1 = pointA2;
        }

        Vector3 closest2 = GetClosestPointOnLineSegment(pointB1, pointB2, closest1);
        closest1 = GetClosestPointOnLineSegment(pointA1, pointA2, closest2);

        //求两个球半径和
        float totalRadius = Mathf.Pow(data1.radius + data2.radius, 2);
        //球两个球心之间的距离
        float distance = (closest1 - closest2).sqrMagnitude;
        //距离小于等于半径和则碰撞
        if (distance <= totalRadius)
        {
            line1.Collided(true);
            line2.Collided(true);
        }
        else
        {
            line1.Collided(false);
            line2.Collided(false);
        }
    }



    //----- Capsule ----- end

    //TODO GJK检测

    //工具函数

    private void Swap(ref float one, ref float two)
    {
        float temp;
        temp = one;
        one = two;
        two = temp;
    }

    private Vector3 GetClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 line = end - start;
        //dot line line 求长度平方
        float ratio = Vector3.Dot(point - start, line) / Vector3.Dot(line, line);
        ratio = Mathf.Min(Mathf.Max(ratio, 0), 1);
        return start + ratio * line;
    }
}
