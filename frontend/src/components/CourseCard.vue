<script setup lang="ts">
defineProps<{
  title: string
  lessons: number
  level: string
}>()
</script>

<template>
  <Card class="h-full">
    <template #title>
      {{ title }}
    </template>

    <template #content>
      <p class="mb-4 text-sm text-muted-color">
        {{ lessons }} lekcí · {{ level }}
      </p>

      <Button label="Zobrazit kurz" outlined fluid />
    </template>
  </Card>
</template>