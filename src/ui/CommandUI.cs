using UnityEngine;

namespace KspVtol
{
    public class CommandUI
    {
        private Rect _windowRect = Rect.zero;
        private Vector2 _size = new Vector2(300, 250);
        
        private Core _core = null;
        public Core core { set { _core = value; } }
        private MainVtol _mainVtol = null;
        public MainVtol mainVtol { set { _mainVtol = value; } }
        
        public CommandUI(Vector2 pos)
        {
            _windowRect = new Rect(pos, _size);
        }
        
        public void Display()
        {
            if ((_core == null) && (_mainVtol == null))
            {
                return;
            }
            
            _windowRect.size = _size;
            _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Keyboard), _windowRect, DoWindow, "Command");
        }
        
        private void DoWindow(int windowID)
        {
            if (Input.GetMouseButtonUp(0)) {
                _core.commandVSpeed = 0.0f;
                _core.commandPitch = 0.0f;
                _core.commandRoll = 0.0f;
            }
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Configure"))
            {
                _mainVtol.ToggleConf();
            }
            GUILayout.FlexibleSpace();
            _core.active = GUILayout.Toggle(_core.active, "Activate");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _core.activeVSpeed = GUILayout.Toggle(_core.activeVSpeed, "Vertical");
            GUILayout.FlexibleSpace();
            _core.activeAttitude = GUILayout.Toggle(_core.activeAttitude, "Attitude");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Info"))
            {
                _mainVtol.ToggleInfo();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _core.commandVSpeed = GUILayout.VerticalSlider(_core.commandVSpeed, _core.verticalMax, -_core.verticalMax);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _core.commandPitch = GUILayout.VerticalSlider(_core.commandPitch, _core.pitchMax, -_core.pitchMax);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            _core.commandRoll = GUILayout.HorizontalSlider(_core.commandRoll, -_core.rollMax, _core.rollMax);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}