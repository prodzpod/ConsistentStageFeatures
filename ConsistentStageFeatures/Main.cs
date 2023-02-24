using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Navigation;
using UnityEngine.AddressableAssets;

namespace LimitedInteractables
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(softdepBulwarksHaunt, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(softdepAetherium, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(softdepForgottenRelics, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(softdepFogboundLagoon, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(softdepShrineOfRepair, BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "prodzpod";
        public const string PluginName = "ConsistentStageFeatures";
        public const string PluginVersion = "1.0.0";
        public const string softdepBulwarksHaunt = "com.themysticsword.bulwarkshaunt";
        public const string softdepAetherium = "com.KomradeSpectre.Aetherium";
        public const string softdepForgottenRelics = "PlasmaCore.ForgottenRelics";
        public const string softdepFogboundLagoon = "JaceDaDorito.FBLStage";
        public const string softdepShrineOfRepair = "com.Viliger.ShrineOfRepair";
        // public const string softdepQuasitemporalObservatory = "prodzpod.SuffersOnThunderkit";
        public static ManualLogSource Log;
        public static Harmony Harmony;
        public static PluginInfo pluginInfo;
        public static ConfigFile Config;
        public static ConfigEntry<string> TimescaleOverwrite;
        public static ConfigEntry<string> StageChances;
        public static ConfigEntry<int> MaxVoidSeeds;
        public static ConfigEntry<bool> BuffBrazierOnStage1;
        public static ConfigEntry<bool> RemoveRandomBuffBrazier;
        public static ConfigEntry<bool> RedPrinterOnSiphoned;
        public static ConfigEntry<bool> GreenPrinterOnPlains;
        public static ConfigEntry<bool> YellowPrinterOnRoost;
        public static ConfigEntry<bool> OrderShrineOnObservatory;
        public static ConfigEntry<bool> LunarBudOnStage2;
        public static ConfigEntry<bool> RemoveRandomLunarBud;
        public static ConfigEntry<bool> AqueductButtonNoRelease;
        public static ConfigEntry<bool> NKuhanaVoidGreen;
        public static ConfigEntry<bool> GoldShrineOnStage3;
        public static ConfigEntry<bool> RemoveRandomGoldShrine;
        public static ConfigEntry<bool> SpecialChestOnPools;
        public static ConfigEntry<bool> HankOffersDrink;
        public static ConfigEntry<bool> ShrineRepairOnStage5;
        public static ConfigEntry<bool> RemoveRandomShrineRepair;
        public static ConfigEntry<bool> SageShrineOnSatellite;
        public static ConfigEntry<bool> RemoveRandomSageShrine;
        public static ConfigEntry<bool> BulwarkSwordOnSatellite;
        public static ConfigEntry<bool> ScrapperOnMoon;
        public static ConfigEntry<bool> RemoveRandomScrapper;
        public static ConfigEntry<bool> FHTeleporterOnStage6;
        public static ConfigEntry<bool> RemoveRandomFHTeleporter;
        public static ConfigEntry<bool> VieldsOnStage7;
        public static ConfigEntry<bool> LocusOnStage10;
        public static ConfigEntry<int> GoldShrineCost;
        public static ConfigEntry<bool> GoldenCoastCombatShrine;
        public static ConfigEntry<bool> FHRadarScannerEffect;
        public static ConfigEntry<bool> HauntedWoodShrine;
        public static ConfigEntry<bool> VieldsNoLoot;

        public static bool hanked = false;

        public void Awake()
        {
            pluginInfo = Info;
            Harmony = new Harmony(PluginGUID); // uh oh!
            Log = Logger;
            Config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, PluginGUID + ".cfg"), true);

            TimescaleOverwrite = Config.Bind("General", "Timescale Unlocks", "goldshores, forgottenhaven", "List of scenes and run time, separated by commas. By default, disables time stop for FH and Golden Coast.");
            StageChances = Config.Bind("General", "Stage Chances", "skymeadow - 3", "List of scenes and chance weight, separated by commas. by default makes sky meadow 3 times as likely so artifact grinding isnt godawful");
            RoR2Application.onLoad += PatchStageChances;
            MaxVoidSeeds = Config.Bind("General", "Max Void Seeds", 1, "Number of Void Seeds that can be spawned at a single stage. Default is 3.");
            if (MaxVoidSeeds.Value != 3) Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC1/VoidCamp/iscVoidCamp.asset").WaitForCompletion().maxSpawnsPerStage = MaxVoidSeeds.Value;

            BuffBrazierOnStage1 = Config.Bind("Stage 1", "Buff Brazier on Stage 1", true, "Guaranteed Buff Brazier on stage 1.");
            RemoveRandomBuffBrazier = Config.Bind("Stage 1", "Remove Random Buff Brazier Spawns", true, "only the fixed spawn exists");
            if (Mods(softdepAetherium)) checkBuffBrazier();
            void checkBuffBrazier()
            {
                if (BuffBrazierOnStage1.Value)
                {
                    foreach (var scene in new string[] { "golemplains", "golemplains2", "blackbeach", "blackbeach2", "snowyforest"/* , "quasitemporalobservatory" */ })
                        SpawnRandomly(scene, Aetherium.Interactables.BuffBrazier.InteractableSpawnCard);
                }
                if (RemoveRandomBuffBrazier.Value) Aetherium.Interactables.BuffBrazier.InteractableSpawnCard.maxSpawnsPerStage = 0;
            }
            RedPrinterOnSiphoned = Config.Bind("Stage 1", "Red Printer on Siphoned Forest", false, "make sure to balance printer before this");
            if (RedPrinterOnSiphoned.Value) SpawnGuaranteed("snowyforest", "SpawnCards/InteractableSpawnCard/iscDuplicatorMilitary", new Vector3(-65.7508f, 80.6369f, -186.9797f), new Vector3(5.6833f, 33.3465f, 2.9875f));
            GreenPrinterOnPlains = Config.Bind("Stage 1", "Green Printer on Golem Plains", false, "make sure to balance printer before this");
            if (GreenPrinterOnPlains.Value)
            {
                SpawnGuaranteed("golemplains", "SpawnCards/InteractableSpawnCard/iscDuplicatorLarge", new Vector3(-196.3943f, -138.4122f, 39.6045f), new Vector3(17.3021f, 93.9709f, 347.3439f));
                SpawnGuaranteed("golemplains2", "SpawnCards/InteractableSpawnCard/iscDuplicatorLarge", new Vector3(-217.0215f, 40.3993f, -29.7606f), new Vector3(25.4828f, 230.3522f, 347.7884f));
            }
            YellowPrinterOnRoost = Config.Bind("Stage 1", "Yellow Printer on Distant Roost", false, "make sure to balance printer before this");
            if (YellowPrinterOnRoost.Value)
            {
                SpawnGuaranteed("blackbeach", "SpawnCards/InteractableSpawnCard/iscDuplicatorWild", new Vector3(203.9892f, -122.2774f, -105.9803f), new Vector3(45.3964f, 71.6722f, 7.0832f));
                SpawnGuaranteed("blackbeach2", "SpawnCards/InteractableSpawnCard/iscDuplicatorWild", new Vector3(-137.3705f, 46.7941f, -97.3107f), new Vector3(15.839f, 115.0119f, 10.6805f));
            }
            // if (Mods(softdepQuasitemporalObservatory)) {
            //  OrderShrineOnObservatory = Config.Bind("Stage 1", "Shrine of Order on Quasitemporal Observatory", true, "hehe");
            //  if (OrderShrineOnObservatory.Value) SpawnGuaranteed("quasitemporalobservatory", "", new Vector3(), new Vector3());
            // }

            LunarBudOnStage2 = Config.Bind("Stage 2", "Lunar bud on Stage 2", true, "Guaranteed Lunar bud on stage 2.");
            if (LunarBudOnStage2.Value)
            {
                SpawnGuaranteed("goolake", "SpawnCards/InteractableSpawnCard/iscLunarChest", new Vector3(285.1561f, -61.79442f, -193.2947f));
                SpawnGuaranteed("ancientloft", "SpawnCards/InteractableSpawnCard/iscLunarChest", new Vector3(-70.1913f, 81.07519f, 221.0971f));
                SpawnGuaranteed("foggyswamp", "SpawnCards/InteractableSpawnCard/iscLunarChest", new Vector3(-121.8996f, -126.0044f, -235.4447f));
                if (Mods(softdepForgottenRelics)) SpawnGuaranteed("drybasin", "SpawnCards/InteractableSpawnCard/iscLunarChest", new Vector3(-228.2513f, 75.6862f, -86.5036f));
            }
            RemoveRandomLunarBud = Config.Bind("Stage 2", "Remove Random Lunar Bud Spawns", false, "only the fixed spawn exists");
            if (RemoveRandomLunarBud.Value) LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscLunarChest").maxSpawnsPerStage = 0;
            AqueductButtonNoRelease = Config.Bind("Stage 2", "Abandoned Aqueduct Pressure Plate Stays Pressed", false, "set to true when you're using difficulty mods, for solo players");
            if (AqueductButtonNoRelease.Value) On.RoR2.PressurePlateController.SetSwitch += (orig, self, input) => { if (input == true) orig(self, input); };
            NKuhanaVoidGreen = Config.Bind("Stage 2", "N`kuhana Skeleton Drops Void Green", true, "i think you deserve it for going through Wetland Aspect");
            if (NKuhanaVoidGreen.Value) HackSkeletonForceSpawn();
            // Aphelian Sanctuary - KannaQoL (cleansing pool)
            // Dry Basin - Forgotten Relics (tar altar)

            GoldShrineOnStage3 = Config.Bind("Stage 3", "Altar of Gold on Stage 3", true, "Guaranteed Altar of Gold on stage 3.");
            if (GoldShrineOnStage3.Value)
            {
                LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess").maxSpawnsPerStage = 1;
                SpawnRandomly("frozenwall", "SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess");
                SpawnRandomly("wispgraveyard", "SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess");
                SpawnRandomly("sulfurpools", "SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess");
                if (Mods(softdepFogboundLagoon)) SpawnRandomly("FBLScene", "SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess");
            }
            RemoveRandomGoldShrine = Config.Bind("Stage 3", "Remove Random Altar of Gold Spawns", false, "only the fixed spawn exists");
            if (RemoveRandomGoldShrine.Value) LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess").maxSpawnsPerStage = 0;
            // Rallypoint Delta - Vanilla (timed chest) & BetterDrones (TC prototype)
            // Scorched Acres - Mystic's Items (ancient mask)
            // Fogbound Lagoon - Fogbound Lagoon (timed chest)
            SpecialChestOnPools = Config.Bind("Stage 3", "Special Chests on Sulfur Pools", true, "Spawns 3 Cloaked Chests and 3 Lockboxes (1 of which will be encrusted if player has void, encrusted nerf kinda)");
            if (SpecialChestOnPools.Value)
            {
                SpawnGuaranteed("sulfurpools", "SpawnCards/InteractableSpawnCard/iscChest1Stealthed", new Vector3(-0.1489f, -11.2628f, -58.985f), new Vector3(332.4133f, 147.7081f, 333.2504f));
                SpawnGuaranteed("sulfurpools", "SpawnCards/InteractableSpawnCard/iscLockbox", new Vector3(22.7433f, -5.3875f, -236.629f), new Vector3(20.4497f, 16.1306f, 5.5421f));
                SpawnGuaranteed("sulfurpools", "SpawnCards/InteractableSpawnCard/iscLockbox", new Vector3(-160.0294f, 7.244f, 122.3465f), new Vector3(27.0561f, 87.8281f, 352.8062f));
            }
            HankOffersDrink = Config.Bind("Stage 3", "Hank Offers Drink", true, "Talking to Hank will result in a random drink.");
            if (Mods(softdepFogboundLagoon)) HandleHank();

            // Siren's Call - Vanilla (AWU)
            // Sundered Grove - Vanilla (Cool legendary chest gimmick)
            // Abyssal Depths - Direseeker (Direseeker)

            ShrineRepairOnStage5 = Config.Bind("Stage 5", "Shrine of Repair on Stage 5", true, "Guaranteed Shrine of Repair on stage 5.");
            RemoveRandomShrineRepair = Config.Bind("Stage 5", "Remove Random Shrine of Repair Spawns", false, "only the fixed spawn exists");
            if (Mods(softdepShrineOfRepair)) HandleShrineOfRepair();
            // Sky Meadow - Vanilla (Artifact Terminal)
            SageShrineOnSatellite = Config.Bind("Stage 5", "Sage`s Shrine on Slumbering Satellite", true, "makes it fixed");
            RemoveRandomSageShrine = Config.Bind("Stage 5", "Remove Random Sage`s Shrine Spawns", true, "only the fixed spawn exists");
            if (Mods(softdepForgottenRelics)) checkSageShrine();
            void checkSageShrine()
            {
                if (ShrineRepairOnStage5.Value) SpawnGuaranteed("slumberingsatellite", FRCSharp.VF2ContentPackProvider.iscSagesShrine, new Vector3(70.5048f, 108.5265f, -323.2408f), new Vector3(11.9998f, 144.8941f, 337.7272f));
                if (RemoveRandomSageShrine.Value) FRCSharp.VF2ContentPackProvider.iscSagesShrine.maxSpawnsPerStage = 0;
            };
            BulwarkSwordOnSatellite = Config.Bind("Stage 5", "Bulwark Sword on Slumbering Satellite", true, "haunted is no longer rng");
            if (Mods(softdepForgottenRelics, softdepBulwarksHaunt)) checkBulwark();
            void checkBulwark()
            {
                if (BulwarkSwordOnSatellite.Value) SpawnGuaranteed("slumberingsatellite", BulwarksHaunt.Items.Sword.swordObjPrefab, new Vector3(-157.3885f, 69.9324f, 154.4549f), new Vector3(344.073f, 60.7323f, 1.596f));
            };

            ScrapperOnMoon = Config.Bind("Commencement", "Scrapper on Moon", false, "Scrapper on the Moon. Use it if you have scrappers rebalanced.");
            if (ScrapperOnMoon.Value)
            {
                SpawnGuaranteed("moon", "SpawnCards/InteractableSpawnCard/iscScrapper", new Vector3(804.2821f, 287.1601f, 214.5148f), new Vector3(351.5653f, 249.8895f, 10.3f));
                SpawnGuaranteed("moon2", "SpawnCards/InteractableSpawnCard/iscScrapper", new Vector3(17.2254f, -190.6997f, -307.0245f), new Vector3(1.2947f, 87.988f, 16.1012f));
            }
            RemoveRandomScrapper = Config.Bind("Commencement", "Remove Random Scrapper Spawns", false, "only the fixed spawn exists");
            if (RemoveRandomScrapper.Value) LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscScrapper").maxSpawnsPerStage = 0;
            // Commencement - Vanilla (Shrine of Order, Lunar Bud)
            // Other Commencement Changes - BetterMoonPillars (pillars)

            FHTeleporterOnStage6 = Config.Bind("Looping", "Shattered Teleporter on Stage 6", true, "Guaranteed Shattered Teleporters on stage 6.");
            RemoveRandomFHTeleporter = Config.Bind("Looping", "Remove Random Shattered Teleporter Spawns", false, "only the fixed spawn exists");
            if (Mods(softdepForgottenRelics)) checkTeleporter();
            void checkTeleporter()
            {
                if (FHTeleporterOnStage6.Value) foreach (var scene in new string[] { "golemplains", "golemplains2", "blackbeach", "blackbeach2", "snowyforest"/* , "quasitemporalobservatory" */ })
                {
                    SceneDirector.onPrePopulateSceneServer += director =>
                    {
                        if (SceneCatalog.mostRecentSceneDef.cachedName != scene || Run.instance.loopClearCount <= 0) return;
                        DirectorPlacementRule placementRule = new DirectorPlacementRule { placementMode = DirectorPlacementRule.PlacementMode.Random };
                        DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(FRCSharp.VF2ContentPackProvider.iscLooseRelic, placementRule, Run.instance.runRNG));
                        DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(FRCSharp.VF2ContentPackProvider.iscShatteredTeleporter, placementRule, Run.instance.runRNG));
                    };
                }   
                if (RemoveRandomScrapper.Value) FRCSharp.VF2ContentPackProvider.iscShatteredTeleporter.maxSpawnsPerStage = 0;
            };
            VieldsOnStage7 = Config.Bind("Looping", "Null Portal on Stage 7", false, "Guaranteed Null Portal on stage 7.");
            // Stolen from vanillavoid
            InteractableSpawnCard iscNullPortal = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            iscNullPortal.name = "iscSpecialVoidFieldPortal";
            iscNullPortal.prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/PortalArena");
            iscNullPortal.sendOverNetwork = true;
            iscNullPortal.hullSize = HullClassification.Golem;
            iscNullPortal.nodeGraphType = MapNodeGroup.GraphType.Ground;
            iscNullPortal.requiredFlags = NodeFlags.None;
            iscNullPortal.forbiddenFlags = NodeFlags.None;
            iscNullPortal.directorCreditCost = 999999;
            iscNullPortal.occupyPosition = true;
            iscNullPortal.orientToFloor = false;
            iscNullPortal.skipSpawnWhenSacrificeArtifactEnabled = false;
            iscNullPortal.maxSpawnsPerStage = 0;
            if (VieldsOnStage7.Value) TeleporterInteraction.onTeleporterChargedGlobal += tp =>
            {
                if (Run.instance.loopClearCount > 0 && Run.instance.stageClearCount % Run.stagesPerLoop == 1)
                    DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(iscNullPortal, new DirectorPlacementRule
                    {
                        minDistance = 1f,
                        maxDistance = 25f,
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        position = tp.transform.position,
                        spawnOnTarget = tp.transform
                    }, Run.instance.stageRng));
            };
            // Stage 8 - Vanilla (Celestial Portal)
            // Stage 9 - idk lol
            LocusOnStage10 = Config.Bind("Looping", "Void Portal on Stage 10", true, "Guaranteed Void Portal on stage 10.");
            if (LocusOnStage10.Value) TeleporterInteraction.onTeleporterChargedGlobal += tp =>
            {
                SpawnCard isc = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscVoidPortal");
                if (Run.instance.loopClearCount > 0 && Run.instance.stageClearCount % Run.stagesPerLoop == 4 && !(tp.portalSpawners?.ToList()?.First(x => x.portalSpawnCard == isc)?.willSpawn ?? true))
                    DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(isc, new DirectorPlacementRule
                    {
                        minDistance = 1f,
                        maxDistance = 25f,
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        position = tp.transform.position,
                        spawnOnTarget = tp.transform
                    }, Run.instance.stageRng));
            };
            // Stage 10 - Vanilla (Primordial Teleporter)

            GoldShrineCost = Config.Bind("Hidden Realm", "Altar of Gold Cost", 100, "Scales with time, vanilla is 200 (8 chests)");
            if (GoldShrineCost.Value != 200) LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Shrines/ShrineGoldshoresAccess").GetComponent<PurchaseInteraction>().cost = 100;
            GoldenCoastCombatShrine = Config.Bind("Hidden Realm", "Guaranteed Combat Shrines in Golden Coast", true, "money printer");
            if (GoldenCoastCombatShrine.Value)
            {
                SpawnGuaranteed("goldshores", "SpawnCards/InteractableSpawnCard/iscShrineCombat", new Vector3(-73.98104f, -6.325237f, 82.57056f), new Vector3(0f, 200.0384f, 0f));
                SpawnGuaranteed("goldshores", "SpawnCards/InteractableSpawnCard/iscShrineCombat", new Vector3(75.62061f, -8.77954f, 116.1982f), new Vector3(0f, 281.339f, 0f));
                SpawnGuaranteed("goldshores", "SpawnCards/InteractableSpawnCard/iscShrineCombat", new Vector3(-5.5166f, -9.3453f, -60.1567f), new Vector3(0f, 338.058f, 0f));
                SpawnGuaranteed("goldshores", "SpawnCards/InteractableSpawnCard/iscShrineBlood", new Vector3(-9.266755f, 69.01228f, -66.08608f));
            }
            FHRadarScannerEffect = Config.Bind("Hidden Realm", "Forgotten Haven Log and Cell Radar Scanner effect", true, "The purchasable logbook entry & putting in cells in the central portal triggers a Radar Scanner effect around it.");
            if (Mods(softdepForgottenRelics)) checkRadar();
            void checkRadar()
            {
                if (FHRadarScannerEffect.Value) FHRadarScannerPatch();
            };
            HauntedWoodShrine = Config.Bind("Hidden Realm", "Shrine of the Woods on Bulwark`s Haunt", true, "No regen?");
            if (Mods(softdepBulwarksHaunt)) checkWoods();
            void checkWoods()
            {
                if (HauntedWoodShrine.Value) SpawnGuaranteed("BulwarksHaunt_GhostWave", "SpawnCards/InteractableSpawnCard/iscShrineHealing", new Vector3(31.1182f, 62.9546f, -29.4124f), new Vector3(350.209f, 1.7844f, 350.0595f));
            };
            VieldsNoLoot = Config.Bind("Hidden Realm", "Void Fields No Chests", true, "It already gives 9 items including a red bruh");
            if (VieldsNoLoot.Value) SceneDirector.onPrePopulateSceneServer += (dir) => { if (SceneCatalog.mostRecentSceneDef.cachedName == "arena") dir.interactableCredit = 0; };
        }

        public static void PatchStageChances()
        {
            List<string> timescales = TimescaleOverwrite.Value.Split(',').ToList().ConvertAll(x => x.Trim());
            On.RoR2.Run.ShouldUpdateRunStopwatch += (orig, self) =>
            {
                if (self.livingPlayerCount <= 0) return orig(self);
                bool ret = orig(self);
                if (timescales.Contains(SceneCatalog.mostRecentSceneDef.cachedName)) return !ret;
                return ret;
            };
            Dictionary<string, float> sceneChance = new();
            foreach (var kv in StageChances.Value.Split(',').ToList().ConvertAll(x => x.Trim()))
            {
                List<string> _kv = kv.Split('-').ToList().ConvertAll(x => x.Trim());
                if (_kv.Count != 2) continue;
                sceneChance.Add(_kv[0], float.Parse(_kv[1]));
            }
            foreach (var scname in new string[] { "sgStage1", "sgStage2", "sgStage3", "sgStage4", "sgStage5" })
            {
                SceneCollection sc = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/" + scname + ".asset").WaitForCompletion();
                List<SceneCollection.SceneEntry> list = new(sc._sceneEntries);
                for (var i = 0; i < list.Count; i++)
                {
                    if (sceneChance.ContainsKey(list[i].sceneDef.cachedName))
                    {
                        SceneCollection.SceneEntry entry = list[i];
                        entry.weight = sceneChance[list[i].sceneDef.cachedName];
                        list[i] = entry;
                    }
                }
                sc._sceneEntries = list.ToArray();
            }
        }

        public static void HackSkeletonForceSpawn()
        {
            Stage.onStageStartGlobal += (stage) =>
            {
                GameObject skeleton = GameObject.Find("HOLDER: Hidden Altar Stuff")?.transform?.Find("AltarCenter")?.Find("AltarSkeletonBody")?.gameObject;
                if (skeleton != null)
                {
                    GameObjectUnlockableFilter filter = skeleton.GetComponent<GameObjectUnlockableFilter>();
                    filter.forbiddenUnlockable = "";
                    filter.forbiddenUnlockableDef = null;
                    filter.Networkactive = true;
                    filter.active = true;
                    skeleton.SetActive(true);
                    Log.LogDebug("*Bad to the Bone Riff*");
                }
            };
            GlobalEventManager.onCharacterDeathGlobal += (report) =>
            {
                if (report?.victimBody?.name == "AltarSkeletonBody")
                    PickupDropletController.CreatePickupDroplet(Run.instance.treasureRng.NextElementUniform(Run.instance.availableVoidTier2DropList), report.victimBody.corePosition, new Vector3(-15f, 13f, -20f));
            };
        }

        public static void HandleHank()
        {
            string[] items = { "Infusion", "HealingPotion", "AttackSpeedAndMoveSpeed", "SprintBonus", "MysticsItems_CoffeeBoostOnItemPickup", "Tonic", "Ketchup", "MysteriousVial", "RandomEquipmentTrigger", "<color=#ED7FCD>VV_ITEM_EHANCE_VIALS_ITEM", "ItemDefSeepingOcean", "SiphonOnLowHealth", "DropOfNecrosis", "SpatteredCollection", "ItemDefSubmergingCistern", "MysticsItems_GateChalice", "EQUIPMENT_JAR_OF_RESHAPING", "Molotov", "PressurizedCanister", "VendingMachine" };
            if (HankOffersDrink.Value)
            {
                Stage.onStageStartGlobal += _ => hanked = false;
                On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
                {
                    orig(self, activator);
                    if (self.gameObject.name == "BadToTheBone" && !hanked)
                    {
                        hanked = true;
                        List<PickupIndex> available =
                            ItemCatalog.allItemDefs.Where(x => items.Contains(x.name)).Select(x => PickupCatalog.FindPickupIndex(x.itemIndex))
                            .Union(EquipmentCatalog.equipmentDefs.Where(x => items.Contains(x.name)).Select(x => PickupCatalog.FindPickupIndex(x.equipmentIndex))).ToList();
                        PickupIndex ret = Run.instance.treasureRng.NextElementUniform(available);
                        PickupDropletController.CreatePickupDroplet(ret, self.gameObject.transform.position + Vector3.up * 1.5f, Vector3.up * 5f + self.gameObject.transform.forward * 20f);
                    }
                };
            }
        }

        public static void HandleShrineOfRepair()
        {
            InteractableSpawnCard isc = ShrineOfRepair.Modules.ShrineOfRepairConfigManager.UsePickupPickerPanel.Value ? ShrineOfRepair.Modules.Interactables.ShrineOfRepairPicker.shrineSpawnCard : ShrineOfRepair.Modules.Interactables.ShrineOfRepairPurchase.shrineSpawnCard;
            if (ShrineRepairOnStage5.Value)
            {
                SpawnRandomly("skymeadow", isc);
                if (Mods(softdepForgottenRelics)) SpawnRandomly("slumberingsatellite", isc);
            }
            if (RemoveRandomShrineRepair.Value) isc.maxSpawnsPerStage = 0;
        }

        public static void FHRadarScannerPatch()
        {
            On.RoR2.RadiotowerTerminal.GrantUnlock += (orig, self, interactor) =>
            {
                orig(self, interactor);
                if (SceneCatalog.mostRecentSceneDef.cachedName == "forgottenhaven") NetworkServer.Spawn(Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner"), self.gameObject.transform.position, Quaternion.identity));
            };
            Harmony.PatchAll(typeof(PatchBatteryInteraction));
        }

        [HarmonyPatch(typeof(FRCSharp.BatteryContainerInteraction), nameof(FRCSharp.BatteryContainerInteraction.OnInteractionBegin))]
        public class PatchBatteryInteraction
        {
            public static void Postfix(FRCSharp.BatteryContainerInteraction __instance)
            {
                NetworkServer.Spawn(Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner"), __instance.gameObject.transform.position, Quaternion.identity));
            }
        }

        public static void SpawnGuaranteed(string scene, string dir, Vector3 pos, Vector3 rot = default) { SpawnGuaranteed(scene, LegacyResourcesAPI.Load<SpawnCard>(dir), pos, rot); }
        public static void SpawnGuaranteed(string scene, SpawnCard spawnCard, Vector3 pos, Vector3 rot = default)
        {
            Stage.onStageStartGlobal += self =>
            {
                if (self.sceneDef.cachedName != scene) return;
                DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule() { placementMode = DirectorPlacementRule.PlacementMode.Direct };
                GameObject spawnedInstance = spawnCard.DoSpawn(pos, Quaternion.Euler(rot), new DirectorSpawnRequest(spawnCard, directorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                spawnedInstance.transform.eulerAngles = rot;
                NetworkServer.Spawn(spawnedInstance);
            };
            Log.LogDebug($"Added a Guaranteed Spawn of {spawnCard.prefab.name} at {scene}");
        }
        public static void SpawnGuaranteed(string scene, GameObject obj, Vector3 pos, Vector3 rot = default)
        {
            Stage.onStageStartGlobal += self =>
            {
                if (self.sceneDef.cachedName != scene) return;
                DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule() { placementMode = DirectorPlacementRule.PlacementMode.Direct };
                GameObject spawnedInstance = Instantiate(obj, pos, Quaternion.Euler(rot));
                spawnedInstance.transform.eulerAngles = rot;
                NetworkServer.Spawn(spawnedInstance);
            };
            Log.LogDebug($"Added a Guaranteed Spawn of {obj.name} at {scene}");
        }

        public static void SpawnRandomly(string scene, string dir) { SpawnRandomly(scene, LegacyResourcesAPI.Load<SpawnCard>(dir)); }
        public static void SpawnRandomly(string scene, SpawnCard spawnCard)
        {
            SceneDirector.onPrePopulateSceneServer += director =>
            {
                if (SceneCatalog.mostRecentSceneDef.cachedName != scene) return;
                DirectorPlacementRule placementRule = new DirectorPlacementRule { placementMode = DirectorPlacementRule.PlacementMode.Random };
                GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, Run.instance.runRNG));
                if (gameObject)
                {
                    PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                    if (component && component.costType == CostTypeIndex.Money) component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                }
            };
            Log.LogDebug($"Added a Random Spawn of {spawnCard.name} at {scene}");
        }
        public static bool Mods(params string[] arr)
        {
            for (int i = 0; i < arr.Length; i++) if (!Chainloader.PluginInfos.ContainsKey(arr[i])) return false;
            return true;
        }
    }
}
