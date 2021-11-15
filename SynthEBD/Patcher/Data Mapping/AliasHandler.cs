﻿using Mutagen.Bethesda.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthEBD
{
    public class AliasHandler
    {
        public static FormKey GetAliasTexMesh(FormKey npcRaceFormKey)
        {
            var alias = PatcherSettings.General.raceAliases.Where(x => x.bApplyToAssets && x.race == npcRaceFormKey).Select(x => x.aliasRace).FirstOrDefault();

            if (!alias.IsNull)
            {
                return alias;
            }
            else
            {
                return npcRaceFormKey;
            }
        }

        public static FormKey GetAliasBodyGen(FormKey npcRaceFormKey)
        {
            var alias = PatcherSettings.General.raceAliases.Where(x => x.bApplyToBodyGen && x.race == npcRaceFormKey).Select(x => x.aliasRace).FirstOrDefault();

            if (!alias.IsNull)
            {
                return alias;
            }
            else
            {
                return npcRaceFormKey;
            }
        }

        public static FormKey GetAliasHeight(FormKey npcRaceFormKey)
        {
            var alias = PatcherSettings.General.raceAliases.Where(x => x.bApplyToHeight && x.race == npcRaceFormKey).Select(x => x.aliasRace).FirstOrDefault();

            if (!alias.IsNull)
            {
                return alias;
            }
            else
            {
                return npcRaceFormKey;
            }
        }
    }
}
