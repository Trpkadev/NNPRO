<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { UserLessonProgressResponseDto } from '@/types/api'

const props = defineProps<{
  progressItems: UserLessonProgressResponseDto[]
}>()

const { t } = useI18n()
const router = useRouter()

const page = ref(1)
const pageSize = 6

const {
  data: myCoursesResponse,
  isLoading,
  isError,
  error,
} = useMyCoursesQuery(
  computed(() => ({
    Page: page.value,
    PageSize: pageSize,
  })),
)

const myCourses = computed(() => myCoursesResponse.value?.data ?? [])
const pagination = computed(() => myCoursesResponse.value?.metadata?.pagination ?? null)
const errorMessage = computed(() => getApiErrorMessage(error.value, t('courses.loadError')))

function getCompletedCountForCourse(courseId: number): number {
  return props.progressItems.filter((p) => p.courseId === courseId && p.isCompleted).length
}
</script>

<template>
  <section class="space-y-4">
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-2.5">
        <div class="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center">
          <i class="oi oi-bookmark" />
        </div>
        <h2 class="text-lg font-bold">{{ t('nav.myCourses') }}</h2>
      </div>
    </div>

    <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <Skeleton v-for="n in 3" :key="n" height="10rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="isError" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

    <div
      v-else-if="myCourses.length === 0"
      class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center text-surface-500 text-sm"
    >
      {{ t('courses.myCoursesEmpty') }}
    </div>

    <div v-else class="space-y-4">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="course in myCourses"
          :key="course.id"
          class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col justify-between gap-4 hover:border-primary/50 transition-colors shadow-xs"
        >
          <div class="space-y-2">
            <div class="flex items-start justify-between gap-2">
              <h3 class="font-bold text-base text-surface-900 dark:text-surface-0 line-clamp-1">
                {{ course.title }}
              </h3>
              <Tag :value="t('courses.enrolledBadge')" severity="success" class="shrink-0" />
            </div>
            <p class="text-sm text-surface-600 dark:text-surface-400 line-clamp-2">
              {{ course.shortDescription }}
            </p>
          </div>

          <div class="space-y-3 pt-2 border-t border-surface-100 dark:border-surface-800">
            <div class="flex items-center justify-between text-xs text-surface-500">
              <span>{{ t('lessons.completed') }}:</span>
              <span class="font-semibold text-primary">
                {{ getCompletedCountForCourse(course.id) }} {{ t('lessons.lessons').toLowerCase() }}
              </span>
            </div>

            <Button
              :label="t('courses.continueStudy')"
              icon="oi oi-arrow-right"
              icon-pos="right"
              size="small"
              class="w-full"
              @click="router.push({ name: 'course-detail', params: { id: course.id } })"
            />
          </div>
        </div>
      </div>

      <PaginationControls
        :pagination="pagination"
        :loading="isLoading"
        @update:page="page = $event"
      />
    </div>
  </section>
</template>
