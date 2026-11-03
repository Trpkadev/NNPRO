
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { VueQueryDevtools } from '@tanstack/vue-query-devtools'

const isLoading = ref(true)

onMounted(() => {
  setTimeout(() => {
    isLoading.value = false
  }, 2000)
})
</script>

<template>
  <AppLoader v-if="isLoading" />

  <template v-else>
    <Toast />
    <ConfirmDialog />

    <div class="min-h-screen bg-surface-0 dark:bg-surface-950 text-surface-900 dark:text-surface-0">
      <RouterView />
    </div>

    <VueQueryDevtools />
  </template>
</template>
