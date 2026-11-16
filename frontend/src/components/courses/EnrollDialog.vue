<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const props = defineProps<{
  visible: boolean
  courseId: number | null
  courseTitle?: string
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  enrolled: [courseId: number]
}>()

const { t } = useI18n()
const toast = useToast()
const enrollMutation = useEnrollCourseMutation()

const accessKey = ref('')
const errorMessage = ref<string | null>(null)

watch(
  () => props.visible,
  (isOpen) => {
    if (isOpen) {
      accessKey.value = ''
      errorMessage.value = null
    }
  },
)

async function handleEnroll() {
  if (!props.courseId) return
  errorMessage.value = null

  try {
    await enrollMutation.mutateAsync({
      courseId: props.courseId,
      accessKey: accessKey.value.trim() ? accessKey.value.trim() : null,
    })

    toast.add({
      severity: 'success',
      summary: t('courses.enrollSuccess'),
      life: 3000,
    })

    emit('enrolled', props.courseId)
    emit('update:visible', false)
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :header="courseTitle ? `${t('courses.enrollDialogTitle')}: ${courseTitle}` : t('courses.enrollDialogTitle')"
    :style="{ width: '28rem', maxWidth: '95vw' }"
    @update:visible="emit('update:visible', $event)"
  >
    <form class="space-y-4" @submit.prevent="handleEnroll">
      <p class="text-sm text-surface-600 dark:text-surface-400">
        {{ t('courses.enrollDialogDesc') }}
      </p>

      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>

      <div class="space-y-1.5">
        <label for="enroll-access-key" class="block text-sm font-medium">
          {{ t('courses.accessKeyLabel') }}
        </label>
        <Password
          input-id="enroll-access-key"
          v-model="accessKey"
          :placeholder="t('courses.accessKeyPlaceholder')"
          :feedback="false"
          toggle-mask
          fluid
        />
      </div>

      <div class="flex justify-end gap-2 pt-2">
        <Button
          type="button"
          :label="t('common.cancel')"
          severity="secondary"
          text
          @click="emit('update:visible', false)"
        />
        <Button
          type="submit"
          :label="t('courses.enrollBtn')"
          icon="oi oi-check"
          :loading="enrollMutation.isPending.value"
        />
      </div>
    </form>
  </Dialog>
</template>
