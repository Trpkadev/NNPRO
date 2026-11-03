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
  <main class="min-h-screen flex flex-col items-center justify-center p-6 text-center select-none">
    <div class="max-w-md w-full space-y-6">
      <!-- Ikona a kód chyby -->
      <div class="space-y-2">
        <div
          class="inline-flex items-center justify-center w-20 h-20 rounded-full bg-primary/10 text-primary mb-2"
        >
          <i class="oi oi-compass text-4xl" />
        </div>
        <p class="text-7xl font-extrabold text-primary tracking-tight">404</p>
        <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
          {{ t('notFound.title') }}
        </h1>
        <p class="text-surface-600 dark:text-surface-400 text-sm leading-relaxed">
          {{ t('notFound.description') }}
        </p>
      </div>

      <!-- Akční tlačítka (OpenVue Button je auto-importován) -->
      <div class="flex flex-col sm:flex-row items-center justify-center gap-3 pt-2">
        <Button
          :label="t('notFound.goBack')"
          icon="oi oi-arrow-left"
          severity="secondary"
          outlined
          class="w-full sm:w-auto"
          @click="goBack"
        />
        <Button
          :label="t('notFound.goHome')"
          icon="oi oi-home"
          class="w-full sm:w-auto"
          @click="router.replace({ name: 'home' })"
        />
      </div>
    </div>
  </main>
</template>
