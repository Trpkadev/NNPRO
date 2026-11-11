<script setup lang="ts">
import type { LessonResponseDto } from '@/types/api'

defineProps<{
  lessons: LessonResponseDto[]
  selectedLessonId: number | null
  completedLessons: Record<number, boolean>
  pendingLessonId: number | null
}>()

const emit = defineEmits<{
  select: [id: number]
  toggle: [id: number]
}>()

const { t } = useI18n()
</script>

<template>
  <div class="space-y-3">
    <div
      v-for="(lesson, index) in lessons"
      :key="lesson.id"
      class="p-4 rounded-xl border bg-surface-0 dark:bg-surface-900 flex items-center justify-between gap-3"
      :class="
        selectedLessonId === lesson.id
          ? 'border-emerald-500'
          : 'border-surface-200 dark:border-surface-800'
      "
    >
      <button
        type="button"
        class="flex-1 flex items-center gap-3 text-left cursor-pointer"
        @click="emit('select', lesson.id)"
      >
        <span class="text-emerald-500 font-semibold"> {{ index + 1 }}. </span>

        <span>{{ lesson.title }}</span>
      </button>

      <Button
        :icon="completedLessons[lesson.id] ? 'pi pi-check-circle' : 'pi pi-circle'"
        :label="completedLessons[lesson.id] ? t('courses.completed') : t('courses.markCompleted')"
        :severity="completedLessons[lesson.id] ? 'success' : 'secondary'"
        :outlined="!completedLessons[lesson.id]"
        size="small"
        :loading="pendingLessonId === lesson.id"
        :disabled="pendingLessonId !== null"
        @click="emit('toggle', lesson.id)"
      />
    </div>
  </div>
</template>
