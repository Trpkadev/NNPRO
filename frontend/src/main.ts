import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import OpenVue from 'openvue/config'
import Aura from '@openvue/themes/aura'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'

import App from './App.vue'
import router from './router'
import { i18n } from './i18n'
import { ConfirmationService, ToastService } from 'openvue'

const app = createApp(App)

// Konfigurace TanStack Vue Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minut
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

app.use(createPinia())
app.use(router)
app.use(i18n)
app.use(VueQueryPlugin, { queryClient })
app.use(ToastService)
app.use(ConfirmationService)

// Konfigurace OpenVue
app.use(OpenVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.dark',
      cssLayer: {
        name: 'openvue',
        order: 'theme, base, openvue, components, utilities',
      },
    },
  },
})

app.config.errorHandler = (err, instance, info) => {
  // pro vývoj
  console.error('Globální chyba Vue:', err, info)
}

app.mount('#app')
