using System.Text;
using UnityEngine;

public class LocalObjExporterScript
{
    private static int StartIndex = 0;

    public static void Start()
    {
        StartIndex = 0;
    }

    public static void End()
    {
        StartIndex = 0;
    }

    public static string MeshToString(MeshFilter mf, Transform t)
    {
        Vector3 s = t.localScale;
        Vector3 p = t.localPosition;
        Quaternion r = t.localRotation;

        int numVertices = 0;
        Mesh m = mf.sharedMesh;
        if (!m)
        {
            return "####Error####";
        }

        StringBuilder sb = new StringBuilder();

        foreach (Vector3 vv in m.vertices)
        {
            Vector3 v = t.TransformPoint(vv);
            numVertices++;
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 nn in m.normals)
        {
            Vector3 v = r * nn;
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
            }
        }

        StartIndex += numVertices;
        return sb.ToString();
    }
}

public static class LocalObjExporter
{
    public static string DoExport(Transform Selection)
    {
        string meshName = Selection.gameObject.name;
        string fileName = meshName + ".obj";

        LocalObjExporterScript.Start();

        StringBuilder meshString = new StringBuilder();

        meshString.Append("#" + meshName + ".obj"
                            + "\n#" + System.DateTime.Now.ToLongDateString()
                            + "\n#" + System.DateTime.Now.ToLongTimeString()
                            + "\n#-------"
                            + "\n\n");

        Transform t = Selection.gameObject.transform;

        Vector3 originalPosition = t.position;
        t.position = Vector3.zero;

        Quaternion originalRotation = t.rotation;
        t.rotation = Quaternion.identity;

        meshString.Append(processTransform(t));

        t.position = originalPosition;
        t.rotation = originalRotation;

        LocalObjExporterScript.End();
        Debug.Log("Exported Mesh: " + fileName);

        return meshString.ToString();
    }

    private static string processTransform(Transform t)
    {
        StringBuilder meshString = new StringBuilder();

        meshString.Append("#" + t.name
                        + "\n#-------"
                        + "\n");

        
        meshString.Append("g ").Append(t.name).Append("\n");
        

        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf)
        {
            meshString.Append(LocalObjExporterScript.MeshToString(mf, t));
        }

        for (int i = 0; i < t.childCount; i++)
        {
            meshString.Append(processTransform(t.GetChild(i)));
        }

        return meshString.ToString();
    }

    public static string MeshToString(Mesh sharedMesh, string Name, Vector3 StretchOffset, Vector3 PosOffset, Quaternion RotOffset)
    {
        Mesh m = sharedMesh;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(Name).Append("\n");

        foreach (Vector3 v in m.vertices)
        {
            Vector3 vector = (RotOffset * new Vector3(v.x * StretchOffset.x, v.y * StretchOffset.y, v.z * StretchOffset.z)) + PosOffset;
            sb.Append(string.Format("v {0} {1} {2}\n", vector.x, vector.y, vector.z));
        }
        sb.Append("\n");

        foreach (Vector3 vn in m.normals)
        {
            Vector3 rvn = RotOffset * vn;
            sb.Append(string.Format("vn {0} {1} {2}\n", rvn.x, rvn.y, rvn.z));
        }
        sb.Append("\n");

        foreach (Vector2 uv in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));
        }
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                       triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }
        return sb.ToString();
    }
}