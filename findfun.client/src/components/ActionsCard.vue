<template>
  <div :class="wrapperClass">
    <div class="flex flex-col mt-2 mb-2 gap-1">
      <template v-if="title">
        <span class="font-bold text-lg text-black-600 mb-5">{{ title }}</span>
      </template>
      <div class="flex flex-wrap gap-2 text-sm">
        <template v-if="chips.length > 4">
          <Tag
            v-for="(chip, i) in chips.slice(0, 4)"
            :key="'chip' + i"
            :value="chip.label || chip"
            :severity="chip.severity || 'info'"
            :icon="chip.icon"
          />
          <Icon
            @click="$emit('go-to-detail')"
            class="text-lg text-blue-500 hover:text-blue-700 cursor-pointer"
            icon="mdi:dots-horizontal"
            width="28"
            height="28"
          />
        </template>
        <template v-else>
          <Tag
            v-for="(chip, i) in chips"
            :key="'chip' + i"
            :value="chip.label || chip"
            :severity="chip.severity || 'info'"
            :icon="chip.icon"
          />
        </template>
      </div>
    </div>
    <div class="flex justify-between items-center mt-2 text-xs text-black-600" v-if="showReviews">
      <span class="flex items-center gap-1 mr-2">
        <span
          v-if="mainAirMetric"
          class="flex items-center gap-1 px-2 py-1 rounded w-fit text-[10px]"
          :style="{ background: mainAirMetric.color + '22' }"
        >
          <Icon
            :icon="mainAirMetric.icon"
            :style="{ color: mainAirMetric.color }"
            class="w-4 h-4 sm:w-4 sm:h-4 md:w-4 md:h-4"
          />
          <span class="text-[10px]">
            {{ (mainAirMetric.name.split('_')[1] || mainAirMetric.name).toUpperCase() }}:
            <b>{{ mainAirMetric.level }}</b>
          </span>
        </span>
        <span class="flex items-center gap-1 px-1 py-0.5 rounded w-fit text-xs">
          <Icon
            :icon="weatherIconClass ?? ''"
            :style="{ color: getWeatherIconColor?.(weatherIconClass) }"
            class="w-4 h-4 sm:w-4 sm:h-4 md:w-4 md:h-4"
          />
          <span class="text-[10px]"> {{ park.weather.temperature }} °C now</span>
        </span>
      </span>
      <span class="flex items-center gap-1">
        <UserRateing :rating="Number(park.rating)" star-size="h-4 w-4" />
        ({{ park.reviewCount }})
      </span>
    </div>
    <div v-if="props.showButtons !== false" :class="buttonWrapperClass">
      <Button
        icon="pi pi-share-alt"
        outlined
        v-if="reverse"
        @click="
          share({
            title: props.park.name || props.title || 'Park',
            text: props.park.description || 'Check out this park!',
            url: props.park.url,
          })
        "
      />
      <Button
        icon="pi pi-map-marker"
        label="Map"
        @click="$emit('scroll-to-marker')"
        outlined
        severity="info"
      />
      <Button
        icon="pi pi-eye"
        label="View"
        :disabled="outOfStock"
        :class="cartClass"
        @click="$emit('go-to-detail')"
      />
      <Button
        icon="pi pi-share-alt"
        outlined
        v-if="!reverse"
        @click="
          share({
            title: props.park.name || props.title || 'Park',
            text: props.park.description || 'Check out this park!',
            url: props.park.url,
          })
        "
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { WeatherInfo } from '@/types/park'
import { useProductActions } from '../composables/useActions'
import type { DefineComponent } from 'vue'
import { Icon } from '@iconify/vue'
import { useShare } from '../composables/useShare'
declare const Button: DefineComponent

const props = defineProps<{
  price?: number | string | undefined
  outOfStock?: boolean
  layout?: string
  chipsFn?: () => any[]
  chipsAreaWrapperClass?: string
  title?: string
  showButtons?: boolean
  showToggleButton?: boolean
  showReviews?: boolean
  park: {
    weather: WeatherInfo
    rating: number | string
    reviewCount: number
    title?: string
    name?: string
    description?: string
    url?: string
    image?: string
  }
  weatherIconClass?: string | undefined
  mainAirMetric?: any
  getWeatherIconColor?: (icon: string | undefined) => string
}>()

defineEmits(['scroll-to-marker', 'go-to-detail'])
const {
  reverse,
  wrapperClass,
  buttonWrapperClass,
  cartClass,
  chips: defaultChips,
} = useProductActions({ 
  price: Number(props.price) || 0, 
  outOfStock: props.outOfStock || false, 
  layout: props.layout 
})
const { share } = useShare()

const chips = props.chipsFn ? props.chipsFn() : defaultChips
</script>
