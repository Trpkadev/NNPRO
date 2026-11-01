
<script setup lang="ts">
defineProps<{
  title: string
  lessons: number
  level: string
}>()
</script>

<template>
  <article class="course-card">
    <h3 class="text-lg font-semibold">{{ title }}</h3>

    <p class="text-sm text-muted">
      {{ lessons }} lekcí · {{ level }}
    </p>

    <Button label="Zobrazit kurz" outlined />
  </article>
</template>

<style scoped>
.course-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 20px;
  display: grid;
  gap: 16px;
}

.text-muted {
  color: var(--color-muted);
}
</style>
