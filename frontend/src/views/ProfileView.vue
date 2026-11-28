<script setup lang="ts">
import StudentProfileProgress from '@/components/profile/StudentProfileProgress.vue'
import InstructorProfileCourses from '@/components/profile/InstructorProfileCourses.vue'

const { t } = useI18n()
const authStore = useAuthStore()

const username = computed(() => authStore.user?.username ?? '')
const initials = computed(() => username.value.slice(0, 2).toUpperCase() || '?')

const roleLabel = computed(() =>
  authStore.userRole ? t(`roles.${authStore.userRole}`) : t('roles.guest'),
)
</script>

<template>
  <div class="space-y-6">
    <!-- Společná profilová hlavička -->
    <div
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden"
    >
      <div class="h-2 bg-primary" />

      <div class="p-5 sm:p-7 flex flex-col sm:flex-row sm:items-center gap-5">
        <div
          class="w-16 h-16 rounded-2xl bg-primary/10 text-primary flex items-center justify-center text-2xl font-bold shrink-0"
        >
          {{ initials }}
        </div>

        <div class="min-w-0 flex-1 space-y-1">
          <h1 class="text-2xl font-bold break-words">
            {{ username || t('profile.title') }}
          </h1>

          <p class="text-sm text-surface-500">
            {{
              authStore.isStudent
                ? t('profile.studentSubtitle')
                : authStore.isInstructor
                  ? t('profile.instructorSubtitle')
                  : authStore.isAdmin
                    ? t('profile.adminSubtitle')
                    : t('profile.subtitle')
            }}
          </p>

          <p v-if="authStore.user?.email" class="text-xs text-surface-500 break-all">
            {{ authStore.user.email }}
          </p>
        </div>

        <Tag :value="roleLabel" severity="secondary" class="self-start sm:self-center" />
      </div>
    </div>

    <!-- STUDENT -->
    <template v-if="authStore.isStudent">
      <StudentProfileProgress />
    </template>

    <!-- LEKTOR -->
    <template v-else-if="authStore.isInstructor">
      <InstructorProfileCourses />
    </template>

    <!-- ADMINISTRÁTOR -->
    <template v-else-if="authStore.isAdmin">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <RouterLink
          :to="{ name: 'admin-dashboard' }"
          class="group p-5 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 hover:border-primary/50 transition-colors no-underline flex items-center gap-4"
        >
          <div
            class="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0"
          >
            <i class="oi oi-shield text-xl" />
          </div>

          <div class="flex-1 min-w-0">
            <h2 class="font-bold text-surface-900 dark:text-surface-0">
              {{ t('nav.adminZone') }}
            </h2>
            <p class="text-sm text-surface-500">
              {{ t('profile.adminManagementDescription') }}
            </p>
          </div>

          <i class="oi oi-chevron-right text-surface-400 group-hover:text-primary" />
        </RouterLink>

        <RouterLink
          :to="{ name: 'instructor-dashboard' }"
          class="group p-5 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 hover:border-primary/50 transition-colors no-underline flex items-center gap-4"
        >
          <div
            class="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0"
          >
            <i class="oi oi-briefcase text-xl" />
          </div>

          <div class="flex-1 min-w-0">
            <h2 class="font-bold text-surface-900 dark:text-surface-0">
              {{ t('nav.instructorZone') }}
            </h2>
            <p class="text-sm text-surface-500">
              {{ t('profile.adminCoursesDescription') }}
            </p>
          </div>

          <i class="oi oi-chevron-right text-surface-400 group-hover:text-primary" />
        </RouterLink>
      </div>
    </template>

    <!-- Nastavení účtu pro všechny role -->
    <div class="max-w-2xl space-y-4">
      <ProfileAccountCard />
    </div>
  </div>
</template>
