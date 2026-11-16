<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import type { SectionResponseDto } from '@/types/api'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()

const courseId = computed(() => Number(route.params.id))

const {
  data: course,
  isLoading: courseLoading,
  isError: courseError,
  error: courseErrorObj,
} = useCourseDetailQuery(courseId)

const { data: sectionsResponse, isLoading: sectionsLoading } = useCourseSectionsQuery(courseId, {
  Page: 1,
  PageSize: 50,
  SortBy: 'Order',
  Descending: false,
})

const createSectionMutation = useCreateSectionMutation()
const updateSectionMutation = useUpdateSectionMutation()
const deleteSectionMutation = useDeleteSectionMutation()

const sections = computed(() =>
  [...(sectionsResponse.value?.data ?? [])].sort((a, b) => a.order - b.order),
)

const activeTab = ref<'curriculum' | 'students'>('curriculum')
const editCourseDialogVisible = ref(false)

// Stav dialogu pro sekci
const sectionDialogVisible = ref(false)
const editingSection = ref<SectionResponseDto | null>(null)
const sectionTitle = ref('')
const sectionOrder = ref(1)
const sectionError = ref<string | null>(null)

function openAddSection() {
  editingSection.value = null
  sectionTitle.value = ''
  sectionOrder.value = sections.value.length + 1
  sectionError.value = null
  sectionDialogVisible.value = true
}

function openEditSection(section: SectionResponseDto) {
  editingSection.value = section
  sectionTitle.value = section.title
  sectionOrder.value = section.order
  sectionError.value = null
  sectionDialogVisible.value = true
}

async function handleSaveSection() {
  sectionError.value = null
  if (!sectionTitle.value.trim()) {
    sectionError.value = t('validation.required')
    return
  }

  const payload = {
    title: sectionTitle.value.trim(),
    order: Math.max(1, Number(sectionOrder.value) || 1),
    courseId: courseId.value,
  }

  try {
    if (editingSection.value) {
      await updateSectionMutation.mutateAsync({
        sectionId: editingSection.value.id,
        payload,
      })
    } else {
      await createSectionMutation.mutateAsync(payload)
    }
    sectionDialogVisible.value = false
  } catch (err) {
    sectionError.value = getApiErrorMessage(err)
  }
}

