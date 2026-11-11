<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const { data: coursesResponse, isLoading, isError, error } = useCoursesQuery({
  Page: 1,
  PageSize: 20,
})
const courses = computed(() => coursesResponse.value?.data ?? [])
const coursesError = computed(() => getApiErrorMessage(error.value, t('courses.loadError')))
</script>

<template>
  <div class="space-y-4">
    <div v-if="isLoading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <Skeleton v-for="n in 3" :key="n" height="10rem" class="rounded-xl" />
    </div>

    <div v-else-if="isError">
      <Message severity="error" :closable="false">{{ coursesError }}</Message>
    </div>

    <div
      v-else-if="courses.length === 0"
      class="p-8 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 text-center space-y-2"
    >
      <i class="oi oi-book text-3xl text-surface-400" />
      <p class="text-sm text-surface-500">{{ t('courses.empty') }}</p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <Card
        v-for="course in courses"
        :key="course.id"
        class="border border-surface-200 dark:border-surface-800"
      >
        <template #title>{{ course.title }}</template>
        <template #content>
          <p class="text-sm text-surface-600 dark:text-surface-400 line-clamp-3">
            {{ course.description }}
          </p>
        </template>
      </Card>
    </div>
  </div>
</template>
