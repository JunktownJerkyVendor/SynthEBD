using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using System.IO;

namespace SynthEBD;

public class OBodyWriter
{
    public static Spell CreateOBodyAssignmentSpell(SkyrimMod outputMod, GlobalShort gBodySlideVerboseMode)
    {
        // create MGEF first
        MagicEffect MGEFApplyBodySlide = outputMod.MagicEffects.AddNew();

        // create Spell (needed for MGEF script)
        Spell SPELApplyBodySlide = outputMod.Spells.AddNew();

        MGEFApplyBodySlide.EditorID = "SynthEBDBodySlideMGEF";
        MGEFApplyBodySlide.Name = "Applies BodySlide assignment to NPC";
        MGEFApplyBodySlide.Flags |= MagicEffect.Flag.HideInUI;
        MGEFApplyBodySlide.Flags |= MagicEffect.Flag.NoDeathDispel;
        MGEFApplyBodySlide.Archetype.Type = MagicEffectArchetype.TypeEnum.Script;
        MGEFApplyBodySlide.TargetType = TargetType.Self;
        MGEFApplyBodySlide.CastType = CastType.ConstantEffect;
        MGEFApplyBodySlide.VirtualMachineAdapter = new VirtualMachineAdapter();

        ScriptEntry ScriptApplyBodySlide = new ScriptEntry() { Name = "SynthEBDBodySlideScript"};

        ScriptStringProperty targetModProperty = new ScriptStringProperty() { Name = "TargetMod", Flags = ScriptProperty.Flag.Edited };
        switch(PatcherSettings.General.BSSelectionMode)
        {
            case BodySlideSelectionMode.OBody: targetModProperty.Data = "OBody"; break;
            case BodySlideSelectionMode.AutoBody: targetModProperty.Data = "AutoBody"; break;
        }
        ScriptApplyBodySlide.Properties.Add(targetModProperty);

        ScriptObjectProperty verboseModeProperty = new ScriptObjectProperty() { Name = "VerboseMode", Flags = ScriptProperty.Flag.Edited } ;
        verboseModeProperty.Object.SetTo(gBodySlideVerboseMode);
        ScriptApplyBodySlide.Properties.Add(verboseModeProperty);

        MGEFApplyBodySlide.VirtualMachineAdapter.Scripts.Add(ScriptApplyBodySlide);

        // Edit Spell
        SPELApplyBodySlide.EditorID = "SynthEBDBodySlideSPEL";
        SPELApplyBodySlide.Name = "Applies BodySlide assignment to NPC";
        SPELApplyBodySlide.CastType = CastType.ConstantEffect;
        SPELApplyBodySlide.TargetType = TargetType.Self;
        SPELApplyBodySlide.Type = SpellType.Ability;
        SPELApplyBodySlide.EquipmentType.SetTo(Skyrim.EquipType.EitherHand);
            
        Effect BodySlideShellEffect = new Effect();
        BodySlideShellEffect.BaseEffect.SetTo(MGEFApplyBodySlide);
        BodySlideShellEffect.Data = new EffectData();
        SPELApplyBodySlide.Effects.Add(BodySlideShellEffect);

        return SPELApplyBodySlide;
    }

