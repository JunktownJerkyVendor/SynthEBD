﻿using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SynthEBD
{
    public class VM_OBodyMiscSettings : INotifyPropertyChanged
    {
        public VM_OBodyMiscSettings()
        {
            MaleBodySlideGroups = new ObservableCollection<VM_CollectionMemberString>();
            FemaleBodySlideGroups = new ObservableCollection<VM_CollectionMemberString>();
            UseVerboseScripts = false;
            AutoBodySelectionMode = AutoBodySelectionMode.INI;

            SetRaceMenuINI = new SynthEBD.RelayCommand(
               canExecute: _ => true,
               execute: _ =>
               {
                   if (RaceMenuIniHandler.SetRaceMenuIniForBodySlide())
                   {
                       Logger.CallTimedLogErrorWithStatusUpdateAsync("RaceMenu Ini set successfully", ErrorType.Warning, 2); // Warning yellow font is easier to see than green
                   }
                   else
                   {
                       Logger.LogErrorWithStatusUpdate("Error encountered trying to set RaceMenu's ini.", ErrorType.Error);
                       Logger.SwitchViewToLogDisplay();
                   }
               }
               );

            AddMaleSliderGroup = new RelayCommand(
                canExecute: _ => true,
                execute: _ => MaleBodySlideGroups.Add(new VM_CollectionMemberString("", MaleBodySlideGroups))
                );

            AddFemaleSliderGroup = new RelayCommand(
                canExecute: _ => true,
                execute: _ => FemaleBodySlideGroups.Add(new VM_CollectionMemberString("", FemaleBodySlideGroups))
                );
        }

        public ObservableCollection<VM_CollectionMemberString> MaleBodySlideGroups { get; set; }
        public ObservableCollection<VM_CollectionMemberString> FemaleBodySlideGroups { get; set; }
        public bool UseVerboseScripts { get; set; }
        public AutoBodySelectionMode AutoBodySelectionMode { get; set; }
        public RelayCommand SetRaceMenuINI { get; }
        public RelayCommand AddMaleSliderGroup { get; set; }
        public RelayCommand AddFemaleSliderGroup { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public static VM_OBodyMiscSettings GetViewModelFromModel(Settings_OBody model)
        {
            VM_OBodyMiscSettings viewModel = new VM_OBodyMiscSettings();
            viewModel.MaleBodySlideGroups.Clear();
            foreach (var g in model.MaleSliderGroups)
            {
                viewModel.MaleBodySlideGroups.Add(new VM_CollectionMemberString(g, viewModel.MaleBodySlideGroups));
            }
            viewModel.FemaleBodySlideGroups.Clear();
            foreach (var g in model.FemaleSliderGroups)
            {
                viewModel.FemaleBodySlideGroups.Add(new VM_CollectionMemberString(g, viewModel.FemaleBodySlideGroups));
            }
            viewModel.UseVerboseScripts = model.UseVerboseScripts;
            viewModel.AutoBodySelectionMode = model.AutoBodySelectionMode;
            return viewModel;
        }

        public static void DumpViewModelToModel(Settings_OBody model, VM_OBodyMiscSettings viewModel)
        {
            model.MaleSliderGroups = viewModel.MaleBodySlideGroups.Select(x => x.Content).ToHashSet();
            model.FemaleSliderGroups = viewModel.FemaleBodySlideGroups.Select(x => x.Content).ToHashSet();
            model.UseVerboseScripts = viewModel.UseVerboseScripts;
            model.AutoBodySelectionMode = viewModel.AutoBodySelectionMode;
        }
    }
}
