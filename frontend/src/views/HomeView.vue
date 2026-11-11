<script setup lang="ts">
import { computed } from 'vue'
import { UserRoles } from '@/types/api'
import { getApiErrorMessage } from '@/services/api'
import CourseSearch from '@/components/courses/CourseSearch.vue'
import { useCourseFilters } from '@/composables/useCourseFilters'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

// Načítání kurzů z backendu
const {
  data: thumbnailsResponse,
  isLoading,
  isError,
  error,
} = useCourseThumbnailsQuery({
  Page: 1,
  PageSize: 12,
  MaxDescriptionLength: 140,
})

const courses = computed(() => thumbnailsResponse.value?.data ?? [])
const { search, filter, filteredCourses } = useCourseFilters(courses)

const coursesError = computed(() => getApiErrorMessage(error.value, t('courses.loadError')))

function openCourse(id: number) {
  router.push({ name: 'course-detail', params: { id } })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Uvítací sekce -->
    <div
      class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4"
    >
      <div class="space-y-1">
        <h1 class="text-2xl font-bold">
          {{ t('welcome') }}
        </h1>

        <p v-if="authStore.user" class="text-sm text-surface-600 dark:text-surface-400">
          {{ t('auth.loggedInAs') }}
          <span class="font-semibold text-surface-900 dark:text-surface-0">
            {{ authStore.user.username || authStore.user.email }}
          </span>
        </p>
      </div>

      <div class="flex items-center gap-2">
        <RoleGuard :roles="[UserRoles.Instructor, UserRoles.Admin]">
          <Button
            :label="t('nav.instructorZone')"
            icon="oi oi-briefcase"
            severity="secondary"
            outlined
            size="small"
            @click="router.push({ name: 'instructor-dashboard' })"
          />
        </RoleGuard>

        <RoleGuard :roles="[UserRoles.Admin]">
          <Button
            :label="t('nav.adminZone')"
            icon="oi oi-shield"
            severity="danger"
            outlined
            size="small"
            @click="router.push({ name: 'admin-dashboard' })"
          />
        </RoleGuard>
      </div>
    </div>

    <!-- Katalog kurzů -->
    <section class="space-y-4">
      <h2 class="text-lg font-semibold">
        {{ t('nav.courses') }}
      </h2>

      <!-- Vyhledávání je mimo nadpis -->
      <CourseSearch v-model="search" v-model:filter="filter" />

      <!-- Načítání -->
      <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <Skeleton v-for="n in 6" :key="n" height="9rem" class="rounded-xl" />
      </div>

      <!-- Chyba API -->
      <Message v-else-if="isError" severity="error" :closable="false">
        {{ coursesError }}
      </Message>

      <!-- Prázdný katalog nebo žádné výsledky -->
      <div
        v-else-if="filteredCourses.length === 0"
        class="p-8 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center text-surface-500 text-sm"
      >
        {{ search.trim() ? t('courses.noSearchResults') : t('courses.empty') }}
      </div>

      <!-- Kurzy z backendu -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <Card
          v-for="course in filteredCourses"
          :key="course.id"
          class="border border-surface-200 dark:border-surface-800 transition-colors hover:border-emerald-500"
        >
          <template #title>
            <RouterLink
              :to="{ name: 'course-detail', params: { id: course.id } }"
              class="block text-inherit no-underline hover:text-emerald-500 focus-visible:outline-2 focus-visible:outline-emerald-500 rounded"
            >
              {{ course.title }}
            </RouterLink>
          </template>

          <template #content>
            <p class="text-sm text-surface-600 dark:text-surface-400">
              {{ course.shortDescription }}
            </p>

            <div class="mt-4">
              <Button
                :label="t('courses.courseContent')"
                icon="oi oi-arrow-right"
                iconPos="right"
                severity="secondary"
                text
                size="small"
                @click="openCourse(course.id)"
              />
            </div>
          </template>
        </Card>
      </div>
    </section>
  </div>
</template>
