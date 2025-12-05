

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("Event/Imersive UI Raycaster", 12)]
[RequireComponent(typeof(GraphicRaycaster))]
public class ImersiveUiRaycaster : TrackedDeviceGraphicRaycaster
{
    [Serializable]
    class RaycastResultInfos
    {
        public float index;
        public GameObject target;

        public RaycastResultInfos(RaycastResult pointerCurrentRaycast)
        {
            target = pointerCurrentRaycast.gameObject;
            index = pointerCurrentRaycast.index;
        }
    }

    GraphicRaycaster graphicRaycaster;
    InputSystemUIInputModule uiInputModule;


    bool m_hasRaycaster;
    bool m_hasInputModule;

    private PointerEventData pData_pointer;
    private PointerEventData pData_center;

    [SerializeField] SPointerEventData s_pointer;

    [SerializeField] Canvas m_uiCanvas;

    Canvas uiCanvas
    {
        get
        {
            if (m_uiCanvas != null)
                return m_uiCanvas;

            TryGetComponent(out m_uiCanvas);
            return m_uiCanvas;
        }
    }

    static List<ImersiveUiRaycaster> m_uiRaycasrters = new();

    protected override void Start()
    {
        m_hasRaycaster = TryGetComponent(out graphicRaycaster);
        if (EventSystem.current && EventSystem.current.TryGetComponent(out uiInputModule))
            m_hasInputModule = true;
        if (m_hasRaycaster && !m_uiRaycasrters.Contains(this))
        {
            m_uiRaycasrters.Add(this);
            pData_pointer = new PointerEventData(EventSystem.current);
            pData_center = new PointerEventData(EventSystem.current);
        }
        _currentCenterHit = _currentPointerHit = new();
        _lastCenterHit = _lastPointerHit = new();

        base.Start();
    }

    protected override void OnDestroy()
    {
        if (m_hasRaycaster && m_uiRaycasrters.Contains(this))
            m_uiRaycasrters.Remove(this);
        base.OnDestroy();
    }

    //private void Update()
    //{
    //    UpdateRaycast(false);
    //}


    //[SerializeField] RaycastResultInfos pointerTarget;
    [SerializeField] bool _pointerHasHIt;
    //[SerializeField] RaycastResultInfos centerTarget;
    [SerializeField] bool _centerScreenHasHIt;

    [SerializeField] bool pointerDrag;
    [SerializeField] Vector2 _dragInput;
    [SerializeField] GameObject _currentDrag;
    [SerializeField] List<GameObject> _hovered;


    private RaycastResult _currentPointerHit = new();
    private RaycastResult _lastPointerHit = new();

    private RaycastResult _currentCenterHit = new();
    private RaycastResult _lastCenterHit = new();

    public void SetPointerEvent(BaseEventData bed)
    {
        if (bed is PointerEventData ped)
            s_pointer = ped;
    }
    void UpdateRaycast(bool useCenterScreenPosition = false)
    {
        if (graphicRaycaster == null || !graphicRaycaster.isActiveAndEnabled || !isActiveAndEnabled)
            return;

        m_hasInputModule = uiInputModule != null;
        if (!m_hasInputModule)
        {
            m_hasInputModule = EventSystem.current && EventSystem.current.TryGetComponent(out uiInputModule);
            if (!m_hasInputModule) return;
        }

        Vector2 screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        screenPos = uiInputModule.point.action.ReadValue<Vector2>();

        _pointerHasHIt = TryGetRayCast(screenPos, ref pData_pointer);
        screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        _centerScreenHasHIt = TryGetRayCast(screenPos, ref pData_center);

        //pointerTarget = new(pData_pointer.pointerCurrentRaycast);
        //centerTarget = new(pData_center.pointerCurrentRaycast);

        _currentPointerHit = pData_pointer.pointerCurrentRaycast;
        ProcessLastHitFromCurrent(pData_pointer, ref _currentPointerHit, ref _lastPointerHit);

        if (_pointerHasHIt)
        {
            ProcessCurrentHit(pData_pointer, ref _currentPointerHit);
            ManageClick(pData_pointer, uiInputModule.leftClick.action);



            ManageScroll(pData_pointer, uiInputModule.scrollWheel.action);
            ManageDrag(pData_pointer);


            _lastPointerHit = _currentPointerHit;
        }
        else
        {

            _currentPointerHit.Clear();
            //if (pData_pointer.pointerPress == null)
            pData_pointer = new PointerEventData(EventSystem.current);
            _lastPointerHit = _currentPointerHit;
        }

        //s_pointer = pData_pointer;
        pointerDrag = pData_pointer.dragging;
        _dragInput = pData_pointer.delta;
        _currentDrag = pData_pointer.pointerDrag;
        _hovered = pData_center.hovered;

        if (useCenterScreenPosition)
        {
            _currentCenterHit = pData_center.pointerCurrentRaycast;
            ProcessLastHitFromCurrent(pData_center, ref _currentCenterHit, ref _lastCenterHit);

            if (_currentCenterHit.isValid && pData_pointer.pointerCurrentRaycast.gameObject != pData_center.pointerCurrentRaycast.gameObject)
            {
                pData_center.position = screenPos;

                ProcessCurrentHit(pData_center, ref _currentCenterHit);


                //ManageScroll(pData_center,);

                _lastCenterHit = _currentCenterHit;
            }
            else
            {
                _currentPointerHit.Clear();
                //if (pData_center.pointerPress == null)
                pData_center = new PointerEventData(EventSystem.current);
                _lastCenterHit = _currentCenterHit;

            }
        }
        //if (!_pointerHasHIt && !_centerScreenHasHIt)
        //    pData = new PointerEventData(EventSystem.current);
    }


