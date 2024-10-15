export SSL_PUBLIC=/etc/letsencrypt/archive/mdls.fr/fullchain.pem
export SSL_PRIVATE=/etc/letsencrypt/archive/mdls.fr/privkey.pem
export SSL_HOST=desktop.mdls.fr
export TiledVizPATH=$HOME/TiledViz


rm -f vrwall.json
./vmbuilder.py -n LowRest -a 0 -b 14 -r 1920x1080 -s 0.25x0.25 -f 1x1 -t vrwall.json -p 6900 -i 192.168.0.1
./vmbuilder.py -n MedRest -a 1 -b 3 -r 1920x1080 -s 0.5x0.5 -f 2x2 -t vrwall.json -p 6901 -i 192.168.0.1
./vmbuilder.py -n HigRest -a 0 -b 1 -r 1920x1080 -s 1x1 -f 4x4 -t vrwall.json -p 6905 -i 192.168.0.1
./concat_json LowRest.json MedRest.json HigRest.json


./LowRest.sh 
./MedRest.sh
./HigRest.sh 


./vmbuilder.py -n LowReso -a 0 -b 14 -r 1920x1080 -s 0.25x0.25 -f 1x1 -t vrwall.json -p 6900 -i 192.168.0.1
./vmbuilder.py -n MedReso -a 1 -b 3 -r 1920x1080 -s 0.5x0.5 -f 2x2 -o 5 -t vrwall.json -p 6901 -i 192.168.0.1
./vmbuilder.py -n HigReso -a 0 -b 1 -r 1920x1080 -s 1x1 -f 4x4 -o 10 -t vrwall.json -p 6905 -i 192.168.0.1
./concat_json LowReso.json MedReso.json HigReso.json

./LowReso.sh 
./MedReso.sh
./HigReso.sh 
