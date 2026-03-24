<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useParksStore } from '@/stores/data'
import type { Events, Park } from '@/types/park'
import { Routes } from '@/config/Enums'

const route = useRoute()
const parksStore = useParksStore()

const id = computed(() => (route.params.id as string) || '')
const name = computed(() => (route.name as string) || '')
const isPark = computed(() => name.value === Routes.ParkDetail)

watch(
  [id, isPark],
  async ([newId, newIsPark]) => {
    if (!newId) return
    if (newIsPark) {
      await parksStore.fetchParkById(newId)
    } else {
      await parksStore.fetchEventById(newId)
    }
  },
  { immediate: true },
)

const park = computed<Park>(() => {
  const found = isPark.value
    ? parksStore.parks.find((p: Park) => p.id === id.value)
    : parksStore.events.find((p: Events) => p.id === id.value)
  return (found as Park) ?? ({} as Park)
})

const images = ref([])
const relatedParks = ref([])
</script>

<template>
  <EventParkDetails
    :parkInfo="park"
    :images="images"
    :relatedParks="relatedParks"
    :isPark="isPark"
  />
</template>
