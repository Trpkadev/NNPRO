<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getApiErrorMessage } from '@/services/api'
import { useCourseDetailQuery } from '@/composables/useCoursesQueries'
import { useCourseContent } from '@/composables/useCourseContent'
import { useLessonProgress } from '@/composables/useLessonProgress'
import CourseHeader from '@/components/courses/CourseHeader.vue'
import CourseSections from '@/components/courses/CourseSections.vue'
import CourseProgress from '@/components/courses/CourseProgress.vue'
import LessonList from '@/components/courses/LessonList.vue'
import LessonDetail from '@/components/courses/LessonDetail.vue'
import LessonNavigation from '@/components/courses/LessonNavigation.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const courseId = computed(() => Number(route.params.id))

const {
  data: course,
  isLoading: courseLoading,
  isError: courseIsError,
  error: courseError,
} = useCourseDetailQuery(courseId)

const {
  sections,
  lessons,
  selectedLesson,
  selectedSectionId,
  selectedLessonId,
  sectionsLoading,
  sectionsIsError,
  lessonsLoading,
  lessonsIsError,
  lessonLoading,
  lessonIsError,
  selectSection,
  openLesson,
  startLearning,
} = useCourseContent(courseId)

const lessonIds = computed(() => lessons.value.map((l) => l.id))

const {
  completedLessons,
  completedCount,
  progressPercent,
  pendingLessonId,
  progressError,
  toggleLesson,
} = useLessonProgress(lessonIds, courseId)
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
    <Button
      :label="t('courses.backToCatalog')"
      icon="pi pi-arrow-left"
      text
      severity="secondary"
      @click="router.push('/home')"
    />

    <Skeleton v-if="courseLoading" height="12rem" />

    <Message v-else-if="courseIsError" severity="error" :closable="false">
      {{ getApiErrorMessage(courseError, t('courses.loadError')) }}
    </Message>

    <template v-else-if="course">
      <CourseHeader
        :title="course.title"
        :description="course.description"
        :can-start="lessons.length > 0"
        @start="startLearning"
      />

      <section class="space-y-4">
        <Skeleton v-if="sectionsLoading || lessonsLoading" height="8rem" />

        <Message v-else-if="sectionsIsError || lessonsIsError" severity="error" :closable="false">
          {{ t('courses.loadError') }}
        </Message>

        <template v-else>
          <CourseSections
            :sections="sections"
            :selected-section-id="selectedSectionId"
            @select="selectSection"
          />

          <CourseProgress
            v-if="lessons.length > 0"
            :completed="completedCount"
            :total="lessons.length"
            :percentage="progressPercent"
          />

          <Message v-if="progressError" severity="error" :closable="false">
            {{ t('courses.progressError') }}
          </Message>

          <LessonList
            :lessons="lessons"
            :selected-lesson-id="selectedLessonId"
            :completed-lessons="completedLessons"
            :pending-lesson-id="pendingLessonId"
            @select="openLesson"
            @toggle="toggleLesson"
          />

          <Message v-if="sections.length === 0" severity="info" :closable="false">
            {{ t('courses.empty') }}
          </Message>
        </template>
      </section>

      <!-- Obsah vybrané lekce a navigace -->
      <div v-if="selectedLessonId !== null" class="space-y-4">
        <LessonDetail :lesson="selectedLesson" :loading="lessonLoading" :error="lessonIsError" />

        <LessonNavigation
          :lessons="lessons"
          :selected-lesson-id="selectedLessonId"
          @select="openLesson"
        />
      </div>
    </template>

    <Message v-else severity="warn" :closable="false">
      {{ t('courses.courseNotFound') }}
    </Message>
  </div>
</template>
