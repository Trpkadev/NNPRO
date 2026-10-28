<script setup lang="ts">
import { VueQueryDevtools } from '@tanstack/vue-query-devtools'
</script>

<template>
  <!-- Globální kontejnery pro OpenVue Toast notifikace a dialogy -->
  <Toast />
  <ConfirmDialog />

  <!-- Hlavní kontejner aplikace, kam se vykreslují routes -->
  <div class="min-h-screen bg-surface-0 dark:bg-surface-950 text-surface-900 dark:text-surface-0">
    <RouterView />
  </div>

  <VueQueryDevtools />
</template>
