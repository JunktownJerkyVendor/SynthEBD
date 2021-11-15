﻿using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace SynthEBD
{
    public class MainLoop
    {
        //Synchronous version for debugging only
        //public static void RunPatcher(List<AssetPack> assetPacks, List<HeightConfig> heightConfigs, HashSet<NPCAssignment> specificNPCAssignments, BlockList blockList, HashSet<string> linkedNPCNameExclusions, HashSet<LinkedNPCGroup> linkedNPCGroups, HashSet<TrimPath> trimPaths)
        public static async Task RunPatcher(List<AssetPack> assetPacks, List<HeightConfig> heightConfigs, HashSet<NPCAssignment> specificNPCAssignments, BlockList blockList, HashSet<string> linkedNPCNameExclusions, HashSet<LinkedNPCGroup> linkedNPCGroups, HashSet<TrimPath> trimPaths)
        {
            Logger.UpdateStatus("Patching", false);
            Logger.StartTimer();

            BlockedNPC blockListNPCEntry;
            BlockedPlugin blockListPluginEntry;
            HashSet<LinkedNPCGroupInfo> generatedLinkGroups = new HashSet<LinkedNPCGroupInfo>();
            HashSet<FlattenedAssetPack> maleAssetPacks = new HashSet<FlattenedAssetPack>();
            HashSet<FlattenedAssetPack> femaleAssetPacks = new HashSet<FlattenedAssetPack>();
            HashSet<FlattenedAssetPack> availableAssetPacks = new HashSet<FlattenedAssetPack>();

            bool blockAssets;
            bool blockBodyGen;
            bool blockHeight;
            bool bodyGenAssignedWithAssets;

            //var AssetPacksByRaceGender = new Dictionary<Tuple<FormKey, Gender>, HashSet<FlattenedAssetPack>>();

            if (PatcherSettings.General.bChangeMeshesOrTextures)
            {
                HashSet<FlattenedAssetPack> flattenedAssetPacks = new HashSet<FlattenedAssetPack>();
                flattenedAssetPacks = assetPacks.Select(x => FlattenedAssetPack.FlattenAssetPack(x, PatcherSettings.General.RaceGroupings, PatcherSettings.General.bEnableBodyGenIntegration)).ToHashSet();
                maleAssetPacks = flattenedAssetPacks.Where(x => x.Gender == Gender.male).ToHashSet();
                femaleAssetPacks = flattenedAssetPacks.Where(x => x.Gender == Gender.female).ToHashSet();
                
                //AssetPacksByRaceGender = DictionaryMapper.GetAssetPacksByRaceGender(flattenedAssetPacks, generalSettings.patchableRaces);
            }

            int npcCounter = 0;
            foreach (var npc in GameEnvironmentProvider.MyEnvironment.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
            {
                npcCounter++;
                if (npcCounter % 100 == 0) 
                {
                    Logger.LogMessage("Examined " + npcCounter.ToString() + " NPCs in " + Logger.GetEllapsedTime()); 
                }

                var currentNPCInfo = new NPCInfo(npc, linkedNPCGroups, generatedLinkGroups, specificNPCAssignments);

                blockListNPCEntry = BlockListHandler.GetCurrentNPCBlockStatus(blockList, npc.FormKey);
                blockListPluginEntry = BlockListHandler.GetCurrentPluginBlockStatus(blockList, npc.FormKey);

                if (blockListNPCEntry.Assets || blockListPluginEntry.Assets) { blockAssets = true; }
                else { blockAssets = false; }

                if (blockListNPCEntry.BodyGen || blockListPluginEntry.BodyGen) { blockBodyGen = true; }
                else { blockBodyGen = false; }

                if (blockListNPCEntry.Height || blockListPluginEntry.Height) { blockHeight = true; }
                else { blockHeight = false; }

                bodyGenAssignedWithAssets = false;

                /*
                if (generalSettings.bExcludePlayerCharacter && npc.FormKey.ModKey.ToString() == "Skyrim.esm" && npc.FormKey.IDString == "000007")
                {
                    continue;
                }
                */

                /*
                if (generalSettings.bExcludePresets && npc.EditorID.Contains("Preset"))
                {
                    continue;
                }
                */

                // Assets/BodyGen assignment
                if (PatcherSettings.General.bChangeMeshesOrTextures && !blockAssets && PatcherSettings.General.patchableRaces.Contains(currentNPCInfo.AssetsRace))
                {
                    //var availableAssetPacks = AssetPacksByRaceGender[new Tuple<FormKey, Gender>(currentNPCInfo.AssetsRace, currentNPCInfo.Gender)];
                    switch (currentNPCInfo.Gender)
                    {
                        case Gender.female: availableAssetPacks = femaleAssetPacks; break;
                        case Gender.male: availableAssetPacks = maleAssetPacks; break;
                    }
                    
                    var assignedComboAndBodyGen = AssetAndBodyGenSelector.ChooseCombinationAndBodyGen(out bodyGenAssignedWithAssets, availableAssetPacks, currentNPCInfo);
                }

                // BodyGen assignment (if assets not assigned in Assets/BodyGen section)
                if (PatcherSettings.General.bEnableBodyGenIntegration && !blockBodyGen && PatcherSettings.General.patchableRaces.Contains(currentNPCInfo.BodyGenRace) && !bodyGenAssignedWithAssets)
                {

                }

                // Height assignment
                if (PatcherSettings.General.bChangeHeight && !blockHeight && PatcherSettings.General.patchableRaces.Contains(currentNPCInfo.HeightRace))
                {

                }
            }

            Logger.StopTimer();
            Logger.LogMessage("Finished patching.");
            Logger.UpdateStatus("Finished Patching", false);
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            Logger.UpdateStatus("Finished Patching", false);
        }
    }
}
