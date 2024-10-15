#pragma once
#include "rfb/rfbclient.h"
#define MAX_UFRAMES 10000
#include <stack>
#include <vector>
struct UFrame{
    
    int x;
    int y; 
    int width;
    int height;
    int size=0;
    uint8_t* pixels;
    int idx = 0;
    public:
    UFrame(int _x,int _y,int _width,int _height):x(_x),y(_y),width(_width),height(_height){
        size = height*width*4;
        pixels = new uint8_t[size];
    }

    void AddPixel(uint8_t value){
        pixels[idx] = value;
        idx = (idx+1)%(size);
    }

    UFrame::~UFrame(){
        delete pixels;
    }

};

class VncID {
    
	rfbClient* client;
	char* password;
	bool updated;
    std::stack<UFrame*> uFrames;


    public: 
    std::vector<UFrame*> corbage; // remove
    VncID(rfbClient* _client,char* _password,int _updated):client(_client),password(_password),updated(_updated){
    }

    VncID::VncID(const VncID& other){
        *this = other;
    }

    VncID& operator=(const VncID& other){
        if(this == &other)
            return *this;

        rfbClientCleanup(client);
        uFrames = std::stack<UFrame*>(uFrames);
        password = other.password;
        updated = other.updated;
        client = other.client;
        return *this;            
    }

    VncID::~VncID(){
      
    }

    void AddFrame(UFrame* frame){uFrames.push(frame); }
    UFrame* GetFrame(){ 
        UFrame* frame = uFrames.top();
        uFrames.pop();
        return frame;
    }

    rfbClient* GetClient(){ return this->client;}   
    char* GetPassword(){ return this->password;}
    bool GetUpdate(){    return this->updated; }
    int GetSize(){return uFrames.size();}
    void SetUpdate(bool value) { this->updated = value;}

};