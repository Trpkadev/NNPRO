<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { LOCALE_STORAGE_KEY } from '@/i18n'
import { UserRoles } from '@/types/api'
import { getApiErrorMessage } from '@/services/api'

const { t, locale } = useI18n()
const router = useRouter()
const route = useRoute()
const toast = useToast()

const authStore = useAuthStore()
const { canManageCourses, canManageUsers } = usePermissions()
const logoutMutation = useLogoutMutation()

const isDark = useDark()
const toggleDark = useToggle(isDark)
const mobileMenuVisible = ref(false)

function toggleLanguage() {
  const nextLocale = locale.value === 'cs' ? 'en' : 'cs'
  locale.value = nextLocale
  localStorage.setItem(LOCALE_STORAGE_KEY, nextLocale)
}

const navItems = computed(() => {
  const items = [
    {
      label: t('nav.home'),
      icon: 'oi oi-home',
      to: { name: 'home' },
    },
    {
      label: t('nav.profile'),
      icon: 'oi oi-user',
      to: { name: 'profile' },
    },
  ]

  if (canManageCourses.value) {
    items.push({
      label: t('nav.instructorZone'),
      icon: 'oi oi-briefcase',
      to: { name: 'instructor-dashboard' },
    })
  }

  if (canManageUsers.value) {
    items.push({
      label: t('nav.adminZone'),
      icon: 'oi oi-shield',
      to: { name: 'admin-dashboard' },
    })
  }

  return items
})

const roleSeverity = computed(() => {
  if (authStore.userRole === UserRoles.Admin) return 'danger'
  if (authStore.userRole === UserRoles.Instructor) return 'warn'
  if (authStore.userRole === UserRoles.Student) return 'info'
  return 'secondary'
})

const roleLabel = computed(() => {
  if (!authStore.userRole) return t('roles.guest')
  return t(`roles.${authStore.userRole}`)
})

async function handleLogout() {
  mobileMenuVisible.value = false
  try {
    await logoutMutation.mutateAsync()
    toast.add({
      severity: 'info',
      summary: t('auth.logoutSuccess'),
      life: 3000,
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: getApiErrorMessage(error, t('auth.logoutError')),
      life: 5000,
    })
  } finally {
    await router.push({ name: 'login' })
  }
}
</script>

<template>
  <div
    class="min-h-screen flex flex-col bg-surface-50 dark:bg-surface-950 text-surface-900 dark:text-surface-0"
  >
    <!-- Hlavní horní navigace -->
    <header
      class="sticky top-0 z-30 bg-surface-0/90 dark:bg-surface-900/90 backdrop-blur border-b border-surface-200 dark:border-surface-800"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <Button
            icon="oi oi-bars"
            severity="secondary"
            text
            rounded
            class="md:hidden"
            aria-label="Menu"
            @click="mobileMenuVisible = true"
          />
          <RouterLink :to="{ name: 'home' }" class="flex items-center gap-2.5 no-underline">
            <div
              class="w-9 h-9 rounded-xl bg-primary flex items-center justify-center text-primary-contrast font-bold shadow-sm"
            >
              <i class="oi oi-book text-lg" />
            </div>
            <span class="text-lg font-bold tracking-tight text-surface-900 dark:text-surface-0">
              {{ t('app.name') }}
            </span>
          </RouterLink>
        </div>

        <nav class="hidden md:flex items-center gap-1">
          <RouterLink
            v-for="item in navItems"
            :key="item.to.name"
            :to="item.to"
            class="px-3.5 py-2 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 no-underline"
            :class="
              route.name === item.to.name
                ? 'bg-primary/10 text-primary font-semibold'
                : 'text-surface-600 dark:text-surface-300 hover:bg-surface-100 dark:hover:bg-surface-800'
            "
          >
            <i :class="item.icon" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </nav>

        <div class="flex items-center gap-2">
          <Button
            :label="locale.toUpperCase()"
            icon="oi oi-globe"
            severity="secondary"
            text
            size="small"
            :aria-label="t('nav.language')"
            @click="toggleLanguage"
          />
          <Button
            :icon="isDark ? 'oi oi-sun' : 'oi oi-moon'"
            severity="secondary"
            text
            rounded
            :aria-label="t('nav.darkMode')"
            @click="toggleDark()"
          />

          <template v-if="authStore.isAuthenticated">
            <div
              class="hidden sm:flex items-center gap-2 pl-2 border-l border-surface-200 dark:border-surface-800"
            >
              <div class="text-right">
                <p class="text-sm font-semibold leading-tight">
                  {{ authStore.user?.username }}
                </p>
                <Tag :value="roleLabel" :severity="roleSeverity" class="text-xs mt-0.5" />
              </div>
              <Button
                icon="oi oi-sign-out"
                severity="secondary"
                text
                rounded
                :aria-label="t('nav.logout')"
                :loading="logoutMutation.isPending.value"
                @click="handleLogout"
              />
            </div>
          </template>

          <template v-else>
            <div class="hidden sm:flex items-center gap-2">
              <Button
                :label="t('nav.login')"
                icon="oi oi-sign-in"
                severity="secondary"
                text
                size="small"
                @click="router.push({ name: 'login' })"
              />
              <Button
                :label="t('nav.register')"
                icon="oi oi-user-plus"
                size="small"
                @click="router.push({ name: 'register' })"
              />
            </div>
          </template>
        </div>
      </div>
    </header>

    <MobileNavDrawer
      v-model:visible="mobileMenuVisible"
      :nav-items="navItems"
      :is-authenticated="authStore.isAuthenticated"
      :username="authStore.user?.username"
      :role-label="roleLabel"
      :role-severity="roleSeverity"
      :logout-pending="logoutMutation.isPending.value"
      @logout="handleLogout"
    />

    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 py-6">
      <RouterView />
    </main>
  </div>
</template>
