import { storeToRefs, defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { useUserLocationStore } from './userLocation'
import { HttpClient, safeRequest } from '@/composables/useHttpClient'
import type { Events, Park } from '@/types/park'
import { RoutePaths } from '@/config/Enums'

export interface GetParksParams
{
  search?: string
  sortBy?: 'name' | 'location' | 'municipality' | 'province' | 'rating' | 'parkType' | 'distance'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
  municipalityId?: number
  provinceId?: number
  autonomousCommunityId?: number
  latitude?: number
  longitude?: number
  radiusKm?: number
  rating?: number
  amenities?: string
  parkType?: string
}

export interface PagedParksResponse
{
  items: Park[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface GetEventsParams
{
  search?: string
  sortBy?: 'title' | 'starttime' | 'endtime' | 'location'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
  streetId?: number
  municipalityId?: number
  provinceId?: number
  autonomousCommunityId?: number
  startDate?: string
  endDate?: string
  upcomingOnly?: boolean
  ongoingOnly?: boolean
}

export interface PagedEventsResponse
{
  items: Events[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export const useParksStore = defineStore('parks', () =>
{
  const client = new HttpClient({
    baseURL: RoutePaths.BaseURL,
  })
  const isLoading = ref(false)
  const parks = ref<Park[]>([])
  const events = ref<Events[]>([])
  const totalPages = ref(1)
  const currentPage = ref(1)
  const eventsTotalPages = ref(1)
  const eventsCurrentPage = ref(1)

  const cancelSource = ref(client.createCancelToken())

  const userLocationStore = useUserLocationStore()
  const { userLocation } = storeToRefs(userLocationStore)

  async function fetchParks(queryParams?: GetParksParams)
  {
    isLoading.value = true
    cancelSource.value.cancel()
    cancelSource.value = client.createCancelToken()
    const params = queryParams || {
      page: 1,
      pageSize: 10,
    }

    // Add geolocation if available and not explicitly provided
    if (
      !params.latitude &&
      !params.longitude &&
      typeof userLocation.value?.coords?.latitude === 'number'
    )
    {
      params.latitude = userLocation.value.coords.latitude
      params.longitude = userLocation.value.coords.longitude
      params.radiusKm = params.radiusKm ?? 20
    }

    const [data, error] = await safeRequest(() =>
      client.get<PagedParksResponse>(
        RoutePaths.Parks,
        { cancelToken: cancelSource.value.token },
        params as Record<string, unknown>,
      ),
    )

    if (error?.canceled || error)
    {
      isLoading.value = false
      return
    }
    parks.value = data?.items ?? []
    totalPages.value = data?.totalPages ?? 1
    currentPage.value = data?.page ?? 1
    isLoading.value = false
  }

  async function fetchEvents(queryParams?: GetEventsParams)
  {
    isLoading.value = true
    cancelSource.value.cancel()
    cancelSource.value = client.createCancelToken()

    const params = queryParams || {
      page: 1,
      pageSize: 10,
    }

    const [data, error] = await safeRequest(() =>
      client.get<PagedEventsResponse>(
        RoutePaths.Events,
        { cancelToken: cancelSource.value.token },
        params as Record<string, unknown>,
      ),
    )

    if (error?.canceled || error)
    {
      isLoading.value = false
      return
    }
    events.value = data?.items ?? []
    eventsTotalPages.value = data?.totalPages ?? 1
    eventsCurrentPage.value = data?.page ?? 1
    isLoading.value = false
  }

  async function fetchParkById(id: string)
  {
    const exists = parks.value.some((p) => p.id === id)
    if (exists) return

    const [data, error] = await safeRequest(() => client.get<Park>(`${RoutePaths.Parks}/${id}`))
    if (!error && data)
    {
      parks.value.push(data)
    }
  }
  async function fetchEventById(id: string)
  {
    const exists = events.value.some((e) => e.id === id)
    if (exists) return

    const [data, error] = await safeRequest(() => client.get<Events>(`${RoutePaths.Events}/${id}`))

    if (!error && data)
    {
      events.value.push(data)
    }
  }
  watch(
    userLocation,
    (loc) =>
    {
      if (loc?.coords?.latitude && loc?.coords?.longitude)
      {
        fetchParks({
          page: 1,
          pageSize: 10,
          latitude: loc.coords.latitude,
          longitude: loc.coords.longitude,
          radiusKm: 20,
        })
      }
    },
    { immediate: true },
  )

  return {
    parks,
    events,
    isLoading,
    totalPages,
    currentPage,
    eventsTotalPages,
    eventsCurrentPage,
    fetchParks,
    fetchEvents,
    fetchParkById,
    fetchEventById,
  }
})
