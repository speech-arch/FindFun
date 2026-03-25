<!-- eslint-disable vue/multi-word-component-names -->
<template>
  <Card>
    <template #content>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 md:gap-x-12 overflow-x-auto max-w-full">
        <div
          class="flex-1 flex flex-col sm:items-start md:items-start lg:items-center gap-6 mb-4 min-w-[260px] min-h-[260px]"
        >
          <Galleria
            v-model:activeIndex="activeIndex"
            v-model:visible="displayCustom"
            :value="images"
            :responsiveOptions="responsiveOptions"
            :numVisible="7"
            containerStyle="w-full md:max-w-xl lg:max-w-2xl"
            :circular="true"
            :fullScreen="true"
            :showItemNavigators="true"
            :showThumbnails="false"
          >
            <template #item="slotProps">
              <img
                :src="slotProps.item.itemImageSrc.replace('localhost', 'localhost:5163')"
                :alt="slotProps.item.alt"
                class="max-w-full max-h-[500px] mx-auto block object-cover"
                loading="lazy"
                decoding="async"
              />
            </template>
            <template #thumbnail="slotProps">
              <img
                :src="slotProps.item.thumbnailImageSrc.replace('localhost', 'localhost:5163')"
                :alt="slotProps.item.alt"
                class="max-w-full max-h-[250px] mx-auto block object-cover"
                loading="lazy"
              />
            </template>
          </Galleria>
          <div v-if="images && images.length" class="grid grid-cols-6 gap-2 max-w-xs md:max-w-lg">
            <div v-for="(image, index) of images" :key="index">
              <img
                :src="image.itemImageSrc.replace('localhost', 'localhost:5163')"
                :alt="'park with pool testing only'"
                class="w-24 h-24 min-w-24 min-h-24 max-w-24 max-h-24 md:w-32 md:h-32 md:min-w-32 md:min-h-32 md:max-w-32 md:max-h-32 cursor-pointer rounded object-cover border border-gray-200 dark:border-gray-700 transition-transform duration-200 hover:scale-105"
                loading="lazy"
                @click="imageClick(index)"
              />
            </div>
          </div>
        </div>
        <!-- Park Info -->
        <div
          class="flex flex-col justify-between gap-6 min-w-[260px] min-h-[260px] md:px-6 lg:px-8"
        >
          <div class="md:px-6 lg:px-8">
            <h1 class="text-3xl font-bold mb-4">{{ parkInfo.name }}</h1>
            <div class="flex flex-col gap-2 text-gray-700 dark:text-gray-200 mt-4">
              <!-- Amenities Section -->
              <div v-if="visibleAmenities.length" class="flex flex-wrap gap-2 mt-2">
                <span
                  v-for="amenity in showMoreAmenities
                    ? [...visibleAmenities, ...hiddenAmenities]
                    : visibleAmenities"
                  :key="amenity.name"
                  class="flex items-center gap-1 px-2 py-1 rounded bg-gray-100 dark:bg-gray-800"
                >
                  <Icon
                    :icon="amenity.icon.icon"
                    :style="{ color: amenity.icon.color }"
                    class="w-5 h-5"
                  />
                  <span class="text-xs">{{ amenity.name }}</span>
                </span>
              </div>
              <Button
                v-if="hiddenAmenities.length"
                label="Amenities"
                :icon="showMoreAmenities ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
                size="small"
                outlined
                class="mt-1 w-fit"
                @click="showMoreAmenities = !showMoreAmenities"
                severity="contrast"
              />
              <div class="flex flex-col gap-1 mt-2">
                <div class="flex flex-col gap-2 mt-2">
                  <span class="text-blue-500 dark:text-blue-300 flex items-center gap-1">
                    <Icon icon="mdi:map-marker" :style="{ color: 'green' }" class="w-6 h-6" />
                    <span> Location: {{ parkInfo.locationName }}, {{ parkInfo.street }} </span>
                  </span>
                </div>
                <div v-if="airQualityData.length" class="flex flex-col gap-2 mt-2">
                  <span
                    v-if="mainAirMetric"
                    class="flex items-center gap-1 px-2 py-1 rounded w-fit"
                    :style="{ background: mainAirMetric.color + '22' }"
                  >
                    <Icon
                      :icon="mainAirMetric.icon"
                      :style="{ color: mainAirMetric.color }"
                      class="w-5 h-5"
                    />
                    <span class="text-xs"
                      >{{ mainAirMetric.name.replace('_', ' ').toUpperCase() }}:
                      <b>{{ mainAirMetric.level }}</b></span
                    >
                  </span>
                  <div v-if="showMoreAirMetrics" class="flex flex-wrap gap-2 mt-2">
                    <span
                      v-for="aq in otherAirMetrics"
                      :key="aq.name"
                      class="flex items-center gap-1 px-2 py-1 rounded"
                      :style="{ background: aq.color + '22' }"
                    >
                      <Icon :icon="aq.icon" :style="{ color: aq.color }" class="w-5 h-5" />
                      <span class="text-xs"
                        >{{ aq.name.replace('_', ' ').toUpperCase() }}: <b>{{ aq.level }}</b></span
                      >
                    </span>
                  </div>

                  <Button
                    v-if="otherAirMetrics.length"
                    label="Air quality"
                    size="small"
                    outlined
                    class="mt-1 w-fit"
                    @click="showMoreAirMetrics = !showMoreAirMetrics"
                    :icon="showMoreAirMetrics ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
                    severity="info"
                  />
                  <div class="flex items-center gap-2">
                    <span class="flex items-center gap-1">
                      <Icon
                        icon="mdi:weather-windy"
                        class="w-6 h-6"
                        :style="{ color: '#60a5fa' }"
                      />
                      <span>
                        <template v-if="weather?.weatherResponse?.current?.wind_speed_10m">
                          Wind: {{ weather.weatherResponse.current.wind_speed_10m }} km/h
                        </template>
                      </span>
                    </span>
                  </div>
                </div>

                <div class="flex items-center gap-2">
                  <span class="flex items-center gap-1">
                    <Icon
                      :icon="weatherIconClass"
                      :style="{ color: getWeatherIconColor(weatherIconClass) }"
                      class="w-6 h-6"
                    />
                    <span>
                      <template
                        v-if="weather?.temperature !== null && weather?.temperature !== undefined"
                      >
                        Temperature {{ weather.temperature }} °C now
                      </template>
                      <template v-else> Temperature: N/A </template>
                    </span>
                  </span>
                </div>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-gray-500 dark:text-gray-300 flex items-center gap-1">
                  <i class="pi pi-comments" />
                  <span>{{ displayedReviews.length }} reviews</span>
                </span>
                <UserRateing :rating="parkRating" star-size="h-5 w-5" />
                <span class="text-xs text-gray-400 dark:text-gray-500"
                  >({{ parkRating.toFixed(1) }})</span
                >
              </div>
            </div>
          </div>
          <div class="mt-6 flex gap-3 md:px-6 lg:px-8">
            <Button
              label="Add to Favorites"
              icon="pi pi-heart"
              outlined
              @click="$emit('add-favorite', parkInfo)"
            />
            <Button
              label="Plan Visit"
              icon="pi pi-calendar"
              @click="$emit('plan-visit', parkInfo)"
            />
          </div>
        </div>
      </div>
      <div class="mt-10 relative z-0">
        <TabView :scrollable="true">
          <TabPanel header="Description" :value="'description'">
            <div>{{ parkInfo.description }}</div>
          </TabPanel>
          <TabPanel header="Reviews" :value="'reviews'">
            <div
              v-if="displayedReviews && displayedReviews.length"
              class="w-full bg-white dark:bg-gray-900 rounded-t-xl shadow-md divide-y divide-gray-200 dark:divide-gray-700"
            >
              <div
                v-for="review in displayedReviews"
                :key="review.id"
                class="flex gap-4 py-6 px-4 first:pt-4 last:pb-2"
              >
                <img
                  :src="'https://i.pravatar.cc/40?u=' + review.userName"
                  :alt="review.userName"
                  class="h-10 w-10 rounded-full object-cover"
                />
                <div>
                  <h4 class="text-sm font-semibold text-gray-800 dark:text-gray-100">
                    {{ review.userName }}
                  </h4>
                  <p class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    {{ formatDate(review.createdAt) }}
                  </p>
                  <UserRateing :rating="review.rating" star-size="h-5 w-5" />
                  <p class="text-sm text-gray-700 dark:text-gray-200">{{ review.content }}</p>
                </div>
              </div>
            </div>
            <div v-else class="text-gray-400 dark:text-gray-500 text-sm">No reviews yet.</div>
            <hr class="border-t border-gray-200 dark:border-gray-700 my-4" />
            <ReviewForm @submit="onReviewFormSubmit" />
          </TabPanel>
          <TabPanel header="Map" :value="'map'">
            <GoogleMap :data="[parkInfo]" :filteredProducts="[parkInfo]" :is-details="true" />
          </TabPanel>
        </TabView>
        <!-- Related Parks -->
        <div class="relative z-10 mt-8">
          <div class="mt-16">
            <RelatedList
              :items="relatedParks || []"
              title="Same Parks Nearby"
              imageKey="image"
              nameKey="name"
              locationKey="location"
              ratingKey="rating"
              class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4"
              card-class="rounded-t-xl h-32 w-full object-cover"
              title-class="text-2xl font-semibold mb-4"
              :customCard="null"
            />
          </div>
        </div>
      </div>
      <div class="mb-6 mt-4">
        <Button label="Back" icon="pi pi-arrow-left" outlined class="mb-4" @click="goBack" />
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import Card from 'primevue/card'
import GoogleMap from '@/components/GoogleMap.vue'
import UserRateing from './UserRateing.vue'
import { Icon } from '@iconify/vue'
import { useParkWeatherAirQuality } from '@/composables/useParkWeatherAirQuality'
import type { Park, ParkImage, Review, ParkRelated } from '@/types/park'
import { matchAmenities } from '../composables/AmenitiesIcon'
import { useParksStore } from '@/stores/data'
import { useToast } from 'primevue/usetoast'
import { ref } from 'vue'

