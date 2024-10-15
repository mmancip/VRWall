using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using Vuforia;
using ZXing;
using Image = Vuforia.Image;
using System.IO;
using UnityEngine.Networking;
using TMPro;


public class VWsessionCreator : MonoBehaviour
{
    public float offset_Y = 1.0f;
    public string DebugJsonAdress;
    public Transform Vnc_parent;
    public GameObject window;
    public TextMeshProUGUI address;
    public Texture2D QrCodeDebug;
    private NotificationAR NAR;
    private GameObject vncclient;
    private string output;
    private WebCamTexture webcamTexture; 
    private Color32[] data;
    

    private void Start()
    {
        NAR = GameObject.FindWithTag("AudioManager").GetComponent<NotificationAR>();
    }



    public void StartDetectedQRCode()
    {
        #if UNITY_EDITOR
            StartCoroutine(GetRequest(DebugJsonAdress));
        #else
            StartCoroutine(GetRequest(address.text));
        #endif
    }

    
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
           
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.Log(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    IntializeSession(JsonUtility.FromJson<VWsession>(webRequest.downloadHandler.text));
                    break;
            }
        }
    }


    public IEnumerator ReadQR()
    {
        yield return new WaitForSeconds(3f);
        VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PixelFormat.RGB888, true);
        Renderer renderer = GetComponent<Renderer>();
        output = "";
        while (String.IsNullOrEmpty(output))
        {
            NAR.Searching();
            try
            {
                CameraDevice c = VuforiaBehaviour.Instance.CameraDevice;
                Image imge = c.GetCameraImage(PixelFormat.RGB888);
                var snap = new Texture2D(imge.Width, imge.Height, TextureFormat.RGB24, false);
                imge.CopyToTexture(snap);
                Debug.Log(QrCodeDebug);
                output = detectQRCode(QrCodeDebug); // Passer les pixels de la texture
                
                if (!string.IsNullOrEmpty(output))
                {
                    NAR.Connection();
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error with detection of the QR CODE : " + e);
            }

            yield return new WaitForSeconds(1f);
        }
        
        if (!string.IsNullOrEmpty(output))
        {
            IntializeSession(JsonUtility.FromJson<VWsession>(output));
        }

    }

    public void IntializeSession(VWsession session){
       
        foreach(Transform tr in Vnc_parent.transform){
            Destroy(tr.gameObject);
        }

        List<Wall> wall = session.GetLstWall();
        Geometry geom = session.geom;
        for (int j = 0; j < wall.Count; j++)
        {
            int nbWindow = session.GetLstWall()[j].GetScreens().Count;
            int height = (int)(session.GetLstWall()[j].height);
            int width = (int)(session.GetLstWall()[j].width);
            for (int i = 0; i < nbWindow; i++)
            {
                Screen screen = wall[j].GetScreens()[i];
                float sizeX = (float)width/(wall[j].scalex);              
                float sizeY = (float)height/(wall[j].scalex);              
                GameObject go = Instantiate(window,this.transform.position +
                    new  Vector3(screen.posX* ((geom.width / (width)) -( wall[j].overlap*sizeX)) - geom.width/2f,geom.height-screen.posY* ((geom.height / (height))-wall[j].overlap*sizeY),0-j/1000.0f),
                    Quaternion.identity);
                go.transform.position = go.transform.position + Vector3.up*offset_Y;
                go.name = "VNC_"+screen.posX + "_"+screen.posY;
                go.transform.parent = Vnc_parent;
                VncClient vnc = go.GetComponentInChildren<VncClient>();
                RenderVNC render = go.GetComponentInChildren<RenderVNC>();
                vnc.interaction = wall[j].interaction;
                render.Maximal_Distance = wall[j].max;
                go.transform.localScale = new  Vector3((geom.width / (width)+sizeX),(geom.height / (height)+sizeY),1);
                vnc.directAdressConect = screen.options.address;
                vnc.data.password = screen.options.password;
                vnc.data.X = (int)screen.posX;
                vnc.data.Y = (int)screen.posY;
                vnc.data.Screen_Height = (int)geom.height;
                vnc.data.Screen_Width = (int)geom.width;
                vnc.data.Min = wall[j].min;
                vnc.data.Max = wall[j].max;
                vnc.data.sizeX = geom.width/width;
                vnc.data.sizeY = geom.height/height;
                vnc.directConnect = true;
                //vnc.Setup();
            }
        }
    }

 
    public static string detectQRCode(Texture2D snap)
    {
        IBarcodeReader barCodeReader = new BarcodeReader();
        
        var result = barCodeReader.Decode(snap.GetRawTextureData(), snap.width, snap.height,
            RGBLuminanceSource.BitmapFormat.RGB24);
        
        string code = "";
            
        if (result != null)
        {
            code = result.Text;
        }

        return code;
    }

    
}

