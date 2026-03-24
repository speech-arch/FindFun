<template>
  <div :class="['min-h-screen', isDark ? 'bg-black text-white' : 'bg-white text-black']">
    <a
      href="#main-content"
      class="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:bg-white focus:text-blue-600 focus:px-3 focus:py-2 focus:rounded"
      @click.prevent="skipToContent"
      >Skip to main content</a
    >
    <Toast
      :position="toastPosition"
      :pt="{
        root: 'w-[calc(100vw-2rem)] sm:w-96 max-w-[calc(100vw-2rem)]',
        message: 'shadow-lg',
      }"
    />
    <DesktopNavigationBar />
    <MobileNavigationBar />
    <main
      id="main-content"
      class="pt-20 pb-20 bg-gray-100 dark:bg-gray-800 rounded shadow w-full min-h-screen flex flex-col max-w-none mx-0"
    >
      <RouterView />
    </main>
    <footer
      class="w-full py-4 px-6 bg-black dark:bg-black border-t border-surface-200 dark:border-surface-700 text-center text-sm text-gray-500 dark:text-gray-400 hidden sm:block sticky bottom-0 left-0"
    >
      © {{ new Date().getFullYear() }} Swings & Slides Parks App. All rights reserved.
    </footer>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch, provide, computed } from 'vue'
import Toast from 'primevue/toast'
import { useUserLocationStore } from './stores/userLocation'
import { useServiceWorker } from './composables/useServiceWorker'

const userLocationStore = useUserLocationStore()

// Initialize service worker (PWA support)
useServiceWorker()

const isMobile = ref(window.innerWidth < 640)
const toastPosition = computed(() => (isMobile.value ? 'top-center' : 'top-right'))

const items = ref([
  { label: 'Home', icon: 'pi pi-home' },
  { label: 'Parks', icon: 'pi pi-image' },
  { label: 'Events', icon: 'pi pi-calendar' },
  { label: 'places', icon: 'pi pi-map-marker' },
  { label: 'Create', icon: 'pi pi-plus' },
  { label: 'About', icon: 'pi pi-info-circle' },
])

const stored = localStorage.getItem('color-mode')
const isDark = ref(stored === 'dark' || (stored === null && window.matchMedia('(prefers-color-scheme: dark)').matches))

watch(isDark, (dark) => {
  document.documentElement.classList.toggle('my-app-dark', dark)
  document.documentElement.classList.toggle('dark', dark)
  localStorage.setItem('color-mode', dark ? 'dark' : 'light')
}, { immediate: true })

function updateIsMobile() {
  isMobile.value = window.innerWidth < 640
}

function skipToContent() {
  const el = document.getElementById('main-content')
  if (el) {
    el.focus({ preventScroll: true })
  }
}

onMounted(() => {
  window.addEventListener('resize', updateIsMobile)
  userLocationStore.fetchUserLocationFromIP()

  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    if (localStorage.getItem('color-mode') === null) {
      isDark.value = e.matches
    }
  })
})

onUnmounted(() => {
  window.removeEventListener('resize', updateIsMobile)
})

provide('items', items)
provide('isMobile', isMobile)
provide('isDark', isDark)
</script>
