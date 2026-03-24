<template>
  <Card
    class="card border-0 m-2 mt-0 p-4 shadow-md transform scale-100 transition-transform duration-300 !rounded-none"
  >
    <template #title>Location</template>
    <template #content>
      <div class="relative w-full h-[450px] shadow">
        <section ref="map" class="w-full h-full" aria-label="Park map"></section>
        <div
          v-if="userLocationStore.isLocationLoading"
          class="absolute inset-0 flex items-center justify-center bg-black opacity-20"
        >
          <MapLoader />
        </div>
        <div
          v-else-if="!userLocation || !userLocation.coords"
          class="absolute inset-0 flex items-center justify-center pointer-events-none"
        >
          <output
            class="bg-white/80 dark:bg-black/80 rounded p-4 shadow text-center text-red-700 dark:text-red-300 text-sm font-semibold"
            aria-live="polite"
          >
            Unable to access your location. Showing default map view.
          </output>
        </div>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import { ref, shallowRef, onUnmounted } from 'vue'
import type { Location } from '../types/park'
import { useUserLocationStore } from '../stores/userLocation'
import { getDistanceKm, setUserMarkerAndCenter } from '../composables/useMapUtils'
import { useMapWindowEvents } from '../composables/useMapWindowEvents'
import Card from 'primevue/card'

type MarkerWithId = google.maps.marker.AdvancedMarkerElement & { parkId?: string | number }

const props = defineProps<{
  data?: Location[]
  filteredProducts?: Location[]
  isDetails?: boolean
}>()
const map = ref<HTMLElement | null>(null)
const userLocationStore = useUserLocationStore()
const { userLocation } = storeToRefs(userLocationStore)
const lat = ref<number>(0)
const lng = ref<number>(0)
const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY || ''

const userMarker = shallowRef<google.maps.marker.AdvancedMarkerElement | null>(null)
const locationMarkers = shallowRef<MarkerWithId[]>([])

watch(
  () => props.data,
  (newData) => {
    if (gmap.value && newData && newData.length > 0 && props.isDetails) {
      gmap.value.setCenter({
        lat: newData[0].latitude,
        lng: newData[0].longitude,
      })
    }
  },
)

const filteredLocations = computed(() => {
  if (props.filteredProducts && props.filteredProducts.length > 0) {
    const ids = new Set(props.filteredProducts.map((p) => String(p.id)))
    return props.data?.filter((loc) => ids.has(String(loc.id)))
  }

  if (!userLocation.value || !userLocation.value.coords) return []
  lat.value = userLocation.value.coords.latitude
  lng.value = userLocation.value.coords.longitude
  return props.data?.filter(
    (loc) => getDistanceKm(lat.value, lng.value, loc?.latitude, loc.longitude) <= 30,
  )
})

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const gmap = shallowRef<any>(null)

function refreshMarkers() {
  if (!gmap.value) return
  if (userLocation.value && userLocation.value.coords) {
    lat.value = userLocation.value.coords.latitude
    lng.value = userLocation.value.coords.longitude
    setUserMarkerAndCenter(gmap.value, userMarker, lat.value, lng.value)
  }
  addLocationMarkersWithClick(
    gmap.value,
    locationMarkers,
    filteredLocations.value ?? [],
    scrollToProduct,
  )
}

const loadGoogleMapsApi = async () => {
  if (!window.google || !window.google.maps) {
    await new Promise((resolve, reject) => {
      const script = document.createElement('script')
      script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=marker&v=weekly`
      script.async = true
      script.onload = () => resolve(null)
      script.onerror = () => reject(new Error('Google Maps failed to load'))
      document.head.appendChild(script)
    })
  }
}
const DEFAULT_CENTER = { lat: lat.value, lng: lng.value }
const createMapInstance = () => {
  if (!map.value) return null
  const GoogleMap = window.google.maps.Map
  const mapInstance = new GoogleMap(map.value, {
    center: DEFAULT_CENTER,
    zoom: 6,
    mapId: import.meta.env.VITE_GOOGLE_MAPS_MAP_ID || 'DEMO_MAP_ID',
    mapTypeControl: false,
    streetViewControl: false,
    fullscreenControl: false,
  })
  return mapInstance
}

function scrollToProduct(productIdOrIndex: string | number) {
  const el = document.getElementById(productIdOrIndex.toString())
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'center' })
    el.classList.add('ring-2', 'ring-blue-400')
    setTimeout(() => el.classList.remove('ring-2', 'ring-blue-400'), 1200)
  }
}

function addLocationMarkersWithClick(
  mapInstance: google.maps.Map | null,
  markersRef: { value: MarkerWithId[] },
  locations: Location[],
  onMarkerClick: (id: string | number) => void,
) {
  if (!window.google || !window.google.maps) return
  markersRef.value.forEach((m) => { m.map = null })
  markersRef.value = []
  locations.forEach((loc) => {
    const marker = new window.google.maps.marker.AdvancedMarkerElement({
      position: { lat: loc.latitude, lng: loc.longitude },
      map: mapInstance ?? undefined,
      title: loc.title || loc.name || 'Nearby Location',
    }) as MarkerWithId
    marker.parkId = loc.id
    marker.addListener('gmp-click', () => {
      onMarkerClick('product-card-' + loc.id)
    })
    markersRef.value.push(marker)
  })
}

const initializeMap = async () => {
  await loadGoogleMapsApi()
  gmap.value = createMapInstance()
  refreshMarkers()
}

onMounted(() => {
  initializeMap()
})

watch(
  [() => props.filteredProducts, userLocation, () => props.data],
  () => {
    if (!gmap.value) return
    refreshMarkers()
  },
  { deep: true, immediate: false },
)

const { cleanup } = useMapWindowEvents(map, gmap, locationMarkers)
onUnmounted(cleanup)
</script>
