using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModHelper.Config;

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
        ModConfig config;

        int InputIDToChange = 0;
        KeyCode ForceGround = KeyCode.None, ForceGroundToggle = KeyCode.None, ForceAnchor = KeyCode.None,
            ForceThrustToggle = KeyCode.None, ForceThrustAddForward = KeyCode.None, ForceThrustRemoveForward = KeyCode.None,
            ForceThrustAddUpward = KeyCode.None, ForceThrustRemoveUpward = KeyCode.None, ForceBoostFuel = KeyCode.None;

        bool ForceThrust = false;
        float ThrustChange = 0f, ForceThrustAmountForward = 0f, ForceThrustAmountUpward = 0f;

        public void Start()
        {
            config = new ModConfig();

            config.BindConfig(this, "ThrustChange");
            config.BindConfig(this, "ForceGround", false);
            config.BindConfig(this, "ForceGroundToggle", false);
            config.BindConfig(this, "ForceAnchor", false);
            config.BindConfig(this, "ForceThrustToggle", false);
            config.BindConfig(this, "ForceThrustAddForward", false);
            config.BindConfig(this, "ForceThrustRemoveForward", false);
            config.BindConfig(this, "ForceThrustAddUpward", false);
            config.BindConfig(this, "ForceThrustRemoveUpward", false);
            config.BindConfig(this, "ForceBoostFuel", false);

            config.UseRefList = false;

            Console.WriteLine(config["ForceGround"].GetType().FullName);
            ForceGround = (KeyCode)(long)config["ForceGround"];
            ForceGroundToggle = (KeyCode)(long)(config["ForceGroundToggle"]);
            ForceAnchor = (KeyCode)(long)(config["ForceAnchor"]);
            ForceThrustToggle = (KeyCode)(long)(config["ForceThrustToggle"]);
            ForceThrustAddForward = (KeyCode)(long)(config["ForceThrustAddForward"]);
            ForceThrustRemoveForward = (KeyCode)(long)(config["ForceThrustRemoveForward"]);
            ForceThrustAddUpward = (KeyCode)(long)(config["ForceThrustAddUpward"]);
            ForceThrustRemoveUpward = (KeyCode)(long)(config["ForceThrustRemoveUpward"]);
            ForceBoostFuel = (KeyCode)(long)config["ForceBoostFuel"];

            config.UseRefList = true;
        }

        Rect WindowRect =  new Rect(0,0,800,400);
        Rect ScrollRect = new Rect(0,0,780,500);
        Vector2 ScrollPos = Vector2.zero;
        bool ShowGUI = false;

        public void OnGUI()
        {
            if (ShowGUI)
            {
                WindowRect = GUI.Window(51809, WindowRect, MiscPage, "Misc Configuration");
            }
        }

        bool TechGrounding = false;
        int anchorcache = 0;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ShowGUI = !ShowGUI;
                if (ShowGUI == false)
                {
                    config.WriteConfigJsonFile();
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

            if (Input.GetKeyDown(this.ForceThrustToggle))
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
            }

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
                        Singleton.playerTank.Anchors.UnanchorAll();
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

        private void MiscPage(int ID)
        {
            if (this.InputIDToChange != 0)
            {
                Event current = Event.current;
                if (current.isKey)
                {
                    switch(InputIDToChange)
                    {
                        case 1: this.ForceGround = current.keyCode; break;
                        case 2: this.ForceGroundToggle = current.keyCode; break;
                        case 3: this.ForceAnchor = current.keyCode; break;
                        case 4: this.ForceThrustToggle = current.keyCode; break;
                        case 5: this.ForceThrustAddForward = current.keyCode; break;
                        case 6: this.ForceThrustRemoveForward = current.keyCode; break;
                        case 7: this.ForceThrustAddUpward = current.keyCode; break;
                        case 8: this.ForceThrustRemoveUpward = current.keyCode; break;
                        case 9: this.ForceBoostFuel = current.keyCode; break;
                    }
                    this.InputIDToChange = 0;
                }
            }
            ScrollPos = GUI.BeginScrollView(new Rect(0, 15, WindowRect.width, WindowRect.height - 15), ScrollPos, ScrollRect);
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
            this.ForceThrust = GUI.Toggle(new Rect(0f, 140f, 500f, 20f), this.ForceThrust, "Toggle Force turbine");
            if (GUI.Button(new Rect(5f, 160f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 4) ? "Press a key to use" : this.ForceThrustToggle.ToString()))
            {
                this.InputIDToChange = 4;
            }
            GUI.Label(new Rect(30f, 180f, 500f, 20f), "Change turbine thrust rate (" + this.ThrustChange.ToString() + ")");
            this.ThrustChange = GUI.HorizontalSlider(new Rect(15f, 205f, this.WindowRect.width - 65f, 10f), this.ThrustChange, 1f, 0f);
            GUI.Label(new Rect(30f, 220f, 500f, 20f), "Turbine Amount Forward (" + this.ForceThrustAmountForward.ToString() + ")");
            this.ForceThrustAmountForward = GUI.HorizontalSlider(new Rect(15f, 240f, this.WindowRect.width - 65f, 10f), this.ForceThrustAmountForward, 1f, -1f);
            GUI.Label(new Rect(30f, 260f, 500f, 20f), "Add turbine thrust Forward");
            if (GUI.Button(new Rect(35f, 280f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 5) ? "Press a key to use" : this.ForceThrustAddForward.ToString()))
            {
                this.InputIDToChange = 5;
            }
            GUI.Label(new Rect(30f, 300f, 500f, 20f), "Remove turbine thrust Forward");
            if (GUI.Button(new Rect(35f, 320f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 6) ? "Press a key to use" : this.ForceThrustRemoveForward.ToString()))
            {
                this.InputIDToChange = 6;
            }
            GUI.Label(new Rect(30f, 340f, 500f, 20f), "Turbine Amount Upward (" + this.ForceThrustAmountUpward.ToString() + ")");
            this.ForceThrustAmountUpward = GUI.HorizontalSlider(new Rect(15f, 360f, this.WindowRect.width - 65f, 10f), this.ForceThrustAmountUpward, 1f, -1f);
            GUI.Label(new Rect(30f, 380f, 500f, 20f), "Add turbine thrust Upward");
            if (GUI.Button(new Rect(35f, 400f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 7) ? "Press a key to use" : this.ForceThrustAddUpward.ToString()))
            {
                this.InputIDToChange = 7;
            }
            GUI.Label(new Rect(30f, 420f, 500f, 20f), "Remove turbine thrust Upward");
            if (GUI.Button(new Rect(35f, 440f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 8) ? "Press a key to use" : this.ForceThrustRemoveUpward.ToString()))
            {
                this.InputIDToChange = 8;
            }
            GUI.Label(new Rect(0f, 460f, 500f, 20f), "Force (Fuel) Boosters");
            if (GUI.Button(new Rect(5f, 480f, this.WindowRect.width * 0.5f, 20f), (this.InputIDToChange == 9) ? "Press a key to use" : this.ForceBoostFuel.ToString()))
            {
                this.InputIDToChange = 9;
            }
            GUI.EndScrollView();
            GUI.DragWindow();
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(this.ForceBoostFuel))
            {
                Singleton.playerTank.control.BoostControl = true;
            }
            if (this.ForceThrust)
            {
                try
                {
                    foreach (FanJet fanJet in Singleton.playerTank.GetComponentsInChildren<FanJet>())
                    {
                        Vector3 vector = Quaternion.Inverse(Singleton.playerTank.control.FirstController.block.transform.rotation) * fanJet.effector.forward;
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
