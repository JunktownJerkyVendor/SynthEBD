﻿using Mutagen.Bethesda;
using Mutagen.Bethesda.Cache.Implementations;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SynthEBD
{
    public class MainWindow_ViewModel : INotifyPropertyChanged
    {
        public GameEnvironmentProvider GameEnvironmentProvider { get; }
        public VM_Settings_General SGVM { get; } = new();
        public VM_SettingsTexMesh TMVM { get; }
        public VM_SettingsBodyGen BGVM { get; }
        public VM_SettingsHeight HVM { get; } = new();
        public VM_SpecificNPCAssignmentsUI SAUIVM { get; }
        public VM_BlockListUI BUIVM { get; } = new();

        public VM_NavPanel NavPanel { get; }

        public VM_RunButton RunButton { get; }
        public object DisplayedViewModel { get; set; }
        public object NavViewModel { get; set; }

        public object StatusBarVM { get; set; }

        public VM_LogDisplay LogDisplayVM { get; set; } = new();
        public List<AssetPack> AssetPacks { get; }
        public List<HeightConfig> HeightConfigs { get; }
        public BodyGenConfigs BodyGenConfigs { get; }
        public HashSet<NPCAssignment> SpecificNPCAssignments { get; }
        public BlockList BlockList { get; }
        public HashSet<string> LinkedNPCNameExclusions { get; set; }
        public HashSet<LinkedNPCGroup> LinkedNPCGroups { get; set; }
        public HashSet<TrimPath> TrimPaths { get; set; }

        public List<SkyrimMod> RecordTemplatePlugins { get; set; }
        public ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter> RecordTemplateLinkCache { get; set; }

        public MainWindow_ViewModel()
        {
            var gameRelease = SkyrimRelease.SkyrimSE;
            var env = GameEnvironment.Typical.Skyrim(gameRelease, LinkCachePreferences.OnlyIdentifiers());
            var LinkCache = env.LinkCache;
            var LoadOrder = env.LoadOrder;

            BGVM = new VM_SettingsBodyGen(SGVM.RaceGroupings);
            TMVM = new VM_SettingsTexMesh(BGVM);
            SAUIVM = new VM_SpecificNPCAssignmentsUI(TMVM, BGVM);

            NavPanel = new SynthEBD.VM_NavPanel(this, SGVM, TMVM, BGVM, HVM, SAUIVM, BUIVM);

            StatusBarVM = new VM_StatusBar();

            RunButton = new VM_RunButton(this);

            // Load general settings
            SettingsIO_General.loadGeneralSettings();
            VM_Settings_General.GetViewModelFromModel(SGVM);

            // get paths
            PatcherSettings.Paths = new Paths();

            // Load texture and mesh settings
            RecordTemplatePlugins = SettingsIO_AssetPack.LoadRecordTemplates();
            RecordTemplateLinkCache = RecordTemplatePlugins.ToImmutableLinkCache();
            PatcherSettings.TexMesh = SettingsIO_AssetPack.LoadTexMeshSettings();
            VM_SettingsTexMesh.GetViewModelFromModel(TMVM, PatcherSettings.TexMesh);

            // load bodygen configs before asset packs - asset packs depend on BodyGen but not vice versa
            PatcherSettings.BodyGen = SettingsIO_BodyGen.LoadBodyGenSettings();
            BodyGenConfigs = SettingsIO_BodyGen.loadBodyGenConfigs(PatcherSettings.General.RaceGroupings);
            VM_SettingsBodyGen.GetViewModelFromModel(BodyGenConfigs, PatcherSettings.BodyGen, BGVM, SGVM.RaceGroupings);

            // load asset packs
            List<string> loadedAssetPackPaths = new List<string>();
            AssetPacks = SettingsIO_AssetPack.loadAssetPacks(PatcherSettings.General.RaceGroupings, loadedAssetPackPaths, RecordTemplatePlugins, BodyGenConfigs); // load asset pack models from json
            TMVM.AssetPacks = VM_AssetPack.GetViewModelsFromModels(AssetPacks, SGVM, PatcherSettings.TexMesh, loadedAssetPackPaths, BGVM, RecordTemplateLinkCache); // add asset pack view models to TexMesh shell view model here

            // load heights
            PatcherSettings.Height = SettingsIO_Height.LoadHeightSettings();
            List<string> loadedHeightPaths = new List<string>();
            HeightConfigs = SettingsIO_Height.loadHeightConfigs(loadedHeightPaths);
            VM_HeightConfig.GetViewModelsFromModels(HVM.AvailableHeightConfigs, HeightConfigs, loadedHeightPaths);
            VM_SettingsHeight.GetViewModelFromModel(HVM, PatcherSettings.Height); /// must do after populating configs

            // load specific assignments
            SpecificNPCAssignments = SettingsIO_SpecificNPCAssignments.LoadAssignments();
            VM_SpecificNPCAssignmentsUI.GetViewModelFromModels(SAUIVM, SpecificNPCAssignments);

            // load BlockList
            BlockList = SettingsIO_BlockList.LoadBlockList();
            VM_BlockListUI.GetViewModelFromModel(BlockList, BUIVM);

            // load Misc settings
            LinkedNPCNameExclusions = SettingsIO_Misc.LoadNPCNameExclusions();
            SGVM.LinkedNameExclusions = VM_CollectionMemberString.InitializeCollectionFromHashSet(LinkedNPCNameExclusions);
            LinkedNPCGroups = SettingsIO_Misc.LoadLinkedNPCGroups();
            SGVM.LinkedNPCGroups = VM_LinkedNPCGroup.GetViewModelsFromModels(LinkedNPCGroups);
            TrimPaths = SettingsIO_Misc.LoadTrimPaths();
            TMVM.TrimPaths = new ObservableCollection<TrimPath>(TrimPaths);

            // Start on the settings VM
            DisplayedViewModel = SGVM;
            NavViewModel = NavPanel;
            Logger.Instance.RunButton = RunButton;

            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            VM_Settings_General.DumpViewModelToModel(SGVM, PatcherSettings.General);
            JSONhandler<Settings_General>.SaveJSONFile(PatcherSettings.General, Paths.GeneralSettingsPath);

            VM_SettingsTexMesh.DumpViewModelToModel(TMVM, PatcherSettings.TexMesh);
            JSONhandler<Settings_TexMesh>.SaveJSONFile(PatcherSettings.TexMesh, PatcherSettings.Paths.TexMeshSettingsPath);
            var assetPackPaths = VM_AssetPack.DumpViewModelsToModels(TMVM.AssetPacks, AssetPacks);
            SettingsIO_AssetPack.SaveAssetPacks(AssetPacks, assetPackPaths);

            // Need code here to dump assset packs and save - see height configs for analogy

            VM_SettingsHeight.DumpViewModelToModel(HVM, PatcherSettings.Height);
            JSONhandler<Settings_Height>.SaveJSONFile(PatcherSettings.Height, PatcherSettings.Paths.HeightSettingsPath);
            var heightConfigPaths = VM_HeightConfig.DumpViewModelsToModels(HVM.AvailableHeightConfigs, HeightConfigs);
            SettingsIO_Height.SaveHeightConfigs(HeightConfigs, heightConfigPaths);

            VM_SettingsBodyGen.DumpViewModelToModel(BGVM, PatcherSettings.BodyGen);
            JSONhandler<Settings_BodyGen>.SaveJSONFile(PatcherSettings.BodyGen, PatcherSettings.Paths.BodyGenSettingsPath);
            // Need code here to dump assset packs and save - see height configs for analogy

            VM_SpecificNPCAssignmentsUI.DumpViewModelToModels(SAUIVM, SpecificNPCAssignments);
            JSONhandler<HashSet<NPCAssignment>>.SaveJSONFile(SpecificNPCAssignments, PatcherSettings.Paths.SpecificNPCAssignmentsPath);

            VM_LinkedNPCGroup.DumpViewModelsToModels(LinkedNPCGroups, SGVM.LinkedNPCGroups);
            JSONhandler<HashSet<LinkedNPCGroup>>.SaveJSONFile(LinkedNPCGroups, PatcherSettings.Paths.LinkedNPCsPath);

            JSONhandler<HashSet<string>>.SaveJSONFile(SGVM.LinkedNameExclusions.Select(cms => cms.Content).ToHashSet(), PatcherSettings.Paths.LinkedNPCNameExclusionsPath);

            JSONhandler<HashSet<TrimPath>>.SaveJSONFile(TMVM.TrimPaths.ToHashSet(), PatcherSettings.Paths.TrimPathsPath);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
