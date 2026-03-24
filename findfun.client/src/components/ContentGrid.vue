<script setup lang="ts">
import { computed } from 'vue'
import { useSearchFilter } from '@/composables/useSearchFilter'
import { storeToRefs } from 'pinia'
import { useParksStore } from '@/stores/data'
import { useSearchStore } from '@/stores/search'
import Calendar from 'primevue/calendar'
import router from '@/router'
import { useBackNavigationRestore } from '@/composables/useBackNavigationRestore'
import { RoutePaths, Routes, ViewType } from '@/config/Enums'

const props = withDefaults(
  defineProps<{
    viewMoreRoute?: string
    title?: string
    hasViewMore?: boolean
  }>(),
  { hasViewMore: false },
)

const searchStore = useSearchStore()
const { searchTerm } = storeToRefs(searchStore)
const parksStore = useParksStore()
const isViewingEvents = computed(() => props.title === ViewType.Event)
const { parks, events, totalPages, eventsTotalPages } = storeToRefs(parksStore)


const sortKey = ref<any>(null) 
const sortOrder = ref<1 | -1>(1)
const selectedFilters = ref<any>(null)
const layout = ref('grid')
const options = ref([
  { icon: 'pi pi-th-large', value: 'grid' },
  { icon: 'pi pi-list', value: 'list' },
])

const dateRange = ref<Array<Date | null>>([new Date()])
const upcomingOnly = ref(false)
const ongoingOnly = ref(false)

const { page, nextPage: goToNextPage, prevPage: goToPrevPage } = useSearchFilter({
  isViewingEvents,
  searchTerm,
  sortKey,
  sortOrder,
  selectedFilters,
  dateRange,
  upcomingOnly,
  ongoingOnly,
  pageSize: 6,
})

const paginatedProducts = computed(() => isViewingEvents.value ? events.value : parks.value)
const totalPagesRef = computed(() => isViewingEvents.value ? eventsTotalPages.value : totalPages.value)

const nextPage = () => {
  goToNextPage(totalPagesRef.value)
}

const prevPage = () => {
  goToPrevPage()
}

const onSortChange = (field: string) => {
  sortKey.value = field
}

const sortOptions = [
  { label: 'Ascending', value: 'asc' },
  { label: 'Descending', value: 'desc' }, 
]

const scrollToMapMarker = (parkId: string) => {
  window.scrollToMapMarker?.(String(parkId))
}

const { save } = useBackNavigationRestore('lastProductId')

const goToDetail = (parkId: string) => {
  save(String(parkId))
  const pagaToGo = props.title === ViewType.Park ? Routes.ParkDetail : Routes.EventDetail
  router.push({ name: pagaToGo, params: { id: parkId } })
}

</script>

<template>
  <ScrollTop
    target="window"
    :threshold="100"
    icon="pi pi-arrow-up"
    :buttonProps="{
      severity: 'contrast',
      raised: true,
      rounded: true,
      size: 'small',
      style: 'margin-bottom: 2rem;',
    }"
  />
  <div
    class="card border-surface-200 dark:border-surface-700 rounded-xl m-2 mt-0 p-4 shadow-md transform scale-100 transition-transform duration-300"
  >
    <DataView
      :value="paginatedProducts"
      :layout="layout"
    >
      <template #header>
        <div class="flex flex-wrap gap-4 items-center justify-between w-full">
          <SectionHeader
            :title="title"
            :sortKey="sortKey"
            :sortOptions="sortOptions"
            :sortOrder="sortOrder"
            :layout="layout"
            :layoutOptions="options.map((o: any) => o.value?.toString() || '')"
            @update:sortKey="(val: any) => sortKey = val"
            @update:layout="(val: string) => layout = val"
            @sort-change="(val: any) => onSortChange(val)"
            @update:sortOrder="(val: any) => sortOrder = val"
            @update:selectedCategories="(val: any) => selectedFilters = val?.[0] || null"
          />
        </div>
        <!-- Date range filter for events -->
        <div v-if="isViewingEvents" class="flex flex-wrap gap-3 mt-4 p-3 bg-surface-50 dark:bg-surface-800 rounded items-end">
          <div class="flex flex-col gap-2">
            <label for="dateRange" class="text-xs font-semibold">Event Date Range</label>
            <Calendar 
              id="dateRange"
              v-model="dateRange" 
              selection-mode="range" 
              :show-time="false"
              date-format="yy-mm-dd"
              input-id="dateRange"
              :min-date="new Date()"
              class="w-full"
              @update:modelValue="(val: any) => { dateRange = val }"
            />
          </div>
          <div class="flex gap-2">
            <label for="upcomingOnly" class="flex items-center gap-2 cursor-pointer">
              <input id="upcomingOnly" v-model="upcomingOnly" type="checkbox" class="rounded" />
              <span class="text-sm">Upcoming Only</span>
            </label>
          </div>
          <div class="flex gap-2">
            <label for="ongoingOnly" class="flex items-center gap-2 cursor-pointer">
              <input id="ongoingOnly" v-model="ongoingOnly" type="checkbox" class="rounded" />
              <span class="text-sm">Ongoing Only</span>
            </label>
          </div>
        </div>
      </template>
      <template #list="slotProps">
        <div class="flex flex-col">
          <div v-for="(item, index) in slotProps.items" :key="index">
            <Card
              :item="item"
              :layout="'list'"
              :index="index"
              @scroll-to-marker="scrollToMapMarker(item.id)"
              @go-to-detail="goToDetail(item.id)"
              :type="title === ViewType.Park ? RoutePaths.Parks : RoutePaths.Events"
            />
          </div>
        </div>
      </template>

      <template #grid="slotProps">
        <div class="grid grid-cols-12 gap-4">
          <div
            v-for="(item, index) in slotProps.items"
            :key="index"
            class="col-span-12 sm:col-span-6 md:col-span-6 xl:col-span-6 p-2"
          >
            <Card
              :item="item"
              :layout="'grid'"
              :index="index"
              @scroll-to-marker="scrollToMapMarker(item.id)"
              @go-to-detail="goToDetail(item.id)"
              :type="title === ViewType.Park ? RoutePaths.Parks : RoutePaths.Events"
            />
          </div>
        </div>
      </template>
    </DataView>
    <div v-if="totalPagesRef > 1" class="flex gap-2 mt-4 justify-center items-center">
      <Button
        :disabled="page === 1"
        icon="pi pi-chevron-left"
        label="Prev"
        @click="prevPage"
        outlined
        size="small"
      />
      <span class="text-xs text-surface-500 dark:text-surface-400"
        >Page {{ page }} / {{ totalPagesRef }}</span
      >
      <Button
        v-if="page < totalPagesRef"
        :disabled="false"
        icon="pi pi-chevron-right"
        label="Next"
        @click="nextPage"
        outlined
        size="small"
      />
      <RouterLink
        v-else-if="hasViewMore"
        :to="viewMoreRoute || RoutePaths.Events"
        custom
        v-slot="{ navigate, href }"
      >
        <Button
          :disabled="false"
          icon="pi pi-arrow-right"
          label="View More"
          :href="href"
          @click="navigate"
          severity="info"
          outlined
          size="small"
        />
      </RouterLink>
    </div>
  </div>
  <NewCard />
</template>
