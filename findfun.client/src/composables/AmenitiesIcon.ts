// --- Amenities Icons Logic ---
export const AmenitiesIcons = [
  { name: 'Playground Equipment', icon: { icon: 'mdi:playground', color: '#FF9800' } },
  { name: 'Picnic Tables', icon: { icon: 'mdi:table-picnic', color: '#8BC34A' } },
  { name: 'Benches', icon: { icon: 'mdi:bench', color: '#795548' } },
  { name: 'Walking Trails', icon: { icon: 'mdi:walk', color: '#4CAF50' } },
  { name: 'Restrooms', icon: { icon: 'mdi:toilet', color: '#607D8B' } },
  { name: 'Water Fountain', icon: { icon: 'mdi:fountain', color: '#03A9F4' } },
  { name: 'Bike Racks', icon: { icon: 'mdi:bike', color: '#009688' } },
  { name: 'Trash Bins', icon: { icon: 'mdi:trash-can', color: '#F44336' } },
  { name: 'Dog Park', icon: { icon: 'mdi:dog', color: '#FF5722' } },
  { name: 'Basketball Court', icon: { icon: 'mdi:basketball', color: '#FFB300' } },
  { name: 'Tennis Court', icon: { icon: 'mdi:tennis', color: '#CDDC39' } },
  { name: 'Skate Park', icon: { icon: 'mdi:skateboard', color: '#9C27B0' } },
  { name: 'Parking Lot', icon: { icon: 'mdi:parking', color: '#607D8B' } },
  { name: 'BBQ Area', icon: { icon: 'mdi:grill', color: '#E91E63' } },
  { name: 'Shelter/Pavilion', icon: { icon: 'mdi:tent', color: '#3F51B5' } },
  { name: 'Lake or Pond', icon: { icon: 'mdi:water', color: '#2196F3' } },
  { name: 'Climbing Wall', icon: { icon: 'mdi:climbing', color: '#9E9E9E' } },
  { name: 'Zip Line', icon: { icon: 'game-icons:flying-fox', color: '#00BCD4' } },
  { name: 'Sandbox', icon: { icon: 'mdi:shovel', color: '#FFC107' } },
  { name: 'Swing Set', icon: { icon: 'game-icons:tree-swing', color: '#9E9E9E' } },
]

const amenityAliases: Record<string, string> = {
  playground: 'Playground Equipment',
  'playground equipment': 'Playground Equipment',
  'picnic area': 'Picnic Tables',
  'picnic tables': 'Picnic Tables',
  bench: 'Benches',
  benches: 'Benches',
  'walking trail': 'Walking Trails',
  trail: 'Walking Trails',
  restroom: 'Restrooms',
  toilet: 'Restrooms',
  'water fountain': 'Water Fountain',
  fountain: 'Water Fountain',
  'bike rack': 'Bike Racks',
  'bicycle parking': 'Bike Racks',
  trash: 'Trash Bins',
  'trash bin': 'Trash Bins',
  'dog park': 'Dog Park',
  basketball: 'Basketball Court',
  'basketball court': 'Basketball Court',
  tennis: 'Tennis Court',
  'tennis court': 'Tennis Court',
  'skate park': 'Skate Park',
  parking: 'Parking Lot',
  'parking lot': 'Parking Lot',
  bbq: 'BBQ Area',
  grill: 'BBQ Area',
  shelter: 'Shelter/Pavilion',
  pavilion: 'Shelter/Pavilion',
  lake: 'Lake or Pond',
  pond: 'Lake or Pond',
  'climbing wall': 'Climbing Wall',
  'zip line': 'Zip Line',
  sandbox: 'Sandbox',
  'sand pit': 'Sandbox',
  swing: 'Swing Set',
  'swing set': 'Swing Set',
}
export type Amenity = (typeof AmenitiesIcons)[number]
export function matchAmenities(rawAmenities: string[] | string): Amenity[] {
  let normalized: string[] = []
  if (Array.isArray(rawAmenities)) {
    normalized = rawAmenities
      .flatMap((item) => item.split(','))
      .map((item) => item.trim().toLowerCase())
  } else if (typeof rawAmenities === 'string') {
    normalized = rawAmenities.split(',').map((item) => item.trim().toLowerCase())
  } else {
    return []
  }
  const canonicalNames = normalized
    .map((name) => amenityAliases[name] || null)
    .filter((name): name is string => Boolean(name))
  return AmenitiesIcons.filter((amenity) => canonicalNames.includes(amenity.name))
}
