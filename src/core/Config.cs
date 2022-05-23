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
            set { if ((value >= 2.0f) && (value <= 1000.0f)) _verticalMax = value; }
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
        
        private bool _useDiffThrust = true;
        public bool useDiffThrust
        {
            get { return _useDiffThrust; }
            set { _useDiffThrust = value; }
        }
        
        private List<PartElement> _partsList = new List<PartElement>();
        public List<PartElement> PartsList { get { return _partsList; } }
        
        public void AddPart(Part part)
        {
            foreach (PartElement partElem in _partsList)
            {
                if (partElem.part == part) return;
            }
            PartElement newElement = new PartElement(part);
            if (newElement.IsValid())
            {
                _partsList.Add(newElement);
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
                if (_useDiffThrust)
                {
                    partElem.SetCommand(throttleP, pitchP, rollP, _isAirplane);
                }
                else
                {
                    partElem.SetCommand(throttleP, 0.0f, 0.0f, _isAirplane);
                }
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
        {
            _part = part;
            if (_part != null)
            {
                // only engine or motor part
                _module = (PartModule) _part.FindModuleImplementing<ModuleEngines>();
                if (_module == null)
                {
                    _module = (PartModule) _part.FindModuleImplementing<ModuleRoboticServoRotor>();
                }
                // store position of part in vessel local space. It will never change
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
            
            // Use differential thrust
            if ((pitchP != 0.0f) || (rollP != 0.0f))
            {
                Vessel vessel = FlightGlobals.ActiveVessel;
                // COM can change during flight. Get current value
                Vector3 posCoM = vessel.transform.InverseTransformPoint(vessel.CoM);
                
                float pitchLever = 0.0f;
                float rollLever = 0.0f;
                if (isAirplane)
                {
                    pitchLever = _position.y - posCoM.y;
                    rollLever = _position.x - posCoM.x;
                }
                else
                {
                    pitchLever = _position.z - posCoM.z;
                    rollLever = _position.x - posCoM.x;
                }
                if (Math.Abs(pitchLever) > 0.5f)
                {
                    if (pitchLever > 0.0f)
                    {
                        inputCmd += pitchP;
                    }
                    else
                    {
                        inputCmd -= pitchP;
                    }
                }
                if (Math.Abs(rollLever) > 0.5f)
                {
                    if (rollLever > 0.0f)
                    {
                        inputCmd -= rollP;
                    }
                    else
                    {
                        inputCmd += rollP;
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