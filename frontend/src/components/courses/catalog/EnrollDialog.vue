
<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  courseId: number | null
  courseTitle?: string
}>()

const emit = defineEmits<{
  enrolled: [courseId: number]
}>()

const { t } = useI18n()
const toast = useToast()
const enrollMutation = useEnrollCourseMutation()

const accessKey = ref('')
const errorMessage = ref<string | null>(null)

watch(visible, (isOpen) => {
  if (isOpen) {
    accessKey.value = ''
    errorMessage.value = null
  }
})

async function handleEnroll() {
  if (!props.courseId) return
  errorMessage.value = null

  try {
    await enrollMutation.mutateAsync({
      courseId: props.courseId,
      accessKey: accessKey.value.trim() || null,
    })

    toast.add({
      severity: 'success',
      summary: t('courses.enrollSuccess'),
      life: 3000,
    })

    emit('enrolled', props.courseId)
    visible.value = false
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :closable="true"
    :show-header="true"
    :style="{ width: '34rem', maxWidth: '95vw' }"
  >
    <template #header>
      <h2 class="min-w-0 text-lg font-semibold leading-snug">
        {{ t('courses.enrollDialogTitle') }}
        <span v-if="courseTitle">: {{ courseTitle }}</span>
      </h2>
    </template>

    <template #closebutton="{ closeCallback }">
      <button
        type="button"
        class="shrink-0 flex items-center justify-center w-9 h-9 rounded-lg hover:bg-surface-100 dark:hover:bg-surface-800"
        :aria-label="t('common.cancel')"
        @click="closeCallback"
      >
        <span class="text-2xl leading-none" aria-hidden="true">×</span>
      </button>
    </template>

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
          @click="visible = false"
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
