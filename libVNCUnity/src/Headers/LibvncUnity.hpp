
#pragma once
#include "rfb/rfbclient.h"
#include "VncID.hpp"
#include <vector>
#include <time.h>


#ifdef  VNC_UNITY_EXPORTS
#define  VNC_UNITY_API __declspec(dllexport)
#else
#define  VNC_UNITY_API __declspec(dllimport)
#endif


namespace LibVncUnity{

    extern "C" {
        VNC_UNITY_API VncID* Initialize(char* adr, char* pswd, int compress, int scale);
        VNC_UNITY_API unsigned char* GetPixelsColor(VncID* vnc, int x, int y, int width, int height);
        VNC_UNITY_API UFrame* GetPixelLastFrame(VncID* vnc);
        VNC_UNITY_API void update(VncID* vnc);
        VNC_UNITY_API void SendButton(VncID* vnc, int x, int y, int button);
        VNC_UNITY_API bool SendKey(VncID* vnc,uint32_t  key,bool down);
        VNC_UNITY_API void Clear(VncID* vnc);
        VNC_UNITY_API int IsUpdated(VncID* vnc);
        VNC_UNITY_API int GetHeight(VncID* vnc);
        VNC_UNITY_API int GetWidth(VncID* vnc);
        VNC_UNITY_API bool IsPointerNullptr(void* ptr);
    } 
}   