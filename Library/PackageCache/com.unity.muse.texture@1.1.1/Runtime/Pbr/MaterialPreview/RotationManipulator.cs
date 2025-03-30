using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class RotationManipulator : Manipulator
    {
        public event Action<Vector2> OnDrag;

        public Vector2 TotalRotation { get; private set; } = new();

        const float k_XSpeed = 20.0f;
        const float k_YSpeed = 20.0f;

        private int m_PointerId = -1;

        public RotationManipulator()
        {
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            OnPointerDown(evt, true);
        }

        public void OnPointerDown(PointerDownEvent evt, bool useModifier, bool capture = true)
        {
            var isShiftPressed = !useModifier || evt.shiftKey;

            if (!isShiftPressed)
                return;

            m_PointerId = evt.pointerId;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
           OnPointerMove(evt, true);
        }

        public void OnPointerMove(PointerMoveEvent evt, bool useModifier, bool capture = true)
        {
            var isShiftPressed =  !useModifier || evt.shiftKey;

            if (!isShiftPressed || evt.pointerId != m_PointerId)
                return;

            if (!target.HasPointerCapture(evt.pointerId) && capture)
            {
                target.CapturePointer(evt.pointerId);
            }

            var rotX = TotalRotation.x + evt.deltaPosition.x * k_XSpeed * 0.02f;
            var rotY =  TotalRotation.y + evt.deltaPosition.y * k_YSpeed * 0.02f;
            TotalRotation = new Vector2(rotX, rotY);

            OnDrag?.Invoke(TotalRotation);
            evt.StopPropagation();
            evt.StopImmediatePropagation();
        }

        public void OnPointerUp(PointerUpEvent evt)
        {
            target.ReleasePointer(evt.pointerId);
            m_PointerId = -1;
        }

        private static void OnMouseMove(MouseMoveEvent evt)
        {
           OnMouseMove(evt, true);
        }

        public static void OnMouseMove(MouseMoveEvent evt, bool useModifier)
        {
            var isShiftPressed = !useModifier || evt.shiftKey;
            if (!isShiftPressed)
                return;
            evt.StopPropagation();
        }

        internal void SetRotation(Vector2 rotation)
        {
            TotalRotation = rotation;
        }
    }
}
