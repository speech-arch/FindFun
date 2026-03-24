import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'

export function useLocationValidation(form: any) {
  const toast = useToast()
  const placeDetails = ref({
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
  })

  function extractAdminDetails(components: any[]) {
    const get = (type: string) => components.find((c) => c.types.includes(type))?.long_name || ''
    return {
      street: get('route'),
      streetNumber: get('street_number'),
      neighborhood: get('neighborhood'),
      sublocality: get('sublocality'),
      locality: get('locality'),
      district: get('administrative_area_level_3'),
      province: get('administrative_area_level_2'),
      region: get('administrative_area_level_1'),
      country: get('country'),
      postalCode: get('postal_code'),
    }
  }

  function waitForGoogleMaps(timeoutMs = 2_000): Promise<void> {
    return new Promise((resolve, reject) => {
      if ((window as any).google?.maps?.Geocoder) {
        resolve()
        return
      }
      const deadline = setTimeout(() => {
        clearInterval(poll)
        reject(new Error('Google Maps did not load in time'))
      }, timeoutMs)
      const poll = setInterval(() => {
        if ((window as any).google?.maps?.Geocoder) {
          clearInterval(poll)
          clearTimeout(deadline)
          resolve()
        }
      }, 100)
    })
  }

  async function validateLocation(): Promise<boolean> {
    const input = form.value.location.trim()
    if (!input) return false

    try {
      await waitForGoogleMaps()
    } catch {
      toast.add({ severity: 'error', summary: 'Map unavailable', detail: 'Could not load map services for location validation. Please check your connection and try again.', life: 5000 })
      return false
    }

    const geocoder = new (window as any).google.maps.Geocoder()

    return new Promise((resolve) => {
      geocoder.geocode({ address: input }, (results: any, status: any) => {
        if (status === 'OK' && results.length > 0) {
          const result = results[0]
          const components = result.address_components
          const geometry = result.geometry.location

          const admin = extractAdminDetails(components)
          placeDetails.value = {
            placeId: result.place_id || '',
            lat: geometry.lat(),
            lng: geometry.lng(),
            formattedAddress: result.formatted_address || '',
            ...admin,
          }

          form.value.location = result.formatted_address || ''
          resolve(true)
        } else {
          toast.add({ severity: 'warn', summary: 'Invalid location', detail: 'Please enter a valid place and select it from the suggestions.', life: 5000 })
          resolve(false)
        }
      })
    })
  }

  return { placeDetails, validateLocation }
}