function confirmDeleteSection(section: SectionResponseDto) {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('instructor.deleteSectionConfirm', { title: section.title }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await deleteSectionMutation.mutateAsync(section.id)
      } catch (err) {
        toast.add({ severity: 'error', summary: getApiErrorMessage(err), life: 4000 })
      }
    },
  })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Horní lišta s návratem na přehled lektora -->
    <div class="flex flex-wrap items-center justify-between gap-3">
      <Button
        :label="t('common.back')"
        icon="oi oi-arrow-left"
        severity="secondary"
        text
        size="small"
        @click="router.push({ name: 'instructor-dashboard' })"
      />

      <div class="flex items-center gap-2">
        <Button
          :label="t('courses.openCourse')"
          icon="oi oi-eye"
          severity="secondary"
          outlined
          size="small"
          @click="router.push({ name: 'course-detail', params: { id: courseId } })"
        />
        <Button
          :label="t('instructor.editCourse')"
          icon="oi oi-pencil"
          severity="secondary"
          outlined
          size="small"
          @click="editCourseDialogVisible = true"
        />
      </div>
    </div>

    <div v-if="courseLoading" class="space-y-4">
      <Skeleton height="6rem" class="rounded-2xl" />
      <Skeleton height="16rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="courseError" severity="error" :closable="false">
      {{ getApiErrorMessage(courseErrorObj) }}
    </Message>

    <template v-else-if="course">
      <!-- Záhlaví kurzu a přepínač záložek (Struktura obsahu vs. Zapsaní studenti) -->
      <div
        class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
      >
        <div>
          <span class="text-xs font-mono text-surface-400">
            {{ t('instructor.courseBadge', { id: course.id }) }}
          </span>
          <h2 class="text-2xl font-bold">{{ course.title }}</h2>
          <p class="text-sm text-surface-600 dark:text-surface-400 mt-1">
            {{ course.description }}
          </p>
        </div>

        <div
          class="flex flex-wrap items-center justify-between gap-3 pt-4 border-t border-surface-100 dark:border-surface-800"
        >
          <div class="flex items-center gap-2">
            <Button
              :label="t('instructor.tabCurriculum')"
              icon="oi oi-list"
              size="small"
              :severity="activeTab === 'curriculum' ? 'primary' : 'secondary'"
              :outlined="activeTab !== 'curriculum'"
              @click="activeTab = 'curriculum'"
            />
            <Button
              :label="t('instructor.tabStudents')"
              icon="oi oi-users"
              size="small"
              :severity="activeTab === 'students' ? 'primary' : 'secondary'"
              :outlined="activeTab !== 'students'"
              @click="activeTab = 'students'"
            />
          </div>

          <Button
            v-if="activeTab === 'curriculum'"
            :label="t('instructor.addSection')"
            icon="oi oi-plus"
            size="small"
            @click="openAddSection"
          />
        </div>
      </div>

      <!-- Záložka 1: Struktura a obsah kurzu (sekce, lekce, přílohy, kvízy, otázky) -->
      <div v-if="activeTab === 'curriculum'" class="space-y-4">
        <div v-if="sectionsLoading" class="space-y-3">
          <Skeleton v-for="n in 2" :key="n" height="8rem" class="rounded-2xl" />
        </div>

        <div
          v-else-if="sections.length === 0"
          class="p-8 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center space-y-3"
        >
          <p class="text-sm text-surface-500">{{ t('courses.noSections') }}</p>
          <Button
            :label="t('instructor.addSection')"
            icon="oi oi-plus"
            size="small"
            @click="openAddSection"
          />
        </div>

        <div v-else class="space-y-4">
          <InstructorSectionPanel
            v-for="section in sections"
            :key="section.id"
            :section="section"
            @edit-section="openEditSection"
            @delete-section="confirmDeleteSection"
          />
        </div>
      </div>

      <!-- Záložka 2: Správa zapsaných studentů -->
      <CourseStudentsManager v-else :course-id="courseId" />

      <!-- Dialog pro úpravu kurzu -->
      <CourseFormDialog v-model:visible="editCourseDialogVisible" :course="course" />

      <!-- Dialog pro vytvoření / úpravu sekce -->
      <Dialog
        v-model:visible="sectionDialogVisible"
        modal
        :header="editingSection ? t('instructor.editSection') : t('instructor.addSection')"
        :style="{ width: '28rem', maxWidth: '95vw' }"
      >
        <form class="space-y-4" @submit.prevent="handleSaveSection">
          <Message v-if="sectionError" severity="error" :closable="false">
            {{ sectionError }}
          </Message>

          <div class="space-y-1">
            <label for="section-title-input" class="block text-sm font-medium">
              {{ t('instructor.sectionTitleLabel') }}
            </label>
            <InputText id="section-title-input" v-model="sectionTitle" class="w-full" required />
          </div>

          <div class="space-y-1">
            <label for="section-order-input" class="block text-sm font-medium">
              {{ t('instructor.sectionOrderLabel') }}
            </label>
            <InputNumber
              input-id="section-order-input"
              v-model="sectionOrder"
              :min="1"
              class="w-full"
            />
          </div>

          <div class="flex justify-end gap-2">
            <Button
              type="button"
              :label="t('common.cancel')"
              severity="secondary"
              text
              @click="sectionDialogVisible = false"
            />
            <Button
              type="submit"
              :label="t('common.save')"
              icon="oi oi-check"
              :loading="
                createSectionMutation.isPending.value || updateSectionMutation.isPending.value
              "
            />
          </div>
        </form>
      </Dialog>
    </template>
  </div>
</template>
