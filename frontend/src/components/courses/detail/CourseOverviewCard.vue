<script setup lang="ts">
import type { CourseInstructorResponseDto, CourseResponseDto } from '@/types/api'

defineProps<{
  course: CourseResponseDto
  instructors: CourseInstructorResponseDto[]
  isEnrolled: boolean
  isStudent: boolean
  unenrollPending: boolean
  showProgress: boolean
  completedCount: number
  totalCourseLessons: number
  progressPercent: number
}>()

const emit = defineEmits<{
  enroll: []
  unenroll: []
}>()

const { t } = useI18n()
</script>

<template>
  <div
    class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-5 shadow-xs"
  >
    <div class="flex flex-col md:flex-row md:items-start justify-between gap-4">
      <div class="space-y-2 flex-1">
        <div class="flex items-center gap-2.5 flex-wrap">
          <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
            {{ course.title }}
          </h1>
          <Tag v-if="isEnrolled" :value="t('courses.enrolledBadge')" severity="success" />
        </div>

        <p class="text-sm text-surface-600 dark:text-surface-300 whitespace-pre-line">
          {{ course.description }}
        </p>
      </div>

      <!-- Tlačítko pro zápis nebo odhlášení z kurzu (pro studenty) -->
      <div v-if="isStudent" class="shrink-0">
        <Button
          v-if="!isEnrolled"
          :label="t('courses.enrollBtn')"
          icon="oi oi-plus"
          @click="emit('enroll')"
        />
        <Button
          v-else
          :label="t('courses.unenrollBtn')"
          icon="oi oi-sign-out"
          severity="danger"
          outlined
          size="small"
          :loading="unenrollPending"
          @click="emit('unenroll')"
        />
      </div>
    </div>

    <!-- Lektoři kurzu -->
    <div
      class="pt-4 border-t border-surface-100 dark:border-surface-800 flex flex-wrap items-center justify-between gap-4"
    >
      <div class="flex items-center gap-2 flex-wrap">
        <span class="text-xs font-semibold text-surface-500">
          {{ t('courses.instructors') }}:
        </span>
        <template v-if="instructors.length > 0">
          <Tag
            v-for="inst in instructors"
            :key="inst.id"
            :value="inst.username"
            severity="secondary"
            class="text-xs"
          />
        </template>
        <span v-else class="text-xs text-surface-400 italic">
          {{ t('courses.noInstructors') }}
        </span>
      </div>
    </div>

    <!-- Celkový průchod kurzem -->
    <div
      v-if="showProgress"
      class="p-4 rounded-xl bg-surface-50 dark:bg-surface-800/50 border border-surface-200/70 dark:border-surface-800 space-y-2"
    >
      <div class="flex items-center justify-between text-xs font-semibold">
        <span>{{ t('courses.overallProgress') }}</span>
        <span class="text-primary">
          {{
            t('courses.completedLessonsCount', {
              completed: completedCount,
              total: totalCourseLessons,
              percent: progressPercent,
            })
          }}
        </span>
      </div>
      <ProgressBar :value="progressPercent" :show-value="false" class="!h-2.5" />
    </div>
  </div>
</template>
