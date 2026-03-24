export const getDistanceKm = (lat1: number, lng1: number, lat2: number, lng2: number): number => {
  const R = 6371
  const dLat = ((lat2 - lat1) * Math.PI) / 180
  const dLng = ((lng2 - lng1) * Math.PI) / 180
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos((lat1 * Math.PI) / 180) * Math.cos((lat2 * Math.PI) / 180) * Math.sin(dLng / 2) ** 2
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
}

function createUserLocationPin(): HTMLElement {
  const pin = new google.maps.marker.PinElement({
    background: '#FBBF24', 
    borderColor: '#D97706', 
    glyphColor: '#D97706',
    scale: 1,
  })
  return pin.element
}

export const setUserMarkerAndCenter = (
  gmap: google.maps.Map,
  userMarkerRef: { value: google.maps.marker.AdvancedMarkerElement | null },
  lat: number,
  lng: number,
  title = 'Your Location',
) => {
  const position = { lat, lng }
  gmap.setCenter(position)

  if (userMarkerRef.value) {
    userMarkerRef.value.position = position
    userMarkerRef.value.title = title
  } else {
    userMarkerRef.value = new google.maps.marker.AdvancedMarkerElement({
      position,
      map: gmap,
      title,
      content: createUserLocationPin(),
    })
  }
}
