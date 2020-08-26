using JetBrains.Annotations;
using UnityEngine;

namespace PaperStag
{
public interface ITouchable { }

public interface ITouchableOnBegan : ITouchable
{
    [PublicAPI]
    void OnTouchBegan(Vector2 position);
}

public interface ITouchableOnEnded : ITouchable
{
    [PublicAPI]
    void OnTouchEnded(Vector2 position);
}
}