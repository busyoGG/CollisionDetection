
using Game;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LineDrawer : MonoBehaviour
{
    private MeshRenderer _mesh;
    [SerializeField]
    private Color _color = Color.white;
    [SerializeField]
    private float _width = 0.02f;
    [SerializeField]
    private float _extentsRatio = 0.51f;

    private float _curExtentsRatio = 0.51f;

    /// <summary>
    /// 画线器
    /// </summary>
    private LineRenderer _line;

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

        _line = this.gameObject.AddComponent<LineRenderer>();
        Material mat = Resources.Load<Material>("Materials/white_unlit");
        _line.loop = true;
        _line.sharedMaterial = mat;
        Debug.Log("加载材质" + mat.name);

        data = GetComponent<CollisionData>();
    }

    // Update is called once per frame
    void Update()
    {
        _type = _collisionScript.GetCollision();
        CheckChange();
        DrawBox();
    }

    private void CheckChange()
    {
        if (_pos != this.transform.position || _rot != this.transform.rotation || _scale != this.transform.localScale || _extentsRatio != _curExtentsRatio ||
            _curType != _type)
        {
            _pos = this.transform.position;
            _rot = this.transform.rotation;
            _scale = this.transform.localScale;
            _curExtentsRatio = _extentsRatio;
            _curType = _type;
            changed = true;
        }
        else
        {
            changed = false;
        }
    }

    private void DrawBox()
    {
        if (!changed)
        {
            return;
        }
        _line.material.color = _color;


        _line.loop = false;
        switch (_type)
        {
            case collision.OBB:
                SetOBB();
                break;
            case collision.Circle:
                SetCircle();
                break;
            case collision.Circle2AABB:
                if(tag == "Circle")
                {
                    SetCircle();
                }
                else
                {
                    SetAABB();
                }
                break;
            case collision.Circle2OBB:
                if (tag == "Circle")
                {
                    SetCircle();
                }
                else
                {
                    SetOBB();
                }
                break;
            case collision.AABB:
            default:
                SetAABB();
                break;
        }
    }

    private void SetAABB()
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
        extents = _mesh.bounds.size * _curExtentsRatio;

        max = center + extents;
        min = center - extents;

        //8个点顶点
        A = new Vector3(min.x, max.y, min.z);
        B = new Vector3(min.x, max.y, max.z);
        C = new Vector3(max.x, max.y, max.z);
        D = new Vector3(max.x, max.y, min.z);
        A1 = new Vector3(min.x, min.y, min.z);
        B1 = new Vector3(min.x, min.y, max.z);
        C1 = new Vector3(max.x, min.y, max.z);
        D1 = new Vector3(max.x, min.y, min.z);

        data.max = max;
        data.min = min;

        data.center = center;

        //设置线段数量
        _line.positionCount = 16;
        //线的宽度
        _line.startWidth = _width;
        _line.endWidth = _width;
        //根据8点画的_line；
        _line.SetPosition(0, B1);//前左下
        _line.SetPosition(1, C1);//前右下
        _line.SetPosition(2, D1);//后右下
        _line.SetPosition(3, A1);//后左下
        _line.SetPosition(4, B1);
        _line.SetPosition(5, B);//前左上
        _line.SetPosition(6, C);//前右上
        _line.SetPosition(7, D);//后右上
        _line.SetPosition(8, A);//后左上
        _line.SetPosition(9, B);
        _line.SetPosition(10, A);
        _line.SetPosition(11, A1);
        _line.SetPosition(12, D1);
        _line.SetPosition(13, D);
        _line.SetPosition(14, C);
        _line.SetPosition(15, C1);
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

        //设置线段数量
        _line.positionCount = 16;
        //线的宽度
        _line.startWidth = _width;
        _line.endWidth = _width;
        //根据8点画的_line；
        _line.SetPosition(0, B1);//前左下
        _line.SetPosition(1, C1);//前右下
        _line.SetPosition(2, D1);//后右下
        _line.SetPosition(3, A1);//后左下
        _line.SetPosition(4, B1);
        _line.SetPosition(5, B);//前左上
        _line.SetPosition(6, C);//前右上
        _line.SetPosition(7, D);//后右上
        _line.SetPosition(8, A);//后左上
        _line.SetPosition(9, B);
        _line.SetPosition(10, A);
        _line.SetPosition(11, A1);
        _line.SetPosition(12, D1);
        _line.SetPosition(13, D);
        _line.SetPosition(14, C);
        _line.SetPosition(15, C1);
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

        //设置线段数量
        _line.positionCount = 36 * 3 + 3 + 9;
        //线的宽度
        _line.startWidth = _width;
        _line.endWidth = _width;
        //根据36点画的_line；
        for (int i = 0; i < 37; i++)
        {
            float x = center.x + radius * Mathf.Sin(i * 10 * Mathf.PI / 180f);
            float y = center.y + radius * Mathf.Cos(i * 10 * Mathf.PI / 180f);
            Vector3 pos = new Vector3(x, y, center.z);
            _line.SetPosition(i, pos);
        }

        for (int i = 0; i < 37; i++)
        {
            float z = center.z + radius * Mathf.Sin(i * 10 * Mathf.PI / 180f);
            float y = center.y + radius * Mathf.Cos(i * 10 * Mathf.PI / 180f);
            Vector3 pos = new Vector3(center.x, y, z);
            _line.SetPosition(i + 37, pos);
        }

        for (int i = 0; i < 9; i++)
        {
            float z = center.z + radius * Mathf.Sin(i * 10 * Mathf.PI / 180f);
            float y = center.y + radius * Mathf.Cos(i * 10 * Mathf.PI / 180f);
            Vector3 pos = new Vector3(center.x, y, z);
            _line.SetPosition(i + 74, pos);
        }

        for (int i = 0; i < 37; i++)
        {
            float x = center.x + radius * Mathf.Sin(i * 10 * Mathf.PI / 180f);
            float z = center.z + radius * Mathf.Cos(i * 10 * Mathf.PI / 180f);
            Vector3 pos = new Vector3(x, center.y, z);
            _line.SetPosition(i + 83, pos);
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
