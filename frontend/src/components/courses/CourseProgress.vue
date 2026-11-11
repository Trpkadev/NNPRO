<script setup lang="ts">
defineProps<{
  completed: number
  total: number
  percentage: number
}>()

const { t } = useI18n()
</script>

<template>
  <div
    class="p-5 rounded-xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 space-y-3"
  >
    <div class="flex items-center justify-between gap-3">
      <h3 class="font-semibold">
        {{ t('courses.sectionProgress') }}
      </h3>

      <span class="font-semibold text-emerald-500"> {{ percentage }} % </span>
    </div>

    <ProgressBar :value="percentage" :showValue="false" style="height: 0.75rem" />

    <p class="text-sm text-surface-500">
      {{
        t('courses.completedCount', {
          count: completed,
          total,
        })
      }}
    </p>
  </div>
</template>
