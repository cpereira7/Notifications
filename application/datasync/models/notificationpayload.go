package models

import "time"

// Define the database notification payload
type NotificationPayload struct {
	Table  string `json:"table"`
	Action string `json:"action"`
	Data   struct {
		ID          int       `json:"id"`
		Type        string    `json:"type"`
		Location    string    `json:"location"`
		Region      string    `json:"region"`
		Description string    `json:"description"`
		Timestamp   time.Time `json:"timestamp"`
	} `json:"data"`
}
