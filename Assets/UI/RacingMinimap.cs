using UnityEngine;

public class RacingMinimap : MonoBehaviour
{
    public CircuitMapGraphic route;
    public RectTransform marker;
    CircuitTrack circuit;
    Transform player;
    public Transform Player => player;
    public void Bind(CircuitTrack track, Transform car)
    {
        circuit = track; player = car;
        route.SetLayout(track.layout);
        UpdateMarker();
    }
    void LateUpdate() { UpdateMarker(); }
    public void UpdateMarker()
    {
        bool bound = circuit != null && player != null;
        marker.gameObject.SetActive(bound);
        if (!bound) return;
        Vector2 local = circuit.transform.InverseTransformPoint(player.position);
        marker.anchoredPosition = route.ProjectClamped(local, 7);
        Vector2 heading = circuit.transform.InverseTransformDirection(player.up);
        marker.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg - 90);
    }
}
