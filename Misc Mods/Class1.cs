using ModHelper.Config;
using System;
using UnityEngine;

namespace Misc_Mods
{
    internal static class Tony
    {
        public static void PokeTony() => Update();
        static void Update() => ExamineKneecaps();
        static void ExamineKneecaps() => throw new Exception("Uh oh! Tony found your kneecaps to be rather unsuitable");
    }
    public class Class1
    {
        public static void Run()
        {
            new GameObject().AddComponent<GUIConfig>();
        }
    }

    public class GUIConfig : MonoBehaviour
    {
        //private ModConfig config;
        GameObject GUIDisp;

        public void Start()
        {
            //config = new ModConfig();

            GUIDisp = new GameObject();
            GUIDisp.AddComponent<GUIDisplay>().inst = this;
            GUIDisp.SetActive(false);
        }

        private Rect WindowRect = new Rect(0, 0, 800, 400);
        private Vector2 ScrollPos = Vector2.zero;
        private bool ShowGUI = false;
        private TankBlock module;

        private void Update()
        {
            if (!Singleton.Manager<ManPointer>.inst.DraggingItem && Input.GetMouseButtonDown(1))
            {
                try
                {
                    module = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                }
                catch
                {
                    //Console.WriteLine(e);
                    //module = null;
                }
            }

            try
            {
                if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    ShowGUI = !ShowGUI;
                    GUIDisp.SetActive(ShowGUI);
                    if (ShowGUI == false)
                    {
                        //config.WriteConfigJsonFile();
                        module = null;
                        log = "Right-click on a block you would like to export, or choose to export your tech";
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("EXCEPTION: " + E.Message + "\n" + E.StackTrace);
            }
        }

        private int SelectedPage = 0;

        private void MiscPage(int ID)
        {
            SelectedPage = GUILayout.SelectionGrid(SelectedPage, new string[] { "ModelExport" }, 3);
            switch (SelectedPage)
            {
                case 0: ExportPage(ID); break;
            }
            GUI.DragWindow();
        }

        public static string log = "";

        private void ExportPage(int ID)
        {
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);
            try
            {
                if (Singleton.playerTank != null)
                {
                    if (GUILayout.Button("Export Current (player) Tech with GrabIt Tools"))
                    {
                        ObjGrabItExporter.ExportWithGrabIt(Singleton.playerTank.gameObject);
                        log = "Processing, please be patient...";
                    }
                }
                GUILayout.Label("Selected Block: " + (module ? module.name : "None"));
                GUILayout.Label(log);
                if (module != null)
                {
                    if (GUILayout.Button("Export Block Model"))
                    {
                        string path = "_Export/Blocks";
                        string Total = LocalObjExporter.DoExport(module.transform);
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                        System.IO.File.WriteAllText(path + "/" + module.name + ".obj", Total);
                        log = "Exported " + module.name + ".obj to " + path;
                    }
                    if (GUILayout.Button("Export Parts of Selected Block"))
                    {
                        string path = "_Export/Parts/" + module.name;
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                        foreach (var mf in module.GetComponentsInChildren<MeshFilter>())
                        {
                            System.IO.File.WriteAllText(path + "/" + mf.mesh.name + ".obj", LocalObjExporter.MeshToString(mf.mesh, mf.mesh.name, Vector3.one, Vector3.zero, Quaternion.identity));
                        }

                        log = "Exported individual .obj files to " + path;
                    }
                }
            }
            catch (Exception E)
            {
                log = E.Message;
                Console.WriteLine(E.Message);
                Console.WriteLine(E.StackTrace);
            }
            GUILayout.EndScrollView();
        }

        internal class GUIDisplay : MonoBehaviour
        {
            public GUIConfig inst;
            public void OnGUI()
            {
                inst.WindowRect = GUI.Window(51809, inst.WindowRect, inst.MiscPage, "Misc Configuration");
            }
        }
    }
}