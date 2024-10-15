using System;
using Unity.VisualScripting;
using UnityEngine;

/* The `VncClient` class that handles VNC client functionality including
connection setup, rendering, input handling, and texture updates. */
[RequireComponent(typeof(Renderer))]
public class VncClient : MonoBehaviour
{
    [System.Serializable]
    public class VncDATA{
        public int X,Y;
        public int Screen_Height,Screen_Width;
        public float Min,Max;
        public float sizeX,sizeY;
        public string password;
        public Vector2Int resolution;
    }
    public VncDATA data;
    public string directAdressConect;
    public bool interaction = true;
    public bool directConnect,stop = false;
    [Range(0,9)]
    public int compress,scale;
    public Texture2D noConnection;
    public ComputeShader shader;

    //privates
    private Renderer renderDisplay;
    private VncManager vncManager;
    private Libvnc libvnc;
    private Coroutine oldClickCorotine;
    private RenderTexture texture;
    private RenderTexture renderTexture;
    //public TextMeshPro fpsDisplay;
    private void Start(){
        Setup();
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.magenta;
       // Debug.Log(data.sizeY);
        Gizmos.DrawLine(this.transform.position+new Vector3(-data.sizeX/2.0f,data.sizeY/2.0f),this.transform.position+new Vector3(data.sizeX/2.0f,data.sizeY/2.0f));
        Gizmos.DrawLine(this.transform.position+new Vector3(-data.sizeX/2.0f,-data.sizeY/2.0f),this.transform.position+new Vector3(-data.sizeX/2.0f,data.sizeY/2.0f));

        Gizmos.DrawLine(this.transform.position+new Vector3(-data.sizeX/2.0f,-data.sizeY/2.0f),this.transform.position+new Vector3(data.sizeX/2.0f,-data.sizeY/2.0f));
        Gizmos.DrawLine(this.transform.position+new Vector3(data.sizeX/2.0f,-data.sizeY/2.0f),this.transform.position+new Vector3(data.sizeX/2.0f,data.sizeY/2.0f));
    }

    /// <summary>
    /// The Setup function initializes necessary components for a VNC connection, including creating a
    /// render texture and setting up event delegates.
    /// </summary>
    public void Setup(){
        vncManager = VncManager.instance;
        data.resolution = new Vector2Int(1,1);
        texture = new RenderTexture(data.resolution.x,data.resolution.y,32);
        texture.enableRandomWrite = true;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Create();
        libvnc = new Libvnc(this,texture,shader);
        renderDisplay = GetComponent<Renderer>();
        
        libvnc.failedDelegate += OnConnectionLose;
        libvnc.changedTexture += updateTexture;
     
        if (directConnect){
            StartConnection(directAdressConect,data.password,compress,0);
        }
        data.resolution = new Vector2Int(texture.width,texture.height);
    }

    public VncDATA getData(){
        return data;
    }

    public int GetFPS(){
        return libvnc.GetFPS();
    }
    
    /// <summary>
    /// The function `updateTexture` retrieves a texture from a library, assigns it to a material's main
    /// texture, and updates the resolution data based on the texture dimensions.
    /// </summary>
    public void updateTexture(){
        var tex = libvnc.GetTexture();
        renderDisplay.material.mainTexture = tex;
        data.resolution = new Vector2Int(tex.width,tex.height);
    }

    public void OnConnectionLose(){
        renderDisplay.material.mainTexture = noConnection;
    }

  /// <summary>
  /// This C# function `StartConnection` initializes a connection with a specified address, password,
  /// compression level, and scaling factor, and updates the display texture with the received data.
  /// </summary>
  /// <param name="address">The `address` parameter in the `StartConnection` method is a string that
  /// represents the IP address or hostname of the server to which you want to connect.</param>
  /// <param name="pass">The `pass` parameter in the `StartConnection` method is a string that
  /// represents the password required to establish the connection.</param>
  /// <param name="compress">The `compress` parameter in the `StartConnection` method likely refers to
  /// the level of compression to be used during the VNC (Virtual Network Computing) connection. This
  /// parameter may determine how much data is compressed before being transmitted over the network,
  /// which can impact the performance and quality of the remote desktop</param>
  /// <param name="scale">The `scale` parameter in the `StartConnection` method is used to specify the
  /// scaling factor for the display. It determines how much the display should be scaled up or down
  /// when rendering the remote desktop content. This can be useful for adjusting the size of the remote
  /// desktop display to fit the local screen</param>
    public void StartConnection(string address,string pass,int compress,int scale){
        Debug.Assert(libvnc != null);
        renderDisplay.material.mainTexture = libvnc.GetTexture();
        libvnc.StartConnection(address,pass,compress,scale);
    }

    private void OnDestroy(){
        libvnc.Close();
    }
    void Update()
    {
        if (!stop){
            libvnc.UpdateScreen();    
        }
    }

    public void SendClick(int x,int y,int button){
        libvnc.SendClick(x,y,button);
    }

    public bool SendKey(UInt32 key,bool down){
        return libvnc.SendKey(key,down);
    }
}
