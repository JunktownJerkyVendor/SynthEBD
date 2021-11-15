﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthEBD
{
    class SettingsIO_BlockList
    {
        public static BlockList LoadBlockList()
        {
            BlockList loadedList = new BlockList();

            if (File.Exists(PatcherSettings.Paths.BlockListPath))
            {
                try
                {
                    loadedList = JSONhandler<BlockList>.loadJSONFile(PatcherSettings.Paths.BlockListPath);
                }
                catch
                {
                    try
                    {
                        var loadedZList = JSONhandler<zEBDBlockList>.loadJSONFile(PatcherSettings.Paths.BlockListPath);
                        loadedList = zEBDBlockList.ToSynthEBD(loadedZList);
                    }
                    catch
                    {
                        // Warn User
                    }
                }
            }

            return loadedList;
        }
    }
}
