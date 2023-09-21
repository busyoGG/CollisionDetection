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
    OBB,
    Circle,
    Circle2AABB,
    Circle2OBB,
    Line2Circle,
    Line2AABB,
    Line2OBB
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
        }
    }

    //----- AABB ----- start

    /// <summary>
    /// AABB���
    /// </summary>
    private void CollisionAABB()
    {
        //��Χ��1����Сֵ�Ȱ�Χ��2�����ֵ���� �� ��Χ��1�����ֵ�Ȱ�Χ��2����Сֵ��С ����ײ
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
    /// SAT��������ײ���֮OBB���
    /// </summary>
    private void CollisionOBB()
    {
        //������OBB��Χ��֮������������ķ�ƽ���� ��9��
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
                //��һ�����ཻ���˳�
                line1.Collided(false);
                line2.Collided(false);
                return;
            }
        }
        line1.Collided(true);
        line2.Collided(true);
    }

    /// <summary>
    /// ����ͶӰ�Ƿ��ཻ
    /// </summary>
    /// <param name="vertexs1"></param>
    /// <param name="vertexs2"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private bool NotInteractiveOBB(Vector3[] vertexs1, Vector3[] vertexs2, Vector3 axis)
    {
        //����OBB��Χ���ڷ������ϵ�ͶӰ����ֵ
        float[] limit1 = GetProjectLimit(vertexs1, axis);
        float[] limit2 = GetProjectLimit(vertexs2, axis);
        //������Χ�м���ֵ���ཻ������ײ
        return limit1[0] > limit2[1] || limit2[0] > limit1[1];
    }

    /// <summary>
    /// ���㶥��ͶӰ����ֵ
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

    //----- Circle ----- start
    /// <summary>
    /// ��������
    /// </summary>
    private void CollisionCircle()
    {
        //��������뾶��
        float totalRadius = Mathf.Pow(data1.radius + data2.radius, 2);
        //����������֮��ľ���
        float distance = (data1.center - data2.center).sqrMagnitude;
        //����С�ڵ��ڰ뾶������ײ
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
    /// ����AABB���
    /// </summary>
    private void CollisionCircle2AABB()
    {
        //��������
        Vector3 center = data1.center;
        Vector3 nearP = Vector3.zero;
        nearP.x = Mathf.Clamp(center.x, data2.min.x, data2.max.x);
        nearP.y = Mathf.Clamp(center.y, data2.min.y, data2.max.y);
        nearP.z = Mathf.Clamp(center.z, data2.min.z, data2.max.z);
        //�������������ĵľ���
        float distance = (nearP - center).sqrMagnitude;
        float radius = Mathf.Pow(data1.radius, 2);
        //����С�ڰ뾶����ײ
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
    /// ����OBB���
    /// </summary>
    private void CollisionCircle2OBB()
    {
        //�������
        Vector3 nearP = GetClosestPointOBB();
        //��AABB���ԭ����ͬ
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
    /// ��ȡһ�㵽OBB�������
    /// </summary>
    /// <returns></returns>
    private Vector3 GetClosestPointOBB()
    {
        Vector3 nearP = data2.center;
        //��������OBB���ĵľ������� ��OBB����ָ������
        Vector3 center1 = data1.center;
        Vector3 center2 = data2.center;
        Vector3 dist = center1 - center2;

        float[] extents = new float[3] { data2.extents.x, data2.extents.y, data2.extents.z };
        Vector3[] axes = data2.axes;

        for (int i = 0; i < 3; i++)
        {
            //�������������OBB�������ͶӰ���� ������������OBB����ϵ�еĶ�Ӧ������ĳ���
            float distance = Vector3.Dot(dist, axes[i]);
            distance = Mathf.Clamp(distance, -extents[i], extents[i]);
            //��ԭ����������
            nearP.x += distance * axes[i].x;
            nearP.y += distance * axes[i].y;
            nearP.z += distance * axes[i].z;
        }
        //ConsoleUtils.Log("�����", nearP);
        return nearP;
    }

    //----- Circle ----- end

    //----- Ray ----- start

    /// <summary>
    /// ���ߺ�����
    /// </summary>
    private void CollisionRay2Circle()
    {
        Vector3 centerDis = data2.center - data1.center;
        Vector3 direction = data1.direction;

        float projection = Vector3.Dot(centerDis, direction);
        float r2 = Mathf.Pow(data2.radius, 2);
        float f = Mathf.Pow(projection, 2) + r2 - centerDis.sqrMagnitude;

        //�����෴
        bool checkDirection = projection < 0;
        //���߹���
        bool checkDistance = centerDis.sqrMagnitude > Mathf.Pow(data1.radius + data2.radius, 2);
        //������������ڲ�
        bool checkInside = centerDis.sqrMagnitude < r2;
        //���ཻ
        bool checkNotCollide = f < 0;

        if (checkDirection || checkDistance || checkInside || checkNotCollide)
        {
            line1.Collided(false);
            line2.Collided(false);
            return;
        }

        line1.Collided(true);
        line2.Collided(true);

        float dir = projection - Mathf.Sqrt(f);
        Vector3 point = data1.center + data1.direction * dir;
        ConsoleUtils.Log("��ײ��", point);
    }

    private void CollisionRay2AABB()
    {
        //�жϷ������
        if (Vector3.Dot(data2.center - data1.center,data1.direction) < 0)
        {
            line1.Collided(false);
            line2.Collided(false);
            return;
        }

        //�жϷ�ƽ�����
        Vector3 min = data2.min - data1.center;
        Vector3 max = data2.max - data1.center;

        Vector3 projection = new Vector3(1 / data1.direction.x, 1 / data1.direction.y, 1 / data1.direction.z);

        Vector3 pMin = Vector3.Scale(min, projection);
        Vector3 pMax = Vector3.Scale(max, projection);

        if (data1.direction.x < 0) Swap(ref pMin.x, ref pMax.x);
        if (data1.direction.y < 0) Swap(ref pMin.y, ref pMax.y);
        if (data1.direction.z < 0) Swap(ref pMin.z, ref pMax.z);


        float n = Mathf.Max(pMin.x,pMin.y,pMin.z);
        float f = Mathf.Min(pMax.x,pMax.y,pMax.z);

        if(n < f)
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

        ConsoleUtils.Log("��ײ��", point);
    }

    //----- Ray ----- end

    //TODO GJK���

    //���ߺ���

    private void Swap(ref float one, ref float two)
    {
        float temp;
        temp = one;
        one = two;
        two = temp;
    }
}
