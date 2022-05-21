using System;
using System.Collections.Generic;
using UnityEngine;

namespace KspVtol
{
    public class InfoUI
    {
        private Rect _windowRect = Rect.zero;
        private Vector2 _size = new Vector2(500, 300);
        private bool _isDisplayed = false;
        private Rect _boxPos = Rect.zero;
        private Vector2 _scrollConfVector = Vector2.zero;
        
        public delegate Tuple<string,string> InfoHandler();
        private List<InfoHandler> _infoList = new List<InfoHandler>();
        public void AddInfo(InfoHandler handler)
        {
            _infoList.Add(handler);
        }
        
        private GameObject _objLine = null;
        private LineRenderer _line = null;
        private Vector3 _lineOrientation = Vector3.zero;
        public Vector3 lineOrientation { set { _lineOrientation = value; } }
        
        public InfoUI(Vector2 pos)
        {
            _windowRect = new Rect(pos, _size);
            
            _objLine = new GameObject( "Line" );
            _line = _objLine.AddComponent<LineRenderer>();
            _line.useWorldSpace = false;

            _line.startColor = Color.red;
            _line.endColor = Color.yellow;
            _line.startWidth = 1.0f;
            _line.endWidth = 0.0f;
            _line.positionCount = 2;
            _line.SetPosition( 0, Vector3.zero );
            _line.SetPosition( 1, Vector3.up * 5 );
            _objLine.SetActive(false);
        }
        
        public void Display()
        {
            if (_isDisplayed)
            {
                _windowRect.size = _size;
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Keyboard), _windowRect, DoWindow, "Information");
            }
        }
        
        public void ToggleDisplay()
        {
            _isDisplayed = !_isDisplayed;
            if (_isDisplayed)
            {
                _line.transform.parent = FlightGlobals.ActiveVessel.transform;
                _line.transform.localPosition = Vector3.zero;
                _line.transform.localEulerAngles = Vector3.zero; 
                _objLine.SetActive(true);
            }
            else
            {
                _objLine.SetActive(false);
            }
        }
        
        private void DoWindow(int windowID)
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
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
        
        public void Update()
        {
            if (_isDisplayed)
            {
                _line.SetPosition( 1, _lineOrientation.normalized * 5 );
            }
        }
    }
}