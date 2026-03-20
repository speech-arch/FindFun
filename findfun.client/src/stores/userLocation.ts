import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useGeolocation } from '../composables/useGeolocation'

interface IPLocationData
{
  latitude: number
  longitude: number
  accuracy: number
  altitude: number | null
  altitudeAccuracy: number | null
  heading: number | null
  speed: number | null
  city: string
  region: string
  country_name: string
  ip: string
}

interface LocationError
{
  message?: string
  code?: number
}

interface LocationMetadata
{
  city: string | null
  region: string | null
  country: string | null
  ip: string | null
}

export const useUserLocationStore = defineStore('userLocation', () =>
{
  const userLocation = ref<GeolocationPosition | null>(null)
  const locationError = ref<string | null>(null)
  const isLocationLoading = ref(false)
  const locationMetadata = ref<LocationMetadata>({
    city: null,
    region: null,
    country: null,
    ip: null,
  })

  const setLocationState = (position: GeolocationPosition): void =>
  {
    userLocation.value = position
    isLocationLoading.value = false
  }

  const setLocationError = (error: LocationError): void =>
  {
    locationError.value = error.message || 'Unknown location error'
    isLocationLoading.value = false
  }

  const { getUserLocationWithWatch, getUserLocationFromIP } = useGeolocation()

  const fetchUserLocationWithWatch = () =>
  {
    isLocationLoading.value = true
    getUserLocationWithWatch(
      (position) =>
      {
        setLocationState(position)
      },
      (error) =>
      {
        setLocationError(error)
      },
    )
  }

  const fetchUserLocationFromIP = async (): Promise<void> =>
  {
    isLocationLoading.value = true
    await getUserLocationFromIP(
      (data: IPLocationData) =>
      {
        userLocation.value = {
          coords: {
            latitude: data.latitude,
            longitude: data.longitude,
            accuracy: data.accuracy,
            altitude: data.altitude,
            altitudeAccuracy: data.altitudeAccuracy,
            heading: data.heading,
            speed: data.speed,
            toJSON: () => ({}),
          },
          timestamp: Date.now(),
          toJSON: () => ({}),
        }
        locationMetadata.value = {
          city: data.city,
          region: data.region,
          country: data.country_name,
          ip: data.ip,
        }
        isLocationLoading.value = false
      },
      (err: LocationError) =>
      {
        setLocationError(err)
      },
    )
  }

  const clearUserLocation = () =>
  {
    userLocation.value = null
    locationError.value = null
    isLocationLoading.value = false
    locationMetadata.value = {
      city: null,
      region: null,
      country: null,
      ip: null,
    }
  }

  return {
    userLocation,
    locationError,
    isLocationLoading,
    locationMetadata,
    setLocationState,
    setLocationError,
    fetchUserLocationWithWatch,
    fetchUserLocationFromIP,
    clearUserLocation,
  }
})
