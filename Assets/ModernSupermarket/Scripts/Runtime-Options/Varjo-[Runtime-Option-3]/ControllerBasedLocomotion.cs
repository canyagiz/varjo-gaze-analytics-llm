using UnityEngine;
using UnityEngine.XR;

public class ControllerBasedLocomotion : MonoBehaviour
{
    public Transform rigRoot; // XR Rig parent
    public XRNode inputSource = XRNode.LeftHand; // Hangi kontrolcüden input alınacak

    public float moveSpeed = 1.5f;

    void Update()
    {
        Vector2 inputAxis;
        var device = InputDevices.GetDeviceAtXRNode(inputSource);

        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis))
        {
            // Kameranın yönüne göre world-space yönler al
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            // Y ekseni hariç (dikey hareket olmasın)
            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            // Girişe göre hareket vektörünü hesapla
            Vector3 direction = camForward * inputAxis.y + camRight * inputAxis.x;

            // Rig'i hareket ettir
            rigRoot.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
