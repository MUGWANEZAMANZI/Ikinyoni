using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class RotationProxy : Manipulator
    {
        public bool active { get; set; } = true;
        private readonly IEnumerable<RotationManipulator> k_RotationManipulators;
        
        public RotationProxy(RotationManipulator rotationManipulator)
        {
            k_RotationManipulators = new[]{rotationManipulator};
        }
        
        public RotationProxy(IEnumerable<RotationManipulator> rotationManipulators)
        {
            k_RotationManipulators = rotationManipulators;
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

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!active)
                return;

            RotationManipulator.OnMouseMove(evt, false);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!active)
                return;
            
            target.ReleasePointer(evt.pointerId);
            foreach (var manipulator in k_RotationManipulators)
            {
                manipulator.OnPointerUp(evt);
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if(!active)
                return;
            
            foreach (var manipulator in k_RotationManipulators)
            {
                manipulator.OnPointerMove(evt, false, false);
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!active || evt.button != 0)
                return;

            target.CapturePointer(evt.pointerId);
            foreach (var manipulator in k_RotationManipulators)
            {
                manipulator.OnPointerDown(evt, false, false);
            }
        }
    }
}