
<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const menuOpen = ref(false)

function closeMenu() {
  menuOpen.value = false
}
</script>

<template>
  <header class="border-b border-surface-200 bg-surface-0">
    <div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
      <RouterLink
        to="/"
        class="flex items-center gap-2 text-xl font-bold text-primary"
        @click="closeMenu"
      >
        <span class="text-2xl">🎓</span>
        <span>EduSpace</span>
      </RouterLink>

      <nav class="hidden items-center gap-8 md:flex">
        <RouterLink to="/" class="hover:text-primary">
          {{ t('nav.home') }}
        </RouterLink>

        <a href="/#courses" class="hover:text-primary">
          {{ t('nav.courses') }}
        </a>

        <a href="/#about" class="hover:text-primary">
          {{ t('nav.about') }}
        </a>

        <Button :label="t('nav.login')" />
      </nav>

      <Button
        class="md:!hidden"
        text
        rounded
        :aria-label="menuOpen ? 'Zavřít menu' : 'Otevřít menu'"
        @click="menuOpen = !menuOpen"
      >
        {{ menuOpen ? '✕' : '☰' }}
      </Button>
    </div>

    <nav
      v-if="menuOpen"
      class="flex flex-col gap-4 border-t border-surface-200 px-6 py-4 md:hidden"
    >
      <RouterLink to="/" @click="closeMenu">
        {{ t('nav.home') }}
      </RouterLink>

      <a href="/#courses" @click="closeMenu">
        {{ t('nav.courses') }}
      </a>

      <a href="/#about" @click="closeMenu">
        {{ t('nav.about') }}
      </a>

      <Button :label="t('nav.login')" />
    </nav>
  </header>
</template>
