using ModHelper.Config;
using System;
using UnityEngine;

namespace Misc_Mods
{
    public class Class1
    {
        public static void Run()
        {
            new GameObject().AddComponent<GUIConfig>();
        }
    }

    public class GUIConfig : MonoBehaviour
    {
        private ModConfig config;

        private int InputIDToChange = 0;

        private KeyCode ForceGround = KeyCode.None, ForceGroundToggle = KeyCode.None, ForceAnchor = KeyCode.None;

        private bool ForceThrust = false;
        private float ThrustChange = 0f, ForceThrustAmountForward = 0f, ForceThrustAmountUpward = 0f;

        public void Start()
        {
            config = new ModConfig();
            
            config.BindConfig(this, "ForceGround", false);
            config.BindConfig(this, "ForceGroundToggle", false);
            config.BindConfig(this, "ForceAnchor", false);

            config.UseRefList = false;
            try
            {
                Console.WriteLine(config["ForceGround"].GetType().FullName);
                ForceGround = (KeyCode)(long)config["ForceGround"];
                ForceGroundToggle = (KeyCode)(long)(config["ForceGroundToggle"]);
                ForceAnchor = (KeyCode)(long)(config["ForceAnchor"]);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message + "\n" + E.StackTrace);
            }
            config.UseRefList = true;
        }

        private Rect WindowRect = new Rect(0, 0, 800, 400);
        private Rect TurbineScrollRect = new Rect(0, 0, 770, 500);
        private Vector2 ScrollPos = Vector2.zero;
        private bool ShowGUI = false;

        public void OnGUI()
        {
            if (ShowGUI)
            {
                WindowRect = GUI.Window(51809, WindowRect, MiscPage, "Misc Configuration");
            }
        }

