#!/bin/sh
docker pull '||registry-source||/dpc-front:||version||' && \
docker tag '||registry-source||/dpc-front:||version||' '||registry-destination||/dpc-front:||version||' && \
docker push '||registry-destination||/dpc-front:||version||'