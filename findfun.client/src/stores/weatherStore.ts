import { getWeatherData } from '@/services/weatherService'
import type { WeatherResponse, WeatherInfo, AirQualityResponse } from '@/types/park'
import { defineStore } from 'pinia'
import { ref, watch, type Ref } from 'vue'
import { getWeatherIcon,   } from '@/utils/weatherUtils'

export const useWeatherStore = defineStore('weather', () =>
{
  const weatherByParkId = ref<
    Record<
      string,
      {
        temperature: number | null
        sunrise?: string
        sunset?: string
        currentTime?: string
        weatherResponse?: WeatherResponse
        airQualityResponse?: AirQualityResponse
      }
    >
  >({})

  const useWeatherInfo = (
    park: Ref<{ id?: string; latitude?: number; longitude?: number } | undefined>,
  ) =>
  {
    const weatherInfo = ref<WeatherInfo>({
      temperature: null,
      sunrise: undefined,
      sunset: undefined,
      currentTime: undefined,
      weatherResponse: undefined,
      airQualityResponse: undefined,
    })
    watch(
      park,
      async (val) =>
      {
        if (val?.latitude && val?.longitude && val?.id)
        {
          weatherInfo.value = await fetchWeatherForPark(
            val as { id: string; latitude: number; longitude: number },
          )
        }
      },
      { immediate: true },
    )
    return weatherInfo
  }

  async function fetchWeatherForPark(park: {
    id: string
    latitude: number
    longitude: number
  }): Promise<{
    temperature: number | null
    sunrise?: string
    sunset?: string
    currentTime?: string
    weatherResponse?: WeatherResponse
    airQualityResponse?: AirQualityResponse
  }>
  {
    const cached = weatherByParkId.value[park.id]
    if (cached?.currentTime)
    {
      const cachedTime = new Date(cached.currentTime).getTime()
      const now = Date.now()
      if (!Number.isNaN(cachedTime) && now - cachedTime < 60 * 60 * 1000)
      {
        return cached
      }
    }

    try
    {
      const openmateo = await getWeatherData(park.latitude, park.longitude)
      const weatherRes = openmateo[0]
      const airQresponse = openmateo[1]
      const today = weatherRes.current.time?.slice(0, 10) || new Date().toISOString().slice(0, 10)

      const idx = weatherRes.daily?.time?.indexOf(today) ?? -1

      const sunrise = idx !== undefined && idx !== -1 ? weatherRes.daily?.sunrise?.[idx] : undefined
      const sunset = idx !== undefined && idx !== -1 ? weatherRes.daily?.sunset?.[idx] : undefined

      const info = {
        temperature: weatherRes.current.temperature_2m,
        sunrise,
        sunset,
        currentTime: weatherRes.current.time,
        weatherResponse: weatherRes,
        airQualityResponse: airQresponse,
      }
      weatherByParkId.value[park.id] = info
      return info
    } catch
    {
      const info = {
        temperature: null,
        sunrise: undefined,
        sunset: undefined,
        currentTime: undefined,
        weatherResponse: undefined,
        aireQualityResponse: undefined,
      }
      weatherByParkId.value[park.id] = info
      return info
    }
  }

  return {
    weatherByParkId,
    fetchWeatherForPark,
    getWeatherIcon,
    useWeatherInfo,
  }
})

export {getWeatherIconColor, mapAirQualityDisplay} from '@/utils/weatherUtils'