        private bool TechGrounding = false;
        private int anchorcache = 0;
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
                    if (ShowGUI == false)
                    {
                        config.WriteConfigJsonFile();
                        module = null;
                        log = "Right-click on a block you would like to export, or choose to export your tech";
                    }
                }
                try
                {
                    if (Input.GetKeyDown(this.ForceGroundToggle))
                    {
                        this.TechGrounding = !this.TechGrounding;
                    }
                    if (this.TechGrounding)
                    {
                        Singleton.playerTank.grounded = true;
                    }
                    if (Input.GetKey(this.ForceGround))
                    {
                        Singleton.playerTank.grounded = true;
                    }
                    if (Input.GetKeyUp(this.ForceGround))
                    {
                        this.TechGrounding = false;
                    }
                }
                catch
                {
                    this.TechGrounding = false;
                }

                /*if (Input.GetKeyDown(this.ForceThrustToggle))
                {
                    this.ForceThrust = !this.ForceThrust;
                    if (!this.ForceThrust)
                    {
                        this.ForceThrustAmountForward = 0f;
                        this.ForceThrustAmountUpward = 0f;
                    }
                }
                if (this.ForceThrust)
                {
                    if (Input.GetKey(this.ForceThrustAddForward))
                    {
                        this.ForceThrustAmountForward += this.ThrustChange;
                        if (this.ForceThrustAmountForward > 1f)
                        {
                            this.ForceThrustAmountForward = 1f;
                        }
                    }
                    else if (Input.GetKey(this.ForceThrustRemoveForward))
                    {
                        this.ForceThrustAmountForward -= this.ThrustChange;
                        if (this.ForceThrustAmountForward < -1f)
                        {
                            this.ForceThrustAmountForward = -1f;
                        }
                    }
                    if (Input.GetKey(this.ForceThrustAddUpward))
                    {
                        this.ForceThrustAmountUpward += this.ThrustChange;
                        if (this.ForceThrustAmountUpward > 1f)
                        {
                            this.ForceThrustAmountUpward = 1f;
                        }
                    }
                    else if (Input.GetKey(this.ForceThrustRemoveUpward))
                    {
                        this.ForceThrustAmountUpward -= this.ThrustChange;
                        if (this.ForceThrustAmountUpward < -1f)
                        {
                            this.ForceThrustAmountUpward = -1f;
                        }
                    }
                }*/

                if (Input.GetKeyDown(this.ForceAnchor))
                {
                    this.anchorcache = (Singleton.playerTank.IsAnchored ? 2 : 1);
                }
                if (Input.GetKey(this.ForceAnchor) && this.anchorcache == (Singleton.playerTank.IsAnchored ? 2 : 1))
                {
                    try
                    {
                        if (Singleton.playerTank.IsAnchored)
                        {
                            Singleton.playerTank.Anchors.UnanchorAll(false, false);
                            this.anchorcache = ((this.anchorcache == (Singleton.playerTank.IsAnchored ? 2 : 1)) ? 2 : 0);
                        }
                        else
                        {
                            Vector3 position = Singleton.playerTank.transform.position;
                            Quaternion rotation = Singleton.playerTank.transform.rotation;
                            Vector3 velocity = Singleton.playerTank.rbody.velocity;
                            Vector3 angularVelocity = Singleton.playerTank.rbody.angularVelocity;
                            Singleton.playerTank.Anchors.TryAnchorAll(false);
                            if (this.anchorcache == (Singleton.playerTank.IsAnchored ? 2 : 1))
                            {
                                Singleton.playerTank.transform.position = position;
                                Singleton.playerTank.transform.rotation = rotation;
                                Singleton.playerTank.rbody.velocity = velocity;
                                Singleton.playerTank.rbody.angularVelocity = angularVelocity;
                            }
                            else
                            {
                                this.anchorcache = 0;
                            }
                        }
                    }
                    catch
                    {
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
            SelectedPage = GUILayout.SelectionGrid(SelectedPage, new string[] { "Keybinds", "Turbines", "ModelExport" }, 3);
            switch (SelectedPage)
            {
                case 0: KeybindPage(ID); break;
                case 1: TurbinePage(ID); break;
                case 2: ExportPage(ID); break;
            }
            GUI.DragWindow();
        }

        private void KeybindPage(int ID)
        {
            if (this.InputIDToChange != 0)
            {
                Event current = Event.current;
                if (current.isKey)
                {
                    switch (InputIDToChange)
                    {
                        case 1: this.ForceGround = current.keyCode; break;
                        case 2: this.ForceGroundToggle = current.keyCode; break;
                        case 3: this.ForceAnchor = current.keyCode; break;
                    }
                    this.InputIDToChange = 0;
                }
            }
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);

            GUI.Label(new Rect(0f, 0f, 500f, 20f), "Force Ground Controls");
            if (GUI.Button(new Rect(5f, 20f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 1) ? "Press a key to use" : this.ForceGround.ToString()))
            {
                this.InputIDToChange = 1;
            }
            GUI.Label(new Rect(0f, 40f, 500f, 20f), "Toggle Force Ground Controls");
            if (GUI.Button(new Rect(5f, 60f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 2) ? "Press a key to use" : this.ForceGroundToggle.ToString()))
            {
                this.InputIDToChange = 2;
            }
            GUI.Label(new Rect(0f, 80f, 500f, 20f), "Force (Un)Anchor");
            if (GUI.Button(new Rect(5f, 100f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 3) ? "Press a key to use" : this.ForceAnchor.ToString()))
            {
                this.InputIDToChange = 3;
            }
            if (GUI.Button(new Rect(5, 120, this.WindowRect.width * 0.4f, 20), "CRASH GAME")) 
            {
                throw new Exception("Uh oh! Tony found your kneecaps to be rather unsuitable");
            }

            GUILayout.EndScrollView();
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
                        string Total = LocalObjExporter.DoExport(module.transform, true, true);
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

        private void TurbinePage(int ID)
        {
            /*if (this.InputIDToChange != 0)
            {
                Event current = Event.current;
                if (current.isKey)
                {
                    switch (InputIDToChange)
                    {
                        case 4: this.ForceThrustToggle = current.keyCode; break;
                        case 5: this.ForceThrustAddForward = current.keyCode; break;
                        case 6: this.ForceThrustRemoveForward = current.keyCode; break;
                        case 7: this.ForceThrustAddUpward = current.keyCode; break;
                        case 8: this.ForceThrustRemoveUpward = current.keyCode; break;
                    }
                    this.InputIDToChange = 0;
                }
            }

            ScrollPos = GUI.BeginScrollView(new Rect(5, 40, WindowRect.width - 10, WindowRect.height - 40), ScrollPos, TurbineScrollRect);

            this.ForceThrust = GUI.Toggle(new Rect(0f, 0f, 500f, 20f), this.ForceThrust, "Toggle Force turbine");
            if (GUI.Button(new Rect(5f, 20f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 4) ? "Press a key to use" : this.ForceThrustToggle.ToString()))
            {
                this.InputIDToChange = 4;
            }
            GUI.Label(new Rect(30f, 40f, 500f, 20f), "Change turbine thrust rate (" + this.ThrustChange.ToString() + ")");
            this.ThrustChange = GUI.HorizontalSlider(new Rect(15f, 65f, this.WindowRect.width - 65f, 20f), this.ThrustChange, 1f, 0f);
            GUI.Label(new Rect(30f, 60f, 500f, 20f), "Turbine Amount Forward (" + this.ForceThrustAmountForward.ToString() + ")");
            this.ForceThrustAmountForward = GUI.HorizontalSlider(new Rect(15f, 100f, this.WindowRect.width - 65f, 20f), this.ForceThrustAmountForward, 1f, -1f);
            GUI.Label(new Rect(30f, 120f, 500f, 20f), "Add turbine thrust Forward");
            if (GUI.Button(new Rect(35f, 140f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 5) ? "Press a key to use" : this.ForceThrustAddForward.ToString()))
            {
                this.InputIDToChange = 5;
            }
            GUI.Label(new Rect(30f, 160f, 500f, 20f), "Remove turbine thrust Forward");
            if (GUI.Button(new Rect(35f, 180f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 6) ? "Press a key to use" : this.ForceThrustRemoveForward.ToString()))
            {
                this.InputIDToChange = 6;
            }
            GUI.Label(new Rect(30f, 200f, 500f, 20f), "Turbine Amount Upward (" + this.ForceThrustAmountUpward.ToString() + ")");
            this.ForceThrustAmountUpward = GUI.HorizontalSlider(new Rect(15f, 220f, this.WindowRect.width - 65f, 20f), this.ForceThrustAmountUpward, 1f, -1f);
            GUI.Label(new Rect(30f, 240f, 500f, 20f), "Add turbine thrust Upward");
            if (GUI.Button(new Rect(35f, 260f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 7) ? "Press a key to use" : this.ForceThrustAddUpward.ToString()))
            {
                this.InputIDToChange = 7;
            }
            GUI.Label(new Rect(30f, 280f, 500f, 20f), "Remove turbine thrust Upward");
            if (GUI.Button(new Rect(35f, 300f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 8) ? "Press a key to use" : this.ForceThrustRemoveUpward.ToString()))
            {
                this.InputIDToChange = 8;
            }
            GUI.EndScrollView();
            */
            GUI.Label(new Rect(30f, 40f, 600f, 20f), "There is nothing to see here, anymore...");
        }

        private void FixedUpdate()
        {
            if (this.ForceThrust)
            {
                try
                {
                    foreach (FanJet fanJet in Singleton.playerTank.GetComponentsInChildren<FanJet>())
                    {
                        Vector3 vector = Quaternion.Inverse(Singleton.playerTank.control.FirstController.block.transform.rotation) * Vector3.forward;
                        if (vector.z > 0.8f)
                        {
                            fanJet.SetSpin(-this.ForceThrustAmountForward);
                        }
                        else if (vector.z < -0.8f)
                        {
                            fanJet.SetSpin(this.ForceThrustAmountForward);
                        }
                        else if (vector.y > 0.8f)
                        {
                            fanJet.SetSpin(-this.ForceThrustAmountUpward);
                        }
                        else if (vector.y < -0.8f)
                        {
                            fanJet.SetSpin(this.ForceThrustAmountUpward);
                        }
                    }
                }
                catch
                {
                    this.ForceThrust = false;
                    this.ForceThrustAmountForward = 0f;
                    this.ForceThrustAmountUpward = 0f;
                }
            }
        }
    }
}