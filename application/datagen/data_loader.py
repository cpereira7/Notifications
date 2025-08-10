"""Module providing functions for reading resource files."""

import os
import csv


def read_cities_from_file():
    """Function for reading the locations file."""
    script_dir = os.path.dirname(os.path.realpath(__file__))
    file_path = os.path.join(script_dir, 'data', 'locations.csv')
    cities = {}
    with open(file_path, 'r', encoding="utf-8") as file:
        reader = csv.DictReader(file)
        for row in reader:
            city = row['city']
            region = row['region']
            cities[city] = region
    return cities


def read_descriptions_from_file():
    """Function for reading the descriptions file."""
    script_dir = os.path.dirname(os.path.realpath(__file__))
    file_path = os.path.join(script_dir, 'data', 'descriptions.csv')
    descriptions = {}
    with open(file_path, 'r', encoding="utf-8") as file:
        reader = csv.DictReader(file)
        for row in reader:
            event_type = row['type']
            description = row['description']
            if event_type not in descriptions:
                descriptions[event_type] = []
            descriptions[event_type].append(description)
    return descriptions
