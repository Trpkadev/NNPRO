<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import type { UserResponseDto } from '@/types/api'

const props = defineProps<{
  courseId: number
}>()

const { t } = useI18n()
const confirm = useConfirm()
const toast = useToast()

const page = ref(1)
const studentIdToEnroll = ref<number | null>(null)
const errorMessage = ref<string | null>(null)

const {
  data: enrollmentsResponse,
  isLoading,
  isError,
  error,
} = useCourseEnrollmentsQuery(
  computed(() => props.courseId),
  computed(() => ({ Page: page.value, PageSize: 15 })),
)

const forceEnrollMutation = useForceEnrollMutation()
const removeStudentMutation = useRemoveStudentEnrollmentMutation()

const students = computed(() => enrollmentsResponse.value?.data ?? [])
const pagination = computed(() => enrollmentsResponse.value?.metadata?.pagination ?? null)

async function handleForceEnroll() {
  errorMessage.value = null
  const id = Number(studentIdToEnroll.value)
  if (!Number.isFinite(id) || id <= 0) {
    errorMessage.value = t('validation.required')
    return
  }

  try {
    await forceEnrollMutation.mutateAsync({
      courseId: props.courseId,
      studentId: id,
    })
    studentIdToEnroll.value = null
    toast.add({
      severity: 'success',
      summary: t('instructor.studentEnrolledSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

function confirmRemoveStudent(student: UserResponseDto) {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('instructor.removeStudentConfirm', { username: student.username }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await removeStudentMutation.mutateAsync({
          courseId: props.courseId,
          studentId: student.id,
        })
        toast.add({
          severity: 'info',
          summary: t('instructor.studentRemovedSuccess'),
          life: 3000,
        })
      } catch (err) {
        errorMessage.value = getApiErrorMessage(err)
      }
    },
  })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Formulář pro přímý zápis studenta lektorem/adminem -->
    <form
      class="p-4 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col sm:flex-row sm:items-end gap-3"
      @submit.prevent="handleForceEnroll"
    >
      <div class="flex-1 space-y-1">
        <label for="force-enroll-student-id" class="block text-xs font-medium">
          {{ t('instructor.forceEnrollLabel') }}
        </label>
        <InputNumber
          input-id="force-enroll-student-id"
          v-model="studentIdToEnroll"
          :placeholder="t('instructor.forceEnrollPlaceholder')"
          :min="1"
          size="small"
          class="w-full"
        />
      </div>
      <Button
        type="submit"
        :label="t('instructor.forceEnrollBtn')"
        icon="oi oi-user-plus"
        size="small"
        :loading="forceEnrollMutation.isPending.value"
      />
    </form>

    <Message v-if="errorMessage || isError" severity="error" :closable="false">
      {{ errorMessage || getApiErrorMessage(error) }}
    </Message>

    <!-- Tabulka zapsaných studentů -->
    <div
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden p-4 space-y-4"
    >
      <h3 class="font-bold text-base">{{ t('instructor.enrolledStudentsTitle') }}</h3>

      <div v-if="isLoading" class="space-y-2">
        <Skeleton v-for="n in 4" :key="n" height="3rem" class="rounded-xl" />
      </div>

      <p
        v-else-if="students.length === 0"
        class="text-sm text-surface-500 text-center py-6"
      >
        {{ t('instructor.noEnrolledStudents') }}
      </p>

      <DataTable v-else :value="students" striped-rows responsive-layout="scroll">
        <Column field="id" header="ID" />
        <Column field="username" :header="t('admin.username')" />
        <Column field="email" :header="t('admin.email')" />
        <Column field="role" :header="t('admin.role')">
          <template #body="{ data }">
            <Tag :value="data.role" severity="info" />
          </template>
        </Column>
        <Column :header="t('common.actions')">
          <template #body="{ data }">
            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
              :aria-label="t('common.delete')"
              @click="confirmRemoveStudent(data)"
            />
          </template>
        </Column>
      </DataTable>

      <PaginationControls
        :pagination="pagination"
        :loading="isLoading"
        @update:page="page = $event"
      />
    </div>
  </div>
</template>
