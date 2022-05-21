using System;
using System.Collections.Generic;
using UnityEngine;
using Expansions.Serenity;

namespace KspVtol
{
    public class Config
    {
        private float _verticalMax = 50.0f;
        public float verticalMax
        {
            get { return _verticalMax; }
            set { if ((value >= 2.0f) && (value <= 200.0f)) _verticalMax = value; }
        }
        
        private float _pitchMax = 20.0f;
        public float pitchMax
        {
            get { return _pitchMax; }
            set { if ((value >= 2.0f) && (value <= 80.0f)) _pitchMax = value; }
        }
        
        private float _rollMax = 20.0f;
        public float rollMax
        {
            get { return _rollMax; }
            set { if ((value >= 2.0f) && (value <= 80.0f)) _rollMax = value; }
        }
        
        private bool _isAirplane = true;
        public bool isAirplane
        {
            get { return _isAirplane; }
            set { _isAirplane = value; }
        }
        public bool isRocket
        {
            get { return !_isAirplane; }
            set { _isAirplane = !value; }
        }
        
        private List<PartElement> _partsList = new List<PartElement>();
        public List<PartElement> PartsList { get { return _partsList;} }
        
        public void AddPart(Part part, bool active, float throttleP, float pitchP, float rollP)
        {
            foreach (PartElement partElem in _partsList)
            {
                if (partElem.part == part) return;
            }
            PartElement newElement = new PartElement(part);
            if (newElement.IsValid())
            {
                _partsList.Add(newElement);
                newElement.Activate(active);
                newElement.SetCommand(throttleP, pitchP, rollP, _isAirplane);
            }
        }
        
        public void RemovePart(PartElement part)
        {
            part.Activate(false);
            _partsList.Remove(part);
        }
        
        public void ActiveParts(bool active)
        {
            foreach (PartElement partElem in _partsList)
            {
                partElem.Activate(active);
            }
        }
        
        public void SetCommand(float throttleP, float pitchP, float rollP)
        {
            foreach (PartElement partElem in _partsList)
            {
                partElem.SetCommand(throttleP, pitchP, rollP, _isAirplane);
            }
        }
    }
    
    public class PartElement
    {
        private Part _part = null;
        public Part part { get { return _part; } }
        private Vector3 _position;
        private PartModule _module = null;
        private bool _isIndependent = false;
        
        public PartElement(Part part)
        {Vessel vessel = FlightGlobals.ActiveVessel;
            _part = part;
            if (_part != null)
            {
                _module = (PartModule) _part.FindModuleImplementing<ModuleEngines>();
                if (_module == null)
                {
                    _module = (PartModule) _part.FindModuleImplementing<ModuleRoboticServoRotor>();
                }
                _position = FlightGlobals.ActiveVessel.transform.InverseTransformPoint(_part.transform.position);
            }
        }
        
        public override string ToString()
        {
            if (!IsValid())
            {
                return "Invalid";
            }
            else
            {
                return _part.ToString();
            }
        }
        
        public bool IsValid()
        {
            return ((_part != null) && (_module != null));
        }
        
        public void Activate(bool active)
        {
            if (!IsValid())
            {
                return;
            }
            if (_module is ModuleEngines)
            {
                if (active)
                {
                    _isIndependent = ((ModuleEngines)_module).independentThrottle;
                    ((ModuleEngines)_module).independentThrottle = true;
                }
                else
                {
                    ((ModuleEngines)_module).independentThrottle = _isIndependent;
                }
            }
            if (_module is ModuleRoboticServoRotor)
            {
                ((ModuleRoboticServoRotor)_module).servoMotorLimit = 0.0f;
            }
        }
        
        public void SetCommand(float throttleP, float pitchP, float rollP, bool isAirplane)
        {
            float inputCmd = throttleP;
            
            if ((pitchP != 0.0f) || (rollP != 0.0f))
            {
                // x,y,z : avion: y = pos avant/arriere (pitch), x = pos gauche/droite (roll), z = hauteur
                // x,y,z : fusee: y = hauteur, x = pos avant/arriere (pitch), z = pos gauche/droite (roll)
                Vessel vessel = FlightGlobals.ActiveVessel;
                Vector3 posCoM = vessel.transform.InverseTransformPoint(vessel.CoM);
                
                if (isAirplane)
                {
                    if (Math.Abs(_position.y - posCoM.y) > 0.5f)
                    {
                        if (_position.y > posCoM.y)
                        {
                            inputCmd += pitchP;
                        }
                        else
                        {
                            inputCmd -= pitchP;
                        }
                    }
                    if (Math.Abs(_position.x - posCoM.x) > 0.5f)
                    {
                        if (_position.x > posCoM.x)
                        {
                            inputCmd -= rollP;
                        }
                        else
                        {
                            inputCmd += rollP;
                        }
                    }
                }
                else
                {
                    if (Math.Abs(_position.x - posCoM.x) > 0.5f)
                    {
                        if (_position.x > posCoM.x)
                        {
                            inputCmd -= pitchP;
                        }
                        else
                        {
                            inputCmd += pitchP;
                        }
                    }
                    if (Math.Abs(_position.z - posCoM.z) > 0.5f)
                    {
                        if (_position.z > posCoM.z)
                        {
                            inputCmd += rollP;
                        }
                        else
                        {
                            inputCmd -= rollP;
                        }
                    }
                }
            }
            
            inputCmd = Mathf.Clamp(inputCmd, 0.0f, 100.0f);
            if (_module is ModuleEngines)
            {
                ((ModuleEngines)_module).independentThrottlePercentage = inputCmd;
            }
            if (_module is ModuleRoboticServoRotor)
            {
                ((ModuleRoboticServoRotor)_module).servoMotorLimit = inputCmd;
            }
        }
    }
}