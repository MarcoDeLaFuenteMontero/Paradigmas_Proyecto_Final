using UnityEngine;

// Runs after the plates have sampled their current weights. Never latches open.
[DefaultExecutionOrder(100)]
public class TwoPressurePlateDoor : MonoBehaviour
{
    [SerializeField] private WeightedPressurePlate first;
    [SerializeField] private WeightedPressurePlate second;
    [SerializeField] private Transform door;
    private Vector3 closedPosition;
    public bool BothPressed => first.IsPressed && second.IsPressed;

    private void Start() { closedPosition = door.localPosition; }
    private void FixedUpdate()
    {
        Vector3 target = closedPosition + (BothPressed ? Vector3.up * 3f : Vector3.zero);
        door.localPosition = Vector3.MoveTowards(door.localPosition, target, 3.5f * Time.fixedDeltaTime);
    }
}
