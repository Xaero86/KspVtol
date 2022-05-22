using System;
using System.Collections.Generic;
using UnityEngine;

namespace KspVtol
{
    public class ConfigUI
    {
        private static GUIStyle BUTTON_ACTIVE_STYLE = null;
        private static GUIStyle BUTTON_DEACTIVE_STYLE = null;
        
        private Rect _windowRect = Rect.zero;
        private Vector2 _size = new Vector2(500, 200);
        private bool _isDisplayed = false;
        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set
            {
                _isDisplayed = value;
                UpdateSelector(false);
            }
        }
        public void ToggleDisplay()
        {
            IsDisplayed = !_isDisplayed;
        }
        
        private Rect _boxPos = Rect.zero;
        private Vector2 _scrollConfVector = Vector2.zero;
        
        private Core _core = null;
        public Core core { set { _core = value; } }
        
        private bool _selecting = false;
        
        private string _vSpeedMax = "";
        private string _pitchMax = "";
        private string _rollMax = "";
        
        private static string TF_NAME_V_SPEED_MAX = "vSpeedMaxTF";
        private static string TF_NAME_PITCH_MAX = "pitchMaxTF";
        private static string TF_NAME_ROLL_MAX = "rollMaxTF";
        
        /*private string _vkp = "";
        private string _vki = "";
        private string _vkd = "";
        private string _akp = "";
        private string _aki = "";
        private string _akd = "";
        private static string TF_NAME_V_KP = "vkp";
        private static string TF_NAME_V_KI = "vki";
        private static string TF_NAME_V_KD = "vkd";
        private static string TF_NAME_A_KP = "akp";
        private static string TF_NAME_A_KI = "aki";
        private static string TF_NAME_A_KD = "akd";*/
        
        public ConfigUI(Vector2 pos)
        {
            _windowRect = new Rect(pos, _size);
        }
        
