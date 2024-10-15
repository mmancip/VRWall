using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UIElements;
using UnityEngine.Events;
/// <summary>
/// This class allow to get the interaction point betwwen the ray hand and the wall, it allow to find the
/// VNC hover and call is differents interaction methodes
/// </summary>
public class InteractorManager : MonoBehaviour 
{
    public static InteractorManager instance;
    public Transform Camera;
    public LayerMask ScreenInteraction;
    public float delta_interaction_distance = 0.01f;

    public MRTKRayInteractor[] MRTKRayInteractors;
    public Ray[] Ray_Hand_Interactors;

    private bool useRayHand = false;
    private bool isClicked = false;
    private bool oldClicked = false;
    //private field
    private GameObject endRay;
    public delegate void OnClick();
    public OnClick clickEvent;
    public delegate void OnUp();
    public OnUp upEvent;
    private bool clicked;
    private Vector3 InteractorPosition;
    private MixedReality.Toolkit.Subsystems.HandsAggregatorSubsystem aggregator;

    public Vector3 GetFingerPosition(){
        
        return InteractorPosition;
    }

    void OnGUI(){
       
    }

    /// <summary>
    /// this function allow to detecte the vnc that interactive with the hand ray
    /// </summary>
    void Interact() {
       

        if(clickEvent == null) return;
            
        aggregator.TryGetPinchProgress(XRNode.RightHand,out bool isReadyToPinchR,out bool isPinchingR,out float pinchAmountR);
        aggregator.TryGetPinchProgress(XRNode.LeftHand,out bool isReadyToPinchL,out bool isPinchingL,out float pinchAmountL);
        bool isPinching = isPinchingR || isPinchingL;

        if(isPinching){
            clickEvent.Invoke();
        }
        else{
            upEvent.Invoke();
        }
        
        RaycastHit contact_point;
        useRayHand = true;
        for(int i = 0; i < Ray_Hand_Interactors.Length; i++){
            if(Ray_Hand_Interactors[i].direction.magnitude < float.Epsilon) continue;
            if(Physics.Raycast(Ray_Hand_Interactors[i], out contact_point, float.MaxValue, ScreenInteraction)){
                Debug.DrawRay(Ray_Hand_Interactors[i].origin,Ray_Hand_Interactors[i].direction,Color.magenta);
                InteractorPosition = MRTKRayInteractors[i].rayEndPoint;
                contact_point.collider.GetComponent<VncInteraction>().OnHover(contact_point.point);
            }
        }
            
    }

    
    /// <summary>
    /// The function `GetScreenPosition` calculates the screen position of a given 3D position relative to a
    /// VNC client.
    /// </summary>
    /// <param name="Vector3">A Vector3 represents a point or position in 3D space using three coordinates
    /// (x, y, z). It is commonly used in 3D graphics and game development to store positions, directions,
    /// or scales in a 3D environment.</param>
    /// <param name="Transform">A Transform in Unity represents the position, rotation, and scale of an
    /// object. It is used to store and manipulate the position and orientation of a game object. In this
    /// context, the Transform parameter `origin` is likely being used to specify the reference point from
    /// which the screen position is calculated.</param>
    /// <param name="VncClient">The `VncClient` parameter in the `GetScreenPosition` method is an object of
    /// type `VncClient`. It is used to retrieve data from the VNC client, specifically the resolution
    /// information needed for calculating the screen position.</param>
    /// <returns>
    /// A tuple containing a Vector2 representing the screen position calculated based on the input
    /// parameters, and a boolean value indicating the success of the operation.
    /// </returns>
    public (Vector2,bool) GetScreenPosition(Vector3 position,Transform origin,VncClient vncClient)
    {
        Vector3 screenPosition = origin.transform.InverseTransformPoint(position);
        Vector2 resolution = vncClient.getData().resolution;
        return (new Vector2( screenPosition.x * resolution.x, 1.0f-screenPosition.y*resolution.y),true);
    } 

    public Ray GetRay(int index){
        return Ray_Hand_Interactors[index];
    }

    void Awake(){
        instance = this;
    }

    public bool IsRayHitScreen(Ray r,Collider coll, out RaycastHit hitInfo ){
        bool hit  =  coll.Raycast(r, out hitInfo, Mathf.Infinity);
        return hit;
    }

    void Start(){
        aggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
        MRTKRayInteractors = Component.FindObjectsByType<MRTKRayInteractor>(FindObjectsSortMode.None);
        Ray_Hand_Interactors = new Ray[MRTKRayInteractors.Length];

    }

    // Update is called once per frame
    void Update() {

        for (int i = 0;i < MRTKRayInteractors.Length;i++){
            if(MRTKRayInteractors[i].isHoverActive){
                Vector3 direction = (MRTKRayInteractors[i].rayEndPoint - MRTKRayInteractors[i].rayOriginTransform.position).normalized;
                Ray_Hand_Interactors[i] = new Ray(MRTKRayInteractors[i].rayOriginTransform.position,direction); 
            }
            else{
                Ray_Hand_Interactors[i] = new Ray(Vector3.zero,Vector3.zero);
            }
        }

        if (XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null) {
            Interact();    
        }
    }
    
}
