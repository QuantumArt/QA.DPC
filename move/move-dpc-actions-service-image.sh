#!/bin/sh
docker pull '||registry-source||/dpc-actions-service:||version||' && \
docker tag '||registry-source||/dpc-actions-service:||version||' '||registry-destination||/dpc-actions-service:||version||' && \
docker push '||registry-destination||/dpc-actions-service:||version||'