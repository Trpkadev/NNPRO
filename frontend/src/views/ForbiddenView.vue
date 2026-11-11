<script setup lang="ts">
const router = useRouter()
const { t } = useI18n()

function goBack() {
  if (window.history.length > 2) {
    router.back()
  } else {
    router.replace({ name: 'home' })
  }
}
</script>

<template>
  <main class="min-h-[70vh] flex flex-col items-center justify-center p-6 text-center select-none">
    <div class="max-w-md w-full space-y-6">
      <div class="space-y-2">
        <div
          class="inline-flex items-center justify-center w-20 h-20 rounded-full bg-red-500/10 text-red-500 mb-2"
        >
          <i class="oi oi-lock text-4xl" />
        </div>
        <p class="text-7xl font-extrabold text-red-500 tracking-tight">403</p>
        <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
          {{ t('forbidden.title') }}
        </h1>
        <p class="text-surface-600 dark:text-surface-400 text-sm leading-relaxed">
          {{ t('forbidden.description') }}
        </p>
      </div>

      <div class="flex flex-col sm:flex-row items-center justify-center gap-3 pt-2">
        <Button
          :label="t('forbidden.goBack')"
          icon="oi oi-arrow-left"
          severity="secondary"
          outlined
          class="w-full sm:w-auto"
          @click="goBack"
        />
        <Button
          :label="t('forbidden.goHome')"
          icon="oi oi-home"
          class="w-full sm:w-auto"
          @click="router.replace({ name: 'home' })"
        />
      </div>
    </div>
  </main>
</template>
