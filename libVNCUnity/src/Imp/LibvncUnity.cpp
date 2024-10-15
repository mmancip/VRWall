#include "../Headers/LibvncUnity.hpp"
#include <iostream>
std::vector<VncID*> lstVNC;

void Update_VNC(VncID* vnc) {
	lstVNC.push_back(vnc);
}




int FindID(rfbClient* client) {
	int find = 0;
	int i = 0;

	while (!find && i < lstVNC.size()) {
		if (lstVNC[i]->GetClient() == client) {
			find = 1;
		}
		else {
			i++;
		}
	}

	if (find) {
		return i;
	}

	return -1;
}

void LibVncUnity::Clear(VncID* vnc) {
	rfbClient* client = vnc->GetClient();
	int idx = FindID(client);
	lstVNC.erase(lstVNC.begin() + idx);
	rfbClientCleanup(client);
	delete vnc;
}


void ClearNoCleanUp(VncID* vnc) {
	rfbClient* client = vnc->GetClient();
	int idx = FindID(client);
	lstVNC.erase(lstVNC.begin() + idx);
	delete vnc;
}

static void HandleRect(rfbClient* client, int x, int y, int w, int h) {
	VncID* id =  lstVNC[FindID(client)];
	id->SetUpdate(true);
	// UFrame* frame = new UFrame(x,y,w,h);
	// int j,i;
	// int row_stride = id->GetClient()->width;
	// int bpp = 4;
	// int height = (y+h);
	// int width = ((x+w)*bpp);

	// for(j=y;j<height;j++){
	// 	for(i=x;i<width;i++){
	// 		uint8_t p= client->frameBuffer[i*(client->width*4)+i];
	// 		frame->AddPixel(p);
	// 	}
	// }
	// id->AddFrame(frame);
}

char* GetPassword(rfbClient* client) {
	return strdup(lstVNC[FindID(client)]->GetPassword());
}

VncID* LibVncUnity::Initialize(char* adr, char* pswd, int compress, int scale) {
	char comp[100];
	sprintf(comp, "%d", compress);
	char sca[100];
	sprintf(sca, "%d", scale);
	int c = 8;
	char* v[9];
	v[0] = "VrWebWall";
	v[1] = adr;
	v[2] = "-compress";
	v[3] = comp;
	v[4] = "-encodings";
	v[5] = "ultra";
	v[6] = "-scale";
	v[7] = sca;
	v[8] = NULL;

	rfbClient* myClient = new rfbClient();
	myClient = rfbGetClient(8, 3, 4);
	myClient->GotFrameBufferUpdate = HandleRect;
	myClient->GetPassword = GetPassword;
	
	VncID* vnc = new VncID(myClient,pswd,false);
	Update_VNC(vnc);

	time_t t = time(NULL);
	if (!rfbInitClient(myClient, &c, v)) {
		ClearNoCleanUp(vnc);
		return NULL;
	}
	
	return vnc;
}

void LibVncUnity::update(VncID* vnc) {
	int i = WaitForMessage(vnc->GetClient(), 10000 / 60); /* useful for timeout to be no more than 10 msec per second (=10000/framerate usec) */
		if (i > 0) {
		HandleRFBServerMessage(vnc->GetClient());
	}
}

int LibVncUnity::IsUpdated(VncID* vnc) {
	return vnc->GetUpdate();
}

int LibVncUnity::GetWidth(VncID* vnc) {
	return vnc->GetClient()->width;
}

int LibVncUnity::GetHeight(VncID* vnc) {
	return vnc->GetClient()->height;
}

unsigned char* LibVncUnity::GetPixelsColor(VncID* vnc, int x, int y, int width, int height) {
	vnc->SetUpdate(false);
	return vnc->GetClient()->frameBuffer;
}

UFrame* LibVncUnity::GetPixelLastFrame(VncID* vnc){
	if(vnc->GetSize() <=0){
		return nullptr;
	}

	UFrame* frame = vnc->GetFrame();
	for (int i = 0; i < vnc->corbage.size(); i++)
	{
		UFrame* f = vnc->corbage[i];
		delete f;
	}
	vnc->corbage.clear();
	vnc->corbage.push_back(frame);
	return frame;
}


void LibVncUnity::SendButton(VncID* vnc, int x, int y, int button) {
	SendPointerEvent(vnc->GetClient(), x, y, button);
}


bool LibVncUnity::SendKey(VncID* vnc,uint32_t  key,bool down){
	return SendKeyEvent(vnc->GetClient(),key,down);
}

bool LibVncUnity::IsPointerNullptr(void* ptr){
	return ptr == nullptr;
}

// int main(){
// 	VncID* id = LibVncUnity::Initialize("192.168.137.91:5900","tcFFWK_Y",0,0);
// 	return 0;
// }