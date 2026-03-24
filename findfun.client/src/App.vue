<template>
  <div :class="[{ 'my-app-dark': isDark }, 'min-h-screen bg-white dark:bg-black dark:text-white']">
    <a
      href="#main-content"
      class="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:bg-white focus:text-blue-600 focus:px-3 focus:py-2 focus:rounded"
      @click.prevent="skipToContent"
      >Skip to main content</a
    >
    <DesktopNavigationBar />
    <MobileNavigationBar />
    <main
      id="main-content"
      class="pt-20 pb-20 bg-gray-100 dark:bg-gray-800 rounded shadow w-full min-h-screen flex flex-col max-w-none mx-0"
    >
      <RouterView />
    </main>
    <footer
      class="w-full py-4 px-6 bg-white dark:bg-black border-t border-surface-200 dark:border-surface-700 text-center text-sm text-gray-500 dark:text-gray-400 hidden sm:block sticky bottom-0 left-0"
    >
      © {{ new Date().getFullYear() }} Swings & Slides Parks App. All rights reserved.
    </footer>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, provide } from 'vue'
import { useUserLocationStore } from './stores/userLocation'
import { useServiceWorker } from './composables/useServiceWorker'

const userLocationStore = useUserLocationStore()

// Initialize service worker (PWA support)
useServiceWorker()

const items = ref([
  {
    label: 'Home',
    icon: 'pi pi-home',
  },
  {
    label: 'Parks',
    icon: 'pi pi-image',
  },
  {
    label: 'Events',
    icon: 'pi pi-calendar',
  },
  {
    label: 'places',
    icon: 'pi pi-map-marker',
  },
  {
    label: 'Create',
    icon: 'pi pi-plus',
  },
  {
    label: 'About',
    icon: 'pi pi-info-circle',
  },
])

const isMobile = ref(window.innerWidth < 640)

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
  // Initialize dark-mode selector for PrimeVue theme
  const stored = localStorage.getItem('color-mode')
  const prefers = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)')
  isDark.value = stored === 'dark' || (stored === null && prefers && prefers.matches)
  if (prefers && prefers.addEventListener) {
    const handler = (e: MediaQueryListEvent) => {
      // only change when no explicit user preference
      if (!localStorage.getItem('color-mode')) {
        isDark.value = e.matches
      }
    }
    prefers.addEventListener('change', handler)
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', updateIsMobile)
})

provide('items', items)
provide('isMobile', isMobile)
</script>
