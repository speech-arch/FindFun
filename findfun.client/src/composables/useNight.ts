/**
 * Determines if the current time is considered night based on sunrise and sunset.
 * @param currentTime - The current time as an ISO string.
 * @param sunrise - Sunrise time as an ISO string.
 * @param sunset - Sunset time as an ISO string.
 * @returns True if it's night, false otherwise.
 */
export function isNight(currentTime?: string, sunrise?: string, sunset?: string): boolean {
  if (!currentTime || !sunrise || !sunset) return false
  const now = new Date(currentTime)
  return now < new Date(sunrise) || now >= new Date(sunset)
}
