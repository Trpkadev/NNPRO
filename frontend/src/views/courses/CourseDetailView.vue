<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import { parsePositiveId } from '@/utils/route'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()
const authStore = useAuthStore()
const { canManageCourses } = usePermissions()

const courseId = computed(() => parsePositiveId(route.params.id))

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

// Přístup k obsahu určuje backend.
const canViewContent = computed(() => course.value?.hasAccess === true)

const {
  data: sectionsResponse,
  isLoading: sectionsLoading,
  isError: sectionsError,
  error: sectionsErrorObj,
} = useCourseSectionsQuery(
  courseId,
  computed(() => ({
    Page: sectionsPage.value,
    PageSize: 20,
    SortBy: 'Order',
    Descending: false,
  })),
  canViewContent,
)

const { data: myProgressResponse } = useMyProgressQuery({
  Page: 1,
  PageSize: 100,
})

const unenrollMutation = useUnenrollCourseMutation()

const instructors = computed(() => instructorsResponse.value?.data ?? [])

const sections = computed(() =>
  [...(sectionsResponse.value?.data ?? [])].sort((a, b) => a.order - b.order),
)

const sectionsPagination = computed(() => sectionsResponse.value?.metadata?.pagination ?? null)

// Student má přístup ke kurzu prostřednictvím zápisu.
// Pro lektory a adminy používáme hasAccess pouze pro přístup k obsahu.
const isEnrolled = computed(() => authStore.isStudent && canViewContent.value)

const completedLessonIds = computed(() => {
  const ids = new Set<number>()

  for (const item of myProgressResponse.value?.data ?? []) {
    if (item.courseId === courseId.value && item.isCompleted) {
      ids.add(item.lessonId)
    }
  }

  return ids
})

const sectionLessonIdsMap = reactive<Record<number, number[]>>({})

function handleSectionLessonsLoaded(sectionId: number, lessonIds: number[]) {
  sectionLessonIdsMap[sectionId] = lessonIds
}

watch(courseId, () => {
  sectionsPage.value = 1

  for (const key of Object.keys(sectionLessonIdsMap)) {
    delete sectionLessonIdsMap[Number(key)]
  }
})

const areAllSectionsLoaded = computed(
  () =>
    canViewContent.value &&
    sections.value.length > 0 &&
    sections.value.every((s) => sectionLessonIdsMap[s.id] !== undefined),
)

const courseLessonIds = computed(() => {
  const allIds = new Set<number>()

  if (!canViewContent.value) return allIds

  for (const section of sections.value) {
    for (const id of sectionLessonIdsMap[section.id] ?? []) {
      allIds.add(id)
    }
  }

  return allIds
})

const totalCourseLessons = computed(() => courseLessonIds.value.size)

const completedCount = computed(() => {
  let count = 0

  for (const id of courseLessonIds.value) {
    if (completedLessonIds.value.has(id)) {
      count += 1
    }
  }

  return count
})

const progressPercent = computed(() => {
  if (!areAllSectionsLoaded.value || totalCourseLessons.value === 0) {
    return 0
  }

  return Math.min(100, Math.round((completedCount.value / totalCourseLessons.value) * 100))
})

const enrollDialogVisible = ref(false)

function handleUnenroll() {
  if (!isEnrolled.value || unenrollMutation.isPending.value) return

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
        v-if="canManageCourses && canViewContent && courseId > 0 && course"
        :label="t('courses.manageInStudio')"
        icon="oi oi-cog"
        severity="warn"
        outlined
        size="small"
        @click="router.push({ name: 'instructor-course-editor', params: { id: courseId } })"
      />
    </div>

    <Message v-if="courseId === 0" severity="error" :closable="false">
      {{ t('courses.courseNotFound') }}
    </Message>

    <div v-else-if="courseLoading" class="space-y-4">
      <Skeleton height="10rem" class="rounded-2xl" />
      <Skeleton height="14rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="courseError" severity="error" :closable="false">
      {{ getApiErrorMessage(courseErrorObj, t('courses.loadError')) }}
    </Message>

    <template v-else-if="course">
      <!-- Základní informace o kurzu -->
      <CourseOverviewCard
        :course="course"
        :instructors="instructors"
        :is-enrolled="isEnrolled"
        :is-student="authStore.isStudent"
        :unenroll-pending="unenrollMutation.isPending.value"
        :show-progress="canViewContent && areAllSectionsLoaded && totalCourseLessons > 0"
        :completed-count="completedCount"
        :total-course-lessons="totalCourseLessons"
        :progress-percent="progressPercent"
        @enroll="enrollDialogVisible = true"
        @unenroll="handleUnenroll"
      />

      <!-- Obsah kurzu -->
      <section class="space-y-4">
        <h2 class="text-lg font-bold">
          {{ t('courses.curriculum') }}
        </h2>

        <!-- Uživatel nemá přístup -->
        <div
          v-if="!canViewContent"
          class="p-6 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 text-center space-y-3"
        >
          <i class="oi oi-lock text-3xl text-surface-400" />

          <p class="font-semibold">
            {{ t('courses.contentLockedTitle') }}
          </p>

          <p class="text-sm text-surface-500">
            {{ t('courses.contentLockedDescription') }}
          </p>

          <Button
            v-if="authStore.isStudent"
            :label="t('courses.enrollBtn')"
            icon="oi oi-user-plus"
            size="small"
            @click="enrollDialogVisible = true"
          />
        </div>

        <!-- Načítání obsahu -->
        <div v-else-if="sectionsLoading" class="space-y-3">
          <Skeleton v-for="n in 3" :key="n" height="5rem" class="rounded-2xl" />
        </div>

        <!-- Chyba API -->
        <Message v-else-if="sectionsError" severity="error" :closable="false">
          {{ getApiErrorMessage(sectionsErrorObj) }}
        </Message>

        <!-- Kurz nemá žádné sekce -->
        <div
          v-else-if="sections.length === 0"
          class="p-8 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center text-surface-500 text-sm"
        >
          {{ t('courses.noSections') }}
        </div>

        <!-- Přístupný obsah kurzu -->
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

      <!-- Dialog pro zápis -->
      <EnrollDialog
        v-model:visible="enrollDialogVisible"
        :course-id="course.id"
        :course-title="course.title"
      />
    </template>

    <Message v-else severity="error" :closable="false">
      {{ t('courses.courseNotFound') }}
    </Message>
  </div>
</template>
