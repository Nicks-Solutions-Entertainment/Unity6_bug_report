

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
            ManageScroll(pData_pointer, uiInputModule.scrollWheel.action);
            ProcessCurrentHit(pData_pointer, ref _currentPointerHit);
            ManageClick(pData_pointer, uiInputModule.leftClick.action);

            ManageDrag(pData_pointer);

            _lastPointerHit = _currentPointerHit;
        }
        else
        {

            pData_pointer.pointerCurrentRaycast.Clear();
            _currentPointerHit.Clear();
            //if (pData_pointer.pointerPress == null)
            //pData_pointer = new PointerEventData(EventSystem.current);
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
            //pData.pointerEnter = lastHit.gameObject;
            OnPointerExit(pData);
            lastHit.Clear();
            pData.pointerEnter = currentHit.gameObject;
        }
    }

    void ProcessCurrentHit(PointerEventData pData, ref RaycastResult currentHit)
    {
        //RaycastResult currentHit = pData.pointerCurrentRaycast;
        s_pointer = pData;
        if (currentHit.isValid)
        {

            //filling PData 
            {
                pData.pointerCurrentRaycast = currentHit;
                pData.pointerEnter = currentHit.gameObject;
                Vector2 _lastPosition = pData.position;
                pData.position = currentHit.screenPosition;
                pData.delta = pData.position - _lastPosition;
                //pData.scrollDelta = 
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
            //ped.pointerPressRaycast = currentHit;

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

    }

    void OnPointerDown(PointerEventData ped)
    {
        // Define o press usando o raycast atual
        RaycastResult currentHit = ped.pointerCurrentRaycast;
        GameObject _clickObj = currentHit.gameObject;

        //if (_clickObj.TryGetComponentInParent(out Selectable _selectable))
        //    _clickObj = _selectable.gameObject;
        //else if (_clickObj.TryGetComponent(out _selectable))
        //    _clickObj = _selectable.gameObject;


        _clickObj = ExecuteEvents.ExecuteHierarchy(
            ped.pointerCurrentRaycast.gameObject,
            ped,
            ExecuteEvents.pointerDownHandler
        );

        if (_clickObj == null)
        {
            _clickObj = ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                ped.pointerCurrentRaycast.gameObject
            );
        }

        ped.pointerPress = _clickObj;
        ped.pointerClick = _clickObj;
        ped.pointerPressRaycast = ped.pointerCurrentRaycast;

        // Se o objeto suporta drag, inicia estado de drag potencial
        var newDrag = ExecuteEvents.GetEventHandler<IDragHandler>(_clickObj);
        if (newDrag != null)
        {
            ped.pointerDrag = newDrag;
            ExecuteEvents.Execute(newDrag, ped, ExecuteEvents.initializePotentialDrag);
        }
        else
        {
            ped.pointerDrag = null;
        }

        ped.eligibleForClick = true;
        ped.useDragThreshold = true;
        ped.dragging = false;

        s_pointer = ped;
    }


    void ManageDrag(PointerEventData ped)
    {
        //return;

        if (ped.pointerDrag == null || !ped.pointerPressRaycast.isValid) return;
        if (!ped.dragging && pData_pointer.delta.magnitude > 0)
        {
            ExecuteEvents.Execute(ped.pointerDrag, ped, ExecuteEvents.beginDragHandler);
            pData_pointer.dragging = true;
        }
        else if (ped.dragging)
        {
            ExecuteEvents.Execute(ped.pointerDrag, ped, ExecuteEvents.dragHandler);
            ped.eligibleForClick = false;
            //ped.pointerPress = null;
        }
    }


    void OnPointerUp(PointerEventData ped)
    {
        // 1. Execute o pointerUpHandler antes de limpar
        if (ped.pointerPress != null)
            ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerUpHandler);

        // 2. Se ele era elegivel para click, execute o click
        if (ped.eligibleForClick && ped.pointerClick != null)
            ExecuteEvents.Execute(ped.pointerClick, ped, ExecuteEvents.pointerClickHandler);

        // 3. Finalizar drag antes da limpeza
        if (ped.dragging && ped.pointerDrag != null)
            ExecuteEvents.Execute(ped.pointerDrag, ped, ExecuteEvents.endDragHandler);

        // 4. Regra REAL do backend para Selectable (Scrollbar depende disso)
        GameObject newPress = ped.pointerCurrentRaycast.gameObject;
        bool insideSame = newPress != null && newPress == ped.pointerPress;

        if (!insideSame)
        {
            ped.pointerPress = null;
        }

        ped.pointerClick = null;
        ped.rawPointerPress = null;
        ped.eligibleForClick = false;
        ped.dragging = false;
        ped.pointerDrag = null;

    }

    void OnPointerExit(PointerEventData ped)
    {
        ExecuteEvents.Execute(ped.pointerEnter, ped, ExecuteEvents.pointerExitHandler);
    }

    bool TryGetRayCast(Vector2 screenPos, ref PointerEventData pData)
    {
        Vector2 _lastPos = pData.position;
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
                //pData.delta = _delta;
                pData.delta = pData.position - _lastPos;
                //pData.position += pData.delta;

                pData.displayIndex = pointer.displayIndex.ReadValue();
            }
        }

        {
            pData.pointerCurrentRaycast = raycastResult;
            if (raycastResult.gameObject != null || pData.pointerEnter != null && pData.dragging)
                pData.pointerEnter = raycastResult.gameObject;
        }

        return pData.pointerCurrentRaycast.gameObject != null;
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