    void ProcessLastHitFromCurrent(PointerEventData pData, ref RaycastResult currentHit, ref RaycastResult lastHit)
    {
        if (currentHit.gameObject != lastHit.gameObject)
        {
            pData.pointerEnter = lastHit.gameObject;
            OnPointerExit(pData);
            lastHit.Clear();
            pData.pointerEnter = currentHit.gameObject;
        }
    }

    void ProcessCurrentHit(PointerEventData pData, ref RaycastResult currentHit)
    {
        //RaycastResult currentHit = pData.pointerCurrentRaycast;

        if (currentHit.isValid)
        {
            //filling PData 
            {
                pData.pointerCurrentRaycast = currentHit;
                pData.pointerEnter = currentHit.gameObject;
                pData.position = currentHit.screenPosition;
            }
            OnPointerEnter(pData);


        }
    }


    void ManageClick(PointerEventData ped, InputAction inputAction)
    {

        RaycastResult currentHit = ped.pointerCurrentRaycast;
        if (inputAction == null) return;

        if (currentHit.gameObject && inputAction.WasPressedThisFrame())
        {
            //filling PData 
            ped.pointerPressRaycast = currentHit;

            //Debug.Log($"Cloick pressed");
            OnPointerDown(ped);

        }

        if (/*currentHit.gameObject && */inputAction.WasReleasedThisFrame())
            //if (ped.pointerPress == currentHit.gameObject)
            OnPointerUp(ped);
    }

    void ManageScroll(PointerEventData ped, InputAction inputAction)
    {

        RaycastResult currentHit = ped.pointerCurrentRaycast;
        if (inputAction == null) return;

        Vector2 _inputVector = inputAction.ReadValue<Vector2>();
        ped.scrollDelta = _inputVector;

        if (currentHit.gameObject)
        {

            if (currentHit.gameObject.TryGetComponentInParent<ScrollRect>(out ScrollRect _scrollRect))
                _scrollRect?.OnScroll(ped);
        }
    }

    void OnPointerEnter(PointerEventData ped)
    {
        ExecuteEvents.Execute(ped.pointerEnter, ped, ExecuteEvents.pointerEnterHandler);

        //UserScreen.Instance?.XR_PointerEnter(ped.pointerEnter, ped);
    }

