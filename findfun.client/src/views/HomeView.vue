<script setup lang="ts">
import TabView from 'primevue/tabview'
import TabPanel from 'primevue/tabpanel'
import { useParksStore } from '../stores/data'
import { storeToRefs } from 'pinia'
import type { Items } from '../types/park'
import { Routes } from '@/config/Enums'
import { ResponsiveOptions } from '@/config/ResponsiveConfig'
import { HomeTabConfig } from '@/config/TabsConfig'

const tabKeys = [Routes.Parks, Routes.Events]
const route = useRoute()
const router = useRouter()
const activeTabIndex = ref(0)
const parksStore = useParksStore()
const { parks, events } = storeToRefs(parksStore) as { parks: Ref<Items>; events: Ref<Items> }

const mapData = computed(() => {
  return activeTabIndex.value === 0 ? parks.value : events.value
})

const mapKey = computed(() => {
  return `${activeTabIndex.value}-${mapData.value.length}`
})

const responsiveOptions = ref(ResponsiveOptions)
const onTabChange = (idx: number) => {
  activeTabIndex.value = idx
}
const setTabFromQuery = () => {
  const tab = route.query.tab as Routes
  const idx = tabKeys.indexOf(tab)
  if (idx !== -1 && idx !== activeTabIndex.value) {
    activeTabIndex.value = idx
  }
}

onMounted(setTabFromQuery)
watch(() => route.query.tab, setTabFromQuery)

watch(activeTabIndex, (idx) => {
  router.replace({ query: { ...route.query, tab: tabKeys[idx] } })
})
</script>

<template>
  <section class="flex flex-col w-full min-h-screen gap-6">
    <MainCarousel :products="mapData" :responsiveOptions="responsiveOptions" :key="mapKey"/>
    <TabView
      :activeIndex="activeTabIndex"
      @activeIndexChange="onTabChange($event)"
      @update:activeIndex="onTabChange($event)"
    >
      <TabPanel v-for="tab in HomeTabConfig" :key="tab.type" :header="tab.type" :value="tab.type">
        <ContentGrid :viewMoreRoute="tab.route" :title="tab.type" hasViewMore />
      </TabPanel>
    </TabView>
    <div class="flex-1 w-full min-h-[450px]">
      <GoogleMap :data="mapData" :filteredProducts="mapData" :key="mapKey" />
    </div>
  </section>
</template>
