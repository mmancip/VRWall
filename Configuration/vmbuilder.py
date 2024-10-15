#! /usr/bin/env python
import sys,os,stat
import argparse
import json
import datetime
import re
import configparser
import secrets

password_length = 8

x_resolution = 1920
y_resolution = 1080
RESOLUTION=str(x_resolution)+'x'+str(y_resolution)
factor = 2
FACTOR=str(factor)+'x'+str(factor)
scale = 1
SCALE=str(scale)+'x'+str(scale)
startport = 5901

TILEDVIZJSON=""

Localhost=False

MIN=0
MAX=20
OVERLAP=0

# Usage
# ./vmbuilder.py -n LowRes -a 0 -b 14 -r 1920x1080 -s 0.5x0.5 -f 1x1 -p 5900 -i 192.168.137.90
# ./vmbuilder.py -n VwHD -a 0 -b 2 -r 1920x1080 -s 1x1 -f 2x2 -o 5 -p 5901 -i 192.168.137.90
# Next
# ./concat_json LowRes.json VWHD.json


# ./vmbuilder.py -n LowRes -a 0 -b 14 -r 3840x2160 -s 0.25x0.25 -l -f 1x1 -t vrwall.json -p 5900 -i 192.168.0.13
# ./vmbuilder.py -n MedRes -a 1 -b 3 -r 3840x2160 -s 0.5x0.5 -l  -o 5 -f 2x2 -t vrwall.json -p 5901 -i 192.168.0.13
# ./vmbuilder.py -n HigRes -a 0 -b 1 -r 3840x2160 -s 1x1 -l -o 10 -f 4x4 -t vrwall.json -p 5905 -i 192.168.0.13

# ./concat_json LowRes.json MedRes.json HigRes.json

def parse_args(argv):
    parser = argparse.ArgumentParser(
        'Build a script and the JSON for a VNC servers set for VRWall.')
    parser.add_argument('-d','--debug', action='store_true',
                        help='Debug VMbuilder messages.')
    parser.add_argument('-n', '--wall-name',
                        help='Must give the Wall name : gives the shell script name and to launch the x11vnc servers and the JSON file name.', required=True)
    parser.add_argument('-r', '--resolution', 
                        help='The resolution of the global wall _Width_x_Height_.', default=RESOLUTION)
    parser.add_argument('-f', '--factor', 
                        help='The crop factors _nW_x_nH_.', default=FACTOR)
    parser.add_argument('-o', '--overlap', 
                        help='The overlap in pixels for crop windows.', default=OVERLAP)
    parser.add_argument('-s', '--scale', 
                        help='The scale factors _sW_x_sH_ on the crop.', default=SCALE)
    parser.add_argument('-t', '--tiledvizjson', 
                        help='The name of the TiledViz JSON file for append this wall. No export if empty (default).', default=TILEDVIZJSON)
    parser.add_argument('-p', '--startport', 
                        help='The START port number for x11vnc server.', default=startport)
    parser.add_argument('-i','--ip',help='IP of the x11vnc server.',required=True)
    parser.add_argument('-a', '--min', 
                        help='Min distance with the virtual wall.', default=MIN)
    parser.add_argument('-b', '--max', 
                        help='Max distance with the virtual wall.', default=MAX)
    parser.add_argument('-l','--localhost', action='store_true',
                        help='Use localhost only servers for security (need ssh tunneling).')
    args = parser.parse_args(argv[1:])
    return args


