#!/bin/sh
docker pull '||registry-source||/dpc-actions-service:||version||' && \
docker tag '||registry-source||/dpc-actions-service:||version||' '||registry||/dpc-actions-service:||version||' && \
docker push '||registry||/dpc-actions-service:||version||'