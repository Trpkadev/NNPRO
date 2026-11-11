<script setup lang="ts">
defineProps<{
  title: string
  description: string
  canStart: boolean
}>()

const emit = defineEmits<{
  start: []
}>()

const { t } = useI18n()
</script>

<template>
  <div
    class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
  >
    <Tag :value="t('courses.course')" severity="success" />

    <h1 class="text-2xl sm:text-3xl font-bold">
      {{ title }}
    </h1>

    <p class="text-surface-600 dark:text-surface-400 whitespace-pre-line">
      {{ description }}
    </p>

    <Button
      :label="t('courses.startLearning')"
      icon="pi pi-play"
      :disabled="!canStart"
      @click="emit('start')"
    />
  </div>
</template>
