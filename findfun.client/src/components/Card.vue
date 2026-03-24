<template>
  <div
    v-if="layout === 'list'"
    :id="`product-card-${item.id}`"
    class="flex flex-col sm:flex-row sm:items-center p-6 gap-4"
    :class="{ 'border-t border-surface-200 dark:border-surface-700': index !== 0 }"
  >
    <div class="md:w- relative">
      <ImageCard
        :src="
          item.parkImages[0]?.replace('localhost', 'localhost:5163') ||
          'https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=300&q=75'
        "
        :alt="item.name"
        :status="item.parkType"
        imgStyle="max-width: 300px; max-height: 300px; width: 100%; height: auto; object-fit: cover;"
        @go-to-detail="$emit('go-to-detail', item.id)"
      />
      <div
        class="absolute left-0 bottom-0 bg-black/60 text-white p-3 z-10 max-w-[300px] max-h-[300px] w-full h-auto"
      >
        <div
          ref="nameRef"
          class="text-xl font-semibold break-words"
          :class="{ truncate: !showFullName }"
          @mouseenter="showFullName = !showFullName"
          @mouseleave="showFullName = !showFullName"
        >
          {{ item.name }}
        </div>
        <div
          ref="descRef"
          class="text-sm text-gray-200 break-words"
          :class="{ truncate: !showFullDesc }"
          @mouseenter="showFullDesc = !showFullDesc"
          @mouseleave="showFullDesc = !showFullDesc"
        >
          {{ item.description }}
        </div>
        <button
          v-if="isNameTruncated || isDescTruncated"
          class="block md:hidden mt-2 text-xs text-blue-200 underline"
          @click="((showFullName = !showFullName), (showFullDesc = !showFullDesc))"
        >
          {{ !showFullName || !showFullDesc ? 'show more' : 'show less' }}
        </button>
      </div>
    </div>
    <div class="flex flex-col md:flex-row justify-between md:items-center flex-1 gap-6">
      <div class="flex flex-col md:items-end gap-2 flex-1 self-stretch min-h-full">
        <ActionsCard
          :layout="layout"
          :chipsFn="
            () =>
              visibleAmenities.map((a) => ({
                label: a.name,
                image: '',
                icon: a.icon?.icon,
                removable: false,
              }))
          "
          @scroll-to-marker="$emit('scroll-to-marker', item.id)"
          @go-to-detail="$emit('go-to-detail', item.id)"
          title="Recommendations"
          :show-buttons="true"
          :show-reviews="true"
          :park="{
            weather: weather,
            rating: item?.averageRating?.toFixed(1),
            reviewCount: item?.reviews?.length,
            name: item?.name,
            description: item?.description,
            url: shareUrl,
          }"
          :weather-icon-class="weatherIconClass"
          :main-air-metric="mainAirMetric"
          :getWeatherIconColor="getWeatherIconColor"
        />
      </div>
    </div>
  </div>
  <div
    v-else
    :id="`product-card-${item.id}`"
    class="p-6 border border-surface-200 dark:border-surface-700 bg-surface-0 dark:bg-surface-900 flex flex-col"
  >
    <div class="bg-surface-50 flex justify-center rounded p-4 relative">
      <div class="relative mx-auto">
        <ImageCard
          :src="'https://images.unsplash.com/photo-1506744038136-46273834b3fb'"
          :alt="item.name"
          :status="item.locationName"
          imgStyle="max-width: 300px; max-height: 300px; width: 100%; height: auto; object-fit: cover;"
          @go-to-detail="$emit('go-to-detail', item.id)"
        />

        <div class="absolute left-0 bottom-0 w-full bg-black/60 text-white p-3 z-10">
          <div class="text-xl font-semibold">{{ item.name }}</div>
          <div class="text-sm text-gray-200">{{ item.description }}</div>
        </div>
      </div>
    </div>
    <div class="pt-6 gap-4 flex flex-col">
      <div class="flex flex-col gap-6 mt-2">
        <ActionsCard
          :layout="layout"
          :chipsFn="
            () =>
              visibleAmenities.map((a) => ({
                label: a.name,
                image: '',
                icon: a.icon?.icon,
                removable: false,
              }))
          "
          @scroll-to-marker="$emit('scroll-to-marker', item.id)"
          @go-to-detail="$emit('go-to-detail', item.id)"
          :show-buttons="true"
          :show-reviews="true"
          :park="{
            weather: weather,
            rating: item?.averageRating?.toFixed(1),
            reviewCount: item?.reviews?.length,
            name: item?.name,
            description: item?.description,
            url: shareUrl,
          }"
          :weather-icon-class="weatherIconClass"
          :main-air-metric="mainAirMetric"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Park } from '@/types/park'
import { useProductCard } from '../composables/useCard'
import ActionsCard from './ActionsCard.vue'
import { matchAmenities } from '@/composables/AmenitiesIcon'
import { useParkWeatherAirQuality } from '@/composables/useParkWeatherAirQuality'

const showFullName = ref(false)
const showFullDesc = ref(false)
const nameRef = ref<HTMLElement | null>(null)
const descRef = ref<HTMLElement | null>(null)

const isNameTruncated = ref(false)
const isDescTruncated = ref(false)

const checkTruncation = () => {
  nextTick(() => {
    if (nameRef.value) {
      isNameTruncated.value = nameRef.value.scrollWidth > nameRef.value.clientWidth
    }
    if (descRef.value) {
      isDescTruncated.value = descRef.value.scrollWidth > descRef.value.clientWidth
    }
  })
}

const props = defineProps<{
  item: Park
  layout?: string
  index?: number
  type?: string
}>()

const shareUrl = computed(() => {
  if (!props.item?.id) return ''

  const base = typeof window !== 'undefined' ? window.location.origin : ''
  return `${base}${props.type}/${props.item.id}`
})

watch(() => [props.item.description, props.item.name], checkTruncation, { immediate: true })

const {
  weather,
  weatherIconClass,
  mainAirMetric,
  getWeatherIconColor: _getWeatherIconColor,
} = useParkWeatherAirQuality(props.item)

const getWeatherIconColor = (icon: string | undefined) => _getWeatherIconColor(icon ?? '')
const allAmenities = computed(() => matchAmenities(props.item.amenities || []))
const visibleAmenities = computed(() => allAmenities.value.slice(0, 5))
const emit = defineEmits(['scroll-to-marker', 'go-to-detail'])
useProductCard(props, emit)
</script>
