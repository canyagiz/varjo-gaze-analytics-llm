using UnityEngine;

public class CrosshairLookSource : MonoBehaviour, ILookSource
{
    public Camera mainCam;

    public bool TryGetLookRay(out Ray ray)
    {
        ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        return true;
    }
}
