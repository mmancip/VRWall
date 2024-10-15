using System;
using System.Collections;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/* The `Libvnc` class in C# is a wrapper for interacting with a native library for VNC functionality in
Unity, providing methods for initializing, updating, sending input, and handling pixel data. */

public class Libvnc 
{

    [DllImport("libvncUnity")]
    private static extern IntPtr Initialize(string adr,string pswd,int compress,int scale);
    
    [DllImport("libvncUnity")]
    private static extern System.IntPtr GetPixelsColor(IntPtr client,int x,int y,int width, int height);
    
    [DllImport("libvncUnity")]
    private static extern void update(IntPtr client);
    
    [DllImport("libvncUnity")]
    private static extern void SendButton(IntPtr client,int x,int y,int button);

    [DllImport("libvncUnity")]
    private static extern bool SendKey(IntPtr client, UInt32 key,bool down);

    [DllImport("libvncUnity")]
    private static extern void Clear(IntPtr client);
    
    [DllImport("libvncUnity")]
    private static extern int IsUpdated(IntPtr client);
    
    [DllImport("libvncUnity")]
    private static extern int GetFps(IntPtr client);
    
    [DllImport("libvncUnity")]
    private static extern int GetHeight(IntPtr client);
    
    [DllImport("libvncUnity")]
    private static extern int GetWidth(IntPtr client);

    [DllImport("libvncUnity")]
    private static extern IntPtr GetPixelLastFrame(IntPtr client);
    
    [DllImport("libvncUnity")]
    private static extern bool IsPointerNullptr(IntPtr ptr);

    
    [DllImport("libvncUnity")]
    private static extern void CreatePool();

    [DllImport("libvncUnity")]
    private static extern void DeletePool();


    struct UFrame{
        public int x,y,width,height,size;
        public IntPtr pixels;
    }
    
    static bool b_CreatePool = false;
    static bool b_DeletePool = false;

    //private properties
    private RenderTexture texture;
    private bool isDrawing,isStarted;
    private int frameRate,frameCounter;
    private NativeArray<byte> addressArray,passwordArray,result;
    private readonly MonoBehaviour mono;
    private NativeArray<IntPtr> connectionResult;
    private IntPtr client;
    private ComputeShader shader;

    private int [] old_frame_buffer;
    //
    public delegate void EventVnc();
    public EventVnc failedDelegate,changedTexture;

    public RenderTexture GetTexture(){return texture;}
    
    
    /* The `Libvnc` class constructor is initializing a new instance of the class. */
    public Libvnc(MonoBehaviour mono,RenderTexture _texture,ComputeShader shader){
        this.shader = shader;
        texture = _texture;
        this.mono = mono;

        int length = texture.width*texture.height;
        old_frame_buffer = new int[length];

        if(!b_CreatePool){
            CreatePool();
        }
    }
    
  /// <summary>
  /// The StartConnection function initiates a VNC connection using the provided address, password,
  /// compression settings, and scaling factor.
  /// </summary>
  /// <param name="vncAddress">The `vncAddress` parameter is a string that represents the address of the
  /// VNC server to which you want to establish a connection. It typically includes the IP address or
  /// hostname of the server and the port.</param>
  /// <param name="vncPassword">The `vncPassword` parameter in the `StartConnection` method is a string
  /// that represents the password required to establish a VNC (Virtual Network Computing) connection.
  /// This password is used to authenticate and authorize the connection to the remote VNC
  /// server.</param>
  /// <param name="comp">The `comp` parameter in the `StartConnection` method is used to
  /// specify whether compression should be applied during the VNC connection
  /// </param>
  /// <param name="scale">The `scale` parameter in the `StartConnection` method is used to specify the
  /// scaling factor for the VNC connection. It determines how much the remote desktop should be scaled
  /// when displayed on the client side. For example, a scale of 2 would display the remote desktop at
  /// twice its original size</param>
    public void StartConnection(string vncAddress,string vncPassword,int comp,int scale)
    {
        Debug.Assert(mono != null);
        byte[] bytes = Encoding.ASCII.GetBytes(vncAddress);
        addressArray = new NativeArray<byte>(bytes, Allocator.TempJob);
        bytes = Encoding.ASCII.GetBytes(vncPassword);
        passwordArray = new NativeArray<byte>(bytes, Allocator.TempJob);
        connectionResult = new NativeArray<IntPtr>(1, Allocator.TempJob);
        VncConnection job = new VncConnection { Address = addressArray,Password = passwordArray,compress = comp, scale = scale, ConnectionResult = connectionResult};

        var connectionHandle = job.Schedule();
        mono.StartCoroutine(WaitVncConnection(connectionHandle));
    }

    
 /// <summary>
 /// It updates the client and VNC.
 /// </summary>
    public void UpdateScreen(){
        Debug.Assert(mono != null);
        
        if (!isStarted) return;
        update(client);
        UpdateVNC();
    }

