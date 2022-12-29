using System.IO;

namespace SynthEBD;

public class SettingsIO_General
{
    private readonly IStateProvider _stateProvider;
    private readonly Logger _logger;
    private readonly SynthEBDPaths _paths;
    public SettingsIO_General(IStateProvider stateProvider, Logger logger, SynthEBDPaths paths)
    {
        _stateProvider = stateProvider;
        _logger = logger;
        _paths = paths;
    }
    public void LoadGeneralSettings(out bool loadSuccess)
    {
        if (File.Exists(_paths.GeneralSettingsPath))
        {
            PatcherSettings.General = JSONhandler<Settings_General>.LoadJSONFile(_paths.GeneralSettingsPath, out loadSuccess, out string exceptionStr);
            if(loadSuccess && string.IsNullOrWhiteSpace(_paths.OutputDataFolder))
            {
                _paths.OutputDataFolder = _stateProvider.DataFolderPath;
            }
            else if (!loadSuccess)
            {
                _logger.LogError("Could not parse General Settings. Error: " + exceptionStr);
            }
        }
        else
        {
            PatcherSettings.General = new Settings_General();
            loadSuccess = true;
        }
    }

    public void DumpVMandSave(VM_Settings_General generalSettingsVM)
    {
        VM_Settings_General.DumpViewModelToModel(generalSettingsVM, PatcherSettings.General);
        JSONhandler<Settings_General>.SaveJSONFile(PatcherSettings.General, _paths.GeneralSettingsPath, out bool saveSuccess, out string exceptionStr);
        if (!saveSuccess) { _logger.LogMessage("Error saving General Settings: " + exceptionStr); }
    }
}