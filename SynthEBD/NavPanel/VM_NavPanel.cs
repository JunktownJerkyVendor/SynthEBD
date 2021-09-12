﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SynthEBD
{
    class VM_NavPanel : INotifyPropertyChanged
    {
        public ICommand ClickSG { get; }
        public ICommand ClickTM { get; }
        public ICommand ClickBG { get; }
        public ICommand ClickH { get; }
        

        public VM_NavPanel(MainWindow_ViewModel MWVM, VM_Settings_General SGVM, VM_SettingsTexMesh TMVM, VM_BodyGenSettings BGVM, VM_HeightSettings HVM)
        {
            ClickSG = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => MWVM.DisplayedViewModel = SGVM
                );

            ClickTM = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => MWVM.DisplayedViewModel = TMVM
                ) ;
            ClickBG = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => MWVM.DisplayedViewModel = BGVM
                );
            ClickH = new SynthEBD.RelayCommand(
                canExecute: _ => true,
                execute: _ => MWVM.DisplayedViewModel = HVM
                );
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

