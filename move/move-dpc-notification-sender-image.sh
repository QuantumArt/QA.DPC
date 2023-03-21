#!/bin/sh
docker pull '||registry-source||/dpc-notification-sender:||version||' && \
docker tag '||registry-source||/dpc-notification-sender:||version||' '||registry||/dpc-notification-sender:||version||' && \
docker push '||registry||/dpc-notification-sender:||version||'