    public static void CreateBodySlideLoaderQuest(SkyrimMod outputMod, GlobalShort gEnableBodySlideScript, GlobalShort gBodySlideVerboseMode)
    {
        Quest bsLoaderQuest = outputMod.Quests.AddNew();
        bsLoaderQuest.Name = "Loads SynthEBD BodySlide Assignments";
        bsLoaderQuest.EditorID = "SynthEBDBSLoaderQuest";

        bsLoaderQuest.Flags |= Quest.Flag.StartGameEnabled;
        bsLoaderQuest.Flags |= Quest.Flag.RunOnce;

        QuestAlias playerQuestAlias = new QuestAlias();
        FormKey.TryFactory("000014:Skyrim.esm", out FormKey playerRefFK);
        playerQuestAlias.ForcedReference.SetTo(playerRefFK);
        bsLoaderQuest.Aliases.Add(playerQuestAlias);

        QuestAdapter bsLoaderScriptAdapter = new QuestAdapter();

        /*
        ScriptEntry bsLoaderScriptEntry = new ScriptEntry() { Name = "SynthEBDBodySlideLoaderQuestScript", Flags = ScriptEntry.Flag.Local };
        ScriptObjectProperty settingsLoadedProperty = new ScriptObjectProperty() { Name = "SynthEBDDataBaseLoaded", Flags = ScriptProperty.Flag.Edited };
        settingsLoadedProperty.Object.SetTo(settingsLoadedGlobal.FormKey);
        bsLoaderScriptEntry.Properties.Add(settingsLoadedProperty);
        bsLoaderScriptAdapter.Scripts.Add(bsLoaderScriptEntry);
        */

        QuestFragmentAlias loaderQuestFragmentAlias = new QuestFragmentAlias();
        loaderQuestFragmentAlias.Property = new ScriptObjectProperty() { Name = "000 Player" };
        loaderQuestFragmentAlias.Property.Object.SetTo(bsLoaderQuest.FormKey);
        loaderQuestFragmentAlias.Property.Name = "Player";
        loaderQuestFragmentAlias.Property.Alias = 0;

        ScriptEntry playerAliasScriptEntry = new ScriptEntry();
        playerAliasScriptEntry.Name = "SynthEBDBodySlideLoaderPAScript";
        playerAliasScriptEntry.Flags = ScriptEntry.Flag.Local;

        ScriptObjectProperty loaderQuestActiveProperty = new ScriptObjectProperty() { Name = "BodySlideScriptActive", Flags = ScriptProperty.Flag.Edited };
        loaderQuestActiveProperty.Object.SetTo(gEnableBodySlideScript);
        playerAliasScriptEntry.Properties.Add(loaderQuestActiveProperty);

        ScriptObjectProperty verboseModeProperty = new ScriptObjectProperty() { Name = "VerboseMode", Flags = ScriptProperty.Flag.Edited };
        verboseModeProperty.Object.SetTo(gBodySlideVerboseMode);
        playerAliasScriptEntry.Properties.Add(verboseModeProperty);

        loaderQuestFragmentAlias.Scripts.Add(playerAliasScriptEntry);
        bsLoaderScriptAdapter.Aliases.Add(loaderQuestFragmentAlias);
        bsLoaderQuest.VirtualMachineAdapter = bsLoaderScriptAdapter;

        // copy quest alias script
        string questAliasSourcePath = Path.Combine(PatcherSettings.Paths.ResourcesFolderPath, "BodySlideScripts", "SynthEBDBodySlideLoaderPAScript.pex");
        string questAliasDestPath = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "Scripts", "SynthEBDBodySlideLoaderPAScript.pex");
        PatcherIO.TryCopyResourceFile(questAliasSourcePath, questAliasDestPath);
    }

    public static void CopyBodySlideScript()
    {
        string sourcePath = Path.Combine(PatcherSettings.Paths.ResourcesFolderPath, "BodySlideScripts", "SynthEBDBodySlideScript.pex");
        string destPath = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "Scripts", "SynthEBDBodySlideScript.pex");
        PatcherIO.TryCopyResourceFile(sourcePath, destPath);
    }

    /*
    public static void WriteBodySlideSPIDIni(Spell bodySlideSpell, Settings_OBody obodySettings, SkyrimMod outputMod)
    {
        string str = "Spell = " + bodySlideSpell.FormKey.ToString().Replace(":", " - ") + " | ActorTypeNPC | NONE | NONE | "; // original format - SPID auto-updates but this is compatible with old SPID versions

        bool hasMaleBodySlides = obodySettings.BodySlidesMale.Where(x => obodySettings.CurrentlyExistingBodySlides.Contains(x.Label)).Any();
        bool hasFemaleBodySlides = obodySettings.BodySlidesFemale.Where(x => obodySettings.CurrentlyExistingBodySlides.Contains(x.Label)).Any();

        if (!hasMaleBodySlides && !hasFemaleBodySlides) { return; }
        else if (hasMaleBodySlides && !hasFemaleBodySlides) { str += "M"; }
        else if (!hasMaleBodySlides && hasFemaleBodySlides) { str += "F"; }

        string outputPath = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "SynthEBDBodySlideDistributor_DISTR.ini");
        Task.Run(() => PatcherIO.WriteTextFile(outputPath, str));
    }
    */

    public static void WriteAssignmentDictionary()
    {
        if (Patcher.BodySlideTracker.Count == 0)
        {
            Logger.LogMessage("No BodySlides were assigned to any NPCs");
            return;
        }

        var outputDictionary = new Dictionary<string, string>();
        foreach (var entry in Patcher.BodySlideTracker)
        {
            outputDictionary.TryAdd(entry.Key.ToJContainersCompatiblityKey(), entry.Value);
        }
        string outputStr = JSONhandler<Dictionary<string, string>>.Serialize(outputDictionary, out bool success, out string exception);

        var destPath = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "SynthEBD", "BodySlideAssignments.json");

        try
        {
            PatcherIO.CreateDirectoryIfNeeded(destPath, PatcherIO.PathType.File);
            File.WriteAllText(destPath, outputStr);
        }
        catch
        {
            Logger.LogErrorWithStatusUpdate("Could not write BodySlide assignments to " + destPath, ErrorType.Error);
        }
    }

    public static void WriteAssignmentIni()
    {
        if (Patcher.BodySlideTracker.Count == 0)
        {
            Logger.LogMessage("No BodySlides were assigned to any NPCs");
            return;
        }

        string outputStr = "";

        foreach (var entry in Patcher.BodySlideTracker)
        {
            outputStr += BodyGenWriter.FormatFormKeyForBodyGen(entry.Key) + "=" + entry.Value + Environment.NewLine;
        }

        var destPath = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "autoBody", "Config", "morphs.ini");

        try
        {
            PatcherIO.CreateDirectoryIfNeeded(destPath, PatcherIO.PathType.File);
            File.WriteAllText(destPath, outputStr);
        }
        catch
        {
            Logger.LogErrorWithStatusUpdate("Could not write BodySlide assignments to " + destPath, ErrorType.Error);
        }
    }

    public static void ClearOutputForJsonMode()
    {
        HashSet<string> toClear = new HashSet<string>()
        {
            Path.Combine(PatcherSettings.Paths.OutputDataFolder, "autoBody", "Config", "morphs.ini"),
            Path.Combine(PatcherSettings.Paths.OutputDataFolder, "Meshes", "actors", "character", "BodyGenData", PatcherSettings.General.PatchFileName, "morphs.ini")
        };

        foreach (string path in toClear)
        {
            if (File.Exists(path))
            {
                PatcherIO.TryDeleteFile(path);
            }
        }

        string autoBodyDir = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "autoBody");
        if (Directory.Exists(autoBodyDir))
        {
            PatcherIO.TryDeleteDirectory(autoBodyDir);
        }
    }

    public static void ClearOutputForIniMode()
    {
        HashSet<string> toClear = new HashSet<string>()
        {
            //Path.Combine(PatcherSettings.Paths.OutputDataFolder, "SynthEBD", "BodySlideDict.json"),
            //Path.Combine(PatcherSettings.Paths.OutputDataFolder, "SynthEBDBodySlideDistributor_DISTR.ini"),
            Path.Combine(PatcherSettings.Paths.OutputDataFolder, "Meshes", "actors", "character", "BodyGenData", PatcherSettings.General.PatchFileName, "morphs.ini")
        };

        var dictDir = Path.Combine(PatcherSettings.Paths.OutputDataFolder, "SynthEBD", "BodySlideAssignments");
        if (Directory.Exists(dictDir))
        {
            foreach (var file in Directory.GetFiles(dictDir))
            {
                string fileName = Path.GetFileName(file);
                if (fileName.StartsWith("BodySlideDict", StringComparison.OrdinalIgnoreCase))
                {
                    toClear.Add(file);
                }
            }
        }

        foreach (string path in toClear)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    Logger.LogErrorWithStatusUpdate("Could not delete file at " + path, ErrorType.Warning);
                }
            }
        }
    }
}