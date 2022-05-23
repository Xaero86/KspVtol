using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;

namespace KspVtol
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class MainVtol : MonoBehaviour
    {
        public const string DEBUG_PREFIX = "[Vtol] ";
        private static string ICON_FILE_NAME = "button.png";
        
        private static Texture2D TEXTURE_BUTTON = null;
        private ApplicationLauncherButton _mainButton = null;
        
        private bool _displayWindow = false;
        
        // UI
        private InfoUI _infoUI;
        private ConfigUI _configUI;
        private CommandUI _commandUI;
        
        // Core
        private Core _core;
        
        public void Awake()
        {
            if (TEXTURE_BUTTON == null)
            {
                TEXTURE_BUTTON = new Texture2D(1, 1);
                string addonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..");
                string iconFile = Path.Combine(addonPath, ICON_FILE_NAME);
                try {
                    byte[] bytes = File.ReadAllBytes(iconFile);
                    TEXTURE_BUTTON.LoadImage(bytes);
                } catch (Exception e) {
                    Debug.LogError(DEBUG_PREFIX + e.Message);
                    TEXTURE_BUTTON.SetPixel(0, 0, Color.blue);
                    TEXTURE_BUTTON.Apply();
                }
            }
            
            _mainButton = ApplicationLauncher.Instance.AddModApplication(
                                () => {_displayWindow = true;}, () => {_displayWindow = false;},
                                null, null, null, null,
                                ApplicationLauncher.AppScenes.FLIGHT, TEXTURE_BUTTON);
            
            _core = null;
            Vector2 infoUiPos = new Vector2(300, 50);
            Vector2 configUiPos = new Vector2(400, 100);
            Vector2 commandUiPos = new Vector2(50, 50);
            _infoUI = new InfoUI(infoUiPos);
            _configUI = new ConfigUI(configUiPos);
            _commandUI = new CommandUI(commandUiPos);
        }
        
        public void Start()
        {
            _core = new Core();
            _configUI.core = _core;
            _commandUI.core = _core;
            _commandUI.mainVtol = this;
            _core.InfoUI = _infoUI;
        }
        
        public void OnDestroy()
        {
            ApplicationLauncher.Instance.RemoveModApplication(_mainButton);
        }
        
        public void OnGUI()
        {
            if (_displayWindow)
            {
                _configUI.Display();
                _commandUI.Display();
            }
            _infoUI.Display();
        }
        
        public void ToggleConfUI()
        {
            _configUI.ToggleDisplay();
        }
        
        public void ToggleInfoUI()
        {
            _infoUI.ToggleDisplay();
        }
    }
}

