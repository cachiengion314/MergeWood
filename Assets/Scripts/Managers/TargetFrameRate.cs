using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        Utility.Print("UNITY_EDITOR");
#elif UNITY_IOS
        Utility.Print("UNITY_IOS");
        Application.targetFrameRate = 300;
#endif
    }
}
