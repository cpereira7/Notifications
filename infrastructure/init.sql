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

CREATE FUNCTION public.notify_on_data_change()
  RETURNS trigger
  LANGUAGE 'plpgsql'
AS $BODY$ 
DECLARE 
  data JSON;
  notification JSON;
BEGIN

IF TG_WHEN <> 'AFTER' THEN
  RAISE EXCEPTION 'public.notify_on_data_change() may only run as an AFTER trigger';
END IF;
  IF (TG_OP = 'DELETE') THEN
    data = row_to_json(OLD);
  ELSE
    data = row_to_json(NEW);
  END IF;

notification = json_build_object(
  'table',TG_TABLE_NAME,
  'action', TG_OP,
  'data', data);  
            
    PERFORM pg_notify('datachange', notification::TEXT);
  RETURN NEW;
END
$BODY$;

CREATE TRIGGER on_data_change
  AFTER INSERT OR DELETE OR UPDATE 
  ON public.EmergencyEvents
  FOR EACH ROW
  EXECUTE PROCEDURE public.notify_on_data_change();