    private int[] DiffBuffer(int[] a,int []b,int width,int height){
        int [] c = new int[width*height];
        for (int i =0; i < width*height;i++){
            c[i] = 0;
            if(a[i] != b[i])
                c[i] = b[i];
        }

        return c;
    }

    /// <summary>
    /// This code get the pixel information, and send it to the shader to update the wall
    /// </summary>
    private void UpdateVNC(){
        if (IsUpdated(client) == 0)
            return;
        if (texture.width != GetWidth(client) || texture.height != GetHeight(client)){
            texture = new RenderTexture(GetWidth(client), GetHeight(client),32);
            texture.enableRandomWrite = true;
            texture.wrapMode = TextureWrapMode.Repeat;
            old_frame_buffer = new int[texture.width*texture.height];
            changedTexture.Invoke();
        }

        
        IntPtr UFrame_ptr = GetPixelLastFrame(client);
        if(IsPointerNullptr(UFrame_ptr)){
            return;
        }

        int gdf = 0;
        while(!IsPointerNullptr(UFrame_ptr) && gdf <= 10000){
            UFrame frame =  (UFrame)Marshal.PtrToStructure(UFrame_ptr,typeof(UFrame));
            Int32[] intArray = new Int32[frame.size/4];
            Marshal.Copy(frame.pixels, intArray, 0, frame.size/4);
            int kernelID = shader.FindKernel("ProcessVNC");
            ComputeBuffer data = new ComputeBuffer(intArray.Length, sizeof(int));
            data.SetData(intArray);
            //Debug.Log(intArray[0]);
            shader.SetTexture(kernelID,"Result",texture);
            shader.SetBuffer(kernelID,"data",data);
            shader.SetInt("frameBufferWidth",frame.width);
            shader.SetInt("frameBufferHeight",frame.height);
            shader.SetInt("UFrame_x",frame.x);
            shader.SetInt("UFrame_y",frame.y);
            shader.SetInt("UFrame_width",frame.width);
            shader.SetInt("UFrame_height",frame.height);
            shader.SetFloat("random_float",UnityEngine.Random.Range(0.0f,255.0f));
            shader.Dispatch(kernelID, frame.width,frame.height,1);
            data.Release();
            UFrame_ptr = GetPixelLastFrame(client);
        }
            
        frameRate++;
        isDrawing = true;
    }
    
    
    static int[] ByteArrayToIntArray(byte[] byteArray)
    {
        int[] intArray = new int[byteArray.Length / 4];

        for (int i = 0; i < intArray.Length; i++)
        {
            // Convert 4 bytes to an integer (assuming little-endian order)
            intArray[i] = BitConverter.ToInt32(byteArray, i * 4);
        }

        return intArray;
    }

    
    public int GetFPS(){return frameCounter;}


