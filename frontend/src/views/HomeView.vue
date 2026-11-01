
<script setup lang="ts">
const courses = [
  { id: 1, title: 'Základy programování', lessons: 12, level: 'Začátečník' },
  { id: 2, title: 'Angličtina pro každého', lessons: 8, level: 'Začátečník' },
  { id: 3, title: 'Úvod do databází', lessons: 10, level: 'Mírně pokročilý' },
  { id: 4, title: 'Základy UX designu', lessons: 6, level: 'Začátečník' },
]
</script>

<template>
  <div class="home-page">
    <header class="page-header">
      <h1 class="text-xl font-bold">🎓 EduSpace</h1>
      <Button label="Přihlásit se" />
    </header>

    <main class="page-content">
      <section class="hero">
        <h2 class="text-3xl font-bold">Uč se vlastním tempem.</h2>
        <p>Objevuj kurzy, procházej lekce a sleduj svůj pokrok.</p>
        <Button label="Prozkoumat kurzy" />
      </section>

      <h2 class="text-2xl font-semibold">Objev naše kurzy</h2>

      <section class="course-grid">
        <CourseCard
          v-for="course in courses"
          :key="course.id"
          :title="course.title"
          :lessons="course.lessons"
          :level="course.level"
        />
      </section>
    </main>
  </div>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
  background: var(--color-background);
  color: var(--color-text);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 8%;
  background: var(--color-surface);
}

.page-content {
  max-width: 1100px;
  margin: auto;
  padding: 40px 20px;
}

.hero {
  background: var(--color-hero);
  border-radius: 16px;
  padding: 60px 40px;
  margin-bottom: 40px;
  display: grid;
  gap: 20px;
  justify-items: start;
}

.course-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 240px), 1fr));
  gap: 20px;
  margin-top: 24px;
}
</style>
