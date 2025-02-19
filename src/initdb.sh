#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER llm WITH ENCRYPTED PASSWORD 'llm';

    CREATE DATABASE llm;

    GRANT ALL PRIVILEGES ON DATABASE llm TO llm;

    GRANT CREATE ON DATABASE llm TO llm;

    --CREATE EXTENSION postgis;
    --CREATE EXTENSION postgis_topology;
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname llm <<-EOSQL
    GRANT ALL ON schema public TO llm;
    CREATE EXTENSION vector;
EOSQL
