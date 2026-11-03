<script setup lang="ts">
const { t } = useI18n()
</script>

<template>
  <main class="p-6 max-w-md mx-auto space-y-6">
    <h1 class="text-2xl font-bold">{{ t('welcome') }}</h1>
    <p>Toto je domovská stránka.</p>
    <Button>Tlačítko</Button>
  </main>
</template>
