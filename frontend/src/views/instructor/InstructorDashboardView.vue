<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import type { CourseResponseDto } from '@/types/api'

const { t } = useI18n()
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()

const page = ref(1)
const searchQuery = ref('')
const debouncedSearch = refDebounced(searchQuery, 350)
const sortBy = ref<'Id' | 'Title'>('Id')
const descending = ref(false)

watch([debouncedSearch, sortBy, descending], () => {
  page.value = 1
})

const {
  data: coursesResponse,
  isLoading,
  isError,
  error,
} = useCoursesQuery(
  computed(() => ({
    Page: page.value,
    PageSize: 12,
    SortBy: sortBy.value,
    Descending: descending.value,
    Filters: debouncedSearch.value.trim() ? { title: debouncedSearch.value.trim() } : undefined,
  })),
)

const deleteCourseMutation = useDeleteCourseMutation()

const courses = computed(() => coursesResponse.value?.data ?? [])
const pagination = computed(() => coursesResponse.value?.metadata?.pagination ?? null)
const coursesError = computed(() => getApiErrorMessage(error.value, t('courses.loadError')))

const courseDialogVisible = ref(false)
const editingCourse = ref<CourseResponseDto | null>(null)

function openCreateCourse() {
  editingCourse.value = null
  courseDialogVisible.value = true
}

function openEditCourse(course: CourseResponseDto) {
  editingCourse.value = course
  courseDialogVisible.value = true
}

function confirmDeleteCourse(course: CourseResponseDto) {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('instructor.deleteCourseConfirm', { title: course.title }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await deleteCourseMutation.mutateAsync(course.id)
        toast.add({
          severity: 'info',
          summary: t('instructor.courseDeleted'),
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
    <!-- Horní lišta s vyhledáváním, řazením a tlačítkem Nový kurz -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h2 class="text-lg font-bold">{{ t('instructor.studioTitle') }}</h2>
        <p class="text-xs text-surface-500">{{ t('instructor.studioSubtitle') }}</p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <InputText
          v-model="searchQuery"
          :placeholder="t('courses.searchPlaceholder')"
          size="small"
          class="w-full sm:w-60"
        />

        <Button
          :label="sortBy === 'Title' ? t('courses.sortTitle') : t('courses.sortId')"
          icon="oi oi-sort-alt"
          severity="secondary"
          outlined
          size="small"
          @click="sortBy = sortBy === 'Id' ? 'Title' : 'Id'"
        />

        <Button
          :icon="descending ? 'oi oi-sort-amount-down' : 'oi oi-sort-amount-up'"
          severity="secondary"
          outlined
          size="small"
          :aria-label="descending ? t('common.sortDesc') : t('common.sortAsc')"
          @click="descending = !descending"
        />

        <Button
          :label="t('instructor.createCourse')"
          icon="oi oi-plus"
          size="small"
          @click="openCreateCourse"
        />
      </div>
    </div>

    <!-- Načítání -->
    <div v-if="isLoading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <Skeleton v-for="n in 6" :key="n" height="11rem" class="rounded-2xl" />
    </div>

    <div v-else-if="isError">
      <Message severity="error" :closable="false">{{ coursesError }}</Message>
    </div>

    <div
      v-else-if="courses.length === 0"
      class="p-8 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 text-center space-y-3"
    >
      <i class="oi oi-book text-3xl text-surface-400" />
      <p class="text-sm text-surface-500">{{ t('courses.empty') }}</p>
      <Button
        :label="t('instructor.createCourse')"
        icon="oi oi-plus"
        size="small"
        @click="openCreateCourse"
      />
    </div>

    <div v-else class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="course in courses"
          :key="course.id"
          class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col justify-between gap-4 hover:border-primary/50 transition-colors shadow-xs"
        >
          <div class="space-y-2">
            <div class="flex items-start justify-between gap-2">
              <h3 class="font-bold text-base text-surface-900 dark:text-surface-0 line-clamp-1">
                {{ course.title }}
              </h3>
              <span class="text-xs font-mono text-surface-400">#{{ course.id }}</span>
            </div>

            <p class="text-sm text-surface-600 dark:text-surface-400 line-clamp-3">
              {{ course.description }}
            </p>
          </div>

          <div
            class="flex items-center justify-between gap-2 pt-3 border-t border-surface-100 dark:border-surface-800"
          >
            <Button
              :label="t('instructor.openEditor')"
              icon="oi oi-cog"
              size="small"
              class="flex-1"
              @click="router.push({ name: 'instructor-course-editor', params: { id: course.id } })"
            />
            <Button
              icon="oi oi-pencil"
              severity="secondary"
              text
              rounded
              size="small"
              :aria-label="t('instructor.editCourse')"
              @click="openEditCourse(course)"
            />
            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
              :aria-label="t('common.delete')"
              @click="confirmDeleteCourse(course)"
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

    <!-- Dialog pro vytvoření / úpravu kurzu -->
    <CourseFormDialog
      v-model:visible="courseDialogVisible"
      :course="editingCourse"
      @saved="
        (courseId) => {
          if (!editingCourse) {
            router.push({ name: 'instructor-course-editor', params: { id: courseId } })
          }
        }
      "
    />
  </div>
</template>
