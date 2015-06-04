using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

public class Kabsch {
    private Matrix<double> _moment;
    private Vector<double> _sum1;
    private Vector<double> _sum2;
    private double _dots;
    private double _weight;
    private double _error;
    private int _pointCount = 0;

    private Quaternion _resultRotation;
    private Vector3 _startCentroid;
    private Vector3 _endCentroid;

    public Quaternion getDeltaRotation() {
        return _resultRotation;
    }

    public Vector3 getDeltaPosition(Vector3 originalPosition) {
        return _resultRotation * (originalPosition - _startCentroid) + _endCentroid - originalPosition;
    }

    public Quaternion getResultingRotation(Quaternion originalRotation) {
        return _resultRotation * originalRotation;
    }

    public Vector3 getResultingPosition(Vector3 originalPosition) {
        return _resultRotation * (originalPosition - _startCentroid) + _endCentroid;
    }

    public float error {
        get {
            return (float)_error;
        }
    }

    public int pointCount {
        get {
            return _pointCount;
        }
    }

    public Kabsch() {
        _moment = DenseMatrix.Build.Dense(3, 3);
        _sum1 = Vector.Build.Dense(3);
        _sum2 = Vector.Build.Dense(3);

        reset();
    }

    public void reset() {
        _moment.Clear();
        _sum1.Clear();
        _sum2.Clear();
        _dots = 0.0;
        _error = 0.0;
        _weight = 0.0;
        _pointCount = 0;
    }

    public void normalize() {
        if (_weight > 0.0) {
            double rep = 1.0 / _weight;
            _moment *= rep;
            _sum1 *= (float)rep;
            _sum2 *= (float)rep;
            _dots *= rep;
            _error *= rep;
            _weight = 1.0;
        }
    }

    public void addPoint(Vector3 point1, Vector3 point2, float weight = 1.0f) {
        Vector<double> point1nWeighted = unityToNumeric(point1 * weight);
        Vector<double> point2n = unityToNumeric(point2);
        _moment += point1nWeighted.ToColumnMatrix() * point2n.ToRowMatrix();
        _sum1 += point1nWeighted;
        _sum2 += point2n * weight;
        _dots += point1.sqrMagnitude * weight;
        _error += (point1 - point2).sqrMagnitude * weight;
        _weight += weight;
        _pointCount++;
    }

    public void solve() {
        if (_weight > 0.0) {
            Matrix<double> covariance = (_moment - (_sum1 / _weight).ToColumnMatrix() * _sum2.ToRowMatrix()) / _weight;
            Svd<double> svd = covariance.Svd();

            Matrix<double> rotation = (svd.VT.Transpose()) * (svd.U.Transpose());
            int sign = rotation.Determinant() > 0 ? 1 : -1;

            Matrix<double> coordinate = DenseMatrix.Build.DenseIdentity(3, 3);
            coordinate[2, 2] = sign;

            rotation = (svd.VT.Transpose()) * coordinate * (svd.U.Transpose());

            _resultRotation = QuaternionFromMatrix(rotation);
            _startCentroid = numericToUnity(_sum1) / (float)_weight;
            _endCentroid = numericToUnity(_sum2) / (float)_weight;
        } else {
            _resultRotation = Quaternion.identity;
            _startCentroid = Vector3.zero;
            _endCentroid = Vector3.zero;    
        }
    }

    public void solveNoRotation() {
        _resultRotation = Quaternion.identity;
        if (_weight > 0.0) {
            _startCentroid = numericToUnity(_sum1) / (float)_weight;
            _endCentroid = numericToUnity(_sum2) / (float)_weight;
        } else {
            _startCentroid = Vector3.zero;
            _endCentroid = Vector3.zero;  
        }
    }

    private Vector<double> unityToNumeric(Vector3 vector) {
        Vector<double> ret = Vector<double>.Build.Dense(3);
        ret[0] = vector.x;
        ret[1] = vector.y;
        ret[2] = vector.z;
        return ret;
    }

    private Vector3 numericToUnity(Vector<double> vector) {
        return new Vector3((float)vector[0], (float)vector[1], (float)vector[2]);
    }

    private Quaternion QuaternionFromMatrix(Matrix<double> m) {
        Vector<double> col2 = m.Column(2);
        Vector<double> col1 = m.Column(1);
        return Quaternion.LookRotation(numericToUnity(col2), numericToUnity(col1));
    }
}
