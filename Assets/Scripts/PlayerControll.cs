using System;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour
{
    [SerializeField] Transform m_head;
    [SerializeField] Vector2 min_max_cameraAngle = new(-60,60);
    [SerializeField] Vector2 _cameraSpeed = new(45,180);
    [Space(8)]
    [SerializeField] float m_moveSpeed = 6.2f;
    [SerializeField] float m_cameraSensibility = .7f;
    [SerializeField] CharacterController m_chCtrl;
    PlayerInputs _inputs;


    [SerializeField, ReadOnly] Vector2 _moveInput;
    [SerializeField, ReadOnly] Vector2 _cameraInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputs = new();
        _inputs.Enable();
        //m_controllsListener.OnMoveEvent += OnMove;
        //m_controllsListener.OnLookEvent += OnLook;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //_cameraInput = context.ReadValue<Vector2>();
        //if (context.canceled || _cameraInput== Vector2.zero)
        //    Debug.Log($"ih");
    }

    public void OnMove(InputAction.CallbackContext contex)
    {
        _moveInput = contex.ReadValue<Vector2>();
        _moveInput = Vector2.ClampMagnitude(_moveInput, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        _cameraInput = _inputs.Player.Look.ReadValue<Vector2>();

        ApplyHeadRotatrion();

        transform.eulerAngles += new Vector3(0, _cameraInput.x * m_cameraSensibility, 0) * (_cameraSpeed.y * Time.deltaTime);
        Vector3 _direction = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        //apply body rotation with camera X input
        m_chCtrl.SimpleMove(_direction * m_moveSpeed);
    }

    void ApplyHeadRotatrion()
    {
        // Normaliza o ângulo para o intervalo de -180 a 180 graus
        Vector3 _headRotation = m_head.localEulerAngles;
        _headRotation.x = (_headRotation.x > 180) ? _headRotation.x - 360 : _headRotation.x;

        float _camSpeed = (_cameraSpeed.x * m_cameraSensibility * Time.deltaTime) * _cameraInput.y;
        // Aplica o Clamp para limitar entre -60 e 60 graus
        _headRotation.x = Mathf.Clamp(_headRotation.x - _camSpeed, min_max_cameraAngle.x, min_max_cameraAngle.y);

        // Atualiza a rotação da cabeça
        m_head.localEulerAngles = _headRotation;
    }

}
