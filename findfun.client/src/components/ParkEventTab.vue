<script setup lang="ts">
import { useParksStore } from '../stores/data'
import { storeToRefs } from 'pinia'
import GoogleMap from '@/components/GoogleMap.vue'
import { ParkTabs, EventTabs } from '@/config/TabsConfig'
import { ViewType } from '@/config/Enums'
import type { Events, Park } from '@/types/park'
import ProgressSpinner from 'primevue/progressspinner'

const props = defineProps<{
  type: ViewType
  title?: string
}>()

const parksStore = useParksStore()
const { parks, events, isLoading } = storeToRefs(parksStore) as {
  parks: Ref<Park[]>
  events: Ref<Events[]>
  isLoading: Ref<boolean>
}
const isPark = computed(() => props.type === ViewType.Park)

const tabs = computed(() => {
  return isPark.value ? ParkTabs : EventTabs
})
const getMapType = () => {
  return isPark.value ? parks.value : events.value
}
const length = computed(() => (isPark.value ? parks.value.length : events.value.length))
</script>

<template>
  <section class="flex flex-col">
    <div class="p-8">
      <div v-if="isLoading" class="card flex justify-center">
        <ProgressSpinner
          class="w-12 h-12"
          strokeWidth="8"
          fill="transparent"
          animationDuration=".5s"
          aria-label="Loading"
        />
      </div>
      <h1 class="text-3xl font-bold mb-4">
        {{ isPark ? 'Parks' : 'Events' }}
      </h1>
      <p>
        {{
          isPark
            ? 'Explore all parks and their features here.'
            : 'Discover upcoming and past events.'
        }}
      </p>
      <div v-if="length === 0 && !isLoading" class="text-center text-lg text-gray-500 my-12">
        <span class="pi pi-info-circle text-2xl mr-2 mb-4"></span>
        {{ isPark ? 'No parks available at the moment.' : 'No events available at the moment.' }}
      </div>

      <TabView v-else :scrollable="true">
        <TabPanel v-for="tab in tabs" :key="tab.label" :header="tab.label" :value="tab.slot">
          <component
            :is="tab.component"
            :key="`${tab.label}-${tab.slot}`"
            :title="type.toLocaleLowerCase()"
          />
        </TabPanel>
      </TabView>

      <GoogleMap
        :data="getMapType()"
        :filteredProducts="getMapType()"
        :key="isPark ? ViewType.Park : ViewType.Event"
      />
    </div>
  </section>
</template>