const props = defineProps<{
  parkInfo: Park
  images: ParkImage[]
  relatedParks?: ParkRelated[]
}>()

const allAmenities = computed(() => matchAmenities(props.parkInfo.amenities || []))
const visibleAmenities = computed(() => allAmenities.value.slice(0, 5))
const hiddenAmenities = computed(() => allAmenities.value.slice(5))
const showMoreAmenities = ref(false)

const {
  weather,
  weatherIconClass,
  airQualityData,
  mainAirMetric,
  otherAirMetrics,
  getWeatherIconColor,
} = useParkWeatherAirQuality(props.parkInfo)
const showMoreAirMetrics = ref(false)

const emit = defineEmits(['add-favorite', 'plan-visit', 'go-back', 'add-review'])
const activeIndex = ref(0)
const responsiveOptions = ref([
  { breakpoint: '1400px', numVisible: 7 },
  { breakpoint: '1200px', numVisible: 6 },
  { breakpoint: '1024px', numVisible: 5 },
  { breakpoint: '900px', numVisible: 4 },
  { breakpoint: '768px', numVisible: 3 },
  { breakpoint: '560px', numVisible: 2 },
  { breakpoint: '400px', numVisible: 2 },
])
const displayCustom = ref(false)

const parkRating = computed(() => props.parkInfo.averageRating ?? 0)

