using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexObject : MonoBehaviour
{
    public enum HexType { GRASS, MOUNTAIN, WATER };

    private Mesh mesh;
    private MeshCollider meshCollider;
    public bool isHighlighted = false;
    public Unit unit = null;
    public HexType hexType = HexType.GRASS;
    public Vector2 Coords;
    public static float outerRadius = 10f;
    public static float innerRadius = outerRadius * 0.866025404f;
    public static Vector3[] corners = {
        new Vector3(-outerRadius, 0f, 0f),
        new Vector3(-0.5f * outerRadius, 0f, innerRadius),
        new Vector3(0.5f * outerRadius, 0f, innerRadius),
        new Vector3(outerRadius, 0f, 0f),
        new Vector3(0.5f * outerRadius, 0f, -innerRadius),
        new Vector3(-0.5f * outerRadius, 0f, -innerRadius)
    };

    private static int[] triangles = {
        0, 1, 2,
        0, 2, 3,
        0, 3, 4,
        0, 4, 5
    };

    private static Vector3[] normals = {
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward
    };

    private static Vector2[] uv = {
        new Vector2(-outerRadius, 0f),
        new Vector2(-0.5f * outerRadius, innerRadius),
        new Vector2(0.5f * outerRadius, innerRadius),
        new Vector2(outerRadius, 0f),
        new Vector2(0.5f * outerRadius, -innerRadius),
        new Vector2(-0.5f * outerRadius,  -innerRadius)
    };

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        mesh.name = "Hex Mesh";
    }

    void Start()
    {
        Triangulate();
    }

    ///<summary>
    ///Create the triangles for the hexagon
    ///</summary>
    private void Triangulate()
    {
        mesh.Clear();
        mesh.vertices = GetVertices();
        mesh.triangles = triangles;
        mesh.normals = normals;
        meshCollider.sharedMesh = mesh;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        Renderer rend = GetComponent<Renderer>();
        Color color = hexType == HexType.MOUNTAIN ? Color.gray : Color.white;
        rend.material.SetColor("_Color", color);
    }

    ///<summary>
    ///Get an array of the word location of the vertices
    ///</summary>
    ///<returns>Returns an array of the world location of the vertices</returns>
    private Vector3[] GetVertices()
    {
        List<Vector3> verts = new List<Vector3>();
        Vector3 center = transform.localPosition;

        for (int i = 0; i < 6; i++)
            verts.Add(center + corners[i]);

        return verts.ToArray();
    }

    ///<summary>
    ///Set a hex highlighted or not
    ///</summary>
    ///<param name="highlight">True if a hex needs to be highlighted</param>
    public void SetHighlighted(bool highlight)
    {
        if (hexType != HexType.GRASS)
            return;

        isHighlighted = highlight;
        Renderer rend = GetComponent<Renderer>();

        if (isHighlighted)
        {
            Color color = new Color((1f / 255f) * 92f, (1f / 255f) * 211f, (1f / 255f) * 235f, 0.9f);

            if (unit != null && unit.unitTeam == Unit.UnitTeamType.ENEMY)
                color = Color.red;

            if (unit != null && unit.unitTeam == Unit.UnitTeamType.PLAYER)
                color = Color.white;

            rend.material.SetColor("_Color", color);
        }
        else
            rend.material.SetColor("_Color", Color.white);
    }

    ///<summary>
    ///Simple gets the unit type from the character
    ///</summary>
    ///<param name="c">The character</param>
    ///<returns>Returns the HexType from the char</returns>
    public static HexType GetTypeFromChar(char c)
    {
        switch (c)
        {
            case 'M': return HexType.MOUNTAIN;
            case '#': return HexType.WATER;
            default: return HexType.GRASS;
        }
    }
}
