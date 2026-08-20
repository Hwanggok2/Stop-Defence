using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private UnityEngine.Camera cam;

    [Header("줌")]
    [SerializeField] private float minOrthoSize = 2f;
    [SerializeField] private float maxOrthoSize = 5.4f;
    [SerializeField] private float zoomSpeed = 0.5f;

    [Header("월드 바운드")]
    [SerializeField] private Collider2D boundaryCollider;

    private Vector2 pendingDelta; // 흔들림 수정: 델타 누적 후 LateUpdate에서 한 번만 적용
    private Vector3 appliedShakeOffset;
    private float shakeTimeRemaining;
    private float shakeDuration;
    private float shakeStrength;
    private bool IsZoomedIn => cam.orthographicSize < maxOrthoSize - 0.01f;
    
    private void Awake()
    {
        Instance = this;

        if (cam == null)
            cam = UnityEngine.Camera.main;

        cam.orthographicSize = Mathf.Min(
            maxOrthoSize,
            GetMaxOrthoSizeFromBounds()
        );

        ClampPosition();
    }

    // 흔들림 수정: LateUpdate에서 한 번만 이동 처리
    private void LateUpdate()
    {
        RemoveShakeOffset();

        if (pendingDelta != Vector2.zero)
        {
            ApplyMove(pendingDelta);
            pendingDelta = Vector2.zero;
        }

        ApplyShakeOffset();
    }

    public void Move(Vector2 mouseDelta)
    {
        pendingDelta += mouseDelta; // 바로 적용하지 않고 누적
    }

    private void ApplyMove(Vector2 mouseDelta)
    {
        float unitsPerPixel = (cam.orthographicSize * 2f) / Screen.height;

        Vector3 move = new Vector3(
            -mouseDelta.x * unitsPerPixel,
            IsZoomedIn ? -mouseDelta.y * unitsPerPixel : 0f,
            0f
        );

        transform.position += move;
        ClampPosition();
    }

    public void Zoom(float scroll)
    {
        if (scroll == 0f) return;

        RemoveShakeOffset();

        float maxAllowed = GetMaxOrthoSizeFromBounds();

        float maxZoomOut = Mathf.Min(maxOrthoSize, maxAllowed);

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed,
            minOrthoSize,
            maxZoomOut
        );

        ClampPosition();
    }

    public void Shake(float duration, float strength)
    {
        if (duration <= 0f || strength <= 0f)
        {
            return;
        }

        shakeDuration = duration;
        shakeTimeRemaining = duration;
        shakeStrength = strength;
    }

    private void ApplyShakeOffset()
    {
        if (shakeTimeRemaining <= 0f)
        {
            return;
        }

        float decay = shakeDuration > 0f
            ? shakeTimeRemaining / shakeDuration
            : 0f;
        Vector2 offset = Random.insideUnitCircle *
                         (shakeStrength * decay);

        appliedShakeOffset = new Vector3(offset.x, offset.y, 0f);
        transform.position += appliedShakeOffset;
        shakeTimeRemaining = Mathf.Max(
            0f,
            shakeTimeRemaining - Time.unscaledDeltaTime);
    }

    private void RemoveShakeOffset()
    {
        if (appliedShakeOffset == Vector3.zero)
        {
            return;
        }

        transform.position -= appliedShakeOffset;
        appliedShakeOffset = Vector3.zero;
    }

    private void OnDisable()
    {
        RemoveShakeOffset();
        shakeTimeRemaining = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 경계 초과 줌아웃 수정: bounds 크기 기반으로 최대 줌아웃 한계 계산
    private float GetMaxOrthoSizeFromBounds()
    {
        if (!boundaryCollider) return maxOrthoSize;

        Bounds bounds = boundaryCollider.bounds;
        float maxByHeight = bounds.size.y / 2f;
        float maxByWidth  = bounds.size.x / 2f / cam.aspect;

        return Mathf.Min(maxByHeight, maxByWidth); // 좁은 쪽 기준
    }

    private void ClampPosition()
    {
        if (!boundaryCollider) return;

        float camHalfH = cam.orthographicSize;
        float camHalfW = camHalfH * cam.aspect;

        Bounds bounds = boundaryCollider.bounds;

        float clampedX = Mathf.Clamp(
            transform.position.x,
            bounds.min.x + camHalfW,
            bounds.max.x - camHalfW
        );

        float clampedY = IsZoomedIn
            ? Mathf.Clamp(transform.position.y, bounds.min.y + camHalfH, bounds.max.y - camHalfH)
            : 0f;

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