const goBack = () => {
  emit('go-back')
}

const imageClick = (index: number) => {
  activeIndex.value = index
  displayCustom.value = true
}

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

const parksStore = useParksStore()
const toast = useToast()

const displayedReviews = ref<Review[]>(props.parkInfo?.reviews ? [...props.parkInfo.reviews] : [])

watch(
  () => props.parkInfo?.reviews,
  (r) => {
    displayedReviews.value = r ? [...r] : []
  },
  { immediate: true },
)

async function onReviewFormSubmit(payload: { author: string; rating: number; comment: string; date?: string }) {
  const body = {
    userName: payload.author,
    content: payload.comment,
    rating: payload.rating,
    Id: props.parkInfo.id,
  }

  const res = await parksStore.createReview(props.parkInfo.id, body)
  if (res.error) {

    type ProblemDetails = { errors?: Record<string, string[]> }
    const orig = (res.error as unknown as { original?: { response?: { data?: ProblemDetails } } }).original
    const srvData = orig?.response?.data as ProblemDetails | undefined
    const idErrors = srvData?.errors?.Id
    if (idErrors && Array.isArray(idErrors)) {
      toast.add({
        severity: 'warn',
        summary: 'Validation',
        detail: `${idErrors.join('; ')} `,
      })
      return
    }
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to submit review' })
    return
  }

  await parksStore.refreshParkById(props.parkInfo.id)
  const updatedPark = parksStore.parks.find((p: Park) => p.id === props.parkInfo.id)
  if (updatedPark && Array.isArray(updatedPark.reviews)) {
    displayedReviews.value = [...updatedPark.reviews]
  }
  toast.add({ severity: 'success', summary: 'Review added', detail: 'Thanks for your feedback' })
}
</script>
<style scoped>
.yellow-stars .p-rating-icon.pi-star-fill {
  color: #facc15 !important;
}
</style>
