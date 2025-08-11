package main

import (
	"context"
	"database/sql"
	"datasync/models"
	"datasync/utils"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"time"

	"github.com/go-redis/redis/v8"
	"github.com/lib/pq"
)

// Define DBManager structure to manage database connection and cache
type DBManager struct {
	DB           *sql.DB
	Redis        *redis.Client
	DBConnection *string
}

// Function to connect the program to the database
func (pg *DBManager) ConnectDatabase() error {
	postgresHost := os.Getenv("POSTGRES_HOST")
	postgresPort := os.Getenv("POSTGRES_PORT")
	postgresUser := os.Getenv("POSTGRES_USER")
	postgresPassword := os.Getenv("POSTGRES_PASSWORD")
	postgresDB := os.Getenv("POSTGRES_DB")

	connectionString := fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=disable",
		postgresHost, postgresPort, postgresUser, postgresPassword, postgresDB)

	var db, err = sql.Open("postgres", connectionString)
	if err != nil {
		return fmt.Errorf("failed to connect to database: %w", err)
	}

	if err := db.Ping(); err != nil {
		return fmt.Errorf("failed to ping database: %w", err)
	}

	log.Println("Successfully connected to PostgreSQL")
	pg.DB = db
	pg.DBConnection = &connectionString

	return nil
}

// Function to connect the program to the cache
func (pg *DBManager) ConnectCache() error {
	redisHost := os.Getenv("REDIS_HOST")
	redisPort := os.Getenv("REDIS_PORT")

	redisClient := redis.NewClient(&redis.Options{
		Addr:     fmt.Sprintf("%s:%s", redisHost, redisPort),
		Password: "", // no password set
		DB:       0,  // use default DB
	})

	var _, err = redisClient.Ping(context.Background()).Result()
	if err != nil {
		return fmt.Errorf("failed to connect to Redis: %w", err)
	}

	log.Println("Successfully connected to Redis Cache")
	pg.Redis = redisClient

	return nil
}

// Function to create the database listener
func CreateListener(pg *DBManager) (*pq.Listener, error) {

	reportProblem := func(et pq.ListenerEventType, err error) {
		if err != nil {
			log.Println("Error in listener:", err)
		}
	}

	listener := pq.NewListener(*pg.DBConnection, 10*time.Second, time.Minute, reportProblem)
	err := listener.Listen("datachange")
	if err != nil {
		log.Fatal("Error setting up listener:", err)
	}

	return listener, nil
}

// Listen function to handle incoming notifications and update cache
func ListenNotifications(l *pq.Listener, pg *DBManager) {
	for {
		// Wait for notifications from the listener
		n := <-l.Notify

		// Process notifications based on the channel
		switch n.Channel {
		case "datachange":
			err := ProcessNotification(n.Extra, pg)
			if err != nil {
				log.Println("Error processing notification:", err)
			}
		}
	}
}

// Function to handle notification processing in the cache
func ProcessNotification(payloadJSON string, pg *DBManager) error {
	var payload models.NotificationPayload
	if err := json.Unmarshal([]byte(payloadJSON), &payload); err != nil {
		return fmt.Errorf("failed to unmarshal payload: %v", err)
	}

	if payload.Table != "emergencyevents" {
		log.Printf("notification payload is not an emergency event")
		return nil
	}

	var err error

	redisKey := utils.GenerateEventKey(payload)
	locationSet := utils.GenerateRegionSet(payload)
	typeSet := utils.GenerateTypeSet(payload)

	switch payload.Action {
	case "INSERT", "UPDATE":
		eventFields := utils.CreatePayloadHashSet(payload)

		if err = pg.Redis.HMSet(context.Background(), redisKey, eventFields).Err(); err != nil {
			return fmt.Errorf("failed to set hash fields in Redis: %w", err)
		}

		if err = pg.Redis.SAdd(context.Background(), locationSet, redisKey).Err(); err != nil {
			return fmt.Errorf("failed to add location to Redis set: %w", err)
		}

		if err = pg.Redis.SAdd(context.Background(), typeSet, redisKey).Err(); err != nil {
			return fmt.Errorf("failed to add type to Redis set: %w", err)
		}

	case "DELETE":
		err = pg.Redis.Del(context.Background(), redisKey).Err()

		if err == nil {
			pg.Redis.SRem(context.Background(), locationSet, redisKey)
		}

	default:
		log.Printf("Unhandled action: %s", payload.Action)
	}

	if err != nil {
		return fmt.Errorf("failed to update Redis cache: %w", err)
	}

	return nil
}

func main() {
	fmt.Println("DataSync Starting..")
	dbManager := &DBManager{}

	if err := dbManager.ConnectDatabase(); err != nil {
		log.Fatal("Error connecting to database:", err)
	}

	if err := dbManager.ConnectCache(); err != nil {
		log.Fatal("Error connecting to cache:", err)
	}

	listener, _ := CreateListener(dbManager)

	go ListenNotifications(listener, dbManager)

	// Keep the main goroutine alive
	select {}
}
