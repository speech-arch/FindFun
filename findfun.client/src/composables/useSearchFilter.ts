import { ref, watch, toRaw, type Ref } from 'vue'
import { useParksStore, type GetParksParams, type GetEventsParams } from '@/stores/data'

const parksSortFieldMap: Record<string, 'name' | 'location' | 'municipality' | 'province' | 'rating'> = {
  name: 'name',
  location: 'location',
  municipality: 'municipality',
  province: 'province',
  rating: 'rating',
}

const eventsSortFieldMap: Record<string, 'title' | 'starttime' | 'endtime' | 'location'> = {
  title: 'title',
  name: 'title',
  location: 'location',
  starttime: 'starttime',
  endtime: 'endtime',
}

function addSortToParams(
  params: GetParksParams | GetEventsParams,
  sortKey?: string | null | { label?: string; value?: string },
  sortOrder?: 1 | -1,
  isEvent = false,
) {
  let extracted: unknown = sortKey

  if (extracted && typeof extracted === 'object' && 'value' in extracted && !Array.isArray(extracted)) {
    extracted = (extracted as { value?: unknown }).value
  }

  if (!extracted || typeof extracted !== 'string') return

  const map = isEvent ? eventsSortFieldMap : parksSortFieldMap
  const mapped = map[extracted.toLowerCase().trim() as keyof typeof map]
  console.log('Mapped sort key:', { extracted, mapped, sortKey, sortOrder })
  if (mapped) {
    params.sortBy = mapped
    params.sortDirection = sortOrder === 1 ? 'asc' : 'desc'
  }
}

function addCascadeFilterToParams(params: GetParksParams | GetEventsParams, filterData: Any) {
  if (!filterData || typeof filterData !== 'object') return

  const value = toRaw(filterData).value
  console.log('Processing cascade filter:', { filterData, value })
  if (!value || typeof value !== 'object') return

  switch (value.type) {
    case 'parkType':
      ;(params as GetParksParams).parkType = value.parkType
      break
    case 'distance':
      ;(params as GetParksParams).radiusKm = value.distance
      break
    case 'rating':
      ;(params as GetParksParams).sortBy = 'rating'
      break
    case 'amenities':
      if (Array.isArray(value.amenities) && value.amenities.length > 0) {
        ;(params as GetParksParams).amenities = value.amenities
      }
      break
  }
}

interface UseSearchFilterOptions {
  isViewingEvents: Ref<boolean>
  searchTerm: Ref<string>
  sortKey: Ref<any>
  sortOrder: Ref<1 | -1>
  selectedFilters: Ref<any>
  dateRange?: Ref<Array<Date | null>>
  upcomingOnly?: Ref<boolean>
  ongoingOnly?: Ref<boolean>
  pageSize?: number
}

export function useSearchFilter(options: UseSearchFilterOptions) {
  const {
    isViewingEvents,
    searchTerm,
    sortKey,
    sortOrder,
    selectedFilters,
    dateRange,
    upcomingOnly,
    ongoingOnly,
    pageSize = 6,
  } = options

  const page = ref(1)

  // Trigger API call when filters, sort, or pagination changes
  const applyFiltersAndSort = async () => {
    // Build params from reactive sources and delegate to shared executor
    if (isViewingEvents.value) {
      const params: GetEventsParams = {
        page: page.value,
        pageSize,
      }

      if (searchTerm.value.trim()) params.search = searchTerm.value.trim()

      addSortToParams(params, sortKey.value, sortOrder.value, true)
      if (dateRange?.value?.[0]) params.startDate = dateRange.value[0].toISOString().split('T')[0]
      if (dateRange?.value?.[1]) params.endDate = dateRange.value[1].toISOString().split('T')[0]
      if (upcomingOnly?.value) params.upcomingOnly = true
      if (ongoingOnly?.value) params.ongoingOnly = true

      await executeSearch(true, params)
    } else {
      const params: GetParksParams = {
        page: page.value,
        pageSize,
      }

      if (searchTerm.value.trim()) params.search = searchTerm.value.trim()

      const rawFilter = selectedFilters.value ? toRaw(selectedFilters.value) : null
      const rawSort = sortKey.value ? toRaw(sortKey.value) : null
      const isPlainField = rawFilter?.value && parksSortFieldMap[String(rawFilter.value)]
      const effectiveSortKey = isPlainField ? String(rawFilter.value) : rawSort ? 'name' : null
      const effectiveSortOrder: 1 | -1 = rawSort?.value === 'desc' ? -1 : 1

      if (effectiveSortKey) addSortToParams(params, effectiveSortKey, effectiveSortOrder, false)
      if (rawFilter && !isPlainField) addCascadeFilterToParams(params, selectedFilters.value)

      await executeSearch(false, params)
    }
  }

  watch(
    [
      sortKey,
      sortOrder,
      selectedFilters,
      ...(dateRange ? [dateRange] : []),
      ...(upcomingOnly ? [upcomingOnly] : []),
      ...(ongoingOnly ? [ongoingOnly] : []),
    ],
    () => {
      page.value = 1
      applyFiltersAndSort()
    },
    { deep: true },
  )

  watch(page, () => {
    applyFiltersAndSort()
  })

  const nextPage = (totalPages: number) => {
    if (page.value < totalPages) {
      page.value++
    }
  }

  const prevPage = () => {
    if (page.value > 1) {
      page.value--
    }
  }

  return {
    page,
    applyFiltersAndSort,
    nextPage,
    prevPage,
  }
}

export interface TriggerSearchOptions {
  isViewingEvents?: boolean
  sortKey?: string | null
  sortOrder?: 1 | -1
  pageSize?: number
}

export function triggerSearch(searchTerm: string, opts: TriggerSearchOptions = {}) {
  const { isViewingEvents = false, sortKey, sortOrder, pageSize = 6 } = opts
  const params: GetEventsParams | GetParksParams = isViewingEvents
    ? ({ page: 1, pageSize, search: searchTerm.trim() || '' } as GetEventsParams)
    : ({ page: 1, pageSize, search: searchTerm.trim() || '' } as GetParksParams)

  addSortToParams(params, sortKey ?? null, sortOrder ?? 1, isViewingEvents)
  executeSearch(isViewingEvents, params)
}

export async function executeSearch(
  isViewingEvents: boolean,
  params: GetParksParams | GetEventsParams,
) {
  const parksStore = useParksStore()
  if (isViewingEvents) {
    await parksStore.fetchEvents(params as GetEventsParams)
  } else {
    await parksStore.fetchParks(params as GetParksParams)
  }
}
