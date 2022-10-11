#!/bin/sh
docker pull '||registry-source||/dpc-api:||version||' && \
docker tag '||registry-source||/dpc-api:||version||' '||registry-destination||/dpc-api:||version||' && \
docker push '||registry-destination||/dpc-api:||version||'