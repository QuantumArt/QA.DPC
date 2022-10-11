#!/bin/sh
docker pull '||registry-source||/dpc-highload-front:||version||' && \
docker tag '||registry-source||/dpc-highload-front:||version||' '||registry-destination||/dpc-highload-front:||version||' && \
docker push '||registry-destination||/dpc-highload-front:||version||'