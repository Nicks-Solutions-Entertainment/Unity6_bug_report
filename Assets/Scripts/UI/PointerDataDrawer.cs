using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

//Um propriety drawer de um PointerEventData

[Serializable]
public class SPointerEventData
{
    public GameObject pointerEnter;

    // The object that received OnPointerDown
    public GameObject m_PointerPress;

    /// <summary>
    /// The raw GameObject for the last press event. This means that it is the 'pressed' GameObject even if it can not receive the press event itself.
    /// </summary>
    [field:SerializeField]
    public GameObject lastPress
    {
        get; private set;
    }

    /// <summary>
    /// The object that the press happened on even if it can not handle the press event.
    /// </summary>
    public GameObject rawPointerPress;

    /// <summary>
    /// The object that is receiving 'OnDrag'.
    /// </summary>
    public GameObject pointerDrag;

    /// <summary>
    /// The object that should receive the 'OnPointerClick' event.
    /// </summary>
    public GameObject pointerClick;

    /// <summary>
    /// RaycastResult associated with the current event.
    /// </summary>
    public RaycastResult pointerCurrentRaycast;
    

    /// <summary>
    /// RaycastResult associated with the pointer press.
    /// </summary>
    public RaycastResult pointerPressRaycast;

    public List<GameObject> hovered = new List<GameObject>();

    /// <summary>
    /// Is it possible to click this frame
    /// </summary>
    public bool eligibleForClick;

    /// <summary>
    /// The index of the display that this pointer event comes from.
    /// </summary>
    public int displayIndex;

    /// <summary>
    /// Id of the pointer (touch id).
    /// </summary>
    public int pointerId;

    /// <summary>
    /// Current pointer position.
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// Pointer delta since last update.
    /// </summary>
    public Vector2 delta;

    /// <summary>
    /// Position of the press.
    /// </summary>
    public Vector2 pressPosition;

    /// <summary>
    /// The last time a click event was sent. Used for double click
    /// </summary>
    public float clickTime;

    public int clickCount;

    /// <summary>
    /// The amount of scroll since the last update.
    /// </summary>
    public Vector2 scrollDelta;

    /// <summary>
    /// Should a drag threshold be used?
    /// </summary>
    /// <remarks>
    /// If you do not want a drag threshold set this to false in IInitializePotentialDragHandler.OnInitializePotentialDrag.
    /// </remarks>
    public bool useDragThreshold;

    /// <summary>
    /// Is a drag operation currently occuring.
    /// </summary>
    public bool dragging;

    /// <summary>
    /// The EventSystems.PointerEventData.InputButton for this event.
    /// </summary>
    public PointerEventData.InputButton button;


    /// <summary>
    /// The amount of pressure currently applied by a touch.
    /// </summary>
    /// <remarks>
    /// If the device does not report pressure, the value of this property is 1.0f.
    /// </remarks>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public float pressure;
    /// <summary>
    /// The pressure applied to an additional pressure-sensitive control on the stylus.
    /// </summary>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public float tangentialPressure;
    /// <summary>
    /// The angle of the stylus relative to the surface, in radians
    /// </summary>
    /// <remarks>
    /// A value of 0 indicates that the stylus is parallel to the surface. A value of pi/2 indicates that it is perpendicular to the surface.
    /// </remarks>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public float altitudeAngle;
    /// <summary>
    /// The angle of the stylus relative to the x-axis, in radians.
    /// </summary>
    /// <remarks>
    /// A value of 0 indicates that the stylus is pointed along the x-axis of the device.
    /// </remarks>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public float azimuthAngle;
    /// <summary>
    /// The rotation of the stylus around its axis, in radians.
    /// </summary>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public float twist;
    /// <summary>
    /// Specifies the angle of the pen relative to the X &amp; Y axis, in radians.
    /// </summary>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public Vector2 tilt;
    /// <summary>
    /// Specifies the state of the pen. For example, whether the pen is in contact with the screen or tablet, whether the pen is inverted, and whether buttons are pressed.
    /// </summary>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public PenStatus penStatus;
    /// <summary>
    /// An estimate of the radius of a touch.
    /// </summary>
    /// <remarks>
    /// Add `radiusVariance` to get the maximum touch radius, subtract it to get the minimum touch radius.
    /// </remarks>
    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
    public Vector2 radius;
    /// <summary>
    /// The accuracy of the touch radius.
    /// </summary>
    /// <remarks>
    /// Add this value to the radius to get the maximum touch radius, subtract it to get the minimum touch radius.
    /// </remarks>
    public Vector2 radiusVariance;
    /// <summary>
    /// Specifies in the case of a pointer exit if the pointer has fully exited the area or if it has just entered a child.
    /// </summary>
    public bool fullyExited;
    /// <summary>
    /// Specifies in the case of a pointer enter if the pointer has entered a new area or if it has just reentered a parent after leaving a child.
    /// </summary>
    public bool reentered;

    /// <summary>
    /// The camera associated with the last OnPointerEnter event.
    /// </summary>
    public Camera enterEventCamera
    {
        get
        {
            return pointerCurrentRaycast.module == null ? null : pointerCurrentRaycast.module.eventCamera;
        }
    }

    /// <summary>
    /// The camera associated with the last OnPointerPress event.
    /// </summary>
    public Camera pressEventCamera
    {
        get
        {
            return pointerPressRaycast.module == null ? null : pointerPressRaycast.module.eventCamera;
        }
    }

    /// <summary>
    /// The GameObject that received the OnPointerDown.
    /// </summary>
    public GameObject pointerPress
    {
        get
        {
            return m_PointerPress;
        }
        set
        {
            if (m_PointerPress == value)
                return;

            lastPress = m_PointerPress;
            m_PointerPress = value;
        }
    }


    public static implicit operator SPointerEventData(PointerEventData v)
    {
        return new()
        {
            altitudeAngle = v.altitudeAngle,
            azimuthAngle = v.azimuthAngle,
            button = v.button,
            clickCount = v.clickCount,
            clickTime = v.clickTime,
            delta = v.delta,
            displayIndex = v.displayIndex,
            dragging = v.dragging,
            eligibleForClick = v.eligibleForClick,
            fullyExited = v.fullyExited,
            hovered = v.hovered,
            lastPress = v.lastPress,  
            pointerPress = v.pointerPress,
            pointerEnter = v.pointerEnter,
            pointerClick = v.pointerClick,
            penStatus = v.penStatus,    
            pointerCurrentRaycast = v.pointerCurrentRaycast,    
            pointerPressRaycast = v.pointerPressRaycast,
            pointerDrag = v.pointerDrag,
            pointerId = v.pointerId,
            position = v.position,
            pressPosition = v.pressPosition,
            radius = v.radius,
            pressure = v.pressure,
            scrollDelta = v.scrollDelta,
        };
    }

    /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
}