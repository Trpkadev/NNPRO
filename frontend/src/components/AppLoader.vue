
<script setup lang="ts">
const { t } = useI18n()

defineProps<{
  message?: string
}>()
</script>

<template>
  <div class="loader" role="status" aria-live="polite">
    <div class="book" aria-hidden="true">
      <div class="book-left"></div>
      <div class="book-right"></div>
      <div class="book-page page-one"></div>
      <div class="book-page page-two"></div>
      <div class="book-page page-three"></div>
      <div class="book-center"></div>
    </div>

    <p>{{ message || t('loading.default') }}</p>
  </div>
</template>

<style scoped>
.loader {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 35px;
  background: var(--color-background);
  color: var(--color-text);
}

.book {
  position: relative;
  width: 250px;
  height: 170px;
  perspective: 800px;
  filter: drop-shadow(0 8px 8px var(--color-border));
}

.book-left, .book-right {
  position: absolute;
  width: 50%;
  height: 100%;
  background: var(--color-primary);
  border: 5px solid var(--color-primary-hover);
}

.book-left {
  left: 0;
  border-radius: 12px 3px 3px 12px;
}

.book-right {
  right: 0;
  border-radius: 3px 12px 12px 3px;
}

.book-page {
  position: absolute;
  left: 50%;
  top: 5px;
  width: 45%;
  height: 90%;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px 8px 8px 2px;
  transform-origin: left center;
  animation: turn-page 2.4s infinite ease-in-out;
}

.page-one { animation-delay: 0s; }
.page-two { animation-delay: 0.6s; }
.page-three { animation-delay: 1.2s; }

.book-center {
  position: absolute;
  left: 49%;
  width: 2%;
  height: 100%;
  background: var(--color-primary-hover);
}

.loader p {
  font-size: 16px;
  font-weight: 500;
}

@keyframes turn-page {
  0%, 15% { transform: rotateY(0deg); }
  65%, 100% { transform: rotateY(-180deg); }
}

@media (prefers-reduced-motion: reduce) {
  .book-page { animation: none; }
}
</style>
