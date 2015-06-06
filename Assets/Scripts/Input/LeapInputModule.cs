using UnityEngine;
using UnityEngine.EventSystems;
using Leap;
using System.Collections;
using System.Collections.Generic;

public class LeapInputModule : PointerInputModule {
    public Canvas relativeTo;

    private Dictionary<int, LeapTouch> _leapTouches = new Dictionary<int, LeapTouch>();
    private Camera _guiCamera;

    private struct LeapTouch {
        public int fingerId;
        public Vector3 position;
        public TouchPhase phase;
    }

    protected override void Awake() {
        base.Awake();
        _guiCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    public override bool IsModuleSupported() {
        return HandControllerUtil.handController != null;
    }

    public override bool ShouldActivateModule() {
        return base.ShouldActivateModule() &&
               HandControllerUtil.handController != null && 
               HandControllerUtil.handController.GetFrame().Hands.Count != 0;
    }

    public override void DeactivateModule() {
        base.DeactivateModule();
        ClearSelection();
    }

    public override void Process() {
        updateLeapTouches();

        foreach (LeapTouch leapTouch in _leapTouches.Values) {
            bool released, pressed;
            PointerEventData pointerData = getLeapPointerEventData(leapTouch, out pressed, out released);

            ProcessTouchPress(pointerData, pressed, released);

            if (!released) {
                ProcessMove(pointerData);
                ProcessDrag(pointerData);
            } else {
                RemovePointerData(pointerData);
            }
        }
    }

    private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released) {
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        // PointerDown notification
        if (pressed) {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            if (pointerEvent.pointerEnter != currentOverGo) {
                // send a pointer enter to the touched element if it isn't the one to select...
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                pointerEvent.pointerEnter = currentOverGo;
            }

            // search for the control that will receive the press
            // if we can't find a press handler set the press
            // handler to be what would receive a click.
            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // Debug.Log("Pressed: " + newPressed);

            float time = Time.unscaledTime;

            if (newPressed == pointerEvent.lastPress) {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;

                pointerEvent.clickTime = time;
            } else {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;

            pointerEvent.clickTime = time;

            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }

        // PointerUp notification
        if (released) {
            // Debug.Log("Executing pressup on: " + pointer.pointerPress);
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

            // see if we mouse up on the same element that we clicked on...
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            } else if (pointerEvent.pointerDrag != null) {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.pointerDrag = null;

            // send exit events as we need to simulate this on touch up on touch device
            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
            pointerEvent.pointerEnter = null;
        }
    }

    private void updateLeapTouches() {
        Frame frame = HandControllerUtil.handController.GetFrame();

        Dictionary<int, LeapTouch> newTouches = new Dictionary<int, LeapTouch>();
        foreach (Hand hand in frame.Hands) {
            foreach (Finger finger in hand.Fingers) {
                LeapTouch leapTouch = new LeapTouch();
                leapTouch.fingerId = finger.Id;

                leapTouch.position = HandControllerUtil.toUnitySpace(finger.TipPosition);

                if (!doesTouch(leapTouch)) {
                    continue;
                }

                if (finger.Type != Finger.FingerType.TYPE_INDEX) {
                    continue;
                }

                LeapTouch previousTouch;
                if (!_leapTouches.TryGetValue(finger.Id, out previousTouch)) {
                    leapTouch.phase = TouchPhase.Began;
                } else {
                    leapTouch.phase = TouchPhase.Moved;
                }

                newTouches[finger.Id] = leapTouch;
            }
        }

        foreach (var pair in _leapTouches) {
            //If we have been tracking a non-ended touch and it is not present in the new frame, generate a touch-ended touch
            if (pair.Value.phase != TouchPhase.Ended && !newTouches.ContainsKey(pair.Key)) {
                LeapTouch endingTouch = new LeapTouch();
                endingTouch.fingerId = pair.Key;
                endingTouch.position = pair.Value.position;
                endingTouch.phase = TouchPhase.Ended;
                newTouches[pair.Key] = endingTouch;
            }
        }

        _leapTouches = newTouches;
    }

    private PointerEventData getLeapPointerEventData(LeapTouch input, out bool pressed, out bool released) {
        PointerEventData pointerData;
        bool created = GetPointerData(input.fingerId, out pointerData, true);

        pointerData.Reset();

        pressed = created || (input.phase == TouchPhase.Began);
        released = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);

        Vector2 screenSpace = _guiCamera.WorldToScreenPoint(input.position);

        if (created)
            pointerData.position = screenSpace;

        if (pressed)
            pointerData.delta = Vector2.zero;
        else
            pointerData.delta = screenSpace - pointerData.position;

        pointerData.position = screenSpace;
        pointerData.button = PointerEventData.InputButton.Left;

        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

        var raycast = FindFirstRaycast(m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = raycast;
        m_RaycastResultCache.Clear();
        return pointerData;
    }

    private bool doesTouch(LeapTouch leapTouch) {
        return relativeTo.transform.InverseTransformPoint(leapTouch.position).z > 0;
    }
}
