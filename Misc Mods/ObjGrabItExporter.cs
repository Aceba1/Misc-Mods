using System.Text;
using UnityEngine;
using WhisperingGibbon.GrabIt;

namespace Misc_Mods
{
    internal static class ObjGrabItExporter
    {
        private static Grab m_Grab;
        private static string Name;
        private static GrabItController m_GrabItController;
        private const System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;
        private static readonly bool Running = false;

        public static void ExportWithGrabIt(GameObject Body)
        {
            Name = Body.name;
            if (Running)
            {
                GUIConfig.log = "Please be patient, there is already a tech being exported! (unless it broke)";
                return;
            }

            if (m_GrabItController == null)
            {
                m_GrabItController = typeof(ManGrabIt).GetField("m_GrabItController", bindingFlags).GetValue(ManGrabIt.inst) as GrabItController;
            }

            m_Grab = Body.GetComponent<Grab>();
            if (m_Grab == null)
            {
                m_Grab = Body.AddComponent<Grab>();
            }
            m_Grab.Reset();
            m_Grab.CacheMaterials(StartGrab);
        }

        private static void StartGrab()
        {
            m_GrabItController.Capture(m_Grab, new CaptureCompleteCallback(Save), null);
        }

        private static void Save(GrabResult result)
        {
            var m = result.GetTMeshData();

            StringBuilder sb = new StringBuilder();

            sb.Append("g ").Append(Name).Append("\n");

            foreach (var v in m.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.position.x, v.position.y, v.position.z));
            }
            sb.Append("\n");

            foreach (var vn in m.vertices)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", vn.normal.x, vn.normal.y, vn.normal.z));
            }
            sb.Append("\n");

            foreach (var uv in m.vertices)
            {
                sb.Append(string.Format("vt {0} {1}\n", uv.texCoord.x, uv.texCoord.y));
            }
            for (int material = 0; material < m.SubMeshCount; material++)
            {
                sb.Append("\n");

                var triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Count; i += 1)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[i].vertexId[0] + 1, triangles[i].vertexId[1] + 1, triangles[i].vertexId[2] + 1));
                }
            }

            string path = "_Export/Techs";

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            System.IO.File.WriteAllText(path + "/" + Name + ".obj", sb.ToString());
            GUIConfig.log = "Exported " + Name + ".obj to " + path + " using the GrabIt API";
        }
    }
}