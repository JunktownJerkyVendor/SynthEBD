﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SynthEBD
{
    public class SettingsIO_General
    {
        public static void loadGeneralSettings()
        {
            if (File.Exists(Paths.GeneralSettingsPath))
            {
                PatcherSettings.General = JSONhandler<Settings_General>.loadJSONFile(Paths.GeneralSettingsPath);
            }
        }
    }
}
