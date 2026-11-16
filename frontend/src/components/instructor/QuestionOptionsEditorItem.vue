<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { AnswerOptionEditorResponseDto, QuestionResponseDto } from '@/types/api'

const props = defineProps<{
  question: QuestionResponseDto
}>()

const { t } = useI18n()

const { data: optionsResponse, isLoading } = useQuestionOptionsEditorQuery(
  computed(() => props.question.id),
  { Page: 1, PageSize: 50 },
)

const createOptionMutation = useCreateAnswerOptionMutation()
const updateOptionMutation = useUpdateAnswerOptionMutation()
const deleteOptionMutation = useDeleteAnswerOptionMutation()

const options = computed(() => optionsResponse.value?.data ?? [])

const newText = ref('')
const newImageUrl = ref('')
const newIsCorrect = ref(false)
const errorMessage = ref<string | null>(null)

async function handleAddOption() {
  errorMessage.value = null
  if (!newText.value.trim()) return

  try {
    await createOptionMutation.mutateAsync({
      questionId: props.question.id,
      payload: {
        text: newText.value.trim(),
        imageUrl: newImageUrl.value.trim() ? newImageUrl.value.trim() : null,
        isCorrect: newIsCorrect.value,
      },
    })
    newText.value = ''
    newImageUrl.value = ''
    newIsCorrect.value = false
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleToggleCorrect(option: AnswerOptionEditorResponseDto) {
  errorMessage.value = null
  try {
    await updateOptionMutation.mutateAsync({
      questionId: props.question.id,
      optionId: option.id,
      payload: {
        text: option.text,
        imageUrl: option.imageUrl,
        isCorrect: !option.isCorrect,
      },
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleDeleteOption(optionId: number) {
  errorMessage.value = null
  try {
    await deleteOptionMutation.mutateAsync({
      questionId: props.question.id,
      optionId,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <div class="pt-3 border-t border-surface-100 dark:border-surface-800 space-y-2.5">
    <p class="text-xs font-semibold text-surface-500">
      {{ t('instructor.optionsTitle') }}
    </p>

    <Message v-if="errorMessage" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

    <div v-if="isLoading" class="space-y-1.5">
      <Skeleton v-for="n in 2" :key="n" height="2.25rem" class="rounded-lg" />
    </div>

    <div v-else class="space-y-1.5">
      <div
        v-for="opt in options"
        :key="opt.id"
        class="px-3 py-2 rounded-xl border flex items-center justify-between gap-2 text-xs"
        :class="
          opt.isCorrect
            ? 'border-emerald-500/40 bg-emerald-500/10'
            : 'border-surface-200 dark:border-surface-800'
        "
      >
        <div class="flex items-center gap-2 min-w-0">
          <button
            type="button"
            class="px-2 py-0.5 rounded-md font-semibold transition-colors cursor-pointer"
            :class="
              opt.isCorrect
                ? 'bg-emerald-600 text-white'
                : 'bg-surface-200 dark:bg-surface-700 text-surface-700 dark:text-surface-200'
            "
            @click="handleToggleCorrect(opt)"
          >
            {{ opt.isCorrect ? '✓ ' + t('instructor.optionIsCorrect') : t('instructor.optionIsCorrect') }}
          </button>
          <span class="truncate font-medium">{{ opt.text }}</span>
          <span v-if="opt.imageUrl" class="text-surface-400 truncate">({{ opt.imageUrl }})</span>
        </div>

        <Button
          icon="oi oi-trash"
          severity="danger"
          text
          rounded
          size="small"
          @click="handleDeleteOption(opt.id)"
        />
      </div>
    </div>

    <!-- Rychlé přidání možnosti odpovědi -->
    <form class="flex flex-wrap items-center gap-2 pt-1" @submit.prevent="handleAddOption">
      <InputText
        v-model="newText"
        :placeholder="t('instructor.optionTextPlaceholder')"
        size="small"
        class="flex-1 min-w-40"
      />
      <InputText
        v-model="newImageUrl"
        :placeholder="t('instructor.optionImagePlaceholder')"
        size="small"
        class="w-40"
      />
      <label class="flex items-center gap-1.5 text-xs cursor-pointer select-none px-2">
        <Checkbox v-model="newIsCorrect" binary />
        <span>{{ t('instructor.optionIsCorrect') }}</span>
      </label>
      <Button
        type="submit"
        :label="t('instructor.addOption')"
        icon="oi oi-plus"
        size="small"
        severity="secondary"
        outlined
        :loading="createOptionMutation.isPending.value"
      />
    </form>
  </div>
</template>
