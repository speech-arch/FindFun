import type { CascadeSelectChangeEvent } from 'primevue';
import { computed, ref, type Ref } from 'vue'

export interface SectionHeaderOptions {
  sortKeyProxy?: Ref<
    string | number | { field: string; order: string } | { label: string; value: unknown } | null
  >
  layoutProxy?: Ref<string>
  categoryProxy?: Ref<string[]>
  onSortChange?: (event: unknown) => void
  isMobile?: Ref<boolean>
  showFilter?: Ref<boolean>
  showSort?: Ref<boolean>
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function useSectionHeader(props: any, emit: any, options: SectionHeaderOptions = {}) {
  const sortKeyProxy =
    options.sortKeyProxy ||
    computed({
      get: () => props.sortKey ?? null,
      set: (
        val:
          | string
          | number
          | { field: string; order: string }
          | { label: string; value: unknown }
          | null,
      ) => emit('update:sortKey', val),
    })
  const layoutProxy =
    options.layoutProxy ||
    computed({
      get: () => props.layout ?? 'grid',
      set: (val: string) => emit('update:layout', val),
    })
  const categoryProxy =
    options.categoryProxy ||
    computed({
      get: () => props.selectedCategories ?? [],
      set: (val: string[]) => emit('update:selectedCategories', val),
    })
  const onSortChange = options.onSortChange || ((event: CascadeSelectChangeEvent) => emit('sort-change', event))

  const windowWidth = ref(window.innerWidth)

  const isMobile = options.isMobile || computed(() => windowWidth.value < 640)

  function updateWidth() {
    windowWidth.value = window.innerWidth
  }

  onMounted(() => {
    window.addEventListener('resize', updateWidth)
  })

  onUnmounted(() => {
    window.removeEventListener('resize', updateWidth)
  })

  const showFilter = options.showFilter || ref(false)
  const showSort = options.showSort || ref(false)

  return {
    sortKeyProxy,
    layoutProxy,
    categoryProxy,
    onSortChange,
    isMobile,
    showFilter,
    showSort,
  }
}
