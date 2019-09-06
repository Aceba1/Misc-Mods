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
                if (md) ITEM.Add("health", md.maxHealth); // Redundant check
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

        static readonly FieldInfo WeaponRound_Damage = typeof(WeaponRound).GetField("m_Damage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        static readonly FieldInfo WeaponRound_DamageType = typeof(WeaponRound).GetField("m_DamageType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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

        static readonly FieldInfo ModuleDrill_DamagePerSecond = typeof(ModuleDrill).GetField("damagePerSecond", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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

        static readonly FieldInfo ModuleHammer_ImpactDamage = typeof(ModuleHammer).GetField("impactDamage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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
    }
}