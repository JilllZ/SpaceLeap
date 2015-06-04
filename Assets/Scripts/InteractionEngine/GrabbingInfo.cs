using UnityEngine;
using Leap;
using System.Collections;
using System.Collections.Generic;

public class GrabbingInfo {
    private class GrabbingInfoCalculator {
        private enum ValueType {
            GRAB,
            RELEASE,
            STABILITY
        }

        private interface IValueCalculator {
            void reset();
            void updateValue();
            float getValue();
            ValueType getValueType();
        }

        private abstract class ValueCalculator<T> : IValueCalculator where T : GrabbingInfoSettings.ValueSetting {
            protected float _value;
            protected GrabbingInfoCalculator _infoCalculator;
            protected T _settings;
            protected ValueType _valueType;

            public ValueCalculator(GrabbingInfoCalculator infoCalculator, T settings, ValueType valueType) {
                _infoCalculator = infoCalculator;
                _settings = settings;
                _valueType = valueType;
            }

            public abstract void reset();

            public abstract void updateValue();

            protected float transformValue(float value) {
                if (_valueType == ValueType.GRAB) {
                    return value;
                } else if (_valueType == ValueType.RELEASE) {
                    return -value;
                } else {
                    if (_infoCalculator._isCurrentlyGrabbing) {
                        return value;
                    } else {
                        return -value;
                    }
                }
            }

            public virtual float getValue() {
                return transformValue(_value);
            }

            public ValueType getValueType() {
                return _valueType;
            }
        }

        private abstract class FilteredValueCalculator<T> : ValueCalculator<T> where T : GrabbingInfoSettings.FilteredValueSetting {
            protected ExponentialFilter _filter = new ExponentialFilter();

            public FilteredValueCalculator(GrabbingInfoCalculator info, T settings,  ValueType valueType) : base(info, settings, valueType) { }

            public override void reset() {
                _value = 0.0f;
                _filter.reset(0);
            }

            protected abstract float getRawValue();

            public override void updateValue() {
                _filter.update(getRawValue(), _settings.smoothingAmount, _infoCalculator.deltaTime);
                _value = _filter.value;
            }
        }

        private abstract class CurvedFilteredValueCalculator<T> : FilteredValueCalculator<T> where T : GrabbingInfoSettings.CurvedFilteredValueSetting {
            public CurvedFilteredValueCalculator(GrabbingInfoCalculator info, T settings, ValueType valueType) : base(info, settings, valueType) { }

            public override float getValue() {
                return transformValue(_settings.valueCurve.Evaluate(_value));
            }
        }


        private class FingerProximityCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.FingerProximitySetting> {
            public FingerProximityCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.FingerProximitySetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                float fingerFactor = 1.0f - Mathf.Clamp01(_infoCalculator._discardedFingertipDistance / _settings.maxFingerDistance);
                float thumbFactor = 1.0f - Mathf.Clamp01(_infoCalculator._thumbDistance / _settings.maxThumbDistance);
                return fingerFactor * thumbFactor;
            }
        }

