using System;
using System.Collections.Generic;
using UnityEngine;

namespace KspVtol
{
    public class Core
    {
        public Core()
        {
            FlightGlobals.ActiveVessel.OnFlyByWire += new FlightInputCallback(InputCallback);
            _pidCtrlVSpeed = new PidController(1.0f, 5.0f, 0.0f, 0.0f, 1.0f);
            _pidCtrlAttitude = new PidController(12.0f, 10.0f, 20.0f);
        }
        
        private PidController _pidCtrlVSpeed;
        public float vPidKp
        {
            get { return _pidCtrlVSpeed.kp; }
            set { _pidCtrlVSpeed.kp = value; }
        }
        public float vPidKi
        {
            get { return _pidCtrlVSpeed.ki; }
            set { _pidCtrlVSpeed.ki = value; }
        }
        public float vPidKd
        {
            get { return _pidCtrlVSpeed.kd; }
            set { _pidCtrlVSpeed.kd = value; }
        }
        private PidController _pidCtrlAttitude;
        public float aPidKp
        {
            get { return _pidCtrlAttitude.kp; }
            set { _pidCtrlAttitude.kp = value; }
        }
        public float aPidKi
        {
            get { return _pidCtrlAttitude.ki; }
            set { _pidCtrlAttitude.ki = value; }
        }
        public float aPidKd
        {
            get { return _pidCtrlAttitude.kd; }
            set { _pidCtrlAttitude.kd = value; }
        }
        
        private InfoUI _infoUI = null;
        public InfoUI InfoUI
        {
            set
            {
                _infoUI = value;
                if (_infoUI != null)
                {
                    _infoUI.AddInfo(() => Tuple.Create("current VSpeed : ",String.Format("{0:0.0000}", _currentVSpeed)));
                    _infoUI.AddInfo(() => Tuple.Create("Current pitch : ",String.Format("{0:0.0000}", _currentPitch)));
                    _infoUI.AddInfo(() => Tuple.Create("Current roll : ",String.Format("{0:0.0000}", _currentRoll)));
                    
                    _infoUI.CreateInfoLine("refVessel", Color.blue);
                    _infoUI.CreateInfoLine("pitchAxis", Color.red);
                    _infoUI.CreateInfoLine("rollAxis", Color.green);
                    _infoUI.CreateInfoLine("requestedUp", Color.white);
                }
            }
        }
        
        private float _currentVSpeed = 0.0f;
        private float _currentPitch = 0.0f;
        private float _currentRoll = 0.0f;
        
        private Config _config = new Config();
        public void AddPart(Part part)
        {
            if (!_active)
            {
                _config.AddPart(part);
            }
        }
        
        public void RemovePart(PartElement partElem)
        {
            _config.RemovePart(partElem);
        }
        
        public IEnumerable<PartElement> PartsList
        {
            get
            {
                foreach (PartElement partElem in _config.PartsList)
                {
                    yield return partElem;
                }
            }
        }
        
        public float verticalMax
        {
            get { return _config.verticalMax; }
            set { _config.verticalMax = value; }
        }
        
        public float pitchMax
        {
            get { return _config.pitchMax; }
            set { _config.pitchMax = value; }
        }
        
        public float rollMax
        {
            get { return _config.rollMax; }
            set { _config.rollMax = value; }
        }
        
        public bool isAirplane
        {
            get { return _config.isAirplane; }
            set { _config.isAirplane = value; }
        }
        public bool isRocket
        {
            get { return _config.isRocket; }
            set { _config.isRocket = value; }
        }
        
        public bool useDiffThrust
        {
            get { return _config.useDiffThrust; }
            set { _config.useDiffThrust = value; }
        }
        
        private bool _active = false;
        public bool active
        {
            get { return _active; }
            set
            {
                if (_active == value) return;
                _active = value;
                _config.ActiveParts(_active);
            }
        }
        
        private bool _activeVSpeed = true;
        public bool activeVSpeed
        {
            get { return _activeVSpeed; }
            set { _activeVSpeed = value; }
        }
        
        private bool _activeAttitude = true;
        public bool activeAttitude
        {
            get { return _activeAttitude; }
            set { _activeAttitude = value; }
        }
        
        private float _requestedVSpeed = 0.0f;
        public float requestedVSpeed
        {
            get { return _requestedVSpeed; }
            set
            {
                if (!_active || !_activeVSpeed) return;
                if (_requestedVSpeed != value)
                {
                    _pidCtrlVSpeed.ResetPID();
                }
                _requestedVSpeed = value;
            }
        }
        
        private float _requestedPitch = 0.0f;
        public float requestedPitch
        {
            get { return _requestedPitch; }
            set
            {
                if (!_active || !_activeAttitude) return;
                if (_requestedPitch != value)
                {
                    _pidCtrlAttitude.ResetPID();
                }
                _requestedPitch = value;
            }
        }
        
        private float _requestedRoll = 0.0f;
        public float requestedRoll
        {
            get { return _requestedRoll; }
            set
            {
                if (!_active || !_activeAttitude) return;
                if (_requestedRoll != value)
                {
                    _pidCtrlAttitude.ResetPID();
                }
                _requestedRoll = value;
            }
        }

