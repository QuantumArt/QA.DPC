#!/bin/sh
docker pull '||registry-source||/dpc-kafka-api:||version||' && \
docker tag '||registry-source||/dpc-kafka-api:||version||' '||registry||/dpc-kafka-api:||version||' && \
docker push '||registry||/dpc-kafka-api:||version||'