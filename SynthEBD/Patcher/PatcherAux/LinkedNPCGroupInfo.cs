﻿using Mutagen.Bethesda.Plugins;

namespace SynthEBD
{
    public class LinkedNPCGroupInfo
    {
        public LinkedNPCGroupInfo(LinkedNPCGroup sourceGroup)
        {
            this.NPCFormKeys = sourceGroup.NPCFormKeys;
            this.AssignedCombination = null;
            this.AssignedMorphs = new List<BodyGenConfig.BodyGenTemplate>();
            this.AssignedBodySlide = null;
            this.AssignedHeight = -1;
            this.PrimaryNPCFormKey = sourceGroup.Primary;
            this.ReplacerAssignments = new List<LinkedAssetReplacerAssignment>();
            this.MixInAssignments = new Dictionary<string, SubgroupCombination>();
        }

        public HashSet<FormKey> NPCFormKeys { get; set; }
        public FormKey PrimaryNPCFormKey { get; set; }
        public SubgroupCombination AssignedCombination { get; set; }
        public List<BodyGenConfig.BodyGenTemplate> AssignedMorphs { get; set; }
        public BodySlideSetting AssignedBodySlide { get; set; }
        public float AssignedHeight { get; set; }
        public List<LinkedAssetReplacerAssignment> ReplacerAssignments { get; set; }
        public Dictionary<string, SubgroupCombination> MixInAssignments { get; set; }

        public class LinkedAssetReplacerAssignment
        {
            public LinkedAssetReplacerAssignment()
            {
                GroupName = "";
                ReplacerName = "";
                AssignedReplacerCombination = null;
            }
            public string GroupName { get; set; }
            public string ReplacerName { get; set; }
            public SubgroupCombination AssignedReplacerCombination { get; set; }
        }

        public static LinkedNPCGroupInfo GetInfoFromLinkedNPCGroup(HashSet<LinkedNPCGroup> definedGroups, HashSet<LinkedNPCGroupInfo> createdGroups, FormKey npcFormKey) // links the UI-defined LinkedNPCGroup (which only contains NPCs) to the corresponding generated LinkedNPCGroupInfo (which contains patcher assignments)
        {
            foreach (var group in definedGroups)
            {
                if (group.NPCFormKeys.Contains(npcFormKey))
                {
                    var associatedGroup = createdGroups.Where(x => x.NPCFormKeys.Contains(npcFormKey)).FirstOrDefault();
                    if (associatedGroup == null)
                    {
                        return new LinkedNPCGroupInfo(group);
                    }
                    else
                    {
                        return associatedGroup;
                    }
                }
            }
            return null;
        }
    }
}
