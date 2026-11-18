<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import { getYoutubeEmbedUrl, sanitizeHtml } from '@/utils/html'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const toast = useToast()

const courseId = computed(() => Number(route.params.courseId))
const lessonId = computed(() => Number(route.params.lessonId))

const { data: lesson, isLoading, isError, error } = useLessonDetailQuery(lessonId)

const { data: sectionLessonsResponse } = useSectionLessonsQuery(
  computed(() => lesson.value?.sectionId ?? 0),
  { Page: 1, PageSize: 50, SortBy: 'Order', Descending: false },
  computed(() => !!lesson.value?.sectionId),
)

const { data: myProgressResponse } = useMyProgressQuery({ Page: 1, PageSize: 100 })
const updateProgressMutation = useUpdateLessonProgressMutation()

const youtubeEmbedUrl = computed(() => getYoutubeEmbedUrl(lesson.value?.videoUrl))

const isCompleted = computed(() =>
  (myProgressResponse.value?.data ?? []).some(
    (p) => p.lessonId === lessonId.value && p.isCompleted,
  ),
)

const orderedSiblingLessons = computed(() =>
  [...(sectionLessonsResponse.value?.data ?? [])].sort((a, b) => a.order - b.order),
)

const currentIndex = computed(() =>
  orderedSiblingLessons.value.findIndex((l) => l.id === lessonId.value),
)

const prevLesson = computed(() =>
  currentIndex.value > 0 ? orderedSiblingLessons.value[currentIndex.value - 1] : null,
)
const nextLesson = computed(() =>
  currentIndex.value >= 0 && currentIndex.value < orderedSiblingLessons.value.length - 1
    ? orderedSiblingLessons.value[currentIndex.value + 1]
    : null,
)

async function handleToggleComplete() {
  if (!lesson.value) return
  try {
    await updateProgressMutation.mutateAsync({
      lessonId: lesson.value.id,
      payload: { isCompleted: !isCompleted.value },
    })
    toast.add({
      severity: 'success',
      summary: t('lessons.progressUpdated'),
      life: 2500,
    })
  } catch (err) {
    toast.add({
      severity: 'error',
      summary: getApiErrorMessage(err),
      life: 4000,
    })
  }
}
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3">
      <Button
        :label="t('lessons.backToCourse')"
        icon="oi oi-arrow-left"
        severity="secondary"
        text
        size="small"
        @click="router.push({ name: 'course-detail', params: { id: courseId } })"
      />
    </div>

    <div v-if="isLoading" class="space-y-4">
      <Skeleton height="6rem" class="rounded-2xl" />
      <Skeleton height="18rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="isError" severity="error" :closable="false">
      {{ getApiErrorMessage(error) }}
    </Message>

    <template v-else-if="lesson">
      <!-- Hlavička lekce + označení dokončení -->
      <div
        class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4 shadow-xs"
      >
        <div class="space-y-1">
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-primary uppercase">
              {{ t('lessons.lesson') }} #{{ lesson.order }}
            </span>
            <Tag
              :value="isCompleted ? t('lessons.completed') : t('lessons.incomplete')"
              :severity="isCompleted ? 'success' : 'secondary'"
            />
          </div>
          <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
            {{ lesson.title }}
          </h1>
        </div>

        <Button
          :label="isCompleted ? t('lessons.markIncomplete') : t('lessons.markComplete')"
          :icon="isCompleted ? 'oi oi-check-circle' : 'oi oi-circle-off'"
          :severity="isCompleted ? 'success' : 'primary'"
          :outlined="isCompleted"
          size="small"
          :loading="updateProgressMutation.isPending.value"
          @click="handleToggleComplete"
        />
      </div>

      <!-- Výukové video -->
      <div
        v-if="lesson.videoUrl"
        class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-3"
      >
        <div class="flex items-center justify-between gap-2">
          <h2 class="text-base font-bold flex items-center gap-2">
            <i class="oi oi-video text-primary" />
            <span>{{ t('lessons.videoSection') }}</span>
          </h2>
          <a
            :href="lesson.videoUrl"
            target="_blank"
            rel="noopener noreferrer"
            class="text-xs text-primary hover:underline flex items-center gap-1"
          >
            <span>{{ t('lessons.openExternalVideo') }}</span>
            <i class="oi oi-external-link text-[10px]" />
          </a>
        </div>

        <div
          v-if="youtubeEmbedUrl"
          class="aspect-video w-full rounded-xl overflow-hidden bg-black border border-surface-200 dark:border-surface-800"
        >
          <iframe
            :src="youtubeEmbedUrl"
            :title="lesson.title"
            class="w-full h-full"
            allow="
              accelerometer;
              autoplay;
              clipboard-write;
              encrypted-media;
              gyroscope;
              picture-in-picture;
            "
            allowfullscreen
          />
        </div>
      </div>

      <!-- Formátovaný obsah lekce -->
      <article
        class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 prose dark:prose-invert max-w-none leading-relaxed text-surface-800 dark:text-surface-100"
        v-html="sanitizeHtml(lesson.content)"
      />

      <!-- Studijní materiály a přílohy ke stažení -->
      <LessonAttachmentsCard :lesson-id="lesson.id" />

      <!-- Navigace mezi lekcemi v sekci -->
      <div class="flex items-center justify-between gap-3 pt-2">
        <Button
          v-if="prevLesson"
          :label="`${t('lessons.prevLesson')}: ${prevLesson.title}`"
          icon="oi oi-chevron-left"
          severity="secondary"
          outlined
          size="small"
          @click="
            router.push({
              name: 'lesson-detail',
              params: { courseId, lessonId: prevLesson.id },
            })
          "
        />
        <div v-else />

        <Button
          v-if="nextLesson"
          :label="`${t('lessons.nextLesson')}: ${nextLesson.title}`"
          icon="oi oi-chevron-right"
          icon-pos="right"
          size="small"
          @click="
            router.push({
              name: 'lesson-detail',
              params: { courseId, lessonId: nextLesson.id },
            })
          "
        />
      </div>
    </template>
  </div>
</template>
