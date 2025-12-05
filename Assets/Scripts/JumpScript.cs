using UnityEngine;

/// <summary>
/// script que executa um jump em um objeto por rigidbody  por metodo publico
/// </summary>
public class JumpScript : MonoBehaviour
{
    [SerializeField]
    private float jumpForce = 5f;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on " + gameObject.name);
        }
    }
    public void Jump()
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log($"Jump");
        }
    }
}
