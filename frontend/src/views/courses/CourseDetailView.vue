<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()
const authStore = useAuthStore()
const { canManageCourses } = usePermissions()

const courseId = computed(() => Number(route.params.id))

const {
  data: course,
  isLoading: courseLoading,
  isError: courseError,
  error: courseErrorObj,
} = useCourseDetailQuery(courseId)

const { data: instructorsResponse } = useCourseInstructorsQuery(courseId, {
  Page: 1,
  PageSize: 20,
})

const sectionsPage = ref(1)
const { data: sectionsResponse, isLoading: sectionsLoading } = useCourseSectionsQuery(
  courseId,
  computed(() => ({
    Page: sectionsPage.value,
    PageSize: 20,
    SortBy: 'Order',
    Descending: false,
  })),
)

const { data: myCoursesResponse } = useMyCoursesQuery({ Page: 1, PageSize: 100 })
const { data: myProgressResponse } = useMyProgressQuery({ Page: 1, PageSize: 100 })

const unenrollMutation = useUnenrollCourseMutation()

const instructors = computed(() => instructorsResponse.value?.data ?? [])
const sections = computed(() =>
  [...(sectionsResponse.value?.data ?? [])].sort((a, b) => a.order - b.order),
)
const sectionsPagination = computed(() => sectionsResponse.value?.metadata?.pagination ?? null)

const isEnrolled = computed(() =>
  (myCoursesResponse.value?.data ?? []).some((c) => c.id === courseId.value),
)

const completedLessonIds = computed(() => {
  const ids = new Set<number>()
  for (const item of myProgressResponse.value?.data ?? []) {
    if (item.courseId === courseId.value && item.isCompleted) {
      ids.add(item.lessonId)
    }
  }
  return ids
})

// Evidence načtených lekcí ze sekcí pro přesný výpočet celkového procenta dokončení kurzu
const sectionLessonIdsMap = reactive<Record<number, number[]>>({})

function handleSectionLessonsLoaded(sectionId: number, lessonIds: number[]) {
  sectionLessonIdsMap[sectionId] = lessonIds
}

const totalCourseLessons = computed(() => {
  const allIds = new Set<number>()
  for (const ids of Object.values(sectionLessonIdsMap)) {
    for (const id of ids) allIds.add(id)
  }
  for (const item of myProgressResponse.value?.data ?? []) {
    if (item.courseId === courseId.value) allIds.add(item.lessonId)
  }
  return allIds.size
})

const completedCount = computed(() => completedLessonIds.value.size)

const progressPercent = computed(() => {
  if (totalCourseLessons.value === 0) return 0
  return Math.min(100, Math.round((completedCount.value / totalCourseLessons.value) * 100))
})

const enrollDialogVisible = ref(false)

function handleUnenroll() {
  confirm.require({
    header: t('courses.unenrollBtn'),
    message: t('courses.unenrollConfirm'),
    acceptLabel: t('common.yes'),
    rejectLabel: t('common.no'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await unenrollMutation.mutateAsync(courseId.value)
        toast.add({
          severity: 'info',
          summary: t('courses.unenrollSuccess'),
          life: 3000,
        })
      } catch (err) {
        toast.add({
          severity: 'error',
          summary: getApiErrorMessage(err),
          life: 4000,
        })
      }
    },
  })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Horní navigace -->
    <div class="flex flex-wrap items-center justify-between gap-3">
      <Button
        :label="t('courses.backToCatalog')"
        icon="oi oi-arrow-left"
        severity="secondary"
        text
        size="small"
        @click="router.push({ name: 'home' })"
      />

      <Button
        v-if="canManageCourses"
        :label="t('courses.manageInStudio')"
        icon="oi oi-cog"
        severity="warn"
        outlined
        size="small"
        @click="router.push({ name: 'instructor-course-editor', params: { id: courseId } })"
      />
    </div>

    <div v-if="courseLoading" class="space-y-4">
      <Skeleton height="10rem" class="rounded-2xl" />
      <Skeleton height="14rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="courseError" severity="error" :closable="false">
      {{ getApiErrorMessage(courseErrorObj, t('courses.loadError')) }}
    </Message>

    <template v-else-if="course">
      <!-- Hlavička kurzu + Zápis / Odhlášení + Lektoři -->
      <div
        class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-5 shadow-xs"
      >
        <div class="flex flex-col md:flex-row md:items-start justify-between gap-4">
          <div class="space-y-2 flex-1">
            <div class="flex items-center gap-2.5 flex-wrap">
              <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
                {{ course.title }}
              </h1>
              <Tag
                v-if="isEnrolled"
                :value="t('courses.enrolledBadge')"
                severity="success"
              />
            </div>

            <p class="text-sm text-surface-600 dark:text-surface-300 whitespace-pre-line">
              {{ course.description }}
            </p>
          </div>

          <!-- Tlačítko pro zápis nebo odhlášení z kurzu (pro studenty) -->
          <div v-if="authStore.isStudent" class="shrink-0">
            <Button
              v-if="!isEnrolled"
              :label="t('courses.enrollBtn')"
              icon="oi oi-plus"
              @click="enrollDialogVisible = true"
            />
            <Button
              v-else
              :label="t('courses.unenrollBtn')"
              icon="oi oi-sign-out"
              severity="danger"
              outlined
              size="small"
              :loading="unenrollMutation.isPending.value"
              @click="handleUnenroll"
            />
          </div>
        </div>

        <!-- Lektoři kurzu -->
        <div class="pt-4 border-t border-surface-100 dark:border-surface-800 flex flex-wrap items-center justify-between gap-4">
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
          v-if="totalCourseLessons > 0 || completedCount > 0"
          class="p-4 rounded-xl bg-surface-50 dark:bg-surface-800/50 border border-surface-200/70 dark:border-surface-800 space-y-2"
        >
          <div class="flex items-center justify-between text-xs font-semibold">
            <span>{{ t('courses.overallProgress') }}</span>
            <span class="text-primary">
              {{
                t('courses.completedLessonsCount', {
                  completed: completedCount,
                  total: Math.max(totalCourseLessons, completedCount),
                  percent: progressPercent,
                })
              }}
            </span>
          </div>
          <ProgressBar :value="progressPercent" :show-value="false" class="!h-2.5" />
        </div>
      </div>

      <!-- Tematické sekce, lekce a kvízy -->
      <section class="space-y-4">
        <h2 class="text-lg font-bold">{{ t('courses.curriculum') }}</h2>

        <div v-if="sectionsLoading" class="space-y-3">
          <Skeleton v-for="n in 3" :key="n" height="5rem" class="rounded-2xl" />
        </div>

        <div
          v-else-if="sections.length === 0"
          class="p-8 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center text-surface-500 text-sm"
        >
          {{ t('courses.noSections') }}
        </div>

        <div v-else class="space-y-3">
          <CourseSectionItem
            v-for="(section, idx) in sections"
            :key="section.id"
            :section="section"
            :course-id="courseId"
            :completed-lesson-ids="completedLessonIds"
            :initially-expanded="idx === 0"
            @lessons-loaded="handleSectionLessonsLoaded"
          />

          <PaginationControls
            :pagination="sectionsPagination"
            :loading="sectionsLoading"
            @update:page="sectionsPage = $event"
          />
        </div>
      </section>

      <EnrollDialog
        v-model:visible="enrollDialogVisible"
        :course-id="course.id"
        :course-title="course.title"
      />
    </template>
  </div>
</template>
