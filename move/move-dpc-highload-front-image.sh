#!/bin/sh
docker pull '||registry-source||/dpc-highload-front:||version||' && \
docker tag '||registry-source||/dpc-highload-front:||version||' '||registry||/dpc-highload-front:||version||' && \
docker push '||registry||/dpc-highload-front:||version||'