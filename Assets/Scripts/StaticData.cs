using UnityEngine;
using UnityEngine.UI;

public class StaticData : MonoBehaviour
{
    //stores the selected ball's color material
    public static Material staticBallColorMat;

    //store the selected ball's color name e.g. "Yellow Ball" so can search for it whenever
    public static string staticColorSelectedName;

    //get gameObject of player, then get the animator avatar by accessing the animator component.
    //then set that avatar to the parent player animator to allow animations
    //grab the gameobject.name e.g. "Lucy" to keep the name selected
    public static string characterSelectedName;

    //used to remember scale of fire VFX from held ball to thrown ball
    public static Vector3 fireScale;
    
}
