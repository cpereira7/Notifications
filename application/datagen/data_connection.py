"""Module providing functions for the database connection."""

import os
import psycopg2

db_user = os.environ.get("POSTGRES_USER", "postgres")
db_password = os.environ.get("POSTGRES_PASSWORD", "postgres")
db_host = os.environ.get("POSTGRES_HOST")
db_port = os.environ.get("POSTGRES_PORT", "5432")
db_name = os.environ.get("POSTGRES_DB")

def connect_database():
    """Function for connecting to the hosts database."""
    try:
        conn = psycopg2.connect(
            dbname=db_name,
            user=db_user,
            password=db_password,
            host=db_host,
            port=db_port
        )
        print("Connected to the database.")
        return conn
    except psycopg2.OperationalError as oe:
        print("Operational error connecting to the database:", oe)
        return None
    except psycopg2.DatabaseError as de:
        print("Database error connecting to the database:", de)
        return None
