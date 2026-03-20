import { isNight } from '@/composables/useNight'
import { useColorForIcon } from '@/composables/useColorGenerator'

export const airQualityLevelMap: Record<string, { color: string; icon: string }> = {
    Good: { color: '#4CAF50', icon: 'mdi:emoticon-happy-outline' },
    Fair: { color: '#CDDC39', icon: 'mdi:emoticon-neutral-outline' },
    Moderate: { color: '#FFC107', icon: 'mdi:emoticon-neutral-outline' },
    Poor: { color: '#FF9800', icon: 'mdi:emoticon-sad-outline' },
    'Very Poor': { color: '#F44336', icon: 'mdi:emoticon-dead-outline' },
    'Extremely Poor': { color: '#B71C1C', icon: 'mdi:emoticon-dead-outline' },
    None: { color: '#50d19fff', icon: 'mdi:emoticon-neutral-outline' },
    Low: { color: '#4CAF50', icon: 'mdi:leaf' },
    High: { color: '#FF9800', icon: 'mdi:leaf' },
    'Very High': { color: '#F44336', icon: 'mdi:leaf' },
}

function mapPM25(value: number): string
{
    if (value <= 10) return 'Good'
    if (value <= 20) return 'Fair'
    if (value <= 25) return 'Moderate'
    if (value <= 50) return 'Poor'
    if (value <= 75) return 'Very Poor'
    return 'Extremely Poor'
}

function mapPM10(value: number): string
{
    if (value <= 20) return 'Good'
    if (value <= 40) return 'Fair'
    if (value <= 50) return 'Moderate'
    if (value <= 100) return 'Poor'
    if (value <= 150) return 'Very Poor'
    return 'Extremely Poor'
}

function mapOzone(value: number): string
{
    if (value <= 50) return 'Good'
    if (value <= 100) return 'Fair'
    if (value <= 130) return 'Moderate'
    if (value <= 240) return 'Poor'
    if (value <= 380) return 'Very Poor'
    return 'Extremely Poor'
}

function mapPollen(value: number | null): string
{
    if (value === null || value === 0) return 'None'
    if (value < 10) return 'Low'
    if (value < 50) return 'Moderate'
    if (value < 200) return 'High'
    return 'Very High'
}

function mapEuropeanAQI(value: number): string
{
    if (value <= 20) return 'Good'
    if (value <= 40) return 'Fair'
    if (value <= 60) return 'Moderate'
    if (value <= 80) return 'Poor'
    if (value <= 100) return 'Very Poor'
    return 'Extremely Poor'
}

/**
 * Maps a value to a display object with name, level, color, and icon for air quality or pollen.
 * @param {string} type - The type of value (pm10, pm2_5, ozone, pollen, european_aqi)
 * @param {number|null} value - The value to map
 * @returns {{ name: string, level: string, color: string, icon: string }}
 */
export function mapAirQualityDisplay(type: string, value: number | null): { name: string; level: string; color: string; icon: string }
{
    let level = ''
    switch (type)
    {
        case 'pm10':
            level = mapPM10(value ?? 0)
            break
        case 'pm2_5':
            level = mapPM25(value ?? 0)
            break
        case 'ozone':
            level = mapOzone(value ?? 0)
            break
        case 'european_aqi':
            level = mapEuropeanAQI(value ?? 0)
            break
        case 'alder_pollen':
        case 'birch_pollen':
        case 'grass_pollen':
        case 'mugwort_pollen':
        case 'olive_pollen':
        case 'ragweed_pollen':
            level = mapPollen(value)
            break
        default:
    }
    const display = airQualityLevelMap[level] || { color: '#BDBDBD', icon: 'mdi:help-circle-outline' }
    return {
        name: type,
        level,
        color: display.color,
        icon: display.icon,
    }
}

/**
 * Generate a unique, deterministic color for any icon using hash-based color generation
 * @param {string} icon - The icon name
 * @returns {string} A consistent hex color for the icon
 */
export const getWeatherIconColor = (icon: string): string => useColorForIcon(icon)

// Weather icon helper functions
const getRainIcon = (rain: number): string | null =>
{
    if (rain > 0)
    {
        return rain > 10 ? 'mdi:weather-pouring' : 'mdi:weather-rainy'
    }
    return null
}

const getCloudIcon = (cloud: number, night: boolean): string | null =>
{
    if (cloud >= 90) return 'mdi:weather-cloudy'
    if (cloud >= 50) return night ? 'mdi:weather-night-partly-cloudy' : 'mdi:weather-partly-cloudy'
    return null
}

const getClearOrFogOrSnowIcon = (
    cloud: number,
    uv: number,
    temp: number,
    night: boolean,
): string | null =>
{
    if (uv === 0 && cloud > 80) return 'mdi:weather-fog'
    if (temp <= 0) return 'mdi:weather-snowy'
    if (cloud < 20 && uv > 2) return night ? 'mdi:weather-night' : 'mdi:weather-sunny'
    return null
}

export const getWeatherIcon = (
    weather?: {
        time: string
        interval: number
        temperature_2m: number
        relative_humidity_2m: number
        wind_speed_10m: number
        precipitation: number
        uv_index: number
        cloudcover: number
        rain: number
    },
    sunrise?: string,
    sunset?: string,
): string =>
{
    if (weather?.temperature_2m == null) return 'mdi:help-circle-outline'
    const night = isNight(weather.time, sunrise, sunset)
    const cloud = weather.cloudcover ?? 0
    const rain = weather.precipitation ?? 0
    const uv = weather.uv_index ?? 0
    const temp = weather.temperature_2m

    const rainIcon = getRainIcon(rain)
    if (rainIcon) return rainIcon

    const cloudIcon = getCloudIcon(cloud, night)
    if (cloudIcon) return cloudIcon

    const clearOrFogOrSnowIcon = getClearOrFogOrSnowIcon(cloud, uv, temp, night)
    if (clearOrFogOrSnowIcon) return clearOrFogOrSnowIcon
    return night ? 'mdi:weather-cloudy' : 'mdi:weather-sunny'
}
