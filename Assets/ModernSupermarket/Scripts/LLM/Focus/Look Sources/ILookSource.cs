using UnityEngine;

public interface ILookSource
{
    bool TryGetLookRay(out Ray ray);

}
