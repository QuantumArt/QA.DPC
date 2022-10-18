#!/bin/sh
docker pull '||registry-source||/dpc-front:||version||' && \
docker tag '||registry-source||/dpc-front:||version||' '||registry||/dpc-front:||version||' && \
docker push '||registry||/dpc-front:||version||'