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
        
        private bool _vFree = false;
        private bool _aFree = false;
        
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
                if (!_vFree)
                {
                    _core.requestedVSpeed = 0.0f;
                }
                if (!_aFree)
                {
                    _core.requestedPitch = 0.0f;
                    _core.requestedRoll = 0.0f;
                }
            }
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Configure"))
            {
                _mainVtol.ToggleConfUI();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Info"))
            {
                _mainVtol.ToggleInfoUI();
            }
            GUILayout.FlexibleSpace();
            _core.active = GUILayout.Toggle(_core.active, "Activate");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _core.activeVSpeed = GUILayout.Toggle(_core.activeVSpeed, "Vertical");
            GUILayout.FlexibleSpace();
            _vFree = GUILayout.Toggle(_vFree, "vFree");
            GUILayout.FlexibleSpace();
            _core.activeAttitude = GUILayout.Toggle(_core.activeAttitude, "Attitude");
            GUILayout.FlexibleSpace();
            _aFree = GUILayout.Toggle(_aFree, "aFree");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _core.requestedVSpeed = GUILayout.VerticalSlider(_core.requestedVSpeed, _core.verticalMax, -_core.verticalMax);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _core.requestedPitch = GUILayout.VerticalSlider(_core.requestedPitch, _core.pitchMax, -_core.pitchMax);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            _core.requestedRoll = GUILayout.HorizontalSlider(_core.requestedRoll, -_core.rollMax, _core.rollMax);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}