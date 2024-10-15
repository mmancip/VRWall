using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class handle interaction for the vnc view, it send click and key
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(VncClient))]
public  class VncInteraction: MonoBehaviour 
{
    public UnityEvent click;
    public Transform origin;
    private int Button_Mask = 0;
    private InteractorManager interactorManager;
    private VncClient VncClient;
    private VncManager vncManager;
    private Vector3 last_position;
    private Vector3 position_cursor;
    private bool update;
    private int last_Mask;

    private void Start(){
        VncClient = GetComponent<VncClient>();
        vncManager = VncManager.instance;
        interactorManager = InteractorManager.instance;
        Debug.Log(interactorManager);
        interactorManager.clickEvent += OnClick;
        interactorManager.upEvent += OnPointerUp;
    }

    private void OnClick(){
        Button_Mask = 1;
    }

    void OnGUI(){
        if(update){
            if (Event.current.isKey && Event.current.keyCode != KeyCode.None)
            {
                bool down = Event.current.type == EventType.KeyDown ? true : false;
                System.UInt32 key = (System.UInt32)Event.current.keyCode;
                if (Event.current.keyCode == KeyCode.Backspace){
                    key = (System.UInt32)0xFF08;
                }
                if((Input.GetKey(KeyCode.LeftShift ) || Input.GetKey(KeyCode.RightShift )) && key >= 97 && key <= 122){
                    key -= 32;
                }

                if (Event.current.keyCode == KeyCode.LeftShift){
                    key = (System.UInt32)0xFFE1;
                }
                VncClient.SendKey(key,down);
                Debug.Log(Event.current.keyCode+" : "+key);
            }
        }
    }

    private void Update(){
        if(update){
            VncClient.SendClick((int)position_cursor.x,(int)position_cursor.y,Button_Mask); 
            
            last_position = position_cursor;
            last_Mask = Button_Mask;
        }else if(last_Mask  == 1){
            VncClient.SendClick((int)last_position.x,(int)last_position.y,0);            
            last_Mask = 0;
        }
    
    }

    public void OnHover(Vector3 position){
        (Vector2,bool) info = interactorManager.GetScreenPosition(position,origin,VncClient);
        //Debug.Log(this.transform.parent.name + " == > "+info.Item1);
        update = info.Item2;
        position_cursor = info.Item1;
    }
    
    public void Delete(){
        Destroy(this.gameObject);
    }

    public void SetTracked(){
        Debug.Assert(vncManager != null,"A vncManager is needed in the scene");
        vncManager.SetTrackedVnc(this.transform);
    }

    public void OnPointerUp(){
        Button_Mask = 0;
    }
    
}
