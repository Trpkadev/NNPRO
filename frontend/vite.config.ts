import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { PrimeVueResolver } from '@openvue/auto-import-resolver'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    tailwindcss(),
    AutoImport({
      // Automatické importy funkcí a composables
      imports: [
        'vue',
        'vue-router',
        'pinia',
        '@vueuse/core',
        'vue-i18n',
        'vee-validate',
        {
          '@tanstack/vue-query': ['useQuery', 'useMutation', 'useQueryClient'],
          'jwt-decode': ['jwtDecode'],
        },
      ],
      dts: 'src/auto-imports.d.ts',
      dirs: ['src/composables/**', 'src/stores/**', 'src/services/**'],
      vueTemplate: true,
      eslintrc: {
        enabled: true,
        filepath: './.eslintrc-auto-import.json',
        globalsPropValue: true,
      },
    }),
    Components({
      // Automatické importy vlastních komponent + OpenVue komponent
      dts: 'src/components.d.ts',
      resolvers: [PrimeVueResolver()],
    }),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'apple-touch-icon.png', 'masked-icon.svg'],
      manifest: {
        name: 'E-Learning Platforma',
        short_name: 'E-Learning',
        description: 'Vzdělávací systém s kurzy a interaktivními kvízy',
        theme_color: '#10b981',
        background_color: '#ffffff',
        display: 'standalone',
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
      },
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
})
