#!/bin/sh
docker pull '||registry-source||/dpc-impact:||version||' && \
docker tag '||registry-source||/dpc-impact:||version||' '||registry||/dpc-impact:||version||' && \
docker push '||registry||/dpc-impact:||version||'