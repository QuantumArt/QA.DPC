#!/bin/sh
docker pull '||registry-source||/dpc-admin:||version||' && \
docker tag '||registry-source||/dpc-admin:||version||' '||registry||/dpc-admin:||version||' && \
docker push '||registry||/dpc-admin:||version||'