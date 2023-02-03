using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthEBD
{
    public class VM_AssetPackMiscMenu
    {
        private readonly VM_AssetPack _parent;
        public VM_AssetPackMiscMenu(VM_AssetPack parentPack)
        {
            _parent = parentPack;

            SetAllowedDescriptorMatchModes = new RelayCommand(
                canExecute: _ => true,
                execute: _ => SetMatchModes(AllowedStr, AllowedDescriptorMatchMode)
            );
            SetDisallowedDescriptorMatchModes = new RelayCommand(
                canExecute: _ => true,
                execute: _ => SetMatchModes(DisallowedStr, DisallowedDescriptorMatchMode)
            );
        }

        public RelayCommand SetAllowedDescriptorMatchModes { get; }
        public DescriptorMatchMode AllowedDescriptorMatchMode { get; set; } = DescriptorMatchMode.All;
        public RelayCommand SetDisallowedDescriptorMatchModes { get; }
        public DescriptorMatchMode DisallowedDescriptorMatchMode { get; set; } = DescriptorMatchMode.Any;

        private const string AllowedStr = "Allowed";
        private const string DisallowedStr = "Disallowed";

        public void SetMatchModes(string descriptorTypes, DescriptorMatchMode mode)
        {
            foreach(var subgroup in _parent.Subgroups)
            {
                SetSubgroupMatchModes(subgroup, descriptorTypes, mode);
            }
            foreach (var replacer in _parent.ReplacersMenu.ReplacerGroups)
            {
                foreach (var subgroup in replacer.Subgroups)
                {
                    SetSubgroupMatchModes(subgroup, descriptorTypes, mode);
                }    
            }
        }

        public static void SetSubgroupMatchModes(VM_Subgroup subgroup, string descriptorType, DescriptorMatchMode mode)
        {
            switch(descriptorType)
            {
                case AllowedStr:
                    subgroup.AllowedBodyGenDescriptors.MatchMode = mode;
                    subgroup.AllowedBodySlideDescriptors.MatchMode = mode;
                    break;
                case DisallowedStr:
                    subgroup.DisallowedBodyGenDescriptors.MatchMode = mode;
                    subgroup.DisallowedBodySlideDescriptors.MatchMode = mode;
                    break;
            }

            foreach (var sg in subgroup.Subgroups)
            {
                SetSubgroupMatchModes(sg, descriptorType, mode);
            }
        }
    }
}
