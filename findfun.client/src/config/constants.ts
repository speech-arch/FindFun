
export const API_BASE_PATH: string = import.meta.env.VITE_API_BASE_URL ?? '/api'

export const DEFAULT_PARK_RADIUS_METERS: number = Number(
  import.meta.env.VITE_DEFAULT_PARK_RADIUS_METERS ?? '2000',
)

export const DEFAULT_PAGE_SIZE: number = Number(import.meta.env.VITE_DEFAULT_PAGE_SIZE ?? '20')
