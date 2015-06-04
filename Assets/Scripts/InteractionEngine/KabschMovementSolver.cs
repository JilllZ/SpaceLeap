using UnityEngine;
using System.Collections;

public class KabschMovementSolver {
    private Kabsch _kabsch = new Kabsch();
    private InteractionObject _affectedGameObject;
    private bool _usingGrabbedMethod = false;

    private bool _shouldDoMovementSolve = false;
    private float _grabTransition = 0.0f;

    public KabschMovementSolver(InteractionObject obj) {
        _affectedGameObject = obj;
    }

    public void addPoint(InteractionPoint point) {
        _kabsch.addPoint(point.globalReferencePosition, point.globalPosition, 1.0f);
        _shouldDoMovementSolve = true;
    }

    public void useGrabbedMethod() {
        _usingGrabbedMethod = true;
    }

    public void solve() {
        if (_shouldDoMovementSolve) {
            doMovementSolve();
            _shouldDoMovementSolve = false;
        } else {
            _grabTransition = 0.0f;
        }

        _kabsch.reset();
        _usingGrabbedMethod = false;
    }

    private void doMovementSolve() {
        if (!_usingGrabbedMethod) {
            Vector3 center1 = _affectedGameObject.gameObject.transform.position;
            Vector3 center2 = center1 + _affectedGameObject.rigidbody.velocity * Time.fixedDeltaTime;

            Vector3 axis = _affectedGameObject.rigidbody.angularVelocity;
            float angle = axis.magnitude * Time.fixedDeltaTime * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.AngleAxis(angle, axis.normalized);

            Vector3 right1 = _affectedGameObject.gameObject.transform.right * 0.1f;
            Vector3 up1 = _affectedGameObject.gameObject.transform.up * 0.1f;
            Vector3 forward1 = _affectedGameObject.gameObject.transform.forward * 0.1f;

            Vector3 right2 = rot * _affectedGameObject.gameObject.transform.right * 0.1f;
            Vector3 up2 = rot * _affectedGameObject.gameObject.transform.up * 0.1f;
            Vector3 forward2 = rot * _affectedGameObject.gameObject.transform.forward * 0.1f;

            _kabsch.addPoint(center1 + right1, center2 + right2, 1.0f);
            _kabsch.addPoint(center1 + up1, center2 + up2, 1.0f);
            _kabsch.addPoint(center1 + forward1, center2 + forward2, 1.0f);
            _kabsch.addPoint(center1 - right1, center2 - right2, 1.0f);
            _kabsch.addPoint(center1 - up1, center2 - up2, 1.0f);
            _kabsch.addPoint(center1 - forward1, center2 - forward2, 1.0f);
        }

        if (_kabsch.pointCount < 4) {
            return;
        }

        _kabsch.solve();

        if (_affectedGameObject.rigidbody == null) {
            _affectedGameObject.gameObject.transform.rotation = _kabsch.getResultingRotation(_affectedGameObject.gameObject.transform.rotation);
            _affectedGameObject.gameObject.transform.position = _kabsch.getResultingPosition(_affectedGameObject.gameObject.transform.position);
        } else {
            if (_usingGrabbedMethod) {
                _grabTransition = Mathf.MoveTowards(_grabTransition, 1.0f, Time.fixedDeltaTime * 4.0f);

                Vector3 solvedPosition = _kabsch.getResultingPosition(_affectedGameObject.gameObject.transform.position);
                Quaternion solvedRotation = _kabsch.getResultingRotation(_affectedGameObject.gameObject.transform.rotation);

                Vector3 delta = solvedPosition - _affectedGameObject.gameObject.transform.position;
                _affectedGameObject.rigidbody.MovePosition(_affectedGameObject.gameObject.transform.position + delta * _grabTransition);
                _affectedGameObject.rigidbody.MoveRotation(solvedRotation);

                _affectedGameObject.position = solvedPosition;
                _affectedGameObject.rotation = solvedRotation;
            } else {
                Vector3 axis;
                float angle;
                _kabsch.getDeltaRotation().ToAngleAxis(out angle, out axis);
                angle *= Mathf.Deg2Rad;
                angle /= Time.fixedDeltaTime;

                Vector3 torqueToApply = axis * angle;
                _affectedGameObject.rigidbody.angularVelocity = torqueToApply;

                Vector3 vel = _kabsch.getDeltaPosition(_affectedGameObject.gameObject.transform.position) / Time.deltaTime;
                _affectedGameObject.rigidbody.velocity = vel;
            }
        }
    }
}