        private class FingerGrabbingPoseCalculator : FilteredValueCalculator<GrabbingInfoSettings.FingerGrabbingPoseSetting> {
            public FingerGrabbingPoseCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.FingerGrabbingPoseSetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                return _settings.fingerCurlCurve.Evaluate(_infoCalculator._discardOpenFingertipCurl);
            }
        }

        private class FingerReleasingPoseCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.FingerReleasingPoseSetting> {
            public FingerReleasingPoseCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.FingerReleasingPoseSetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                float raw = 1.0f - _infoCalculator._discardCloseFingertipCurl;
                DebugGraph.Log("Release Pose Raw", raw);
                return raw;
            }
        }

        private class FingerGrabbingSpeedCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.FingerGrabbingSpeedSetting> {
            private float _previousFingerCurl = 1.0f;

            public FingerGrabbingSpeedCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.FingerGrabbingSpeedSetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                float speed = (_infoCalculator._discardOpenFingertipCurl - _previousFingerCurl) / _infoCalculator.deltaTime;
                _previousFingerCurl = _infoCalculator._discardOpenFingertipCurl;
                return speed;
            }

            public override float getValue() {
                float value = base.getValue();
                if (value < _settings.minGrabSpeedThreshold && !_infoCalculator._isCurrentlyGrabbing) {
                    return -1;
                } else {
                    return value;
                }
            }
        }

        private class FingerReleasingSpeedCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.FingerReleasingSpeedSetting> {
            private float _previousFingerCurl;

            public FingerReleasingSpeedCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.FingerReleasingSpeedSetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                float ret = (_previousFingerCurl - _infoCalculator._discardCloseFingertipCurl) / _infoCalculator.deltaTime;
                _previousFingerCurl = _infoCalculator._discardCloseFingertipCurl;
                return ret;
            }
        }

        private class HandPalmDirectionCalculator : FilteredValueCalculator<GrabbingInfoSettings.HandPalmDirectionSetting> {

            public HandPalmDirectionCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.HandPalmDirectionSetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                Vector3 palmNorm = HandControllerUtil.toUnityDir(_infoCalculator._hand.PalmNormal);

                //Rough surface normal
                Vector3 internalNorm = _infoCalculator._closestPointToPalm - _infoCalculator._palmPos;
                if (_infoCalculator._palmToSurfaceDistance < 0) {
                    internalNorm = -internalNorm;
                }

                return Vector3.Angle(palmNorm, internalNorm) < _settings.maxPalmToSurfaceAngle ? 0.0f : 1.0f;
            }
        }

        private class HandPalmDistanceReleaseCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.HandPalmDistanceReleaseSetting> {
            public HandPalmDistanceReleaseCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.HandPalmDistanceReleaseSetting settings, ValueType valueType) : base(info, settings, valueType) { }

            protected override float getRawValue() {
                DebugGraph.Log("Palm To Surface Distance", _infoCalculator._palmToSurfaceDistance);
                return _infoCalculator._palmToSurfaceDistance / _settings.maxPalmDistance;
            }
        }

        private class HandMovementRateCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.HandMovementRateSettings> {
            public HandMovementRateCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.HandMovementRateSettings settings, ValueType valueType) : base(info, settings, valueType) { }

            private Vector3 _prevPalm = Vector3.zero;
            protected override float getRawValue() {
                Vector3 delta = _infoCalculator._palmPos - _prevPalm;
                float value = delta.magnitude / _infoCalculator.deltaTime;
                _prevPalm = _infoCalculator._palmPos;
                return value;
            }
        }

        private class HandRotationRateCalculator : CurvedFilteredValueCalculator<GrabbingInfoSettings.HandRotationRateSettings> {
            public HandRotationRateCalculator(GrabbingInfoCalculator info, GrabbingInfoSettings.HandRotationRateSettings settings, ValueType valueType) : base(info, settings, valueType) { }

            private Quaternion _prevRot = Quaternion.identity;
            protected override float getRawValue() {
                Quaternion rotation = _infoCalculator._hand.Basis.Rotation();
                float value = Quaternion.Angle(rotation, _prevRot) / _infoCalculator.deltaTime;
                _prevRot = rotation;
                return value;
            }
        }

        private GrabbingInfoSettings _settings;

        private List<IValueCalculator> _valueCalculators = new List<IValueCalculator>();

        private Hand _hand;
        private InteractionObject _targetVolume;

        private float _grabValue = 0.0f;
        private long _lastUpdatedFrameId;

        public long lastUpdatedFrameId {
            get {
                return _lastUpdatedFrameId;
            }
        }

        public float grabValue {
            get {
                return _grabValue;
            }
        }

        private string _logSuffix;
        private string _logId;
        private string _multiLogId;
        private bool _isCurrentlyGrabbing = false;

        private Vector3 _palmPos;
        private Vector3 _closestPointToPalm = Vector3.zero;
        private float _palmToSurfaceDistance = 0.0f;
        private float _discardOpenFingertipCurl = 0.0f;
        private float _discardCloseFingertipCurl = 0.0f;
        private float _thumbDistance = 0.0f;
        private float _discardedFingertipDistance = 0.0f;

        private float deltaTime;

        public GrabbingInfoCalculator(GrabbingInfoSettings settings, InteractionObject targetVolume) {
            reset(settings, targetVolume);
        }

        public void reset(GrabbingInfoSettings newSettings, InteractionObject targetVolume) {
            if (newSettings != _settings) {
                _settings = newSettings;

                _valueCalculators.Clear();

                _valueCalculators.Add(new FingerProximityCalculator(this, _settings.fingerProximity, ValueType.GRAB));
                _valueCalculators.Add(new FingerGrabbingPoseCalculator(this, _settings.fingerGrabbingPose, ValueType.GRAB));
                _valueCalculators.Add(new FingerGrabbingSpeedCalculator(this, _settings.fingerGrabbingSpeed, ValueType.GRAB));

                _valueCalculators.Add(new FingerReleasingPoseCalculator(this, _settings.fingerReleasingPose, ValueType.RELEASE));
                _valueCalculators.Add(new FingerReleasingSpeedCalculator(this, _settings.fingerReleasingSpeed, ValueType.RELEASE));
                _valueCalculators.Add(new HandPalmDirectionCalculator(this, _settings.handPalmDirection, ValueType.RELEASE));
                _valueCalculators.Add(new HandPalmDistanceReleaseCalculator(this, _settings.handPalmDistanceRelease, ValueType.RELEASE));

                _valueCalculators.Add(new HandMovementRateCalculator(this, _settings.movementRate, ValueType.STABILITY));
                _valueCalculators.Add(new HandRotationRateCalculator(this, _settings.rotationRate, ValueType.STABILITY));
            } else {
                foreach (IValueCalculator c in _valueCalculators) {
                    c.reset();
                }
            }

            _targetVolume = targetVolume;
        }

        private void calculateFingertipDistance() {
            _discardedFingertipDistance = signedTipDistToSurface(_hand.Fingers[0]);
            float farthestDistance = _discardedFingertipDistance;

            for (int i = 1; i < 5; i++) {
                float dist = signedTipDistToSurface(_hand.Fingers[i]);
                _discardedFingertipDistance += dist;
                if (dist > farthestDistance) {
                    farthestDistance = dist;
                }
            }

            _thumbDistance = signedTipDistToSurface(_hand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0]);

            _discardedFingertipDistance = (_discardedFingertipDistance - farthestDistance) / 4.0f;
        }

        private void calculateFingerCurl() {
            float curlSum = fingerCurl(_hand.Fingers[0]);
            float highestCurl = _discardCloseFingertipCurl;
            float lowestCurl = _discardCloseFingertipCurl;

            for (int i = 1; i < 5; i++) {
                float curl = fingerCurl(_hand.Fingers[i]);
                curlSum += curl;
                if (curl > highestCurl) {
                    highestCurl = curl;
                }
                if (curl < lowestCurl) {
                    lowestCurl = curl;
                }
            }

            _discardCloseFingertipCurl = (curlSum - highestCurl) / 4.0f;
            _discardOpenFingertipCurl = (curlSum - lowestCurl) / 4.0f;
        }

        private void calculatePalmDistanceToSurface() {
            _palmPos = HandControllerUtil.toUnitySpace(_hand.PalmPosition);
            _closestPointToPalm = _targetVolume.closestPointOnSurfaceW2W(_palmPos);
            _palmToSurfaceDistance = Vector3.Distance(_closestPointToPalm, _palmPos);
        }

        private float fingerCurl(Finger finger) {
            return finger.Direction.Dot(_hand.Basis.zBasis) * 0.5f + 0.5f;
        }

        private float signedTipDistToSurface(Finger finger) {
            Vector3 tip = HandControllerUtil.toUnitySpace(finger.TipPosition);
            return _targetVolume.signedDistanceToW2W(tip);
        }

        public void ensureUpdated(Hand newHand, bool isCurrentlyGrabbing) {
            if (newHand == _hand) {
                return;
            }
            _isCurrentlyGrabbing = isCurrentlyGrabbing;
            deltaTime = Time.fixedDeltaTime;

            _hand = newHand;

            calculatePalmDistanceToSurface();
            calculateFingerCurl();
            calculateFingertipDistance();

            float grabMax = 0.0f, releaseMax = 0.0f;
            float grabSum = 0.0f, releaseSum = 0.0f;

            foreach (IValueCalculator vc in _valueCalculators) {
                vc.updateValue();
                float value = vc.getValue();

                if (value > 0) {
                    grabSum += value;
                    if (value > grabMax) {
                        grabMax = value;
                    }
                } else {
                    releaseSum += -value;
                    if (-value > releaseMax) {
                        releaseMax = -value;
                    }
                }

                if (vc.getValueType() == ValueType.GRAB) {
                    DebugGraph.Log(vc.ToString(), value, value >= 0.0f ? Color.green : Color.red);
                } else {
                    DebugGraph.Log(vc.ToString(), -value, -value >= 0.0f ? Color.red : Color.green);
                } 
            }

            float grabWeighted = grabMax * grabSum / (grabSum + releaseSum);
            float releaseWeighted = releaseMax * releaseSum / (grabSum + releaseSum);

            _grabValue = Mathf.Clamp01(grabWeighted - releaseWeighted);

            Color logColor = Color.blue;
            if (_grabValue > _settings.releaseGrabThreshold.y) {
                logColor = Color.green;
            }
            if (_grabValue < _settings.releaseGrabThreshold.x) {
                logColor = Color.red;
            }

            DebugGraph.Log("GrabValue", _grabValue, logColor);

            _lastUpdatedFrameId = _hand.Frame.Id;
        }
    }

    private struct HandObjectPair{
        public int handId;
        public InteractionObject volume;

        public HandObjectPair(int handId, InteractionObject volume) {
            this.handId = handId;
            this.volume = volume;
        }
    }

    private static List<GrabbingInfoCalculator> _calculators = new List<GrabbingInfoCalculator>();
    private static Dictionary<HandObjectPair, int> _indexMap = new Dictionary<HandObjectPair, int>();

    private const long CALCULATOR_FRAME_TIMEOUT = 60; //number of frames to pass before a calculator can be recycled

    private static GrabbingInfoCalculator getCalculator(GrabbingInfoSettings settings, Hand hand, InteractionObject volume) {
        HandObjectPair key = new HandObjectPair(hand.Id, volume);
        long frameId = hand.Frame.Id;

        int index = -1;

        if (!_indexMap.TryGetValue(key, out index)) {
            for (int i = 0; i < _calculators.Count; i++) {
                if (_calculators[i].lastUpdatedFrameId < frameId - CALCULATOR_FRAME_TIMEOUT) {
                    _calculators[i].reset(settings, volume);
                    _indexMap[key] = i;
                    return _calculators[i];
                }
            }

            GrabbingInfoCalculator newCalculator = new GrabbingInfoCalculator(settings, volume);
            _indexMap[key] = _calculators.Count;
            _calculators.Add(newCalculator);
            return newCalculator;
        }

        return _calculators[index];
    }

    public static float getGrabStrength(GrabbingInfoSettings settings, Hand hand, InteractionObject volume, bool isCurrentlyGrabbing) {
        GrabbingInfoCalculator calculator = getCalculator(settings, hand, volume);
        calculator.ensureUpdated(hand, isCurrentlyGrabbing);
        return calculator.grabValue;
    }

    public static bool shouldGrab(GrabbingInfoSettings settings, Hand hand, InteractionObject volume) {
        return getGrabStrength(settings, hand, volume, false) > settings.releaseGrabThreshold.y;
    }

    public static bool shouldRelease(GrabbingInfoSettings settings, Hand hand, InteractionObject volume) {
        return getGrabStrength(settings, hand, volume, true) < settings.releaseGrabThreshold.x;
    }

    public static int totalCalculatorCount() {
        return _calculators.Count;
    }
}


