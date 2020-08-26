using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace PaperStag
{
[PublicAPI]
public class TouchHelper
{
    private class TouchWrapper
    {
        private static readonly Dictionary<string, FieldInfo> Fields;

        private readonly object _touch;

        [PublicAPI]
        public float DeltaTime
        {
            get => ((Touch)_touch).deltaTime;
            set => Fields["m_TimeDelta"].SetValue(_touch, value);
        }

        [PublicAPI]
        public int TapCount
        {
            get => ((Touch)_touch).tapCount;
            set => Fields["m_TapCount"].SetValue(_touch, value);
        }

        [PublicAPI]
        public TouchPhase Phase
        {
            get => ((Touch)_touch).phase;
            set => Fields["m_Phase"].SetValue(_touch, value);
        }

        [PublicAPI]
        public Vector2 DeltaPosition
        {
            get => ((Touch)_touch).deltaPosition;
            set => Fields["m_PositionDelta"].SetValue(_touch, value);
        }

        [PublicAPI]
        public int FingerId
        {
            get => ((Touch)_touch).fingerId;
            set => Fields["m_FingerId"].SetValue(_touch, value);
        }

        [PublicAPI]
        public Vector2 Position
        {
            get => ((Touch)_touch).position;
            set => Fields["m_Position"].SetValue(_touch, value);
        }

        [PublicAPI]
        public Vector2 RawPosition
        {
            get => ((Touch)_touch).rawPosition;
            set => Fields["m_RawPosition"].SetValue(_touch, value);
        }

        public Touch Create() { return (Touch)_touch; }

        public TouchWrapper() { _touch = new Touch(); }

        static TouchWrapper()
        {
            Fields = new Dictionary<string, FieldInfo>();
            foreach (var field in typeof(Touch).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic))
            {
                Fields.Add(field.Name, field);
            }
        }
    }

    private static TouchWrapper _touchWrapper;

    public static List<Touch> GetTouches()
    {
        var touches = new List<Touch>();
        touches.AddRange(Input.touches);

        #if UNITY_EDITOR
        if (_touchWrapper == null) _touchWrapper = new TouchWrapper();

        if (Input.GetMouseButtonDown(0))
        {
            _touchWrapper.Phase = TouchPhase.Began;
            _touchWrapper.DeltaPosition = new Vector2(0, 0);
            _touchWrapper.Position = new Vector2(Input.mousePosition.x,
                Input.mousePosition.y);
            _touchWrapper.FingerId = 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _touchWrapper.Phase = TouchPhase.Ended;
            Vector2 newPosition = new Vector2(Input.mousePosition.x,
                Input.mousePosition.y);
            _touchWrapper.DeltaPosition = newPosition
                - _touchWrapper.Position;
            _touchWrapper.Position = newPosition;
            _touchWrapper.FingerId = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 newPosition = new Vector2(Input.mousePosition.x,
                Input.mousePosition.y);
            _touchWrapper.DeltaPosition = newPosition
                - _touchWrapper.Position;
            _touchWrapper.Phase = _touchWrapper.DeltaPosition
                    .magnitude
                < .3f
                    ? TouchPhase.Stationary
                    : TouchPhase.Moved;
            _touchWrapper.Position = newPosition;
            _touchWrapper.FingerId = 0;
        }
        else
        {
            _touchWrapper = null;
        }

        if (_touchWrapper != null) touches.Add(_touchWrapper.Create());
        #endif
        return touches;
    }
}
}