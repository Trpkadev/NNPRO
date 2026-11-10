
<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { mockCourses } from '@/mocks/courses'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

// Vyhledání kurzu podle ID z URL
const course = computed(() =>
  mockCourses.find((item) => item.id === Number(route.params.id)),
)

// Aktuálně vybraná lekce
const selectedLesson = ref<number | null>(null)

// Při změně kurzu resetujeme vybranou lekci
watch(
  () => route.params.id,
  () => {
    selectedLesson.value = null
  },
)

// Otevření vybrané lekce
function openLesson(index: number) {
  selectedLesson.value = index
}
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
    <!-- Navigace zpět na katalog -->
    <Button
      :label="t('courses.backToCatalog')"
      icon="pi pi-arrow-left"
      text
      severity="secondary"
      @click="router.push('/home')"
    />

    <template v-if="course">
      <!-- Hlavička kurzu -->
      <div
        class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
      >
        <div class="flex items-center gap-3">
          <Tag
            :value="t('courses.course')"
            severity="success"
          />

          <span class="text-sm text-surface-500">
            {{ t('courses.onlineCourse') }}
          </span>
        </div>

        <h1 class="text-3xl font-bold">
          {{ t(course.titleKey) }}
        </h1>

        <p class="text-surface-600 dark:text-surface-400 max-w-3xl">
          {{ t(course.descriptionKey) }}
        </p>

        <div class="flex items-center gap-4 text-sm text-surface-500">
          <span class="flex items-center gap-2">
            <i class="pi pi-book" />
            {{ course.lessonKeys.length }} {{ t('courses.lessons') }}
          </span>
        </div>

        <Button
          :label="t('courses.startLearning')"
          icon="pi pi-play"
          :disabled="course.lessonKeys.length === 0"
          @click="openLesson(0)"
        />
      </div>

      <!-- Obsah kurzu -->
      <section class="space-y-4">
        <h2 class="text-xl font-semibold">
          {{ t('courses.courseContent') }}
        </h2>

        <div class="space-y-3">
          <button
            v-for="(lessonKey, index) in course.lessonKeys"
            :key="lessonKey"
            type="button"
            class="w-full p-4 rounded-xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 hover:border-emerald-500 transition-colors flex items-center justify-between text-left cursor-pointer"
            :class="{
              '!border-emerald-500': selectedLesson === index,
            }"
            @click="openLesson(index)"
          >
            <div class="flex items-center gap-4">
              <span class="text-emerald-500 font-semibold">
                {{ index + 1 }}.
              </span>

              <span>{{ t(lessonKey) }}</span>
            </div>

            <i class="pi pi-chevron-right text-surface-400" />
          </button>
        </div>
      </section>

      <!-- Zobrazení vybrané lekce -->
      <div
        v-if="selectedLesson !== null"
        class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-3"
      >
        <Tag
          :value="t('courses.sampleLesson')"
          severity="info"
        />

        <h2 class="text-xl font-semibold">
          {{ t(course.lessonKeys[selectedLesson]!) }}
        </h2>

        <p class="text-surface-600 dark:text-surface-400">
          {{ t('courses.lessonPlaceholder') }}
        </p>
      </div>
    </template>

    <!-- Kurz nebyl nalezen -->
    <Message v-else severity="warn" :closable="false">
      {{ t('courses.courseNotFound') }}
    </Message>
  </div>
</template>
