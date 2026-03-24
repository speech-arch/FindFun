<script setup lang="ts">
import type { Ref } from 'vue'

const isDark = inject<Ref<boolean>>('isDark')!

const toggleColorMode = () => {
  isDark.value = !isDark.value
  localStorage.setItem('color-mode', isDark.value ? 'dark' : 'light')
}
</script>

<template>
  <Button
    :icon="isDark ? 'pi pi-sun' : 'pi pi-moon'"
    class="p-button-rounded-full p-button-text w-10 h-10 flex items-center justify-center object-cover aspect-square"
    aria-label="Toggle color mode"
    @click="toggleColorMode"
  />
</template>
