// Global augmentations for window and google maps

interface ImportMetaEnv
{
  readonly VITE_API_BASE_URL?: string
  readonly VITE_DEFAULT_PARK_RADIUS_METERS?: string
  readonly VITE_DEFAULT_PAGE_SIZE?: string
}

declare global
{
  interface ImportMeta
  {
    readonly env: ImportMetaEnv
  }

  interface Window
  {
    google: typeof google
    scrollToMapMarker?: (id: string) => void
  }
}

export { }
