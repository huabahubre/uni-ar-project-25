using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MarkerDragHandler : MonoBehaviour
{
    private Camera mainCam;
    private bool isDragging = false;
    private Vector3 offset;
    private float markerY;

    private int draggingFingerId = -1;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    // ─────────────────────────────────────────────────────────────
    // MOUSE INPUT (Editor or Standalone)
    // ─────────────────────────────────────────────────────────────
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDragging(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            DragTo(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // TOUCH INPUT (Mobile)
    // ─────────────────────────────────────────────────────────────
    void HandleTouchInput()
    {
        if (Input.touchCount == 0)
            return;

        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (TryStartDragging(touch.position))
                        draggingFingerId = touch.fingerId;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDragging && touch.fingerId == draggingFingerId)
                        DragTo(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging && touch.fingerId == draggingFingerId)
                        StopDragging();
                    break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // DRAGGING LOGIC
    // ─────────────────────────────────────────────────────────────
    bool TryStartDragging(Vector2 screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                markerY = transform.position.y;

                Plane plane = new Plane(Vector3.up, new Vector3(0, markerY, 0));
                if (plane.Raycast(ray, out float enter))
                {
                    offset = transform.position - ray.GetPoint(enter);
                }

                return true;
            }
        }

        return false;
    }

    void DragTo(Vector2 screenPos)
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0, markerY, 0));
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 targetPos = ray.GetPoint(enter) + offset;
            transform.position = new Vector3(targetPos.x, markerY, targetPos.z); // lock Y
        }
    }

    void StopDragging()
    {
        isDragging = false;
        draggingFingerId = -1;
    }
}
