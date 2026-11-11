<script setup lang="ts">
import { computed } from 'vue'
import type { LessonResponseDto } from '@/types/api'

const props = defineProps<{
  lessons: LessonResponseDto[]
  selectedLessonId: number | null
}>()

const emit = defineEmits<{
  select: [id: number]
}>()

const { t } = useI18n()

const currentIndex = computed(() =>
  props.lessons.findIndex((lesson) => lesson.id === props.selectedLessonId),
)

const previousLesson = computed(() =>
  currentIndex.value > 0 ? props.lessons[currentIndex.value - 1] : undefined,
)

const nextLesson = computed(() =>
  currentIndex.value >= 0 ? props.lessons[currentIndex.value + 1] : undefined,
)
</script>

<template>
  <div v-if="currentIndex >= 0" class="flex flex-wrap items-center justify-between gap-3">
    <Button
      :label="t('courses.previousLesson')"
      icon="pi pi-arrow-left"
      severity="secondary"
      outlined
      :disabled="!previousLesson"
      @click="previousLesson && emit('select', previousLesson.id)"
    />

    <span class="text-sm text-surface-500">
      {{
        t('courses.lessonPosition', {
          current: currentIndex + 1,
          total: lessons.length,
        })
      }}
    </span>

    <Button
      :label="t('courses.nextLesson')"
      icon="pi pi-arrow-right"
      icon-pos="right"
      :disabled="!nextLesson"
      @click="nextLesson && emit('select', nextLesson.id)"
    />
  </div>
</template>
