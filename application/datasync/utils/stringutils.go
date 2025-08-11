package utils

import (
	"strings"
	"unicode"
)

func CleanAndLowercase(s string) string {
	var sb strings.Builder
	for _, r := range s {
		if unicode.IsLetter(r) || unicode.IsDigit(r) || unicode.IsPunct(r) {
			sb.WriteRune(unicode.ToLower(r))
		} else if r == ' ' {
			sb.WriteRune('_')
		}
	}
	return sb.String()
}
