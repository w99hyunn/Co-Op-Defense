using System;
using UnityEngine.EventSystems;

namespace OpenFrame
{
    public sealed class DefensePointer : UnityEngine.MonoBehaviour, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerMoveHandler, IPointerExitHandler
    {
        public Action<PointerEventData> Clicked;
        public Action<PointerEventData> DragStarted;
        public Action<PointerEventData> Dragged;
        public Action<PointerEventData> DragEnded;
        public Action<PointerEventData> Moved;
        public Action Exited;
        public void OnPointerClick(PointerEventData data) { if (!data.dragging) Clicked?.Invoke(data); }
        public void OnBeginDrag(PointerEventData data) => DragStarted?.Invoke(data);
        public void OnDrag(PointerEventData data) => Dragged?.Invoke(data);
        public void OnEndDrag(PointerEventData data) => DragEnded?.Invoke(data);
        public void OnPointerMove(PointerEventData data) => Moved?.Invoke(data);
        public void OnPointerExit(PointerEventData data) => Exited?.Invoke();
    }
}
