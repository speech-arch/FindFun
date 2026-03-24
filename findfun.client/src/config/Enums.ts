export enum Routes {
  Home = 'home',
  About = 'about',
  Events = 'events',
  EventDetail = 'event-detail',
  Parks = 'parks',
  ParkDetail = 'park-detail',
  Login = 'login',
  Signup = 'signup',
  CreatePark = 'create-park',
}
export enum RoutePaths {
  Home = '/',
  About = '/about',
  Events = '/events',
  EventDetail = '/events/:id',
  Parks = '/parks',
  ParkDetail = '/parks/:id',
  Login = '/login',
  Signup = '/signup',
  CreatePark = '/create',
  BaseURL = '/api',
  InitialData = '/initial-data',
}
export enum ViewType {
  Park = 'park',
  Event = 'event',
}
