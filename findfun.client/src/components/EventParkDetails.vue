<template>
  <Details
    :back-to="backTo"
    :parkInfo="parkInfo"
    :images="
      parkInfo.parkImages
        ? parkInfo.parkImages.map((img: string) => ({
            itemImageSrc: img,
            thumbnailImageSrc: img,
            alt: '',
            title: '',
          }))
        : []
    "
    
    :relatedParks="relatedParks || []"
    :allowUserRating="true"
    :userInitialRating="userInitialRating"
    @add-favorite="onAddFavorite"
    @plan-visit="onPlanVisit"
    @go-back="goBack"
    @rate="onUserRate"
  />
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { ParkImage, ParkRelated, Park } from '@/types/park'
import { RoutePaths } from '@/config/Enums'

const props = withDefaults(
  defineProps<{
    parkInfo: Park
    images: ParkImage[]
    relatedParks?: ParkRelated[]
    isPark?: boolean
    backTo?: string
  }>(),
  { isPark: true },
)

const router = useRouter()

const userInitialRating = ref<number>(0)

const onAddFavorite = (park: Park) => {
  // handle add to favorites
}
const onPlanVisit = (park: Park) => {
  // handle plan visit
}
const goBack = () => {
  if (window.history.length > 1) {
    router.back()
  } else {
    const routePath = props.isPark ? RoutePaths.Parks : RoutePaths.Events
    router.push(routePath)
  }
}
const onUserRate = (rating: number) => {
  userInitialRating.value = rating
  // Optionally, send to API or update store
}
</script>
