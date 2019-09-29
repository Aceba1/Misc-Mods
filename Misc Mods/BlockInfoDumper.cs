using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Misc_Mods
{
    class BlockInfoDumper
    {
        public static int Dump()
        {
            bool RemoveSecret = !Input.GetKey((UnityEngine.KeyCode)308);
            Console.WriteLine("Starting block dump...");

            var DUMP = new JObject
                {
                    { "game_version", SKU.DisplayVersion }, // Display Version
                    { "date", DateTime.Now.ToString() } // Date of dump
                }; // Create DUMP, which will be outputted to a file

            var CHUNKS = new JArray(); // Create CHUNKS array
                                       // Populate CHUNKS array with resource chunks
            foreach (ChunkTypes id in Enum.GetValues(typeof(ChunkTypes)))
            {
                var ITEM = new JObject()
                    {
                        { "name", StringLookup.GetItemName(ObjectTypes.Chunk, (int)id) },
                        { "id", (int)id },
                        { "price", RecipeManager.inst.GetChunkPrice(id) }
                    };
                GetRecipe(ITEM, (int)id, ObjectTypes.Chunk);
                CHUNKS.Add(ITEM);
            }

            DUMP.Add("data_chunks", CHUNKS); // Add CHUNKS array to DUMP

            var DATA = new JArray(); // Create DATA array
                                     // Populate DATA array with all blocks

            int NumExported = 0;
            foreach (BlockTypes id in Enum.GetValues(typeof(BlockTypes)))
            {
                var ITEM = new JObject(); // Create DATA entry
                TankBlock Block = ManSpawn.inst.GetBlockPrefab(id);
                if (Block == null) continue; // Escape if block is empty
                GameObject Base = Block.gameObject;
                if (Base == null) continue; // Escape if object is empty. Redundant
                int grade = ManLicenses.inst.GetBlockTier(id, true);
                if (RemoveSecret && grade > 127)
                {
                    continue;
                }
                FactionSubTypes corp = ManSpawn.inst.GetCorporation(id);
                BlockCategories category = ManSpawn.inst.GetCategory(id);

                // Block name
                ITEM.Add("block", StringLookup.GetItemName(ObjectTypes.Block, (int)id));
                // Resource name
                ITEM.Add("resource_name", Base.name);
                // Description
                ITEM.Add("description", StringLookup.GetItemDescription(ObjectTypes.Block, (int)id));
                // ID
                ITEM.Add("id", (int)id);
                // Enum
                ITEM.Add("enum", id.ToString());
                // Mass
                ITEM.Add("mass", Block.m_DefaultMass);
                // Corp int
                ITEM.Add("corp_int", (int)corp);
                // Corp
                ITEM.Add("corp", StringLookup.GetCorporationName(corp));
                // Category int
                ITEM.Add("category_int", (int)category);
                // Category
                ITEM.Add("category", StringLookup.GetBlockCategoryName(category));
                // Grade
                ITEM.Add("grade", grade);
                // Price
                ITEM.Add("price", RecipeManager.inst.GetBlockBuyPrice(id, true));
                // Rarity
                ITEM.Add("rarity", Block.BlockRarity.ToString());
                // Rarity
                ITEM.Add("blocklimit_cost", ManBlockLimiter.inst.GetBlockCost(id));
                // Health
                var md = Base.GetComponent<ModuleDamage>();
                if (md) // Redundant check
                {
                    ITEM.Add("health", md.maxHealth);
                    ITEM.Add("fragility", md.m_DamageDetachFragility);
                    if (Block.visible && Block.visible.damageable)
                    {
                        ITEM.Add("damageable_type", Block.visible.damageable.DamageableType.ToString());
                        ITEM.Add("damageable_type_int", (int)Block.visible.damageable.DamageableType);
                    }
                }
                try
                {
                    // Cells
                    ITEM.Add("cell_count", Block.filledCells.Length);
                    // APs
                    ITEM.Add("ap_count", Block.attachPoints.Length);
                }
                catch { }

                // Recipe
                GetRecipe(ITEM, (int)id);
                // FireData
                GetFireData(ITEM, Base);
                // ModuleWeapon
                GetWeaponData(ITEM, Base);
                // ModuleDrill
                GetDrillData(ITEM, Base);
                // ModuleHammer
                GetHammerData(ITEM, Base);
                // ModuleEnergyStore
                GetEnergyData(ITEM, Base);
                // ModuleFuelTank
                GetFuelData(ITEM, Base);
                // ModuleShieldGenerator
                GetShieldData(ITEM, Base);
                // ModuleHover
                GetHoverData(ITEM, Base);
                // ModuleWheels
                GetWheelData(ITEM, Base);

                // Add to DATA
                DATA.Add(ITEM);
                NumExported++;
            }

            DUMP.Add("data_blocks", DATA); // Add DATA array to DUMPwww

            System.IO.File.WriteAllText("_Export/BlockInfoDump.json", DUMP.ToString(Newtonsoft.Json.Formatting.Indented));
            return NumExported;
        }

        static void GetRecipe(JObject BLOCK, int Id, ObjectTypes type = ObjectTypes.Block)
        {
            try
            {
                var DATA = new JArray();
                foreach (var thing in RecipeManager.inst.GetRecipeByOutputType(new ItemTypeInfo(type, Id)).m_InputItems)
                {
                    var ITEM = new JObject
                    {
                        { "name", StringLookup.GetItemName(ObjectTypes.Chunk, thing.m_Item.ItemType) },
                        { "id", thing.m_Item.ItemType },
                        { "count", thing.m_Quantity }
                    };
                    DATA.Add(ITEM);
                }
                BLOCK.Add("recipe", DATA);
            }
            catch { }
        }

        static readonly BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static readonly FieldInfo WeaponRound_Damage = typeof(WeaponRound).GetField("m_Damage", binding);
        static readonly FieldInfo WeaponRound_DamageType = typeof(WeaponRound).GetField("m_DamageType", binding);
        static void GetFireData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var firedata = Base.GetComponent<FireData>();
                if (firedata != null)
                {
                    var DATA = new JObject
                    {
                        { "bullet_spray_variance", firedata.m_BulletSprayVariance },
                        { "bullet_velocity", firedata.m_MuzzleVelocity },
                        { "kickback_strength", firedata.m_KickbackStrength }
                    };
                    if (firedata.m_BulletPrefab != null)
                    {
                        DATA.Add("damage", (int)WeaponRound_Damage.GetValue(firedata.m_BulletPrefab));
                        DATA.Add("damage_type", ((ManDamage.DamageType)WeaponRound_DamageType.GetValue(firedata.m_BulletPrefab)).ToString());
                    }
                    BLOCK.Add("FireData", DATA);
                }
            }
            catch { }
        }

        static void GetWeaponData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleWeapon>();
                if (obj != null)
                {
                    var DATA = new JObject
                    {
                        { "shot_cooldown", obj.ShotCooldown },
                        { "rotate_speed", obj.RotateSpeed }
                    };
                    BLOCK.Add("ModuleWeapon", DATA);
                }
            }
            catch { }
        }

        static readonly FieldInfo ModuleDrill_DamagePerSecond = typeof(ModuleDrill).GetField("damagePerSecond", binding);
        static void GetDrillData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleDrill>();
                if (obj != null)
                {
                    var DATA = new JObject
                    {
                        { "damape_per_second", (float)ModuleDrill_DamagePerSecond.GetValue(obj) }
                    };
                    BLOCK.Add("ModuleDrill", DATA);
                }
            }
            catch { }
        }

        static readonly FieldInfo ModuleHammer_ImpactDamage = typeof(ModuleHammer).GetField("impactDamage", binding);
        static void GetHammerData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleHammer>();
                if (obj != null)
                {
                    var DATA = new JObject
                    {
                        { "impact_damage", (float)ModuleHammer_ImpactDamage.GetValue(obj) }
                    };
                    BLOCK.Add("ModuleHammer", DATA);
                }
            }
            catch { }
        }

        static void GetEnergyData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleEnergyStore>();
                if (obj != null)
                {
                    var DATA = new JObject
                    {
                        { "capacity", obj.m_Capacity }
                    };
                    BLOCK.Add("ModuleEnergyStore", DATA);
                }
            }
            catch { }
        }

        static void GetFuelData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleFuelTank>();
                if (obj != null)
                {
                    var DATA = new JObject
                    {
                        { "capacity", obj.Capacity },
                        { "refill_rate", obj.RefillRate }
                    };
                    BLOCK.Add("ModuleFuelTank", DATA);
                }
            }
            catch { }
        }

        static void GetShieldData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleShieldGenerator>();
                if (obj != null)
                {
                    var DATA = new JObject
                    {
                        { "radius", obj.m_Radius },
                        { "initial_charge_energy", obj.m_InitialChargeEnergy },
                        { "energy_idle", obj.m_EnergyConsumptionPerSec },
                        { "energy_shield", obj.m_EnergyConsumedPerDamagePoint },
                        { "energy_heal", obj.m_EnergyConsumedPerPointHealed },
                        { "heal_interval", obj.m_HealingHeartbeatInterval }
                    };
                    BLOCK.Add("ModuleShieldGenerator", DATA);
                }
            }
            catch { }
        }

        static void GetHoverData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleHover>();
                if (obj != null)
                {
                    var jets = Base.GetComponentsInChildren<HoverJet>(true);
                    var DATA = new JObject
                    {
                        { "power", obj.HoverPower },
                        { "size", (int)obj.HoverSize },
                        { "jet_count", jets.Length },
                        { "jet_range", jets[0].forceRangeMax },
                        { "jet_radius", jets[0].jetRadius },
                        { "jet_force", jets[0].forceMax },
                    };
                    BLOCK.Add("ModuleHover", DATA);
                }
            }
            catch { }
        }

        static FieldInfo m_Force = typeof(BoosterJet).GetField("m_Force", binding);
        static FieldInfo m_FireRateFalloff = typeof(BoosterJet).GetField("m_FireRateFalloff", binding);
        static void GetBoosterData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleBooster>();
                if (obj != null)
                {
                    var jets = Base.GetComponentsInChildren<BoosterJet>(true);
                    var DATA = new JObject
                    {
                        { "fuel_per_second", obj.FuelBurnPerSecond() },
                        { "jet_count", jets.Length },
                        { "jet_force", (float)m_Force.GetValue(jets[0]) },
                        { "jet_falloff", (float)m_FireRateFalloff.GetValue(jets[0]) },
                    };
                    BLOCK.Add("ModuleBooster", DATA);
                }
            }
            catch { }
        }

        static void GetGyroData(JObject BLOCK, GameObject Base)
        {
            //try
            //{
            //    var obj = Base.GetComponent<ModuleGyro>();
            //    if (obj != null)
            //    {
            //        var DATA = new JObject
            //        {
            //            { "fuel_per_second", obj() },
            //            { "jet_count", jets.Length },
            //            { "jet_force", (float)m_Force.GetValue(jets[0]) },
            //            { "jet_falloff", (float)m_FireRateFalloff.GetValue(jets[0]) },
            //        };
            //        BLOCK.Add("ModuleGyro", DATA);
            //    }
            //}
            //catch { }
        }

        static FieldInfo m_TorqueParams = typeof(ModuleWheels).GetField("m_TorqueParams", binding);
        static FieldInfo m_WheelParams = typeof(ModuleWheels).GetField("m_WheelParams", binding);

        static void GetWheelData(JObject BLOCK, GameObject Base)
        {
            try
            {
                var obj = Base.GetComponent<ModuleWheels>();
                if (obj != null)
                {
                    ManWheels.TorqueParams torque = (ManWheels.TorqueParams)m_TorqueParams.GetValue(obj);
                    ManWheels.WheelParams wheel = (ManWheels.WheelParams)m_WheelParams.GetValue(obj);
                    var DATA = new JObject {
                        { "TorqueParams", new JObject {
                            { "basicFrictionTorque", torque.basicFrictionTorque },
                            { "fullCompressFrictionTorque", torque.fullCompressFrictionTorque},
                            { "passiveBrakeMaxTorque", torque.passiveBrakeMaxTorque},
                            { "reverseBrakeMaxRpm", torque.reverseBrakeMaxRpm},
                            { "torqueCurveMaxRpm", torque.torqueCurveMaxRpm},
                            { "torqueCurveMaxTorque", torque.torqueCurveMaxTorque} }
                        },
                        { "WheelParams", new JObject {
                            { "maxSuspensionAcceleration", wheel.maxSuspensionAcceleration },
                            { "radius", wheel.radius },
                            { "steerAngleMax", wheel.steerAngleMax },
                            { "steerSpeed", wheel.steerSpeed },
                            { "suspensionDamper", wheel.suspensionDamper },
                            { "suspensionQuadratic", wheel.suspensionQuadratic },
                            { "suspensionSpring", wheel.suspensionSpring },
                            { "suspensionTravel", wheel.suspensionTravel },
                            { "thicknessAngular", wheel.thicknessAngular } }
                        }
                    };
                    BLOCK.Add("ModuleWheels", DATA);
                }
            }
            catch { }
        }

        public static System.Collections.Generic.Dictionary<object, string> DeepDumpClassCache = new System.Collections.Generic.Dictionary<object, string>();

        private struct h { public string name;  public Type type; public Component component; public int depth; public string path; public JObject obj; }
        private static System.Collections.Generic.List<h> hl = new System.Collections.Generic.List<h>();

        public static JToken DeepDumpAll(Transform transform, int Depth)
        {
            hl.Clear();
            var d = DeepDumpTransform(transform, Depth);
            foreach (var l in hl)
            {
                l.obj.Add(l.name, DeepDumpObj(l.type, l.component, l.depth - 1, l.path));
            }
            return d;
        }

        static JToken DeepDumpTransform(Transform transform, int Depth, string path = "#")
        {
            if (Depth < 0)
            {
                return "Too deep!";
            }
            JObject OBJ = new JObject();
            var components = transform.gameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                Type type = component.GetType();
                string name = type.Name, _name = name;
                int _c = 0;
                while (OBJ.TryGetValue(_name, out _))
                {
                    _name = name + " (" + (++_c).ToString() + ")";
                }
                DeepDumpClassCache.Add(component, path + _name);
                hl.Add(new h { name = _name, type = type, component = component, depth = Depth - 1, path = path, obj = OBJ });
            }
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var Child = transform.GetChild(i);
                OBJ.Add("GameObject|" + Child.name, DeepDumpTransform(Child, Depth, path + "/" + Child.name));
            }
            return OBJ;
        }

        static Type ttrans = typeof(Transform);

        static JToken DeepDumpObj(Type type, object component, int Depth, string path)
        {
            if (Depth < 0)
            {
                Console.WriteLine("Too deep!");
                return "Too deep!";
            }
            var OBJ = new JObject();
            Console.WriteLine("-".PadRight(Depth * 2) + type.Name);
            var fields = type.GetFields(binding | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                try
                {
                    DeepDumpObjInternal(type, component, Depth, path, OBJ, field.Name, field.GetValue(component), field.FieldType);
                }
                catch (Exception E)
                {
                    Console.WriteLine($"{type.Name} {Depth}: {E.Message}, {E.StackTrace}");
                }
            }
            var props = type.GetProperties(binding | BindingFlags.DeclaredOnly);
            foreach (var prop in props)
            {
                try
                {
                    DeepDumpObjInternal(type, component, Depth, path, OBJ, prop.Name, prop.GetValue(component, null), prop.PropertyType);
                }
                catch (Exception E)
                {
                    Console.WriteLine($"{type.Name} {Depth}: {E.Message}, {E.StackTrace}");
                }
            }
            return OBJ;
        }

        private static void DeepDumpObjInternal(Type type, object component, int Depth, string path, JObject OBJ, string cName, object cValue, Type cType)
        {
            try
            {
                OBJ.Add(cName, new JValue(cValue));
            }
            catch
            {
                if (cType.IsArray)
                {
                    Array v_array = (Array)cValue;
                    if (v_array.Length == 0)
                    {
                        OBJ.Add(cName, new JArray());
                    }
                    else
                    {
                        var ARR = new JArray();
                        Type itemType = v_array.GetValue(0).GetType();
                        if (itemType.IsClass)
                        {
                            if (itemType == ttrans && type == ttrans)
                            {
                                ARR.Add("Transform (depth-cutoff)");
                            }
                            else
                            {
                                try
                                {
                                    int i = -1;
                                    foreach (var item in v_array)
                                    {
                                        i++;
                                        if (DeepDumpClassCache.TryGetValue(item, out string _path))
                                        {
                                            ARR.Add(_path);
                                        }
                                        else
                                        {
                                            string pathAdd = path + "/" + cName + "[" + i.ToString() + "]";
                                            DeepDumpClassCache.Add(item, pathAdd);
                                            ARR.Add(DeepDumpObj(itemType, item, Depth - 1, pathAdd));

                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            foreach (var item in v_array)
                            {
                                try
                                {
                                    ARR.Add(new JValue(item));
                                }
                                catch { }
                            }
                        }
                        OBJ.Add(cName, ARR);
                    }
                }
                else
                {
                    try
                    {
                        if (cType == ttrans && type == ttrans)
                        {
                            OBJ.Add(cName, "Transform (depth-cutoff)");
                        }
                        else
                        {
                            if (DeepDumpClassCache.TryGetValue(cValue, out string _path))
                            {
                                OBJ.Add(_path);
                            }
                            else
                            {
                                string pathAdd = path + "/" + cName;
                                DeepDumpClassCache.Add(cValue, pathAdd);
                                OBJ.Add(cName, DeepDumpObj(cType, cValue, Depth - 1, pathAdd));
                            }
                        }
                    }
                    catch { }
                }
            }
        }
    }
}