    void OnPointerDown(PointerEventData ped)
    {

        ped.pointerPress = ped.pointerPressRaycast.gameObject;
        ped.pointerClick = ped.pointerPressRaycast.gameObject;
        ped.position = ped.pointerPressRaycast.screenPosition;
        ped.button = PointerEventData.InputButton.Left;


        ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerDownHandler);

        ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.initializePotentialDrag);
        if (ped.pointerPress.TryGetComponent(out Selectable _))
        {
            Debug.Log($"Selected:{ped.pointerPress.name}", ped.pointerPress);
            ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.selectHandler);

            //EventSystem.current.SetSelectedGameObject(ped.pointerPress);
        }

        //UserScreen.Instance?.XR_PointerDown(ped.pointerEnter, ped);
    }

    void OnPointerUp(PointerEventData ped)
    {


        ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerUpHandler);
        if (ped.dragging)
            ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.endDragHandler);

        if (ped.pointerCurrentRaycast.gameObject == ped.pointerPress)
            ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerClickHandler);

        //UserScreen.Instance?.XR_PointerUp(ped.pointerEnter, ped);

        ped.pointerPress = null;
        ped.pointerClick = null;
        ped.pointerDrag = null;
        ped.dragging = false;
        ped.position = default;
    }

    void OnPointerExit(PointerEventData ped)
    {
        ExecuteEvents.Execute(ped.pointerEnter, ped, ExecuteEvents.pointerExitHandler);

        //UserScreen.Instance?.XR_PointerExit(ped.pointerEnter, ped);
    }

    bool TryGetRayCast(Vector2 screenPos, ref PointerEventData pData)
    {
        //pData = new PointerEventData(EventSystem.current);
        pData.position = screenPos;

        if (graphicRaycaster == null || !graphicRaycaster.isActiveAndEnabled || !isActiveAndEnabled)
            return false;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pData, results);
        results = results
        .Where(h => h.gameObject.transform.IsChildOf(uiCanvas.transform)).ToList();

        RaycastResult raycastResult = results
            .FirstOrDefault();

        pData.hovered = results.Select(r => r.gameObject).ToList();
        //Selectable _selectable = raycastResult.gameObject?.GetComponentInParent<Selectable>();

        //if (_selectable != null)
        //    raycastResult.gameObject = _selectable.gameObject;

        if (uiInputModule.point.action != null)
        {
            if (GetDisplayPoinerFromInputControl(uiInputModule.point.action.activeControl, out Pointer pointer))
            {
                Vector2 _delta = pointer.delta.ReadValue();

                if (!pData.dragging && pData.pointerPress != null)
                {
                    pData.dragging = _delta.magnitude > 0;
                }
                pData.delta = _delta;
                pData.position += pData.delta;

                pData.displayIndex = pointer.displayIndex.ReadValue();
            }
        }

        //if (pData.pointerPress == null)
        {
            pData.pointerCurrentRaycast = raycastResult;
            if (raycastResult.gameObject != null || pData.pointerEnter != null && pData.dragging)
                pData.pointerEnter = raycastResult.gameObject;
        }
        //else
        //{
        //    raycastResult = pData.pointerCurrentRaycast;

        //    raycastResult.screenPosition = pData.pointerCurrentRaycast.screenPosition;
        //    raycastResult.worldPosition = pData.pointerCurrentRaycast.worldPosition;

        //    pData.pointerCurrentRaycast = raycastResult;
        //}

        return pData.pointerCurrentRaycast.gameObject != null;
    }

    void ManageDrag(PointerEventData ped)
    {
        if (!ped.dragging && ped.pointerPress)
            pData_pointer.dragging = pData_pointer.delta.magnitude > 0;

        if (ped.dragging && ped.pointerDrag == null)
        {
            ped.pointerDrag = ped.pointerPress;
            string parent = ped.pointerDrag.transform.parent?.name;
            Debug.Log($"ManageDrag  on {ped.pointerDrag.name} (parent:{parent})", ped.pointerDrag);
            ExecuteEvents.Execute(ped.pointerDrag, ped, ExecuteEvents.beginDragHandler);
        }
        else if (ped.dragging)
        {
            ExecuteEvents.Execute(ped.pointerDrag, ped, ExecuteEvents.dragHandler);

        }
    }

    bool GetDisplayPoinerFromInputControl(InputControl control, out Pointer pointer)
    {
        pointer = null;
        var displayIndex = 0;
        if (control != null && control.device is Pointer pointerCast && pointerCast != null)
        {
            pointer = pointerCast;
            displayIndex = pointerCast.displayIndex.ReadValue();
            Debug.Assert(displayIndex <= byte.MaxValue, "Display index was larger than expected", this);
        }

        return pointer != null;
    }

    public static void SimulateRaycast(bool useCenterScreenPosition = false)
    {
        foreach (var raycaster in m_uiRaycasrters)
        {
            if (raycaster == null || !raycaster.isActiveAndEnabled)
                continue;

            raycaster.UpdateRaycast(useCenterScreenPosition);
        }
    }

    public static void SimulateUiPressOnCenter(InputAction.CallbackContext context)
    {
        foreach (var raycaster in m_uiRaycasrters)
        {
            if (raycaster == null || !raycaster.isActiveAndEnabled)
                continue;

            raycaster.ManageClick(raycaster.pData_center, context.action);
        }
    }
}
