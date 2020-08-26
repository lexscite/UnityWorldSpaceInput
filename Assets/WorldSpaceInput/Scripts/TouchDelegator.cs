using UnityEngine;

namespace PaperStag
{
public class TouchDelegator
    : MonoBehaviour,
        ITouchableOnBegan,
        ITouchableOnEnded
{
    [SerializeField]
    protected Component _targetComponent;

    [SerializeField]
    protected bool _enabledOnly;

    private bool _delegateOnBegan;
    private bool _delegateOnEnded;

    private ITouchableOnBegan _targetOnBegan;
    private ITouchableOnEnded _targetOnEnded;

    private void Awake()
    {
        if (_targetComponent is ITouchableOnBegan targetOnBegan)
        {
            _delegateOnBegan = true;
            _targetOnBegan = targetOnBegan;
        }

        if (_targetComponent is ITouchableOnEnded targetOnEnded)
        {
            _delegateOnEnded = true;
            _targetOnEnded = targetOnEnded;
        }
    }

    public void OnTouchBegan(Vector2 position)
    {
        if (!_delegateOnBegan) return;
        if (!_enabledOnly
            || _targetComponent.gameObject.activeInHierarchy)
        {
            _targetOnBegan.OnTouchBegan(position);
        }
    }

    public void OnTouchEnded(Vector2 position)
    {
        if (!_delegateOnEnded) return;
        if (!_enabledOnly
            || _targetComponent.gameObject.activeInHierarchy)
        {
            _targetOnEnded.OnTouchEnded(position);
        }
    }
}
}