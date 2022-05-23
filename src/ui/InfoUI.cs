using System;
using System.Collections.Generic;
using UnityEngine;

namespace KspVtol
{
    public class InfoUI
    {
        private Rect _windowRect = Rect.zero;
        private Vector2 _size = new Vector2(300, 150);
        private bool _isDisplayed = false;
        public void ToggleDisplay()
        {
            _isDisplayed = !_isDisplayed;
            // hide lines
            _lineDisplayed = false;
        }
        private bool _lineDisplayedVal = false;
        private bool _lineDisplayed
        {
            get { return _lineDisplayedVal; }
            set
            {
                if (_lineDisplayedVal != value)
                {
                    // active/deactive parent object of line
                    _lineDisplayedVal = value;
                    foreach(GameObject objLine in _lines.Values)
                    {
                        objLine.SetActive(_lineDisplayedVal);
                        if (_lineDisplayedVal)
                        {
                            LineRenderer line = objLine.GetComponent<LineRenderer>(); 
                            line.transform.parent = FlightGlobals.ActiveVessel.transform;
                            line.transform.localPosition = Vector3.zero;
                            line.transform.localEulerAngles = Vector3.zero; 
                        }
                    }
                }
            }
        }
        private Rect _boxPos = Rect.zero;
        private Vector2 _scrollConfVector = Vector2.zero;
        
        // Textual info: delegate function that return 2 string: info title and info value
        public delegate Tuple<string,string> InfoHandler();
        private List<InfoHandler> _infoList = new List<InfoHandler>();
        public void AddInfo(InfoHandler handler)
        {
            _infoList.Add(handler);
        }
        
        // Vectorial info
        private Dictionary<string,GameObject> _lines = new Dictionary<string,GameObject>();
        private Material _lineMaterial = null;
        // Create vector info with name as key. Vector direction is set by DisplayInfoLine
        public void CreateInfoLine(string name, Color color)
        {
            if (!_lines.ContainsKey(name))
            {
                GameObject objLine = new GameObject("DebugLine"+name);
                LineRenderer line = objLine.AddComponent<LineRenderer>();
                line.useWorldSpace = false;
                if (_lineMaterial != null)
                {
                    line.material = _lineMaterial;
                    line.material.color = color;
                }
                line.startColor = color;
                line.endColor = color;
                line.startWidth = 1.0f;
                line.endWidth = 0.0f;
                line.positionCount = 2;
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, Vector3.zero);
                objLine.SetActive(false);
                
                _lines.Add(name, objLine);
            }
        }
        
        // Configure vector direction
        public void DisplayInfoLine(string name, Vector3 direction)
        {
            GameObject objLine = null;
            
            if (_lines.TryGetValue(name, out objLine) && (objLine != null))
            {
                LineRenderer line = objLine.GetComponent<LineRenderer>();
                line.SetPosition(1, direction.normalized * 5);
            }
        }
        
        public InfoUI(Vector2 pos)
        {
            _windowRect = new Rect(pos, _size);
            
            _lineMaterial = new Material(Shader.Find("Diffuse"));
        }
        
        public void Display()
        {
            if (_isDisplayed)
            {
                _windowRect.size = _size;
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Keyboard), _windowRect, DoWindow, "Information");
            }
        }
        
        private void DoWindow(int windowID)
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            _lineDisplayed = GUILayout.Toggle(_lineDisplayed, "Show vectors");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
            {
                _isDisplayed = false;
                _lineDisplayed = false;
            }
            GUILayout.EndHorizontal();
            
            if (Event.current.type == EventType.Repaint)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                RectOffset rctOff = GUI.skin.button.margin;
                _boxPos = new Rect(rctOff.left,
                                lastRect.y+lastRect.height+rctOff.top,
                                _windowRect.width-rctOff.horizontal,
                                _windowRect.height-(lastRect.y+lastRect.height+rctOff.vertical));
            }
            GUILayout.BeginArea(_boxPos, GUI.skin.GetStyle("Box"));
            _scrollConfVector = GUILayout.BeginScrollView(_scrollConfVector);
            
            foreach (InfoHandler handler in _infoList)
            {
                Tuple<string,string> info = handler();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(info.Item1);
                GUILayout.FlexibleSpace();
                GUILayout.Label(info.Item2);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUILayout.EndVertical();
            
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}