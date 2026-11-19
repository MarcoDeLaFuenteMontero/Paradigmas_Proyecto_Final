using UnityEngine;

// Cierra el acceso solo cuando el jugador completo ha cruzado el umbral.
public class TutorialExitSeal : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private ObjectInteraction interaction;
    [SerializeField] private BoxCollider entryVolume;
    [SerializeField] private GameObject closingWall;
    [SerializeField] private GameObject[] tutorialObjects;

    public bool IsSealed { get; private set; }

    private void Update()
    {
        if (IsSealed || player == null || entryVolume == null)
            return;

        Vector3 point = entryVolume.transform.InverseTransformPoint(player.position)
            - entryVolume.center;
        Vector3 half = entryVolume.size * 0.5f;
        if (Mathf.Abs(point.x) > half.x || Mathf.Abs(point.y) > half.y
            || Mathf.Abs(point.z) > half.z)
            return;

        // Retira tambien objetos que se hayan arrojado antes al garaje.
        if (tutorialObjects != null)
        {
            foreach (GameObject target in tutorialObjects)
            {
                if (target == null) continue;
                if (interaction != null) interaction.ReleaseIfHolding(target);
                target.SetActive(false);
            }
        }

        if (closingWall != null) closingWall.SetActive(true);
        IsSealed = true;
    }
}