        public void InputCallback(FlightCtrlState fcs)
        {
            if (!_active)
            {
                _pidCtrlVSpeed.ResetPID();
                _pidCtrlAttitude.ResetPID();
                return;
            }
            
            Vessel vessel = FlightGlobals.ActiveVessel;
            
            float throttleCmd = 0.0f;
            float pitchCmd = 0.0f;
            float rollCmd = 0.0f;
            
            _currentVSpeed = Vector3.Dot(vessel.velocityD, vessel.up);
            
            if (_activeVSpeed)
            {
                float errorV = _requestedVSpeed - _currentVSpeed;
                Vector3 commandV = _pidCtrlVSpeed.Compute(new Vector3(0.0f,errorV,0.0f));
                throttleCmd = 100.0f * commandV.y;
            }
            else
            {
                _pidCtrlVSpeed.ResetPID();
            }
            
            Vector3 refVessel = Vector3.zero;
            Vector3 pitchAxis = Vector3.zero;
            Vector3 rollAxis = Vector3.zero;
            if (_config.isAirplane)
            {
                refVessel = -Vector3.forward;
                pitchAxis = Vector3.right;
                rollAxis = Vector3.up;
            }
            else
            {
                refVessel = Vector3.up;
                pitchAxis = Vector3.right;
                rollAxis = Vector3.forward;
            }

            Vector3 localUp = vessel.transform.InverseTransformDirection(vessel.up).normalized;
            Quaternion qPitch = Quaternion.AngleAxis(_requestedPitch, pitchAxis);
            Quaternion qRoll = Quaternion.AngleAxis(-_requestedRoll, rollAxis);
            Vector3 requestedUp = qPitch * qRoll * localUp;
            
            if (_infoUI != null)
            {
                Vector3 currentPitchV = Vector3.ProjectOnPlane(localUp, pitchAxis);
                _currentPitch = Vector3.SignedAngle(currentPitchV, refVessel, pitchAxis);
                Vector3 currentRollV = Vector3.ProjectOnPlane(localUp, rollAxis);
                _currentRoll = Vector3.SignedAngle(currentRollV, refVessel, rollAxis);
                _infoUI.DisplayInfoLine("refVessel", refVessel);
                _infoUI.DisplayInfoLine("pitchAxis", pitchAxis);
                _infoUI.DisplayInfoLine("rollAxis", rollAxis);
                _infoUI.DisplayInfoLine("requestedUp", requestedUp);
            }
            
            if (_activeAttitude)
            {
                Vector3 errorA = requestedUp - refVessel;
                Vector3 commandA = _pidCtrlAttitude.Compute(errorA);
                if (_config.isAirplane)
                {
                    // SAS cmd
                    fcs.pitch = -commandA.y;
                    fcs.roll = commandA.x;
                    // Differential thrust
                    pitchCmd = -commandA.y;
                    rollCmd = commandA.x;
                }
                else
                {
                    // SAS cmd
                    fcs.pitch = -commandA.z;
                    fcs.yaw = commandA.x;
                    // Differential thrust
                    pitchCmd = -commandA.z;
                    rollCmd = commandA.x;
                }
            }
            else
            {
                _pidCtrlAttitude.ResetPID();
            }
            
            throttleCmd = Mathf.Clamp(throttleCmd, 0.0f, 100.0f);
            pitchCmd = Mathf.Clamp(pitchCmd, -20.0f, 20.0f);
            rollCmd = Mathf.Clamp(rollCmd, -20.0f, 20.0f);
            _config.SetCommand(throttleCmd, pitchCmd, rollCmd);
        }
    }
    
    public class PidController
	{
		private float _kp, _ki, _kd; // proportional, integral, derivative
		private Vector3 _pError, _iError, _dError;
        private float _min, _max;

		public PidController(float kp, float ki, float kd)
		{
			_kp = kp;
			_ki = ki;
			_kd = kd;
			_pError = Vector3.zero;
			_iError = Vector3.zero;
			_dError = Vector3.zero;
            _min = -1.0f;
            _max = 1.0f;
		}
        
        public PidController(float kp, float ki, float kd, float min, float max)
		{
			_kp = kp;
			_ki = ki;
			_kd = kd;
			_pError = Vector3.zero;
			_iError = Vector3.zero;
			_dError = Vector3.zero;
            _min = min;
            _max = max;
		}
        
		public Vector3 Compute(Vector3 error)
		{
			_iError += error * TimeWarp.fixedDeltaTime;
			_dError = (error - _pError) / TimeWarp.fixedDeltaTime;

			Vector3 result = _kp * error + _ki * _iError + _kd * _dError;

			Vector3 clampedResult = new Vector3(Mathf.Clamp(result.x, _min, _max),
			                                    Mathf.Clamp(result.y, _min, _max),
			                                    Mathf.Clamp(result.z, _min, _max));

			if (Math.Abs((clampedResult - result).magnitude) > 0.01)
            {
				_iError -= error * TimeWarp.fixedDeltaTime;
			}

			_pError = error;

			return clampedResult;
		}

		public float kp
		{
			get { return _kp; }
			set
            {
				if (_kp == value) return;
				_kp = value;
				ResetPID();
			}
		}

		public float ki
		{
			get { return _ki; }
			set
            {
				if (_ki == value) return;
				_ki = value;
				ResetPID();
			}
		}

		public float kd
		{
			get { return _kd; }
			set
            {
				if (_kd == value) return;
				_kd = value;
				ResetPID();
			}
		}
        
		public void ResetPID()
		{
			_pError = Vector3.zero;
			_iError = Vector3.zero;
			_dError = Vector3.zero;
		}
	}
}