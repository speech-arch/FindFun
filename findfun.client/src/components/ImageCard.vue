<template>
  <div class="relative mx-auto">
    <img
      class="rounded w-full"
      :src="src"
      :alt="alt"
      :style="styleValue"
      loading="lazy"
      decoding="async"
      @click="$emit('go-to-detail')"
    />
    <div class="absolute bg-black/70 rounded-border left-1 top-1">
      <StatusTag :status="statusText" :tag-style="tagStyle" />
    </div>
    <slot />
  </div>
</template>

<script setup lang="ts">
import { useProductImage } from '../composables/useImage'

const props = defineProps<{
  src: string
  alt: string
  status: string
  imgStyle?: string
  tagStyle?: string
}>()
defineEmits(['scroll-to-marker', 'go-to-detail'])
const { statusText, styleValue } = useProductImage(props)
</script>
