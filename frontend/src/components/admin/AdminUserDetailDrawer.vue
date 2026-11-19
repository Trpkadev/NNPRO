<script setup lang="ts">
const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  userId: number | null
}>()

const { t } = useI18n()

const activeUserId = computed(() => props.userId ?? 0)
const isEnabled = computed(() => visible.value && activeUserId.value > 0)

const { data: userDetail, isLoading: detailLoading } = useAdminUserDetailQuery(
  activeUserId,
  isEnabled,
)
</script>

<template>
  <Drawer
    v-model:visible="visible"
    position="right"
    :header="t('admin.userDetailsTitle')"
    class="!w-full sm:!w-[34rem]"
  >
    <div v-if="detailLoading" class="space-y-3">
      <Skeleton height="5rem" class="rounded-xl" />
      <Skeleton height="8rem" class="rounded-xl" />
      <Skeleton height="10rem" class="rounded-xl" />
    </div>

    <div v-else-if="userDetail" class="space-y-6">
      <!-- Statistiky uživatele -->
      <div class="grid grid-cols-2 gap-3">
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.enrollmentsCount') }}</span>
          <span class="text-xl font-bold">{{ userDetail.enrollmentCount }}</span>
        </div>
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.completedLessonsCount') }}</span>
          <span class="text-xl font-bold">{{ userDetail.lessonProgressCount }}</span>
        </div>
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.quizAttemptsCount') }}</span>
          <span class="text-xl font-bold">{{ userDetail.quizAttemptCount }}</span>
        </div>
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.activeSessionsCount') }}</span>
          <span class="text-xl font-bold text-primary">{{ userDetail.activeSessionCount }}</span>
        </div>
      </div>

      <AdminUserEditForm :user-detail="userDetail" />

      <AdminUserSessionsList :user-id="userDetail.id" />
    </div>
  </Drawer>
</template>
