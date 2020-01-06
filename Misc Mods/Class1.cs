using ModHelper.Config;
using System;
using UnityEngine;
using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace Misc_Mods
{
    internal static class Tony
    {
        public static void PokeTony() => Update();
        static void Update() => ExamineKneecaps();
        static void ExamineKneecaps() => throw new Exception("Kneecaps not found! Please reinstall kneecaps to proceed, else Tony shalln't  W-A-L-K");
    }
    public class Class1
    {
        public static ModConfig config;

        public static float FanJetMultiplier = 1f,
            FanJetVelocityRestraint = 0f,
            BoosterJetMultiplier = 1f,
            ModuleWingMultiplier = 1f;
        public static float WorldDrag = 1f,
            TechDrag = 1f;

        public static void Run()
        {
            config = new ModConfig();
            config.BindConfig<Class1>(null, "FanJetMultiplier");
            config.BindConfig<Class1>(null, "FanJetVelocityRestraint");
            config.BindConfig<Class1>(null, "BoosterJetMultiplier");
            config.BindConfig<Class1>(null, "ModuleWingMultiplier");
            config.BindConfig<Class1>(null, "WorldDrag");
            config.BindConfig<Class1>(null, "TechDrag");
            config.UpdateConfig += GUIConfig.ResetMultipliers;

            HarmonyInstance mod = HarmonyInstance.Create("aceba1.ttmm.misc");
            mod.PatchAll(Assembly.GetExecutingAssembly());
            new GameObject().AddComponent<GUIConfig>();
            GUIConfig.ResetMultipliers();
        }
    }

    public class GUIConfig : MonoBehaviour
    {
        GameObject GUIDisp;


        public void Start()
        {
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
                        Class1.config.WriteConfigJsonFile();
                        module = null;
                        log = "Right-click on a block to select it here";
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("EXCEPTION: " + E.Message + "\n" + E.StackTrace);
            }
        }

        private void MiscPage(int ID)
        {
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);
              GUILayout.Label("Selected Block: " + (module ? module.name : "None"));
              GUILayout.Label(log);
              GUILayout.BeginVertical("Model Exporter", GUI.skin.window);
                ExportPage(ID);
              GUILayout.EndVertical();
              GUILayout.Space(16);
              GUILayout.BeginVertical("Block JSON Dumper", GUI.skin.window);
                BlockInfoDumperPage(ID);
              GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        public static string log = "";

        private void ExportPage(int ID)
        {
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
        }

        private void BlockInfoDumperPage(int ID)
        {
            try
            {
                if (GUILayout.Button("Export all block info"))
                {
                    log = "Logged " + BlockInfoDumper.Dump().ToString() + " blocks to file";
                }
                if (module != null)
                {
                    if (GUILayout.Button("Export Block JSON"))
                    {
                        string path = "_Export/BlockJson";
                        BlockInfoDumper.DeepDumpClassCache.Clear();
                        var Total = BlockInfoDumper.DeepDumpAll(module.transform, 6).ToString();
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                        System.IO.File.WriteAllText(path + "/" + module.name + ".json", Total);
                        log = "Exported " + module.name + ".json to " + path;
                    }
                    if (GUILayout.Button("Export FireData Projectile JSON"))
                    {
                        string path = "_Export/BlockJson";
                        BlockInfoDumper.DeepDumpClassCache.Clear();
                        var Total = BlockInfoDumper.DeepDumpAll(module.GetComponent<FireData>().m_BulletPrefab.transform, 12).ToString();
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                        System.IO.File.WriteAllText(path + "/" + module.name + "_BulletPrefab.json", Total);
                        log = "Exported " + module.name + "_BulletPrefab.json to " + path;
                    }
                }
            }
            catch (Exception E)
            {
                log = E.Message;
                Console.WriteLine(E.Message);
                Console.WriteLine(E.StackTrace);
            }
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