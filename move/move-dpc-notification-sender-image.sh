#!/bin/sh
docker pull '||registry-source||/dpc-notification-sender:||version||' && \
docker tag '||registry-source||/dpc-notification-sender:||version||' '||registry-destination||/dpc-notification-sender:||version||' && \
docker push '||registry-destination||/dpc-notification-sender:||version||'