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
  isFetching,
  isError,
  error,
  refetch,
} = useCourseEnrollmentsQuery(
  computed(() => props.courseId),
  computed(() => ({ Page: page.value, PageSize: 15 })),
)

const forceEnrollMutation = useForceEnrollMutation()
const removeStudentMutation = useRemoveStudentEnrollmentMutation()

const students = computed(() => enrollmentsResponse.value?.data ?? [])
const pagination = computed(() => enrollmentsResponse.value?.metadata?.pagination ?? null)

const isEnrolling = computed(() => forceEnrollMutation.isPending.value)
const isRemoving = computed(() => removeStudentMutation.isPending.value)

const isValidStudentId = computed(
  () =>
    studentIdToEnroll.value !== null &&
    Number.isSafeInteger(studentIdToEnroll.value) &&
    studentIdToEnroll.value > 0 &&
    studentIdToEnroll.value <= 4294967295,
)

async function handleForceEnroll() {
  if (isEnrolling.value) return

  errorMessage.value = null

  if (!isValidStudentId.value || studentIdToEnroll.value === null) {
    errorMessage.value = t('validation.invalidNumberRange', {
      min: 1,
      max: 4294967295,
    })
    return
  }

  const studentId = studentIdToEnroll.value

  if (students.value.some((student) => student.id === studentId)) {
    errorMessage.value = t('instructor.studentAlreadyEnrolled')
    return
  }

  try {
    await forceEnrollMutation.mutateAsync({
      courseId: props.courseId,
      studentId,
    })

    studentIdToEnroll.value = null
    page.value = 1

    toast.add({
      severity: 'success',
      summary: t('instructor.studentEnrolledSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleRefresh() {
  if (isFetching.value) return

  errorMessage.value = null
  await refetch()
}

function confirmRemoveStudent(student: UserResponseDto) {
  if (isRemoving.value) return

  errorMessage.value = null

  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('instructor.removeStudentConfirm', { username: student.username }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },

    accept: async () => {
      if (isRemoving.value) return

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
  <div class="space-y-5">
    <!-- Manuální zápis studenta -->
    <form
      class="p-4 sm:p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-3"
      @submit.prevent="handleForceEnroll"
    >
      <div class="flex flex-col sm:flex-row sm:items-end gap-3">
        <div class="flex-1 min-w-0 space-y-2">
          <label for="force-enroll-student-id" class="block text-sm font-medium">
            {{ t('instructor.forceEnrollLabel') }}
          </label>

          <InputNumber
            input-id="force-enroll-student-id"
            v-model="studentIdToEnroll"
            :placeholder="t('instructor.forceEnrollPlaceholder')"
            :min="1"
            :max="4294967295"
            :max-fraction-digits="0"
            size="small"
            class="w-full"
            :input-style="{ width: '100%' }"
            :disabled="isEnrolling"
            @update:model-value="errorMessage = null"
          />
        </div>

        <Button
          type="submit"
          :label="t('instructor.forceEnrollBtn')"
          icon="oi oi-user-plus"
          size="small"
          class="w-full sm:w-auto justify-center"
          :loading="isEnrolling"
          :disabled="!isValidStudentId || isEnrolling"
        />
      </div>

      <p class="text-xs text-surface-500">
        {{ t('instructor.forceEnrollHint') }}
      </p>

      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>
    </form>

    <!-- Seznam zapsaných studentů -->
    <div
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden p-4 sm:p-5 space-y-4"
    >
      <div class="flex flex-wrap items-center justify-between gap-3">
        <h3 class="font-bold text-base">
          {{ t('instructor.enrolledStudentsTitle') }}
        </h3>

        <Button
          :label="t('common.refresh')"
          icon="oi oi-refresh"
          severity="secondary"
          outlined
          size="small"
          :loading="isFetching"
          :disabled="isFetching"
          @click="handleRefresh"
        />
      </div>

      <Message v-if="isError" severity="error" :closable="false">
        {{ getApiErrorMessage(error) }}
      </Message>

      <div v-if="isLoading" class="space-y-2">
        <Skeleton v-for="n in 4" :key="n" height="3rem" class="rounded-xl" />
      </div>

      <p
        v-else-if="!isError && students.length === 0"
        class="text-sm text-surface-500 text-center py-6"
      >
        {{ t('instructor.noEnrolledStudents') }}
      </p>

      <div v-else-if="!isError" class="min-w-0 overflow-x-auto">
        <DataTable :value="students" striped-rows responsive-layout="scroll" class="min-w-[420px]">
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
                :disabled="isRemoving"
                :loading="isRemoving"
                @click="confirmRemoveStudent(data)"
              />
            </template>
          </Column>
        </DataTable>
      </div>

      <PaginationControls
        v-if="!isError && pagination"
        :pagination="pagination"
        :loading="isLoading"
        @update:page="page = $event"
      />
    </div>
  </div>
</template>
