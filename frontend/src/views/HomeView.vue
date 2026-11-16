<script setup lang="ts">
import { UserRoles } from '@/types/api'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

// Načtení zapsaných kurzů a studijního postupu přihlášeného uživatele
const { data: myCoursesAllResponse } = useMyCoursesQuery({ Page: 1, PageSize: 100 })
const { data: myProgressResponse } = useMyProgressQuery({ Page: 1, PageSize: 100 })

const enrolledCourseIds = computed(
  () => new Set((myCoursesAllResponse.value?.data ?? []).map((c) => c.id)),
)
const progressItems = computed(() => myProgressResponse.value?.data ?? [])
const completedLessonsTotal = computed(
  () => progressItems.value.filter((p) => p.isCompleted).length,
)

const enrollDialogVisible = ref(false)
const enrollTargetCourse = ref<{ id: number; title: string } | null>(null)

function openEnrollDialog(course: { id: number; title: string }) {
  enrollTargetCourse.value = course
  enrollDialogVisible.value = true
}
</script>

<template>
  <div class="space-y-8">
    <!-- Uvítací přehledový banner -->
    <div
      class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col lg:flex-row lg:items-center justify-between gap-6 shadow-xs"
    >
      <div class="space-y-1.5">
        <div class="flex items-center gap-2">
          <h1 class="text-2xl font-bold">{{ t('welcome') }}, {{ authStore.user?.username }}!</h1>
        </div>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          {{ t('app.subtitle') }} &bull;
          <span>
            {{ t('admin.enrollmentsCount') }}:
            <strong class="text-surface-900 dark:text-surface-0">{{ enrolledCourseIds.size }}</strong>
          </span>
          &bull;
          <span>
            {{ t('lessons.completed') }}:
            <strong class="text-primary">{{ completedLessonsTotal }}</strong>
          </span>
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <Button
          :label="t('nav.profile')"
          icon="oi oi-user"
          severity="secondary"
          outlined
          size="small"
          @click="router.push({ name: 'profile' })"
        />

        <RoleGuard :roles="[UserRoles.Instructor, UserRoles.Admin]">
          <Button
            :label="t('nav.instructorZone')"
            icon="oi oi-briefcase"
            severity="warn"
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

    <!-- Sekce: Moje zapsané kurzy -->
    <MyCoursesSection :progress-items="progressItems" />

    <!-- Sekce: Katalog všech kurzů (vyhledávání, řazení, infinite scroll) -->
    <CourseCatalogSection
      :enrolled-course-ids="enrolledCourseIds"
      :can-enroll-as-student="authStore.isStudent"
      @request-enroll="openEnrollDialog"
    />

    <!-- Dialog pro zápis do kurzu (veřejný i s přístupovým klíčem) -->
    <EnrollDialog
      v-model:visible="enrollDialogVisible"
      :course-id="enrollTargetCourse?.id ?? null"
      :course-title="enrollTargetCourse?.title"
      @enrolled="(id) => router.push({ name: 'course-detail', params: { id } })"
    />
  </div>
</template>
