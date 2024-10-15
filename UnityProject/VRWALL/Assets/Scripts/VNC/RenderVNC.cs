using UnityEngine;

public class RenderVNC : MonoBehaviour {

    [HideInInspector]
    public float Minimal_Distance;
    [HideInInspector]
    public float Maximal_Distance;
    [HideInInspector]
    public Renderer render;

    private Transform camera;
    private VncClient vncClient;
    private Collider collider;

    void Start() {
        camera = Camera.main.transform;
        vncClient = GetComponent<VncClient>();
        render = GetComponent<Renderer>();
        collider = GetComponent<Collider>();
    }

    void Update() {
        vncClient.stop = true;
        render.enabled = false;
        Vector3 normal = ( this.transform.position - Camera.main.transform.position).normalized;
        Vector3 camera_forward = Camera.main.transform.forward;
        float distance = Vector3.Distance(this.transform.position,camera.transform.position);
        if(Vector3.Dot(normal,camera_forward)>0.8f &&  distance>=vncClient.data.Min && distance <= vncClient.data.Max){
            vncClient.stop = false;
            render.enabled = true;    
        }

        collider.enabled =render.enabled;
    }
}
