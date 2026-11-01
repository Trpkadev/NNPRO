
<script setup lang="ts">
import AppNavbar from '@/components/AppNavbar.vue'
import CourseCard from '@/components/CourseCard.vue'

const courses = [
  { id: 1, title: 'Základy programování', lessons: 12, level: 'Začátečník' },
  { id: 2, title: 'Angličtina pro každého', lessons: 8, level: 'Začátečník' },
  { id: 3, title: 'Úvod do databází', lessons: 10, level: 'Mírně pokročilý' },
  { id: 4, title: 'Základy UX designu', lessons: 6, level: 'Začátečník' },
]

function scrollToCourses() {
  document.getElementById('courses')?.scrollIntoView({
    behavior: 'smooth',
  })
}
</script>

<template>
  <div class="min-h-screen bg-[var(--color-background)] text-[var(--color-text)]">
    <AppNavbar />

    <main class="mx-auto max-w-6xl px-5 py-10">
      <section
        class="mb-10 flex flex-col items-start gap-5 rounded-2xl
               bg-[var(--color-hero)] px-6 py-12 md:px-10 md:py-16"
      >
        <h1 class="text-3xl font-bold md:text-4xl">
          Uč se vlastním tempem.
        </h1>

        <p>Objevuj kurzy, procházej lekce a sleduj svůj pokrok.</p>

        <Button label="Prozkoumat kurzy" @click="scrollToCourses" />
      </section>

      <section id="courses" class="scroll-mt-6">
        <h2 class="mb-6 text-2xl font-semibold">
          Objev naše kurzy
        </h2>

        <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
          <CourseCard
            v-for="course in courses"
            :key="course.id"
            :title="course.title"
            :lessons="course.lessons"
            :level="course.level"
          />
        </div>
      </section>
    </main>
  </div>
</template>
