<script setup lang="ts">
import type { LessonResponseDto } from '@/types/api'

defineProps<{
  lesson: LessonResponseDto | undefined
  loading: boolean
  error: boolean
}>()

const { t } = useI18n()
</script>

<template>
  <div
    class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-3"
  >
    <Skeleton v-if="loading" height="5rem" />

    <Message v-else-if="error" severity="error" :closable="false">
      {{ t('courses.loadError') }}
    </Message>

    <template v-else-if="lesson">
      <Tag :value="t('courses.lesson')" severity="info" />

      <h2 class="text-xl font-semibold">
        {{ lesson.title }}
      </h2>

      <p class="text-surface-600 dark:text-surface-400 whitespace-pre-line">
        {{ lesson.content }}
      </p>

      <a
        v-if="lesson.videoUrl"
        :href="lesson.videoUrl"
        target="_blank"
        rel="noopener noreferrer"
        class="text-emerald-500 hover:underline"
      >
        {{ lesson.videoUrl }}
      </a>
    </template>
  </div>
</template>
