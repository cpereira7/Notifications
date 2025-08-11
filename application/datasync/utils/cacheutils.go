package utils

import (
	"datasync/models"
	"fmt"
	"strings"
)

func CreatePayloadHashSet(payload models.NotificationPayload) map[string]interface{} {
	eventFields := map[string]interface{}{
		"type":        strings.ToLower(payload.Data.Type),
		"location":    strings.ToLower(payload.Data.Location),
		"region":      strings.ToLower(payload.Data.Region),
		"description": strings.ToLower(payload.Data.Description),
		"timestamp":   payload.Data.Timestamp,
	}
	return eventFields
}

func GenerateEventKey(payload models.NotificationPayload) string {

	eventKey := fmt.Sprintf("event:%d:%s:%s", payload.Data.ID, payload.Data.Type, payload.Data.Location)
	return CleanAndLowercase(eventKey)
}

func GenerateRegionSet(payload models.NotificationPayload) string {

	regionSet := fmt.Sprintf("region:%s", payload.Data.Region)
	return CleanAndLowercase(regionSet)
}

func GenerateTypeSet(payload models.NotificationPayload) string {

	typeSet := fmt.Sprintf("type:%s", payload.Data.Type)
	return CleanAndLowercase(typeSet)
}
