using UnityEngine;
using Leap;
using System.Collections;

public class GrabbingInfoSettings : MonoBehaviour {
    public abstract class ValueSetting {
    }

    public abstract class FilteredValueSetting : ValueSetting {
        [Range (0, 1)]
        public float smoothingAmount;
    }

    public abstract class CurvedFilteredValueSetting : FilteredValueSetting{
        public AnimationCurve valueCurve;
    }

    [System.Serializable]
    public class FingerProximitySetting : CurvedFilteredValueSetting {
		public float maxFingerDistance;
        public float maxThumbDistance;
	}

    [System.Serializable]
    public class FingerGrabbingPoseSetting : FilteredValueSetting {
        public AnimationCurve fingerCurlCurve;
    }

    [System.Serializable]
    public class FingerReleasingPoseSetting : CurvedFilteredValueSetting { }

    [System.Serializable]
    public class FingerGrabbingSpeedSetting : CurvedFilteredValueSetting {
        public float maxFingerDistance = 0.0f;
        public float minGrabSpeedThreshold = 0.0f;
    }

    [System.Serializable]
    public class FingerReleasingSpeedSetting : CurvedFilteredValueSetting { }

    [System.Serializable]
    public class HandPalmDistanceReleaseSetting : CurvedFilteredValueSetting {
        public float maxPalmDistance;
    }

    [System.Serializable]
    public class HandPalmDirectionSetting : FilteredValueSetting {
        [Range (0, 180)]
        public float maxPalmToSurfaceAngle = 0.0f;
    }

    [System.Serializable]
    public class HandMovementRateSettings : CurvedFilteredValueSetting { }

    [System.Serializable]
    public class HandRotationRateSettings : CurvedFilteredValueSetting { }

    [MinMax(0, 1)]
    public Vector2 releaseGrabThreshold = new Vector2(0.5f, 0.75f);

    [Header("Grabbing")]
    public FingerProximitySetting fingerProximity;
    public FingerGrabbingPoseSetting fingerGrabbingPose;
    public FingerGrabbingSpeedSetting fingerGrabbingSpeed;

    [Header("Releasing")]
    public FingerReleasingPoseSetting fingerReleasingPose;
    public FingerReleasingSpeedSetting fingerReleasingSpeed;
    public HandPalmDirectionSetting handPalmDirection;
    public HandPalmDistanceReleaseSetting handPalmDistanceRelease;

    [Header("Stability")]
    [Tooltip ("Value is in Meters Per Second")]
    public HandMovementRateSettings movementRate;
    [Tooltip ("Value is in Degrees Per Second")]
    public HandRotationRateSettings rotationRate;
}
