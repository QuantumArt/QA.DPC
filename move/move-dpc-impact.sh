#!/bin/sh
docker pull '||registry-source||/dpc-impact:||version||' && \
docker tag '||registry-source||/dpc-impact:||version||' '||registry-destination||/dpc-impact:||version||' && \
docker push '||registry-destination||/dpc-impact:||version||'