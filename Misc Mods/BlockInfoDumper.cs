using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using System.Collections.Generic;

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
                var mdmg = Base.GetComponent<ModuleDamage>();
                if (mdmg) // Redundant check
                {
                    ITEM.Add("health", mdmg.maxHealth);
                    ITEM.Add("fragility", mdmg.m_DamageDetachFragility);

                    // Death Explosion
                    GetExplosionData(ITEM, mdmg.deathExplosion, "DeathExplosion");
                }
                // Damageability
                var dmgb = Block.GetComponent<Damageable>();
                if (dmgb) // Redundant check
                {
                    ITEM.Add("damageable_type", dmgb.DamageableType.ToString());
                    ITEM.Add("damageable_type_int", (int)dmgb.DamageableType);
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
                // ModuleBooster
                GetBoosterData(ITEM, Base);
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
        static readonly FieldInfo Projectile_Explosion= typeof(Projectile).GetField("m_Explosion", binding);
        static readonly FieldInfo Projectile_LifeTime = typeof(Projectile).GetField("m_LifeTime", binding);
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
                        var projectile = firedata.m_BulletPrefab.GetComponent<Projectile>();
                        if (projectile)
                        {
                            DATA.Add("lifetime", (float)Projectile_LifeTime.GetValue(projectile));
                            var explosion = (Transform)Projectile_Explosion.GetValue(projectile);
                            if (explosion)
                                GetExplosionData(DATA, explosion);
                        }
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
                        { "range", jets[0].forceRangeMax },
                        { "radius", jets[0].jetRadius },
                        { "force", jets[0].forceMax },
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

                    JObject DATA = new JObject
                    {
                        { "uses_drive_controls", obj.UsesDriveControls },
                    };
                    var jets = Base.GetComponentsInChildren<BoosterJet>(true);
                    if (jets == null || jets.Length == 0)
                    {
                        var fans = Base.GetComponentsInChildren<FanJet>(true);
                        if (fans == null || fans.Length == 0)
                        {
                            DATA.Add("is_rotor", obj.IsRotor);
                            DATA.Add("fan_count", fans.Length);
                            DATA.Add("force", (float)m_Force.GetValue(fans[0]));
                            DATA.Add("spin_speed", fans[0].spinSpeed);
                            DATA.Add("spin_delta", fans[0].spinDelta);
                            DATA.Add("force", fans[0].force);
                            DATA.Add("back_force", fans[0].backForce);
                        }
                    }
                    else
                    {
                        DATA.Add("fuel_per_second", obj.FuelBurnPerSecond());
                        DATA.Add("jet_count", jets.Length);
                        DATA.Add("force", (float)m_Force.GetValue(jets[0]));
                        DATA.Add("falloff_rate", (float)m_FireRateFalloff.GetValue(jets[0]));
                    }
                    BLOCK.Add("ModuleBooster", DATA);
                }
            }
            catch { }
        }

        static void GetExplosionData(JObject BLOCK, Transform ExplosionBase, string newBlockName = "Explosion")
        {
            try
            {
                if (ExplosionBase == null) return;
                var explosion = ExplosionBase.GetComponent<Explosion>();
                if (explosion == null) return;
                var DATA = new JObject
                {
                    { "radius_outer", explosion.m_EffectRadius },
                    { "radius_inner", explosion.m_EffectRadiusMaxStrength },
                    { "damage", explosion.m_MaxDamageStrength },
                    { "impulse", explosion.m_MaxImpulseStrength },
                };
                BLOCK.Add(newBlockName, DATA);
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

        public static Dictionary<object, string> DeepDumpClassCache = new Dictionary<object, string>();
        public static Dictionary<Transform, string> CachedTransforms = new Dictionary<Transform, string>();

        private struct h { public string name; public Type type; public Component component; public int depth; public string path; public JObject obj; }
        private static List<h> hl = new List<h>();

        public static JToken DeepDumpAll(Transform transform, int Depth)
        {
            hl.Clear();
            CachedTransforms.Add(transform, "/");
            var d = DeepDumpTransform(transform, Depth);
            for (int i = 0; i < hl.Count; i++)
            {
                var l = hl[i];
                string name = l.name, _name = name;
                int _c = 0;
                while (l.obj.TryGetValue(_name, out _))
                {
                    _name = name + " (" + (++_c).ToString() + ")";
                }
                //DeepDumpClassCache.Add(l.component, l.path + _name);
                l.obj.Add(_name, DeepDumpObj(l.type, l.component, l.depth - 1, l.path));
            }
            return d;
        }

        static JToken DeepDumpTransform(Transform transform, int Depth, string path = "")
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
                hl.Add(new h { name = type.Name, type = type, component = component, depth = Depth - 1, path = path, obj = OBJ });
            }
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var Child = transform.GetChild(i);
                string name = "GameObject|" + Child.name, _name = name;
                int _c = 0;
                while (OBJ.TryGetValue(_name, out _))
                {
                    _name = name + " (" + (++_c).ToString() + ")";
                }
                string newpath = path + "/" + Child.name;
                CachedTransforms.Add(Child, newpath);
                OBJ.Add(_name, DeepDumpTransform(Child, Depth, newpath));
            }
            return OBJ;
        }

        static Type ttrans = typeof(Transform), tmesh = typeof(Mesh);
        static PropertyInfo transgo = ttrans.GetProperty("gameObject", binding);

        static JToken DeepDumpObj(Type type, object component, int Depth, string path)
        {
            if (Depth < 0)
            {
                //Console.WriteLine("Too deep!");
                return "Depth exceeded!";

            }
            var OBJ = new JObject();
            //Console.WriteLine("-".PadRight(Depth * 2) + type.Name);
            if (type == ttrans)
            {
                try
                {
                    DeepDumpObjInternal(type, component, Depth, path, OBJ, "gameObject", transgo.GetValue(component), transgo.PropertyType);
                }
                catch { }
            }
            var fields = type.GetFields(binding);// | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                try
                {
                    DeepDumpObjInternal(type, component, Depth, path, OBJ, field.Name, field.GetValue(component), field.FieldType);
                }
                catch (Exception E)
                {
                    Console.WriteLine($"{field.Name} {type.Name} {Depth}: {E.Message}, {E.StackTrace}");
                    //OBJ.Add(field.Name, "Error");
                }
            }
            var props = type.GetProperties(binding);// | BindingFlags.DeclaredOnly);
            foreach (var prop in props)
            {
                if (!prop.CanRead)
                {
                    OBJ.Add(prop.Name, "Write Only: " + prop.PropertyType.ToString());
                }
                else
                {
                    try
                    {
                        DeepDumpObjInternal(type, component, Depth, path, OBJ, prop.Name, prop.GetValue(component), prop.PropertyType);
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine($"{prop.Name} {type.Name} {Depth}: {E.Message}, {E.StackTrace}");
                        //OBJ.Add(prop.Name, "Error");
                    }
                }
            }
            return OBJ;
        }

        private static void DeepDumpObjInternal(Type type, object component, int Depth, string path, JObject OBJ, string fieldName, object fieldValue, Type fieldType)
        {
            if (fieldType == tmesh)
            {
                OBJ.Add(fieldName, new JObject());
            }
            else if (fieldType == ttrans)
            {
                if (type == ttrans)
                {
                    OBJ.Add(fieldName, "Transform " + (fieldValue as Transform).name);
                }
                else
                {
                    Transform fTransfrom = fieldValue as Transform;
                    if (CachedTransforms.TryGetValue(fTransfrom, out string fPath))
                    {
                        Console.WriteLine("Using past transform " + fTransfrom.name + "  " + path);
                        OBJ.Add(fieldName, fPath);
                    }
                    else
                    {
                        Console.WriteLine("Discovered new transform " + fTransfrom.name + "  " + path);
                        OBJ.Add(fieldName, DeepDumpTransform(fTransfrom, Depth, path));
                    }
                }
            }
            else
            {
                try
                {
                    JValue fieldJValue = JToken.FromObject(fieldValue) as JValue;
                    if (fieldJValue != null)
                    {
                        OBJ.Add(fieldName, fieldJValue);
                        return;
                    }
                    else if (fieldType.IsClass)
                    {
                        if (DeepDumpClassCache.TryGetValue(fieldValue, out string _path))
                        {
                            OBJ.Add(_path);
                        }
                        else
                        {
                            string pathAdd = path + "/" + fieldName;
                            DeepDumpClassCache.Add(fieldValue, pathAdd);
                            OBJ.Add(fieldName, DeepDumpObj(fieldType, fieldValue, Depth - 1, pathAdd));
                        }
                        return;
                    }
                    if (fieldValue is System.Collections.IList iList)
                    {
                        try
                        {
                            Type itemType;
                            if (type.IsGenericType) itemType = type.GetGenericArguments()[0];
                            else itemType = type.GetElementType();
                            for (int i = 0; i < iList.Count; i++)
                            {
                                OBJ.Add(fieldName, DeepDumpObj(itemType, iList[i], Depth - 1, path + "/" + fieldName + "[" + i + "]"));
                            }
                            return;
                        }
                        catch { }
                    }
                }
                catch { }

                try { OBJ.Add(fieldName, JToken.FromObject(fieldValue)); }
                catch
                {
                    try { OBJ.Add(fieldName, JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(fieldValue))); }
                    catch { /*OBJ.Add(fieldName, "Error");*/ }
                }
            }
        }
    }
}