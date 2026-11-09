<script setup lang="ts">
import { UserRoles } from '@/types/api'
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

const { data: thumbnailsResponse, isLoading, isError, error } = useCourseThumbnailsQuery({
  Page: 1,
  PageSize: 12,
  MaxDescriptionLength: 140,
})

const courses = computed(() => thumbnailsResponse.value?.data ?? [])
const coursesError = computed(() =>
  getApiErrorMessage(error.value, t('courses.loadError')),
)
</script>

<template>
  <div class="space-y-6">
    <!-- Uvítací sekce -->
    <div
      class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4"
    >
      <div class="space-y-1">
        <h1 class="text-2xl font-bold">{{ t('welcome') }}</h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          {{ t('auth.loggedInAs') }}
          <span class="font-semibold text-surface-900 dark:text-surface-0">
            {{ authStore.user?.username || authStore.user?.email }}
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

    <!-- Přehled kurzů (Katalog) -->
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-semibold">{{ t('nav.courses') }}</h2>
      </div>

      <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <Skeleton v-for="n in 6" :key="n" height="9rem" class="rounded-xl" />
      </div>

      <Message v-else-if="isError" severity="error" :closable="false">
        {{ coursesError }}
      </Message>

      <div
        v-else-if="courses.length === 0"
        class="p-8 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center text-surface-500 text-sm"
      >
        {{ t('courses.empty') }}
      </div>

      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <Card
          v-for="course in courses"
          :key="course.id"
          class="border border-surface-200 dark:border-surface-800"
        >
          <template #title>{{ course.title }}</template>
          <template #content>
            <p class="text-sm text-surface-600 dark:text-surface-400">
              {{ course.shortDescription }}
            </p>
          </template>
        </Card>
      </div>
    </section>
  </div>
</template>
