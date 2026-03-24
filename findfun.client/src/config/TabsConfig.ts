import ContentGrid from '@/components/ContentGrid.vue'
import ClosingSchedule from '@/components/ClosingSchedule.vue'
import { RoutePaths, ViewType } from './Enums'
import { Calendar, Dropdown, FileUpload, InputNumber, InputText, SelectButton, Textarea } from 'primevue'
import type { Component } from 'vue'
import AmenitiesSelector from '@/components/AmenitiesSelector.vue'

export type TabConfig = {
  label: string
  slot: string
  component: Component
}

export const ParkTabs: TabConfig[] = [
  { label: 'All Parks', slot: 'all', component: ContentGrid },
  { label: 'Water Parks', slot: 'water', component: ContentGrid },
  { label: 'Amusement Parks', slot: 'amusement', component: ContentGrid },
  { label: 'National Parks', slot: 'national', component: ContentGrid },
  { label: 'Indoor Parks', slot: 'indoor', component: ContentGrid },
]

export const EventTabs: TabConfig[] = [
  { label: 'Upcoming Events', slot: 'upcoming', component: ContentGrid },
  { label: 'Past Events', slot: 'past', component: ContentGrid },
  { label: 'Featured Events', slot: 'featured', component: ContentGrid },
]

export type HomeTabConfigItem = {
  type: ViewType
  route: string
}

export const HomeTabConfig: HomeTabConfigItem[] = [
  {
    type: ViewType.Park,
    route: RoutePaths.Parks,
  },
  {
    type: ViewType.Event,
    route: RoutePaths.Events,
  },
]
export type FormField = {
  model: string
  component: Component
  label?: string
  labelByType?: Record<string, string>
  placeholder?: string
  placeholderByType?: Record<string, string>
  props?: Record<string, unknown>
  types: string[]
}
export const fieldRegistry: Record<string, FormField> = {
  name: {
    model: 'name',
    component: InputText,
    label: 'Name',
    placeholderByType: {
      park: 'Park Name',
      event: 'Event Name',
    },
    props: { required: true },
    types: ['park', 'event'],
  },
  description: {
    model: 'description',
    component: Textarea,
    label: 'Description',
    props: { rows: 3, required: true, placeholder: 'simple this description goes here' },
    types: ['park', 'event'],
  },
  location: {
    model: 'location',
    component: InputText,
    label: 'Location',
    placeholder: 'Location',
    props: { required: true },
    types: ['park', 'event'],
  },
  isFree: {
    model: 'isFree',
    component: SelectButton,
    label: 'Is it Free?',
    props: {
      options: [
        { label: 'Free', value: true },
        { label: 'Paid', value: false },
      ],
      optionLabel: 'label',
      optionValue: 'value',
      required: true,
      ariaLabelledby: 'isFree-label',
    },
    types: ['park', 'event'],
  },
  amenities: {
    model: 'amenities',
    component: AmenitiesSelector,
    label: 'Amenities',
    placeholder: 'Select amenities',
    props: {
      required: true,
    },
    types: ['park', 'event'],
  },
  organizer: {
    model: 'organizer',
    component: InputText,
    label: 'Organizer',
    placeholder: 'Organizer',
    props: { required: true },
    types: ['park', 'event'],
  },
  entranceFee: {
    model: 'entranceFee',
    component: InputNumber,
    label: 'Entrance Fee',
    props: {
      mode: 'currency',
      currency: 'EUR',
      locale: 'en-US',
      minFractionDigits: 2,
      min: 0,
    },
    types: ['park', 'event'],
  },
  closingSchedule: {
    model: 'closingSchedule',
    component: ClosingSchedule,
    types: ['park', 'event'],
  },
  ageRecommendation: {
    model: 'ageRecommendation',
    component: Dropdown,
    label: 'Age Recommendation',
    props: {
      options: [
        { label: 'All Ages', value: 'all' },
        { label: '0-5', value: '0-5' },
        { label: '6-12', value: '6-12' },
        { label: '13-18', value: '13-18' },
        { label: '18+', value: '18+' },
      ],
      optionLabel: 'label',
      optionValue: 'value',
      required: true,
    },
    types: ['park', 'event'],
  },
  date: {
    model: 'date',
    component: Calendar,
    labelByType: {
      park: 'Established Date',
      event: 'Event Date',
    },
    props: {
      showIcon: true,
      dateFormat: 'yy-mm-dd',
      required: true,
      selectionMode: 'range',
      manualInput: 'false',
    },
    types: ['park', 'event'],
  },
  image: {
    model: 'image',
    component: FileUpload,
    label: 'Image',
    props: {
      mode: 'basic',
      name: 'image',
      accept: 'image/*',
      customUpload: true,
      chooseLabel: 'Choose Image',
      multiple: true,
    },
    types: ['park', 'event'],
  },
}
