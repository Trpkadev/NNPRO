<script setup lang="ts">
const { t } = useI18n()
</script>

<template>
  <div class="space-y-6">
    <!-- Administrační sub-hlavička -->
    <div
      class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-surface-200 dark:border-surface-800"
    >
      <div class="flex items-center gap-3">
        <div
          class="w-10 h-10 rounded-xl bg-red-500/10 text-red-600 dark:text-red-400 flex items-center justify-center"
        >
          <i class="oi oi-shield text-xl" />
        </div>
        <div>
          <h1 class="text-xl font-bold">{{ t('nav.adminZone') }}</h1>
          <p class="text-xs text-surface-500 dark:text-surface-400">
            {{ t('nav.usersManagement') }} &bull; {{ t('nav.coursesManagement') }}
          </p>
        </div>
      </div>
    </div>

    <RouterView />
  </div>
</template>

