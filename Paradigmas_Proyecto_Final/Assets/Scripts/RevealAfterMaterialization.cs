using UnityEngine;

public class RevealAfterMaterialization : MonoBehaviour
{
    [SerializeField] private RoomPerspectiveAlignment puzzle;
    [SerializeField] private GameObject clue;

    private void Start() { clue.SetActive(false); }

    private void LateUpdate()
    {
        if (!puzzle.IsMaterialized) return;
        clue.SetActive(true);
        enabled = false;
    }
}
