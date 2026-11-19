using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ScaleDependentMass : MonoBehaviour
{
    [SerializeField] private float baseMass = 1f;
    [SerializeField] private Vector3 referenceScale = Vector3.one;
    private Rigidbody body;

    private void Awake() { body = GetComponent<Rigidbody>(); }

    private void FixedUpdate()
    {
        Vector3 scale = transform.lossyScale;
        float referenceVolume = Mathf.Abs(referenceScale.x * referenceScale.y * referenceScale.z);
        float volume = Mathf.Abs(scale.x * scale.y * scale.z);
        body.mass = Mathf.Clamp(baseMass * volume / Mathf.Max(referenceVolume, 0.000001f), 0.01f, 10000f);
    }
}
