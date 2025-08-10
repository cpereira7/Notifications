"""Module providing functions to randomize data."""

import random


def get_random_description(event_type, descriptions):
    """Function to get random descriptions considering an event type."""
    if event_type in descriptions:
        return random.choice(descriptions[event_type])
    return "Invalid event type"


def get_random_city_and_region(cities):
    """Function to get random cities"""
    city = random.choice(list(cities.keys()))
    region = cities[city]
    return city, region

def get_random_event(events):
    """Function to get random events"""
    return random.choice(events)