if __name__ == '__main__':
    
    args = parse_args(sys.argv)

    scriptname=args.wall_name+".sh"
    scriptf = open(scriptname,"w")
    script=[]
    jsonname=args.wall_name+".json"


    # TiledViz
    tiledvizjson=args.tiledvizjson
    otiledviz=False
    json_tiledviz={}
    json_tiledviz["nodes"]=[]
    if ( tiledvizjson != "" ):
        otiledviz=True
        #TODO : option with TiledViz
        #SSL_PUBLIC (/etc/letsencrypt/.../fullchain.pem)
        #SSL_PRIVATE (/etc/letsencrypt/.../privkey.pem)
        #SSL_HOST (desktop.mdls.fr)
        #TiledVizPATH ($HOME/TiledViz)
        SSL_PUBLIC=os.getenv("SSL_PUBLIC")
        SSL_PRIVATE=os.getenv("SSL_PRIVATE")
        SSL_HOST=os.getenv("SSL_HOST")
        TiledVizPATH=os.getenv("TiledVizPATH")
        if (SSL_PUBLIC==""):
            SSL_PUBLIC="ssl_public.pem"
            print("WARNING : Give SSL public key with '-t tiledviz.json' option !!")
        if(SSL_PRIVATE==""):
            SSL_PRIVATE="ssl_private.pem"
            print("WARNING : Give SSL private key with '-t tiledviz.json' option !!")
        if(SSL_HOST==""):
            SSL_HOST="tiledViz.mdls.fr"
            print("WARNING : Give DNS Host with '-t tiledviz.json' option !!")
        if(TiledVizPATH==""):
            TiledVizPATH="/home/user/TiledViz"
            print("WARNING : Download TiledViz source with '-t tiledviz.json' option !!")

        print("ENV vars from env :",SSL_PUBLIC,SSL_PRIVATE,SSL_HOST,TiledVizPATH)

        try:
            with open(tiledvizjson,'r') as TiledViz_file:
                json_tiledviz=json.load(TiledViz_file)
                TiledViz_file.close()
        except:
            pass

    overlap=int(args.overlap)
    boverlap=False
    if (overlap > 0):
        boverlap=True
    
    search_resol = re.compile(r'\s*(?P<width>\d+)\s*x\s*(?P<height>\d+)\s*')

    RESOL=search_resol.search(args.resolution)
    if (args.debug):
        print(args.resolution)
        print(RESOL.group('width'),RESOL.group('height'))
    x_resolution=int(RESOL.group('width'))
    y_resolution=int(RESOL.group('height'))

    FACT=search_resol.search(args.factor)
    if (args.debug):
        print(FACT.group('width'),FACT.group('height'))
    factorx=int(FACT.group('width'))
    factory=int(FACT.group('height'))
    
    search_scale = re.compile(r'\s*(?P<width>\d+(\.\d*)*)\s*x\s*(?P<height>\d+(\.\d*)*)\s*')
    SCAL=search_scale.search(args.scale)
    if (args.debug):
        print(SCAL.group('width'),SCAL.group('height'))
    scalex=float(SCAL.group('width'))
    scaley=float(SCAL.group('height'))
    
    port = int(args.startport)

    script = ""
    #"xrandr -s %s && " % (args.resolution)

    resolutionx=int(x_resolution/factorx)
    resolutiony=int(y_resolution/factory)

    x_scale=int(resolutionx*scalex)
    y_scale=int(resolutiony*scaley)
    
    wall={}
    wall["name"]=args.wall_name
    wall["min"]=args.min
    wall["max"]=args.max
    wall["screens"]=[]

    # TiledViz
    if (otiledviz):
        tiles=[]

    # Localhost
    Localhost=bool(args.localhost)
    if (Localhost):
        LOCALHOST="-listen localhost"
    else:
        LOCALHOST=""
        
    id=1
    for x in range(0,factorx):
        for y in range(0,factory):
            x_offset = int(resolutionx*x)
            y_offset = int(resolutiony*y)
            password = secrets.token_urlsafe(password_length)

            x_scale_=x_scale
            resolutionx_=resolutionx
            x_offset_=x_offset
            y_scale_=y_scale
            resolutiony_=resolutiony
            y_offset_=y_offset
            if (boverlap):
                if (x==0):
                    x_scale_=x_scale_+overlap
                    resolutionx_=resolutionx_+overlap/scalex
                elif (x==factorx-1):
                    x_scale_=x_scale_+overlap
                    x_offset_=x_offset_-overlap
                    resolutionx_=resolutionx_+overlap/scalex
                else:
                    x_scale_=x_scale_+2*overlap
                    x_offset_=x_offset_-overlap
                    resolutionx_=resolutionx_+2*overlap/scalex
                if (y==0):
                    y_scale_=y_scale_+overlap
                    resolutiony_=resolutiony_+overlap/scaley
                elif (y==factory-1):
                    y_scale_=y_scale_+overlap
                    y_offset_=y_offset_-overlap
                    resolutiony_=resolutiony_+overlap/scaley
                else:
                    y_scale_=y_scale_+2*overlap
                    y_offset_=y_offset_-overlap
                    resolutiony_=resolutiony_+2*overlap/scaley
                
            RESOLUTION=str(int(resolutionx_))+'x'+str(int(resolutiony_))
            
            
            file_passwd="${HOME}/.vnc/passwd_"+args.wall_name+"_"+str(x)+str(y)
            script += "x11vnc -storepasswd '"+password+"' "+file_passwd +" \n "
            if (otiledviz):
                freeport="$freeport"
                script += "freeport=$(. /etc/bash.bashrc; python3 -c 'import socket; s=socket.socket(); s.bind((\"\", 0)); print(s.getsockname()[1]); s.close()' )"+" \n"
            else:
                freeport=port

            script += 'x11vnc -geometry {1}x{2} -rfbport {0} -noncache -cursor arrow -forever -no6 -noipv6 -noxdamage -shared -nowf -rfbauth {8} '\
                '-clip {9}+{3}+{4} {10} -shared -nowf -http > /tmp/log_vnc_{5}_$(date +%F_%H-%M-%S)_{6}x{7} 2>&1 & \n'\
                .format(freeport,x_scale_,y_scale_,x_offset_,y_offset_,scriptname,x,y,file_passwd,RESOLUTION,LOCALHOST)
            #             0       1        2        3         4          5        6 7   8           9
            if (otiledviz):
                script += 'sleep 2 \ncd '+os.path.join(TiledVizPATH,'TVConnections')+' && '+\
                './wss_websockify  '+SSL_PUBLIC+' '+SSL_PRIVATE+' '+SSL_HOST+':{0}'.format(port)+' $freeport '+os.path.join(TiledVizPATH,'TVWeb')+\
                ' 2>&1 > /tmp/websockify_$(date +%F_%H-%M-%S)_{6}x{7}.log'+' & \n'
                #/tmp/websockify_$(date +%F_%s%N | cut -b1-24).log

            screen={}
            screen["ID"]="%02.d" % (id)
            screen["posX"]=x
            screen["posY"]=y
            options={}
            if (otiledviz):
                options["address"]=SSL_HOST+":"+str(port)
            else:
                options["address"]=args.ip+":"+str(port)
            options["password"]=password
            screen["options"]=options
            wall["screens"].append(screen)

            if (otiledviz):
                tiles.append({"title" :  wall["name"]+'_'+str(screen["posX"])+'-'+str(screen["posY"]) ,
                              "comment" : wall["name"]+'_'+str(screen),
                              "tags":     [
                                  '{ID,0,'+str(id)+','+str(factorx*factory)+'}',
                                  'Name_'+wall["name"],
                                  'Factor_'+args.factor,
                                  'Scale_'+args.scale,
                                  'Resol_'+RESOLUTION,
                                  '{Min,0,'+wall["min"]+',20}',
                                  '{Max,1,'+wall["max"]+',20}'
                                           ],
                              "url" : "https://desktop.mdls.fr/noVNC/vnc.html?autoconnect=1&host="+SSL_HOST+"&port="+str(port)+"&encrypt=1&password="+password+"&true_color=1",
                              })
            
            port+=1
            id=id+1

    wall["width"]=factorx
    wall["height"]=factory

    scalex=int(x_resolution*scalex)
    scaley=int(y_resolution*scaley)

    wall["scalex"]=scalex
    wall["scaley"]=scaley
    
    if (boverlap):
        wall["overlap"]=overlap
    
    script += "echo OK"
    scriptf.write(str(script))
    scriptf.close()
    os.system("chmod u+x "+scriptname)
    
    with open(jsonname,"w+") as jsonfile:
        json.dump(wall,jsonfile)

    if (otiledviz):
        TiledViz_file=open(tiledvizjson,'w+')
        json_tiledviz["nodes"]=json_tiledviz["nodes"]+tiles
        json_tiles_text=json.JSONEncoder().encode(json_tiledviz)
        TiledViz_file.write(json_tiles_text)
        TiledViz_file.close()
