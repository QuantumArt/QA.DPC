#!/bin/sh
docker pull '||registry-source||/dpc-admin:||version||' && \
docker tag '||registry-source||/dpc-admin:||version||' '||registry-destination||/dpc-admin:||version||' && \
docker push '||registry-destination||/dpc-admin:||version||'