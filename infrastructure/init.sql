CREATE TYPE EmergencyType AS ENUM (
    'fire', 
    'medical', 
    'accident', 
    'disaster',
    'crime',
    'power',
    'gas'
);

CREATE TABLE EmergencyEvents (
    id SERIAL PRIMARY KEY,
    type EmergencyType NOT NULL,
    location VARCHAR(100) NOT NULL,
    region VARCHAR(50) NOT NULL,
    description TEXT,
    timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

