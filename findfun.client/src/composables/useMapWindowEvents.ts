type MarkerWithId = google.maps.marker.AdvancedMarkerElement & { parkId?: string | number }

interface GMapOps {
  panTo(latLng: object): void
  setZoom(zoom: number): void
}

export function useMapWindowEvents(
  mapRef: { readonly value: HTMLElement | null },
  gmapRef: { readonly value: GMapOps | null },
  markersRef: { readonly value: MarkerWithId[] },
) {
  const timeouts: ReturnType<typeof setTimeout>[] = []
  // Use `any` to avoid structural type collision between google.maps.InfoWindow
  // and unrelated types that share the same class name in TypeScript's type graph.
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  let infoWindow: any = null

  window.scrollToMapMarker = (productId: string) => {
    const gmap = gmapRef.value
    const marker = markersRef.value.find((m) => String(m.parkId) === String(productId))
    if (!marker || !gmap) return

    // 1. Pan map to the marker and zoom in
    const pos = marker.position as google.maps.LatLngLiteral | google.maps.LatLng | null
    if (pos) gmap.panTo(pos)
    gmap.setZoom(11)

    // 2. Scroll the map section into view
    mapRef.value?.scrollIntoView({ behavior: 'smooth', block: 'center' })

    // 3. Show InfoWindow on the marker
    if (!infoWindow) infoWindow = new google.maps.InfoWindow()
    infoWindow.setContent(`<b style="color:#000">${marker.title ?? 'Location'}</b>`)
    infoWindow.open({ map: gmap, anchor: marker })

    // 4. Briefly scale up the marker's content to draw attention
    const el = marker.content as HTMLElement | null
    if (el) {
      el.style.transition = 'transform 0.3s ease'
      el.style.transform = 'scale(1.5)'
      timeouts.push(setTimeout(() => { el.style.transform = 'scale(1)' }, 1200))
    }

    timeouts.push(setTimeout(() => infoWindow?.close(), 5000))
  }

  return {
    cleanup: () => {
      timeouts.forEach(clearTimeout)
      timeouts.length = 0
      infoWindow?.close()
    },
  }
}


