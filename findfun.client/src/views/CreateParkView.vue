<template>
  <div class="flex flex-col items-center justify-center min-h-[60vh] p-8 gap-6">
    <h1 class="text-2xl font-bold mb-4 text-center">Add a {{ type }}</h1>
    <div class="mb-6">
      <TabView v-model:activeIndex="activeTabIndex" class="w-full">
        <TabPanel
          v-for="option in typeOptions"
          :key="option.value"
          :header="option.label"
          :value="option.value"
          @click="type = option.value"
        >
          <form @submit.prevent="handleSubmit" class="space-y-4 p-auto">
            <DynamicFields
              v-for="field in visibleFields"
              :key="field.model"
              :field="field"
              :value="form[field.model]"
              @update:value="form[field.model] = $event"
              :previews="field.model === 'image' ? imagePreviews : []"
              :listeners="field.listeners"
            />
            <Button
              type="submit"
              :label="isSubmitting ? 'Creating...' : 'Create'"
              :loading="isSubmitting"
              :disabled="isSubmitting"
              class="w-full p-button-primary mt-4"
            />
          </form>
        </TabPanel>
      </TabView>
    </div>
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import { useToast } from 'primevue/usetoast'
import { useLocationValidation } from '@/composables/useLocationValidation'
import { useImageUpload } from '@/composables/useImageUpload'
import { useFormFields } from '@/composables/useFormFields'
import { ViewType } from '@/config/Enums'
import { useParksStore } from '@/stores/data'

interface ScheduleEntry {
  days: string[]
  openingTime: Date | null
  closingTime: Date | null
}

interface AmenityBlock {
  items: string[]
  note?: string
}

interface AmenitiesValue {
  hasAmenities?: boolean
  amenities?: AmenityBlock[]
}

interface ClosingScheduleData {
  venunCloses: boolean
  schedules: ScheduleEntry[]
}

interface ParkFormData {
  name: string
  description: string
  location: string
  date: unknown
  image: File[]
  organizer: string
  isFree: boolean
  entranceFee: number
  amenities: AmenitiesValue
  ageRecommendation: string
  closingSchedule: ClosingScheduleData
  [key: string]: unknown
}

const typeOptions = [
  { label: 'Park', value: ViewType.Park },
  { label: 'Event', value: ViewType.Event },
]
const locationInput = ref<HTMLInputElement | null>(null)
const isSubmitting = ref(false)
const toast = useToast()
const parksStore = useParksStore()
const form = ref<ParkFormData>({
  name: '',
  description: '',
  location: '',
  date: null,
  image: [],
  organizer: '',
  isFree: false,
  entranceFee: 0,
  amenities: {},
  ageRecommendation: '',
  closingSchedule: {
    venunCloses: false,
    schedules: [],
  },
})

const activeTabIndex = ref(0)
const type = computed(() => typeOptions[activeTabIndex.value].value)
const visibleFields = computed(() =>
  useFormFields(type.value)
    // Hide entranceFee when the park is free
    .filter((field) => !(field.model === 'entranceFee' && form.value.isFree === true))
    .map((field) => ({
    ...field,
    props: {
      ...field.props,
    },
    listeners:
      field.model === 'image'
        ? {
            select: onImageSelect,
            uploader: onImageUpload,
          }
        : ({} as Record<string, () => void>),
  })),
)

const { imagePreviews, onImageSelect, onImageUpload } = useImageUpload(form)
const { placeDetails, validateLocation } = useLocationValidation(form)

/** Format a Date object from PrimeVue Calendar (timeOnly) to HH:mm */
function formatTime(d: Date | null): string {
  if (!d) return ''
  return d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
}

/** Serialize AmenitiesValue to the server format: "item1,item2:optional note" */
function serializeAmenities(value: AmenitiesValue): string {
  if (!value?.hasAmenities || !Array.isArray(value.amenities)) return ''
  const allItems: string[] = []
  const allNotes: string[] = []
  for (const block of value.amenities) {
    allItems.push(...block.items)
    if (block.note) allNotes.push(block.note)
  }
  if (!allItems.length) return ''
  return allNotes.length ? `${allItems.join(',')}:${allNotes.join(', ')}` : allItems.join(',')
}

/** Serialize ClosingScheduleData to server format: "mon,tue:09:00-18:00;sat:10:00-15:00" */
function serializeClosingSchedule(schedule: ClosingScheduleData): string {
  if (!schedule?.venunCloses || !Array.isArray(schedule.schedules)) return ''
  return schedule.schedules
    .filter((s) => s.days.length > 0 && s.openingTime && s.closingTime)
    .map((s) => `${s.days.join(',')}:${formatTime(s.openingTime)}-${formatTime(s.closingTime)}`)
    .join(';')
}

function resetForm() {
  form.value = {
    name: '',
    description: '',
    location: '',
    date: null,
    image: [],
    organizer: '',
    isFree: false,
    entranceFee: 0,
    amenities: {},
    ageRecommendation: '',
    closingSchedule: { venunCloses: false, schedules: [] },
  }
  imagePreviews.value = []
  placeDetails.value = {
    placeId: '',
    lat: null,
    lng: null,
    formattedAddress: '',
    street: '',
    streetNumber: '',
    neighborhood: '',
    sublocality: '',
    locality: '',
    district: '',
    province: '',
    region: '',
    country: '',
    postalCode: '',
  }
  if (locationInput.value) locationInput.value.value = ''
}

async function handleSubmit() {
  isSubmitting.value = true
  try {
    const isValid = await validateLocation()
    if (!isValid) return
    const fd = new window.FormData()
    fd.append('Name', form.value.name || '')
    fd.append('Description', form.value.description || '')
    fd.append('Organizer', form.value.organizer || '')
    fd.append('IsFree', String(!!form.value.isFree))
    fd.append('EntranceFee', String(form.value.isFree ? 0 : (form.value.entranceFee ?? 0)))
    fd.append('AgeRecommendation', form.value.ageRecommendation || '')
    fd.append('Amenities', serializeAmenities(form.value.amenities as AmenitiesValue))
    fd.append('ParkType', String(type.value))

    // Attach files — field name must match server's IFormFileCollection parameter
    ;(form.value.image as File[]).forEach((f) => fd.append('ParkImages', f))

    fd.append('ClosingSchedule', serializeClosingSchedule(form.value.closingSchedule as ClosingScheduleData))

    // Geocoded coordinates — server expects "longitude,latitude"
    fd.append('Coordinates', `${placeDetails.value.lng ?? ''},${placeDetails.value.lat ?? ''}`)
    fd.append('FormattedAddress', placeDetails.value.formattedAddress || '')
    fd.append('Street', placeDetails.value.street || '')
    fd.append('Number', placeDetails.value.streetNumber || '')
    fd.append('Locality', placeDetails.value.locality || '')
    fd.append('PostalCode', placeDetails.value.postalCode || '')

    const { data, error } = await parksStore.createPark(fd)

    if (!error && data) {
      toast.add({ severity: 'success', summary: 'Park created', detail: `Park #${data.parkId} was added successfully.`, life: 4000 })
      resetForm()
    } else {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const original = error?.original as Record<string, any> | undefined
      const status: unknown = original?.['response']?.['status'] ?? 'unknown'
      const detail: unknown = original?.['response']?.['data']?.['detail'] ?? original?.['response']?.['data'] ?? error
      toast.add({
        severity: 'error',
        summary: `Failed to create park (${String(status)})`,
        detail: typeof detail === 'object' ? JSON.stringify(detail) : String(detail),
        life: 6000,
      })
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>
