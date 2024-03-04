
using Game;
using JetBrains.Annotations;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LineDrawer : MonoBehaviour
{
    private MeshRenderer _mesh;
    //[SerializeField]
    //private Color _color = Color.white;
    [SerializeField]
    private float _length = 0.5f;

    private float _curLength = 0.5f;
    [SerializeField]
    private float _extentsRatio = 0.51f;

    private float _curExtentsRatio = 0.51f;

    /// <summary>
    /// 画线器
    /// </summary>
    //private LineRenderer _line;

    /// <summary>
    /// 碰撞类型
    /// </summary>
    private collision _type = collision.AABB;

    private collision _curType = collision.AABB;

    /// <summary>
    /// 碰撞检测脚本
    /// </summary>
    private Collision _collisionScript = null;

    //方块属性

    private Vector3 _pos = Vector3.zero;

    private Quaternion _rot = Quaternion.identity;

    private Vector3 _scale = Vector3.zero;

    private CollisionData data = null;

    public bool changed { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        //初始化大小
        _mesh = this.GetComponent<MeshRenderer>();
        _collisionScript = GameObject.Find("Root").GetComponent<Collision>();

        //_line = this.gameObject.AddComponent<LineRenderer>();
        //Material mat = Resources.Load<Material>("Materials/white_unlit");
        //_line.loop = true;
        //_line.sharedMaterial = mat;
        //Debug.Log("加载材质" + mat.name);

        data = GetComponent<CollisionData>();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    _type = _collisionScript.GetCollision();
    //    CheckChange();
    //    DrawBox();
    //}

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            _type = _collisionScript.GetCollision();
            CheckChange();
            SetBox();
            DrawBox();
        }
    }


    private void CheckChange()
    {
        if (_pos != this.transform.position || _rot != this.transform.rotation || _scale != this.transform.localScale || _extentsRatio != _curExtentsRatio ||
            _curType != _type || _length != _curLength)
        {
            _pos = this.transform.position;
            _rot = this.transform.rotation;
            _scale = this.transform.localScale;
            _curExtentsRatio = _extentsRatio;
            _curType = _type;
            _curLength = _length;
            changed = true;
        }
        else
        {
            changed = false;
        }
    }

    private void SetBox()
    {
        if (!changed)
        {
            return;
        }
        //_line.material.color = _color;


        //_line.loop = false;
        switch (_type)
        {
            case collision.OBB:
                SetOBB();
                break;
            case collision.Circle:
                SetCircle();
                break;
            case collision.Circle2AABB:
                if (tag == "Change")
                {
                    SetCircle();
                }
                else
                {
                    SetAABB();
                }
                break;
            case collision.Circle2OBB:
                if (tag == "Change")
                {
                    SetCircle();
                }
                else
                {
                    SetOBB();
                }
                break;
            case collision.Line2Circle:
                if (tag == "Change")
                {
                    SetLine();
                }
                else
                {
                    SetCircle();
                }
                break;
            case collision.Line2AABB:
                if (tag == "Change")
                {
                    SetLine();
                }
                else
                {
                    SetAABB();
                }
                break;
            case collision.Line2OBB:
                if (tag == "Change")
                {
                    SetLine();
                }
                else
                {
                    SetOBB();
                }
                break;
            case collision.Capsule:
                SetCapsule();
                break;
            case collision.AABB:
            default:
                SetAABB();
                break;
        }
    }

    private void DrawBox()
    {
        Gizmos.color = Color.red;
        //_line.loop = false;
        switch (_type)
        {
            case collision.OBB:
                DrawCube(true);
                break;
            case collision.Circle:
                DrawSphere();
                break;
            case collision.Circle2AABB:
                if (tag == "Change")
                {
                    DrawSphere();
                }
                else
                {
                    DrawCube();
                }
                break;
            case collision.Circle2OBB:
                if (tag == "Change")
                {
                    DrawSphere();
                }
                else
                {
                    DrawCube(true);
                }
                break;
            case collision.Line2Circle:
                if (tag == "Change")
                {
                    DrawLine();
                }
                else
                {
                    DrawSphere();
                }
                break;
            case collision.Line2AABB:
                if (tag == "Change")
                {
                    DrawLine();
                }
                else
                {
                    DrawCube();
                }
                break;
            case collision.Line2OBB:
                if (tag == "Change")
                {
                    DrawLine();
                }
                else
                {
                    DrawCube(true);
                }
                break;
            case collision.Capsule:
                DrawCapsule();
                break;
            case collision.AABB:
            default:
                DrawCube();
                break;
        }
    }

    private void SetAABB()
    {
        Vector3 center;
        Vector3 extents;

        Vector3 max;
        Vector3 min;

        center = _mesh.bounds.center;
        extents = _mesh.bounds.size * _curExtentsRatio;

        max = center + extents;
        min = center - extents;

        data.max = max;
        data.min = min;

        data.center = center;
        data.extents = extents;

        data.axes[0] = transform.right;
        data.axes[1] = transform.up;
        data.axes[2] = transform.forward;
    }

    private void SetOBB()
    {
        Vector3 center;
        Vector3 extents;

        Vector3 max;
        Vector3 min;

        //8个点顶点
        Vector3 A;
        Vector3 B;
        Vector3 C;
        Vector3 D;
        Vector3 A1;
        Vector3 B1;
        Vector3 C1;
        Vector3 D1;

        center = _mesh.bounds.center;
        extents = this.transform.localScale * _curExtentsRatio;

        Quaternion rotation = this.transform.rotation;

        data.rotation = rotation;

        max = extents;
        min = -extents;

        //8个点顶点
        A = rotation * new Vector3(min.x, max.y, min.z) + center;
        B = rotation * new Vector3(min.x, max.y, max.z) + center;
        C = rotation * new Vector3(max.x, max.y, max.z) + center;
        D = rotation * new Vector3(max.x, max.y, min.z) + center;
        A1 = rotation * new Vector3(min.x, min.y, min.z) + center;
        B1 = rotation * new Vector3(min.x, min.y, max.z) + center;
        C1 = rotation * new Vector3(max.x, min.y, max.z) + center;
        D1 = rotation * new Vector3(max.x, min.y, min.z) + center;

        data.vertexts[0] = A;
        data.vertexts[1] = B;
        data.vertexts[2] = C;
        data.vertexts[3] = D;
        data.vertexts[4] = A1;
        data.vertexts[5] = B1;
        data.vertexts[6] = C1;
        data.vertexts[7] = D1;

        data.extents = extents;

        data.axes[0] = transform.right;
        data.axes[1] = transform.up;
        data.axes[2] = transform.forward;

        data.center = center;
    }

    private void SetCircle()
    {
        Vector3 center;
        Vector3 extents;

        center = _mesh.bounds.center;
        extents = this.transform.localScale * _curExtentsRatio;

        //Mathf.Sqrt(3)
        float radius = 1.732f * extents.x;
        data.radius = radius;
        data.center = center;
    }

    private void SetLine()
    {
        Vector3 center = transform.position;
        Vector3 direction = transform.rotation * Vector3.right;
        float radius = _curExtentsRatio * 10;

        data.radius = radius;
        data.center = center;
        data.direction = direction;

    }

    private void SetCapsule()
    {
        Vector3 center = transform.position;
        float radius = _curExtentsRatio * 1.5f;
        Vector3 direction = transform.rotation * Vector3.up;
        Vector3 extents = new Vector3(0, _length, 0);

        data.center = center;
        data.radius = radius;
        data.direction = direction;
        data.extents = extents;
        data.rotation = this.transform.rotation;
    }

    public void DrawLine()
    {
        Gizmos.DrawLine(data.center, data.center + data.direction * data.radius);
    }

    public void DrawCube(bool isObb = false)
    {
        if (isObb)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(data.center, data.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, data.extents * 2);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            Gizmos.DrawWireCube(data.center, data.extents * 2);
        }
    }

    public void DrawSphere()
    {
        Gizmos.DrawWireSphere(data.center, data.radius);
    }

    void DrawCapsule()
    {
        Vector3 pos = data.center;
        Quaternion rot = data.rotation;
        float height = data.extents.y * 2;
        float radius = data.radius;
        // Calculate points for upper and lower hemispheres
        Vector3 upCenter = pos + rot * Vector3.up * (height * 0.5f);
        Vector3 downCenter = pos + rot * Vector3.down * (height * 0.5f);

        // Draw upper hemisphere
        DrawHemisphere(upCenter, rot, radius, true);

        // Draw lower hemisphere
        DrawHemisphere(downCenter, rot, radius, false);

        // Draw connecting lines
        DrawConnectingLines(upCenter, downCenter, rot, radius);
    }

    void DrawHemisphere(Vector3 center, Quaternion rot, float radius, bool top)
    {
        const int numSegments = 36; // Number of segments for drawing the hemisphere
        float angleStep = 360f / numSegments;

        Vector3 lastPos = Vector3.zero;
        Vector3 lastPosCircle = Vector3.zero;

        for (int i = 0; i < numSegments; i++)
        {
            float angle = i * angleStep;
            Vector3 point = center + rot * Quaternion.Euler(0, angle, 0) * Vector3.right * radius;
            Gizmos.DrawLine(point, center);

            if (i > 0)
            {
                Gizmos.DrawLine(lastPosCircle, point);
            }
            lastPosCircle = point;

            Quaternion rotQ = Quaternion.Euler(0, angle, 0);

            for (int j = 0; j < numSegments * 0.5; j++)
            {
                float angleY = top ? AngleToRad(j * angleStep) : AngleToRad(j * -angleStep);
                Vector3 nextPos = center + rot * rotQ * new Vector3(Mathf.Cos(angleY) * radius, Mathf.Sin(angleY) * radius, 0);
                if (j > 0)
                {
                    Gizmos.DrawLine(lastPos, nextPos);
                }
                lastPos = nextPos;
            }
        }
    }

    private float AngleToRad(float angle)
    {
        return Mathf.Deg2Rad * angle;
    }

    void DrawConnectingLines(Vector3 upCenter, Vector3 downCenter, Quaternion rot, float radius)
    {
        Vector3[] points = new Vector3[4];
        const int numSegments = 36; // Number of segments for drawing the hemisphere
        float angleStep = 360f / numSegments;

        for (int i = 0; i < numSegments; i++)
        {
            float angle = i * angleStep;
            points[0] = upCenter + rot * Quaternion.Euler(0, angle, 0) * Vector3.right * radius;
            points[1] = downCenter + rot * Quaternion.Euler(0, angle, 0) * Vector3.right * radius;

            //points[2] = upCenter + rot * Quaternion.Euler(0, angle + 90, 0) * Vector3.right * radius;
            //points[3] = downCenter + rot * Quaternion.Euler(0, angle + 90, 0) * Vector3.right * radius;

            Gizmos.DrawLine(points[0], points[1]);
            //Gizmos.DrawLine(points[1], points[3]);
            //Gizmos.DrawLine(points[3], points[2]);
            //Gizmos.DrawLine(points[2], points[0]);
        }
    }

    /// <summary>
    /// 是否碰撞
    /// </summary>
    /// <param name="b"></param>
    public void Collided(bool b)
    {
        if (b)
        {
            _mesh.material.color = Color.red;
        }
        else
        {
            _mesh.material.color = Color.white;
        }
    }
}
