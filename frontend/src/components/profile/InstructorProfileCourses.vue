<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import { useManageableCoursesQuery } from '@/composables/useCoursesQueries'

const { t } = useI18n()

const {
  data: coursesResponse,
  isLoading,
  isError,
  error,
} = useManageableCoursesQuery({
  Page: 1,
  PageSize: 5,
})

const courses = computed(() => coursesResponse.value?.data ?? [])
const pagination = computed(() => coursesResponse.value?.metadata?.pagination ?? null)
const totalCourses = computed(() => pagination.value?.totalItems ?? courses.value.length)
</script>

<template>
  <div class="space-y-5">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <div
        class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex items-center gap-4"
      >
        <div
          class="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center"
        >
          <i class="oi oi-briefcase text-xl" />
        </div>

        <div>
          <p class="text-sm text-surface-500">
            {{ t('profile.managedCourses') }}
          </p>
          <Skeleton v-if="isLoading" width="4rem" height="2rem" />
          <p v-else class="text-2xl font-bold">
            {{ isError ? '—' : totalCourses }}
          </p>
        </div>
      </div>

      <RouterLink
        :to="{ name: 'instructor-dashboard' }"
        class="group p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 hover:border-primary/50 transition-colors flex items-center gap-4 no-underline"
      >
        <div
          class="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center"
        >
          <i class="oi oi-cog text-xl" />
        </div>

        <div class="flex-1">
          <p class="font-semibold text-surface-900 dark:text-surface-0">
            {{ t('nav.instructorZone') }}
          </p>
          <p class="text-sm text-surface-500">
            {{ t('profile.manageCoursesDescription') }}
          </p>
        </div>

        <i class="oi oi-chevron-right text-surface-400 group-hover:text-primary" />
      </RouterLink>
    </div>

    <section
      class="p-5 sm:p-6 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 space-y-4"
    >
      <h2 class="text-lg font-bold">
        {{ t('profile.myManagedCourses') }}
      </h2>

      <div v-if="isLoading" class="space-y-3">
        <Skeleton v-for="n in 3" :key="n" height="4rem" class="rounded-xl" />
      </div>

      <Message v-else-if="isError" severity="error" :closable="false">
        {{ getApiErrorMessage(error) }}
      </Message>

      <p v-else-if="courses.length === 0" class="text-sm text-surface-500 py-6 text-center">
        {{ t('profile.noManagedCourses') }}
      </p>

      <div v-else class="space-y-3">
        <RouterLink
          v-for="course in courses"
          :key="course.id"
          :to="{
            name: 'instructor-course-editor',
            params: { id: course.id },
          }"
          class="group flex items-center gap-3 p-4 rounded-xl border border-surface-200 dark:border-surface-700 hover:border-primary/50 transition-colors no-underline"
        >
          <div
            class="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0"
          >
            <i class="oi oi-book" />
          </div>

          <div class="min-w-0 flex-1">
            <p
              class="font-semibold text-sm text-surface-900 dark:text-surface-0 group-hover:text-primary break-words"
            >
              {{ course.title }}
            </p>
            <p class="text-xs text-surface-500 line-clamp-1">
              {{ course.description }}
            </p>
          </div>

          <i class="oi oi-chevron-right text-surface-400" />
        </RouterLink>
      </div>
    </section>
  </div>
</template>