    IEnumerator WaitVncConnection(JobHandle jobHandle)
    {
        while (jobHandle.IsCompleted == false){
            yield return null;
        }
        jobHandle.Complete();
        addressArray.Dispose();
        passwordArray.Dispose();
        client = connectionResult[0];
        connectionResult.Dispose();
        
        if (client == IntPtr.Zero){
            Debug.Log("The connection could not be established at ");
            failedDelegate.Invoke();
        }
        else{ 
            Debug.Log("Start vnc");
            isStarted = true;
        }
    }
    
    IEnumerator WaitForProcessVnc(/*JobHandle jobHandle*/) {
        yield return new WaitForEndOfFrame();
    }
    
    /// <summary>
    /// free vncclient
    /// </summary>
    public void Close(){
        if(isStarted){
            Clear(client);
        }
        if(b_DeletePool){
            DeletePool();
        }
    }

    /// <summary>
    /// The SendClick function sends a button click event to a client at specified coordinates if
    /// drawing is enabled.
    /// </summary>
    /// <param name="x">The x parameter represents the horizontal position where the click event should
    /// occur.</param>
    /// <param name="y">The `y` parameter in the `SendClick` method represents the vertical coordinate
    /// of the click location on the screen.</param>
    /// <param name="button">The `button` parameter in the `SendClick` method represents the type
    /// of mouse button that was clicked. It could be an integer value representing different buttons
    /// such as left click (1), right click (2), middle click (3), etc.</param>
    /// <returns>
    /// The method `SendClick` is returning a boolean value.
    /// </returns>
    public void SendClick(int x, int y, int button)
    {
        if (!isDrawing) return;

        SendButton(client, x, y, button);
    }

    /// <summary>
    /// The function `SendKey` in C# sends a key press or release event to a client.
    /// </summary>
    /// <param name="UInt32">The `UInt32` data type is an unsigned 32-bit integer in C#. It represente the key that are pressed.</param>
    /// <param name="down">The `down` parameter is a boolean value that indicates whether the key should be
    /// pressed down (`true`) or released (`false`).</param>
    /// <returns>
    /// The method `SendKey` is returning a boolean value.
    /// </returns>
    public bool SendKey(UInt32 key,bool down){
        return SendKey(client,key,down);
    }

    private struct VncConnection : IJob{
        public NativeArray<byte> Address;
        public NativeArray<byte> Password;
        public int compress;
        public int scale;
        public NativeArray<IntPtr> ConnectionResult;
        public void Execute()
        {
            string a = Encoding.ASCII.GetString(Address.ToArray());
            string p = Encoding.ASCII.GetString(Password.ToArray());
            ConnectionResult[0] = Initialize(a,p,compress,scale);
        }
    }
    
    private struct ProcessVnc : IJob
    {
        public int Width;
        public int Height;
        public NativeArray<byte> ResultArray;
        [NativeDisableUnsafePtrRestriction]
        public IntPtr Client;
        public void Execute()
        {
            IntPtr intPtr = GetPixelsColor(Client,0,0,100,100);
            int length = Width*Height*4;
            byte[] intArray = new byte[length];
            Marshal.Copy(intPtr, intArray, 0, length);
            MoveFromByteArray(ref intArray, ref ResultArray);
        }
    }

    // code to convert byte[] to NativeArray<byte> source : https://forum.unity.com/threads/copy-any-nativearray-t-into-a-byte-array-and-back.549727/
    public static unsafe void MoveFromByteArray<T>(ref byte[] src, ref NativeArray<T> dst) where T : struct
    {
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst));
                if (src == null)
                    throw new ArgumentNullException(nameof(src));
        #endif
        var size = UnsafeUtility.SizeOf<T>();
        if (src.Length != (size * dst.Length)) {
            dst.Dispose();
            dst = new NativeArray<T>(src.Length / size, Allocator.Persistent);
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst));
        #endif
        }
 
        var dstAddr = (byte*)dst.GetUnsafeReadOnlyPtr();
        fixed (byte* srcAddr = src) {
            UnsafeUtility.MemCpy(&dstAddr[0], &srcAddr[0], src.Length);
        }
    } 

}
    