using UnityEngine;
using UnityEngine.XR;

public class LLMPanelCloser : MonoBehaviour
{
    public XRNode controllerNode = XRNode.RightHand; // Sağ ya da sol el grip kontrolü
    private InputDevice device;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
    }

    void Update()
    {
        // Eğer grip butonuna basıldıysa ve sahnede panel varsa
        bool gripPressed;
        if (device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed) && gripPressed)
        {
            var panel = FindObjectOfType<LLMResponsePanelController>();
            if (panel != null)
            {
                panel.ClosePanel();
            }
        }
    }
}
