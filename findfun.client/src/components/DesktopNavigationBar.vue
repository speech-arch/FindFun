<script setup lang="ts">
import { useUserLocationStore } from '../stores/userLocation'
import { computed, inject } from 'vue'

const isMobile = inject('isMobile')
const items = inject('items')
const userLocationStore = useUserLocationStore()

const fetchLocationFromIP = () => {
  userLocationStore.fetchUserLocationWithWatch()
}

const filteredItems = computed(() => {
  let rawItems = items
  if (rawItems && typeof rawItems === 'object' && 'value' in rawItems) {
    rawItems = rawItems.value
  }
  if (!Array.isArray(rawItems)) return []
  return rawItems.filter((item) => {
    const label = typeof item.label === 'string' ? item.label : ''
    return label?.toLowerCase() !== 'about'
  })
})
</script>

<template>
  <header v-if="!isMobile">
    <!-- Desktop Navigation -->
    <nav class="fixed top-0 left-0 right-0 z-50 w-full">
      <Menubar :model="filteredItems" class="card items-center hidden sm:flex w-full !rounded-none">
        <template #start>
          <Logo />
        </template>
        <template #item="{ item, root, hasSubmenu, props }">
          <RouterLink
            v-if="
              !item.items &&
              typeof item.label === 'string' &&
              item.label.toLowerCase() !== 'places' &&
              item.label.toLowerCase() !== 'create' &&
              item.label.toLowerCase() !== 'about'
            "
            :to="{ name: (item.label || '').toLowerCase() }"
            class="items-center hidden sm:flex px-2 py-1 rounded transition-colors hover:bg-blue-100 dark:hover:bg-blue-900 no-underline"
            active-class="bg-blue-600 text-white"
          >
            <span>{{ item.label }}</span>
            <Badge
              v-if="item.badge"
              :class="{ 'ml-auto': !root, 'ml-2': root }"
              :value="item.badge"
            />
            <span
              v-if="item.shortcut"
              class="ml-auto border border-surface rounded bg-emphasis text-muted-color text-xs p-1"
              >{{ item.shortcut }}</span
            >
            <i
              v-if="hasSubmenu"
              :class="[
                'pi pi-angle-down ml-auto',
                { 'pi-angle-down': root, 'pi-angle-right': !root },
              ]"
            />
          </RouterLink>
          <a
            v-else
            v-ripple
            class="flex items-center"
            v-bind="props.action"
            @click="typeof item.label === 'string' && item.label.toLowerCase() === 'places' ? fetchLocationFromIP() : null"
          >
            <span>{{ item.label }}</span>
            <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down" />
          </a>
        </template>
        <template #end>
          <NavBarUserSection
            avatar-class="w-10 h-10 rounded-full object-cover aspect-square ml-4 !rounded-none"
            color-mode-class=""
            search-class="w-full"
          />
        </template>
      </Menubar>
    </nav>
  </header>
</template>
