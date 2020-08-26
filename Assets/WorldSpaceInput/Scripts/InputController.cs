using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PaperStag
{
public class InputController : MonoBehaviour
{
    [SerializeField]
    protected Camera _camera;

    [SerializeField]
    protected LayerMask _layerMask;

    [PublicAPI]
    public static Action<Touch> OnTouchBegan = touch => { };

    [PublicAPI]
    public static Action<Touch> OnTouchMoved = touch => { };

    [PublicAPI]
    public static Action<Touch> OnTouchEnded = touch => { };

    [PublicAPI]
    public static Action<Touch> OnTouchCanceled = touch => { };

    [PublicAPI]
    public static Action OnClickedNowhere = () => { };

    [PublicAPI]
    public static Action<ITouchable>
        OnTouchableClicked = touchable => { };

    [SerializeField]
    protected float _movementThreshold = .2f;

    /// <summary>
    /// Pixel size in world space
    /// </summary>
    [PublicAPI]
    public static float Unit { get; private set; }

    /// <summary>
    /// Threshold vector of movement start
    /// </summary>
    private static Vector2 ThresholdVector { get; set; }

    private static readonly Vector3 DepthVector = new Vector3(0, 0, 1);
    private bool _canMove;
    private bool _moving;

    private void Awake()
    {
        Unit = Vector2.Distance(_camera.ScreenToWorldPoint(Vector2.zero),
            _camera.ScreenToWorldPoint(Vector2.right));
    }

    private void Update()
    {
        var touches = TouchHelper.GetTouches();

        if (touches.Count <= 0) return;
        var touch = touches[0];
        if (GetIsPointerOverUIObject(touch.position)) return;

        switch (touch.phase)
        {
        case TouchPhase.Began:
            HandleTouchBegan(touch);
            break;
        case TouchPhase.Moved:
            HandleTouchMoved(touch);
            break;
        case TouchPhase.Ended:
            HandleTouchEnded(touch);
            break;
        case TouchPhase.Canceled:
            Reset();
            OnTouchCanceled.Invoke(touch);
            break;
        }
    }

    private void Reset()
    {
        ThresholdVector = Vector2.zero;
        _moving = false;
        _canMove = false;
    }

    private void HandleTouchMoved(Touch touch)
    {
        if (_canMove && !_moving)
        {
            if (ThresholdVector.magnitude
                > _movementThreshold
                / Unit)
            {
                _moving = true;
            }
            else
            {
                ThresholdVector += touch.deltaPosition;
            }
        }

        if (_moving)
        {
            OnTouchMoved.Invoke(touch);
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        Reset();

        if (!GetIsPointerOverUIObject(touch.position))
        {
            var hit = Hit(touch.position);
            if (hit != null)
            {
                var touchables = hit.GetComponents<ITouchableOnBegan>();
                if (touchables.Length > 0)
                {
                    foreach (var t in touchables)
                    {
                        t.OnTouchBegan(touch.position);
                    }
                }
            }

            _canMove = true;
        }

        OnTouchBegan.Invoke(touch);
    }

    private void HandleTouchEnded(Touch touch)
    {
        if (_moving)
        {
            Reset();
        }
        else
        {
            if (!GetIsPointerOverUIObject(touch.position))
            {
                var wasHit = false;
                var hit = Hit(touch.position);
                if (hit != null)
                {
                    var touchables = hit.GetComponents<ITouchable>();
                    if (touchables.Length > 0)
                    {
                        foreach (var t in touchables)
                        {
                            OnTouchableClicked.Invoke(t);
                            if (t is ITouchableOnEnded te)
                            {
                                te.OnTouchEnded(touch.position);
                            }
                        }

                        wasHit = true;
                    }
                }

                if (!wasHit)
                {
                    OnClickedNowhere.Invoke();
                }
            }
        }

        OnTouchEnded.Invoke(touch);
    }

    private GameObject Hit(Vector2 position)
    {
        var worldPosition = _camera.ScreenToWorldPoint(position);
        var ray = new Ray(worldPosition, DepthVector);

        var hit = Physics2D.GetRayIntersection(ray,
            1000f,
            _layerMask);

        return hit.collider != null ? hit.transform.gameObject : null;
    }

    private bool GetIsPointerOverUIObject(Vector2 position)
    {
        PointerEventData eventData =
            new PointerEventData(EventSystem.current)
            {
                position = position
            };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
}