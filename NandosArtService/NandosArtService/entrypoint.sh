#!/bin/bash

DB_CONN_SERVER=${DB_CONN_SERVER:-"mysql"}
DB_CONN_USER_ID=${DB_CONN_USER_ID:-"remoteuser"}
DB_CONN_PASS=${DB_CONN_PASS:-"dummypassword"}
DB_CONN_PORT=${DB_CONN_PORT:-"3306"}
DB_CONN_DATABASE=${DB_CONN_DATABASE:-"dummydb"}
CLARIFAI_SCORE=${CLARIFAI_SCORE:-"0.7"}

sed -i -e "s/__SERVER__/$DB_CONN_SERVER/g" \
    -e "s/__USER__/$DB_CONN_USER_ID/g" \
    -e "s/__PASSWORD__/$DB_CONN_PASS/g" \
    -e "s/__PORT__/$DB_CONN_PORT/g" \
    -e "s/__DATABASE__/$DB_CONN_DATABASE/g" \
    -e "s/__CLARIFAI_SCORE__/$CLARIFAI_SCORE/g" appsettings.json

dotnet NandosArtService.dll