        public void Display()
        {
            if (_isDisplayed)
            {
                _windowRect.size = _size;
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Keyboard), _windowRect, DoWindow, "Config");
            }
        }
        
        private void DoWindow(int windowID)
        {
            if (BUTTON_ACTIVE_STYLE == null)
            {
                BUTTON_ACTIVE_STYLE = new GUIStyle(GUI.skin.button);
            }
            if (BUTTON_DEACTIVE_STYLE == null)
            {
                BUTTON_DEACTIVE_STYLE = new GUIStyle(GUI.skin.button);
                BUTTON_DEACTIVE_STYLE.normal.textColor = Color.red;
                BUTTON_DEACTIVE_STYLE.hover.textColor = Color.red;
            }
            
            float labelWidth = GUI.skin.label.CalcSize(new GUIContent("XXXXXXXXXXXX")).x;
            float labelWidth2 = GUI.skin.label.CalcSize(new GUIContent("XXXXXXX")).x;
            float labelWidth3 = GUI.skin.label.CalcSize(new GUIContent("XXX")).x;
            List<PartElement> removed = new List<PartElement>();
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUIStyle style = _selecting ? BUTTON_DEACTIVE_STYLE : BUTTON_ACTIVE_STYLE;
            if (GUILayout.Button("Add engine", style))
            {
                UpdateSelector(!_selecting);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
            {
                _isDisplayed = false;
            }
            GUILayout.EndHorizontal();
            
            if (Event.current.type == EventType.Repaint)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                RectOffset rctOff = GUI.skin.button.margin;
                _boxPos = new Rect(2 * labelWidth + rctOff.left,
                                lastRect.y+lastRect.height+rctOff.top,
                                _windowRect.width-rctOff.horizontal - 2 * labelWidth,
                                _windowRect.height-(lastRect.y+lastRect.height+rctOff.vertical));
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginArea(_boxPos, GUI.skin.GetStyle("Box"));
            _scrollConfVector = GUILayout.BeginScrollView(_scrollConfVector);
            
            foreach (PartElement partElem in _core.PartsList)
            {
                GUILayout.BeginHorizontal();
                // Name
                GUILayout.Label(partElem.ToString(), GUILayout.Width(labelWidth));
                GUILayout.FlexibleSpace();
                // Remove
                if (GUILayout.Button("Remove"))
                {
                    removed.Add(partElem);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("V speed max:", GUILayout.Width(labelWidth));
            GUI.SetNextControlName(TF_NAME_V_SPEED_MAX);
            _vSpeedMax = GUILayout.TextField(_vSpeedMax, 25, GUILayout.Width(labelWidth2));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch max °:", GUILayout.Width(labelWidth));
            GUI.SetNextControlName(TF_NAME_PITCH_MAX);
            _pitchMax = GUILayout.TextField(_pitchMax, 25, GUILayout.Width(labelWidth2));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll max °:", GUILayout.Width(labelWidth));
            GUI.SetNextControlName(TF_NAME_ROLL_MAX);
            _rollMax = GUILayout.TextField(_rollMax, 25, GUILayout.Width(labelWidth2));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _core.isAirplane = GUILayout.Toggle(_core.isAirplane, "Airplane");
            _core.isRocket = GUILayout.Toggle(_core.isRocket, "Rocket");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            /*GUILayout.BeginHorizontal();
            GUILayout.Label("v");
            GUI.SetNextControlName(TF_NAME_V_KP);
            _vkp = GUILayout.TextField(_vkp, 5, GUILayout.Width(labelWidth3));
            GUI.SetNextControlName(TF_NAME_V_KI);
            _vki = GUILayout.TextField(_vki, 5, GUILayout.Width(labelWidth3));
            GUI.SetNextControlName(TF_NAME_V_KD);
            _vkd = GUILayout.TextField(_vkd, 5, GUILayout.Width(labelWidth3));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("a");
            GUI.SetNextControlName(TF_NAME_A_KP);
            _akp = GUILayout.TextField(_akp, 5, GUILayout.Width(labelWidth3));
            GUI.SetNextControlName(TF_NAME_A_KI);
            _aki = GUILayout.TextField(_aki, 5, GUILayout.Width(labelWidth3));
            GUI.SetNextControlName(TF_NAME_A_KD);
            _akd = GUILayout.TextField(_akd, 5, GUILayout.Width(labelWidth3));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();*/
            
            GUILayout.BeginHorizontal();
            _core.useDiffThrust = GUILayout.Toggle(_core.useDiffThrust, "Diff. thrust");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            if (GUI.GetNameOfFocusedControl() != TF_NAME_V_SPEED_MAX)
            {
                try {
                    _core.verticalMax = float.Parse(_vSpeedMax);
                } catch (FormatException) {}
                _vSpeedMax = _core.verticalMax.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_PITCH_MAX)
            {
                try {
                    _core.pitchMax = float.Parse(_pitchMax);
                } catch (FormatException) {}
                _pitchMax = _core.pitchMax.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_ROLL_MAX)
            {
                try {
                    _core.rollMax = float.Parse(_rollMax);
                } catch (FormatException) {}
                _rollMax = _core.rollMax.ToString();
            }
            
            /*if (GUI.GetNameOfFocusedControl() != TF_NAME_V_KP)
            {
                try {
                    _core.vPidKp = float.Parse(_vkp);
                } catch (FormatException) {}
                _vkp = _core.vPidKp.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_V_KI)
            {
                try {
                    _core.vPidKi = float.Parse(_vki);
                } catch (FormatException) {}
                _vki = _core.vPidKi.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_V_KD)
            {
                try {
                    _core.vPidKd = float.Parse(_vkd);
                } catch (FormatException) {}
                _vkd = _core.vPidKd.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_A_KP)
            {
                try {
                    _core.aPidKp = float.Parse(_akp);
                } catch (FormatException) {}
                _akp = _core.aPidKp.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_A_KI)
            {
                try {
                    _core.aPidKi = float.Parse(_aki);
                } catch (FormatException) {}
                _aki = _core.aPidKi.ToString();
            }
            if (GUI.GetNameOfFocusedControl() != TF_NAME_A_KD)
            {
                try {
                    _core.aPidKd = float.Parse(_akd);
                } catch (FormatException) {}
                _akd = _core.aPidKd.ToString();
            }*/
            
            foreach (PartElement partElem in removed)
            {
                _core.RemovePart(partElem);
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
        
        public void SelectPart(Part part)
        {
            Part selectedPart = null;
            if (part != null)
            {
                if (part.HighlightActive)
                {
                    // when it work
                    selectedPart = part;
                }
                else
                {
                    // Some part dont handle click. parent part is handled instead
                    foreach (Part child in part.children)
                    {
                        if (child.HighlightActive)
                        {
                            selectedPart = child;
                            break;
                        }
                    }
                }
            }
            if (selectedPart != null) {
                _core.AddPart(selectedPart);
            }
        }
        
        public void UpdateSelector(bool selecting)
        {
            if (_selecting == selecting)
            {
                return;
            }
            _selecting = selecting;
            foreach (Part part in FlightGlobals.ActiveVessel.parts)
            {
                if (_selecting)
                {
                    part.AddOnMouseDown(this.SelectPart);
                }
                else
                {
                    part.RemoveOnMouseDown(this.SelectPart);
                }
            }
        }
    }
}