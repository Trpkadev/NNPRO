<script setup lang="ts">
const { t, locale } = useI18n()
const isDark = useDark()
const toggleDark = useToggle(isDark)

function toggleLanguage() {
  locale.value = locale.value === 'cs' ? 'en' : 'cs'
}
</script>

<template>
  <div
    class="min-h-screen flex flex-col bg-surface-50 dark:bg-surface-950 text-surface-900 dark:text-surface-0"
  >
    <!-- Horní lišta s přepínačem jazyka a tématu -->
    <header class="w-full px-6 py-4 flex items-center justify-between max-w-7xl mx-auto">
      <RouterLink :to="{ name: 'home' }" class="flex items-center gap-2.5 no-underline">
        <div
          class="w-10 h-10 rounded-xl bg-primary flex items-center justify-center text-primary-contrast font-bold shadow-sm"
        >
          <i class="oi oi-book text-xl" />
        </div>
        <div>
          <span class="text-lg font-bold tracking-tight text-surface-900 dark:text-surface-0 block">
            {{ t('app.name') }}
          </span>
          <span class="text-xs text-surface-500 dark:text-surface-400 block">
            {{ t('app.subtitle') }}
          </span>
        </div>
      </RouterLink>

      <div class="flex items-center gap-2">
        <Button
          :label="locale.toUpperCase()"
          icon="oi oi-globe"
          severity="secondary"
          text
          size="small"
          :aria-label="t('nav.language')"
          @click="toggleLanguage"
        />
        <Button
          :icon="isDark ? 'oi oi-sun' : 'oi oi-moon'"
          severity="secondary"
          text
          rounded
          :aria-label="t('nav.darkMode')"
          @click="toggleDark()"
        />
      </div>
    </header>

    <!-- Hlavní obsah pro přihlášení / registraci -->
    <main class="flex-1 flex items-center justify-center p-4 sm:p-6">
      <div
        class="w-full max-w-md bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 rounded-2xl shadow-sm p-6 sm:p-8"
      >
        <RouterView />
      </div>
    </main>
  </div>
</template>

