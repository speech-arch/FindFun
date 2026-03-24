<template>
  <div class="w-full p-4 bg-gray-50 dark:bg-gray-900 rounded-b-xl shadow flex flex-col gap-4">
    <h3 class="text-lg font-semibold text-gray-800 dark:text-gray-100 mb-2">Leave your review</h3>
    <form @submit.prevent="handleSubmit" class="flex flex-col gap-3">
      <div class="flex flex-col gap-1">
        <label for="author" class="text-sm text-gray-700 dark:text-gray-200">Name</label>
        <input
          v-model="form.author"
          id="author"
          type="text"
          required
          maxlength="32"
          class="rounded border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm text-gray-700 dark:text-gray-200">Rating</label>
        <Rating v-model="form.rating" :cancel="false" :stars="5" class="yellow-stars" />
      </div>
      <div class="flex flex-col gap-1">
        <label for="comment" class="text-sm text-gray-700 dark:text-gray-200">Comment</label>
        <textarea
          v-model="form.comment"
          id="comment"
          required
          maxlength="300"
          class="rounded border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>
      <!-- <div class="flex flex-col gap-1">
        <label for="street" class="text-sm text-gray-700 dark:text-gray-200">Street</label>
        <input v-model="street" id="street" type="text" maxlength="64"
          class="rounded border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400" />
      </div>
      <div class="flex flex-col gap-1">
        <label for="postal" class="text-sm text-gray-700 dark:text-gray-200">Postal Code</label>
        <input v-model="postalCode" id="postal" type="text" maxlength="16"
          class="rounded border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400" />
        <span v-if="postalError" class="text-red-500 text-xs mt-1">{{ postalError }}</span>
        <span v-if="postalResult" class="text-green-600 text-xs mt-1">Found: {{ postalResult }}</span>
      </div> -->
      <button
        type="submit"
        class="self-start bg-blue-600 hover:bg-blue-700 text-white font-semibold px-5 py-2 rounded shadow disabled:opacity-50 disabled:cursor-not-allowed"
        :disabled="!form.author || !form.rating || !form.comment"
      >
        <i class="pi pi-check mr-2"></i>Submit Review
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
const emit = defineEmits<{
  (e: 'submit', value: { author: string; rating: number; comment: string; date: string }): void
}>()

interface ReviewFormState {
  author: string
  rating: number
  comment: string
}

const form = ref<ReviewFormState>({ author: '', rating: 0, comment: '' })
const street = ref('')
const postalCode = ref('')
const postalError = ref('')
const postalResult = ref('')
import { watch } from 'vue'
watch([street, postalCode], async ([streetVal, postalVal]) => {
  postalError.value = ''
  postalResult.value = ''
  if (streetVal && postalVal && postalVal.length === 5) {
    await searchPostalCode()
  }
})
async function searchPostalCode() {
  postalError.value = ''
  postalResult.value = ''
  if (!postalCode.value) {
    postalError.value = 'Postal code is required.'
    return
  }
  try {
    let url = `https://nominatim.openstreetmap.org/search?format=json`
    if (street.value) url += `&street=${encodeURIComponent(street.value)}`
    if (postalCode.value) url += `&postalcode=${encodeURIComponent(postalCode.value)}`
    const response = await fetch(url)
    const data = await response.json()
    if (!data.length) {
      postalError.value = 'No location found for this address.'
    } else {
      const result = data[0].display_name
      postalResult.value = result
    }
  } catch {
    postalError.value = 'Error searching address.'
  }
}

function handleSubmit() {
  if (!form.value.author || !form.value.rating || !form.value.comment) return
  emit('submit', {
    author: form.value.author,
    rating: form.value.rating,
    comment: form.value.comment,
    date: new Date().toISOString().slice(0, 10),
  })
  form.value = { author: '', rating: 0, comment: '' }
}
</script>

<style scoped>
.yellow-stars .p-rating-icon.pi-star-fill {
  color: #facc15 !important;
}
</style>

