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
            _pidCtrlAttitude = new PidController(12, 10, 20);
        }
        
        private PidController _pidCtrlAttitude;
        
        private InfoUI _infoUI = null;
        public InfoUI InfoUI
        {
            set
            {
                _infoUI = value;
                if (_infoUI != null)
                {
                    _infoUI.AddInfo(() => Tuple.Create("eulerAngles.x : ",String.Format("{0:0.0000}", scaleAngle(_srfRotation.eulerAngles.x))));
                    _infoUI.AddInfo(() => Tuple.Create("eulerAngles.y : ",String.Format("{0:0.0000}", scaleAngle(_srfRotation.eulerAngles.y))));
                    _infoUI.AddInfo(() => Tuple.Create("eulerAngles.z : ",String.Format("{0:0.0000}", scaleAngle(_srfRotation.eulerAngles.z))));
/*                    _infoUI.AddInfo(() => Tuple.Create("error.x : ",String.Format("{0:0.0000}", _error.x)));
                    _infoUI.AddInfo(() => Tuple.Create("error.y : ",String.Format("{0:0.0000}", _error.y)));
                    _infoUI.AddInfo(() => Tuple.Create("error.z : ",String.Format("{0:0.0000}", _error.z)));
*/
                    _infoUI.AddInfo(() => Tuple.Create("command.x : ",String.Format("{0:0.0000}", _command.x)));
                    _infoUI.AddInfo(() => Tuple.Create("command.y : ",String.Format("{0:0.0000}", _command.y)));
                    _infoUI.AddInfo(() => Tuple.Create("command.z : ",String.Format("{0:0.0000}", _command.z)));

                    _infoUI.AddInfo(() => Tuple.Create("_throttleCmd : ",String.Format("{0:0.0000}", _throttleCmd)));
                }
            }
        }
        
        // log Temp
        private Quaternion _srfRotation = Quaternion.identity;
        private Vector3 _command = Vector3.zero;
        private Vector3 _error = Vector3.zero;
        
        
        private float _throttleCmd = 0.0f;
        private float _pitchCmd = 0.0f;
        private float _rollCmd = 0.0f;
        
        private Config _config = new Config();
        public void AddPart(Part part)
        {
            _config.AddPart(part, _active, _throttleCmd, _pitchCmd, _rollCmd);
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
        
        private float _commandVSpeed = 0.0f;
        public float commandVSpeed
        {
            get { return _commandVSpeed; }
            set
            {
                if (!_active || !_activeVSpeed) return;
                _commandVSpeed = value;
            }
        }
        
        private float _commandPitch = 0.0f;
        public float commandPitch
        {
            get { return _commandPitch; }
            set
            {
                if (!_active || !_activeAttitude) return;
                if (_commandPitch != value)
                {
                    _pidCtrlAttitude.ResetPID();
                }
                _commandPitch = value;
            }
        }
        
        private float _commandRoll = 0.0f;
        public float commandRoll
        {
            get { return _commandRoll; }
            set
            {
                if (!_active || !_activeAttitude) return;
                if (_commandRoll != value)
                {
                    _pidCtrlAttitude.ResetPID();
                }
                _commandRoll = value;
            }
        }

        private double _previousCompute = 0.0;
        private double _previousPitch = 0.0;
        private double _previousRoll = 0.0;
        
        private static double V_SPEED_MIN_STEP = 0.5;
        private static double PITCH_MIN_STEP = 0.5;
        private static double ROLL_MIN_STEP = 0.5;
        
        private static double scaleAngle(double angle)
        {
            return (angle > 180.0f) ? (360.0f - angle) : -angle;
        }
        
        public void InputCallback(FlightCtrlState fcs)
        {
            if (!_active)
            {
                _pidCtrlAttitude.ResetPID();
                return;
            }
            
            Vessel vessel = FlightGlobals.ActiveVessel;
            
            if (_activeVSpeed)
            {
                double verticalSpeed = Vector3d.Dot(vessel.velocityD, vessel.up);
                double verticalAcceleration = Vector3d.Dot(vessel.acceleration, vessel.up);
                double correction = _commandVSpeed - verticalSpeed;

                if (correction > V_SPEED_MIN_STEP) {
                    if (verticalAcceleration <= -1.0) {
                        _throttleCmd += 10.00f;
                    } else if (verticalAcceleration <= 0.0) {
                        _throttleCmd += 5.00f;
                    }
                } else if (correction < -V_SPEED_MIN_STEP) {
                    if (verticalAcceleration >= 0) {
                        _throttleCmd = 0.0f;
                    }
                }
            }
            else
            {
                _throttleCmd = 0.0f;
            }
            
            if (_activeAttitude)
            {
                Vector3 heading = vessel.up;
                Vector3 refVessel = Vector3.zero;
                if (_config.isAirplane)
                {
                    refVessel = Vector3.back;
                }
                else
                {
                    refVessel = Vector3.up;
                }

                Vector3d north = Vector3.ProjectOnPlane((vessel.mainBody.position + vessel.mainBody.transform.up * (float)vessel.mainBody.Radius) - vessel.CoMD, vessel.up).normalized;
                Quaternion srfRotation = Quaternion.Inverse(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(vessel.transform.rotation) * Quaternion.LookRotation(north, vessel.up));
                
                _srfRotation = srfRotation;
                
                Quaternion qPitch = Quaternion.AngleAxis(-_commandPitch, srfRotation * Vector3.right);
                Quaternion qRoll = Quaternion.AngleAxis(-_commandRoll, srfRotation * Vector3.forward);
                heading = qPitch * qRoll * vessel.up;
                //heading = qPitch * qRoll * srfRotation * Vector3.up;
                
                Vector3 error = vessel.transform.InverseTransformDirection(heading).normalized - refVessel;
                _error = error;
                if (_infoUI != null)
                {
                    _infoUI.lineOrientation = error;
                }
                Vector3 command = _pidCtrlAttitude.Compute(error);
                
                _command = command;
                
                if (_config.isAirplane)
                {
                    fcs.pitch = -command.y;
                    fcs.roll = command.x;
                }
                else
                {
                    fcs.pitch = -command.z;
                    fcs.yaw = command.x;
                }
                
                /*double currentTime = Time.time;
                Vector3d north = Vector3.ProjectOnPlane((vessel.mainBody.position + vessel.mainBody.transform.up * (float)vessel.mainBody.Radius) - vessel.CoMD, vessel.up).normalized;
                Quaternion srfRotation = Quaternion.Inverse(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(vessel.transform.rotation) * Quaternion.LookRotation(north, vessel.up));
                
                double pitch = 0.0;
                double roll = 0.0;
                // Avion: heading = eulerY ; pitch = eulerX ; roll = eulerZ
                // fusee: heading = eulerZ ; pitch = eulerX ; roll = eulerY
                if (_config.isAirplane)
                {
                    pitch = scaleAngle(srfRotation.eulerAngles.x);
                    roll = scaleAngle(srfRotation.eulerAngles.z);
                }
                else
                {
                    pitch = scaleAngle(srfRotation.eulerAngles.x) - 90.0;
                    roll = scaleAngle(srfRotation.eulerAngles.y);
                }
                
                if ((_previousCompute != 0.0) && (_previousCompute != currentTime))
                {
                    double pitchSpeed = (pitch - _previousPitch) / (currentTime - _previousCompute);
                    double rollSpeed = (roll - _previousRoll) / (currentTime - _previousCompute);
                    double correctionPitch = _commandPitch - pitch;
                    double correctionRoll = _commandRoll - roll;
                    
                    if (correctionPitch > PITCH_MIN_STEP)
                    {
                        // il faut rajouter du pitch
                        if (pitchSpeed <= -3.0)
                        {
                            // on est en train d'en enlever beaucoup => augmente la balance beaucoup
                            _pitchCmd += 0.50f;
                        }
                        else if (pitchSpeed <= 0.0)
                        {
                            // on est en train d'en enlever => augmente la balance
                            _pitchCmd += 0.10f;
                        }
                        else if (pitchSpeed >= 2.0)
                        {
                            // on est en train d'en rajouter trop vite => diminue la balance
                            _pitchCmd -= 0.05f;
                        }
                    }
                    else if (correctionPitch < -PITCH_MIN_STEP)
                    {
                        // il faut enlever du pitch
                        if (pitchSpeed >= 3.0)
                        {
                            // on est en train d'en rajouter beaucoup => diminue la balance beaucoup
                            _pitchCmd -= 0.50f;
                        }
                        else if (pitchSpeed >= 0.0)
                        {
                            // on est en train d'en rajouter => diminue la balance
                            _pitchCmd -= 0.10f;
                        }
                        else if (pitchSpeed <= -2.0)
                        {
                            // on est en train d'en enlever trop vite => augmente la balance
                            _pitchCmd += 0.05f;
                        }
                    }
                    else
                    {
                        // on est dans la marge. On annule la balance
                        _pitchCmd = 0.0f;
                    }
                    if (correctionRoll > ROLL_MIN_STEP)
                    {
                        // il faut rajouter du roll
                        if (rollSpeed <= -3.0)
                        {
                            // on est en train d'en enlever beaucoup => augmente la balance beaucoup
                            _rollCmd += 0.50f;
                        }
                        else if (rollSpeed <= 0.0)
                        {
                            // on est en train d'en enlever => augmente la balance
                            _rollCmd += 0.10f;
                        }
                        else if (rollSpeed >= 2.0)
                        {
                            // on est en train d'en rajouter trop vite => diminue la balance
                            _rollCmd -= 0.05f;
                        }
                    }
                    else if (correctionRoll < -ROLL_MIN_STEP)
                    {
                        // il faut enlever du pitch
                        if (rollSpeed >= 3.0)
                        {
                            // on est en train d'en rajouter beaucoup => diminue la balance beaucoup
                            _rollCmd -= 0.50f;
                        }
                        else if (rollSpeed >= 0.0)
                        {
                            // on est en train d'en rajouter => diminue la balance
                            _rollCmd -= 0.10f;
                        }
                        else if (rollSpeed <= -2.0)
                        {
                            // on est en train d'en enlever trop vite => augmente la balance
                            _rollCmd += 0.05f;
                        }
                    }
                    else
                    {
                        // on est dans la marge. On annule la balance
                        _rollCmd = 0.0f;
                    }
                }
                _previousCompute = currentTime;
                _previousPitch = pitch;
                _previousRoll = roll;*/
                _pitchCmd = 0.0f;
                _rollCmd = 0.0f;
            }
            else
            {
                _pitchCmd = 0.0f;
                _rollCmd = 0.0f;
                _previousCompute = 0.0f;
                _pidCtrlAttitude.ResetPID();
            }
            
            _throttleCmd = Mathf.Clamp(_throttleCmd, 0.0f, 100.0f);
            _pitchCmd = Mathf.Clamp(_pitchCmd, -20.0f, 20.0f);
            _rollCmd = Mathf.Clamp(_rollCmd, -20.0f, 20.0f);
            _config.SetCommand(_throttleCmd, _pitchCmd, _rollCmd);
        }
        
    }
    
    public class PidController
	{
		private float _kp, _ki, _kd; // proportional, integral, derivative
		private Vector3 _pError, _iError, _dError;

		public PidController(float kp, float ki, float kd)
		{
			_kp = kp;
			_ki = ki;
			_kd = kd;
			_pError = Vector3.zero;
			_iError = Vector3.zero;
			_dError = Vector3.zero;
		}
        
		public Vector3 Compute(Vector3 error)
		{
			_iError += error * TimeWarp.fixedDeltaTime;
			_dError = (error - _pError) / TimeWarp.fixedDeltaTime;

			Vector3 result = _kp * error + _ki * _iError + _kd * _dError;

			Vector3 clampedResult = new Vector3(Mathf.Clamp(result.x, -1.0f, 1.0f),
			                                    Mathf.Clamp(result.y, -1.0f, 1.0f),
			                                    Mathf.Clamp(result.z, -1.0f, 1.0f));

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