import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import Aura from '@primeuix/themes/aura'
import 'primeicons/primeicons.css'
import { Icon } from '@iconify/vue'

import App from '@/App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.my-app-dark',
    },
  },
})
app.use(ToastService)

// Register global components
// eslint-disable-next-line vue/multi-word-component-names
app.component('Icon', Icon)

// Configure global error handler
app.config.errorHandler = (err, instance, info) =>
{
  // TODO: Send error to logging service (e.g., Sentry)
  // Log only in development
  if (import.meta.env.DEV)
  {
    console.error(`Error in ${info}:`, err)
  }
}

app.mount('#app')
