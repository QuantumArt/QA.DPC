#!/bin/sh
docker pull '||registry-source||/dpc-api:||version||' && \
docker tag '||registry-source||/dpc-api:||version||' '||registry||/dpc-api:||version||' && \
docker push '||registry||/dpc-api:||version||'