"""Main Module. Generate random events and adds them to a database."""

import sys
import os
import time
import psycopg2
from data_connection import connect_database
from data_loader import read_cities_from_file, read_descriptions_from_file
from data_random import get_random_description, get_random_city_and_region, get_random_event


def generate_events(cursor, num_events):
    """Function to generate the emergency events"""
    query = "INSERT INTO EmergencyEvents (type, location, region, description) \
          VALUES (%s, %s, %s, %s)"
    events_to_insert = []

    for _ in range(num_events):
        event_type = get_random_event(event_types)
        location, region = get_random_city_and_region(cities)
        description = get_random_description(event_type, descriptions)
        events_to_insert.append((event_type, location, region, description))

    try:
        cursor.executemany(query, events_to_insert)
        conn.commit()
    except psycopg2.Error as e:
        print("Error inserting event:", e)

conn = connect_database()
if conn is None:
    sys.exit(1)

# define events data
event_types = ['fire', 'medical', 'accident', 'disaster',
               'crime', 'power', 'gas']
cities = read_cities_from_file()
descriptions = read_descriptions_from_file()

# define events amount
events_total = int(os.environ.get("GENERATOR_ITEMS", 10))
events_interval = float(os.environ.get("GENERATOR_INTERVAL", 10))

cur = conn.cursor()
try:
    while True:
        generate_events(cur, events_total)
        time.sleep(events_interval)
except KeyboardInterrupt:
    print("Terminating...")
finally:
    cur.close()
    conn.close()
