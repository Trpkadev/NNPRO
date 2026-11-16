<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { LessonResponseDto } from '@/types/api'

const props = defineProps<{
  visible: boolean
  lesson: LessonResponseDto | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

const { t } = useI18n()

const lessonId = computed(() => props.lesson?.id ?? 0)
const isEnabled = computed(() => props.visible && lessonId.value > 0)

const { data: attachmentsResponse, isLoading } = useLessonAttachmentsQuery(
  lessonId,
  { Page: 1, PageSize: 50 },
  isEnabled,
)

const addMutation = useAddLessonAttachmentMutation()
const deleteMutation = useDeleteLessonAttachmentMutation()

const attachments = computed(() => attachmentsResponse.value?.data ?? [])

const fileName = ref('')
const url = ref('')
const contentType = ref('application/pdf')
const sizeBytes = ref<number | null>(null)
const errorMessage = ref<string | null>(null)

function formatBytes(bytes: number | null | undefined): string {
  if (!bytes || bytes <= 0) return ''
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

async function handleAdd() {
  if (!props.lesson) return
  errorMessage.value = null
  if (!fileName.value.trim() || !url.value.trim()) {
    errorMessage.value = t('validation.required')
    return
  }

  try {
    await addMutation.mutateAsync({
      lessonId: props.lesson.id,
      fileName: fileName.value.trim(),
      url: url.value.trim(),
      contentType: contentType.value.trim() || null,
      sizeBytes: sizeBytes.value ? Number(sizeBytes.value) : null,
    })
    fileName.value = ''
    url.value = ''
    sizeBytes.value = null
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleRemove(attachmentId: number) {
  if (!props.lesson) return
  errorMessage.value = null
  try {
    await deleteMutation.mutateAsync({
      lessonId: props.lesson.id,
      attachmentId,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :header="t('instructor.attachmentsDialogTitle', { title: lesson?.title ?? '' })"
    :style="{ width: '36rem', maxWidth: '95vw' }"
    @update:visible="emit('update:visible', $event)"
  >
    <div class="space-y-5">
      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>

      <!-- Seznam existujících příloh -->
      <div class="space-y-2">
        <div v-if="isLoading" class="space-y-2">
          <Skeleton v-for="n in 2" :key="n" height="2.75rem" class="rounded-xl" />
        </div>

        <p
          v-else-if="attachments.length === 0"
          class="text-xs text-surface-500 italic py-2 text-center"
        >
          {{ t('lessons.noAttachments') }}
        </p>

        <div v-else class="space-y-2">
          <div
            v-for="item in attachments"
            :key="item.id"
            class="p-3 rounded-xl border border-surface-200 dark:border-surface-800 flex items-center justify-between gap-3"
          >
            <div class="min-w-0">
              <p class="text-sm font-semibold truncate">{{ item.fileName }}</p>
              <p class="text-xs text-surface-500 truncate">
                {{ item.contentType || 'file' }}
                <span v-if="item.sizeBytes">&bull; {{ formatBytes(item.sizeBytes) }}</span>
              </p>
            </div>

            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
              :loading="deleteMutation.isPending.value"
              @click="handleRemove(item.id)"
            />
          </div>
        </div>
      </div>

      <!-- Formulář pro přidání nové přílohy -->
      <form
        class="p-4 rounded-xl bg-surface-50 dark:bg-surface-800/50 border border-surface-200 dark:border-surface-800 space-y-3"
        @submit.prevent="handleAdd"
      >
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.attachmentFileName') }}</label>
            <InputText v-model="fileName" size="small" class="w-full" required />
          </div>
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.attachmentUrl') }}</label>
            <InputText v-model="url" size="small" class="w-full" required />
          </div>
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.attachmentContentType') }}</label>
            <InputText v-model="contentType" size="small" class="w-full" />
          </div>
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.attachmentSizeBytes') }}</label>
            <InputNumber v-model="sizeBytes" size="small" class="w-full" :min="0" />
          </div>
        </div>

        <div class="flex justify-end">
          <Button
            type="submit"
            :label="t('instructor.addAttachmentBtn')"
            icon="oi oi-plus"
            size="small"
            :loading="addMutation.isPending.value"
          />
        </div>
      </form>
    </div>
  </Dialog>
</template>
