using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(VWsessionCreator))]
public class CommandeModule : MonoBehaviour
{
    private VWsessionCreator VWsession; 

    public void ScanQRCode()
    {
        VWsession.StartDetectedQRCode();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        VWsession = GetComponent<VWsessionCreator>();
    }

    public void Delete(){
        VncManager.instance.SetVncManagerState(1);
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
