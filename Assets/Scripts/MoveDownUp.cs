using UnityEngine;
using System.Collections;

public class MoveDownUp : MonoBehaviour {
    public const float HEIGHT = 10.0f;
    public const float TIME = 2.0f;

    private Vector3 _highLocation;
    private Vector3 _lowLocation;
    private float _transition = 1.0f;
    private float _destination = 0.0f;

    void Start() {
        _lowLocation = transform.position;
        _highLocation = _lowLocation + Vector3.up * HEIGHT;
    }

    void Update() {
        transform.position = Vector3.Lerp(_lowLocation, _highLocation, Mathf.Pow(_transition, 3.0f));
        _transition = Mathf.MoveTowards(_transition, _destination, Time.deltaTime / TIME);

        if (_transition == 1.0f) {
            Destroy(gameObject);
        }
    }

    public void moveUp() {
        _destination = 1.0f;
    }
}
