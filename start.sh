#!/bin/bash

echo "starting db middleware"

export DB_SERVER=127.0.0.1,10094
export DB_NAME=Ratchet_Deadlocked
export DB_USER=svc_RatchetDeadlockedDB
export DB_PASSWORD=trombonePhoneHome2
export ASPNETCORE_ENVIRONMENT=Development

nohup ./DeadlockedDatabase > logs/db.log 2>&1 &

sleep 2

echo "db middleware started"

