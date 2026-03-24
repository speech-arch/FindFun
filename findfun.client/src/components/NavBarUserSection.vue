<script setup lang="ts">
import { ref } from 'vue'
import { useSearchStore } from '../stores/search'
import { triggerSearch } from '@/composables/useSearchFilter'
import Menu from 'primevue/menu'
import { useRouter } from 'vue-router'

defineProps<{
  avatarClass?: string
  colorModeClass?: string
  searchClass?: string
  wrapperClass?: string
}>()

const searchStore = useSearchStore()
const searchTermInput = ref('')
const menu = ref()
const router = useRouter()

function onSearchInputKey(e: KeyboardEvent) {
  if (e.key === 'Enter' || e.key === 'Search') {
    searchStore.searchTerm = searchTermInput.value
    triggerSearch(searchTermInput.value)
  }
}

function handleSearchInput(e: Event) {
  const value = (e.target as HTMLInputElement).value
  if (value === '') {
    searchStore.searchTerm = ''
    triggerSearch('')
  }
}
const menuItems = [
  {
    label: 'Login',
    icon: 'pi pi-sign-in',
    command: () => {
      router.push({ name: 'login' })
    },
  },
  {
    label: 'Signup',
    icon: 'pi pi-user-plus',
    command: () => {
      router.push({ name: 'signup' })
    },
  },
]

function showMenu(event: Event) {
  menu.value.toggle(event)
}
</script>

<template>
  <div class="flex items-center gap-2 w-full" :class="wrapperClass ?? ''">
    <FloatLabel variant="on">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          id="search"
          v-model="searchTermInput"
          type="search"
          :class="searchClass ?? 'w-full'"
          @keydown="onSearchInputKey"
          @input="handleSearchInput"
        />
      </IconField>
      <label for="search">Search</label>
    </FloatLabel>
    <ColorMode :class="colorModeClass" />
    <Avatar
      image="https://primefaces.org/cdn/primevue/images/avatar/amyelsner.png"
      shape="circle"
      :class="avatarClass ?? 'w-10 h-10 rounded-full object-cover aspect-square ml-4'"
      @click="showMenu"
      class="cursor-pointer"
    />
    <Menu ref="menu" :model="menuItems" :popup="true" class="w-40" />
  </div>
</template>
