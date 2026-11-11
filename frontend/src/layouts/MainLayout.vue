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
        <!-- Logo + Mobilní tlačítko menu -->
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

        <!-- Desktopová navigace -->
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

        <!-- Akce vpravo (jazyk, téma, profil / přihlášení) -->
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
              <p class="text-sm font-semibold leading-tight">
                {{ authStore.user?.username }}
              </p>
              <Tag :value="roleLabel" :severity="roleSeverity" class="text-xs mt-0.5" />
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

    <!-- Mobilní Drawer navigace -->
    <Drawer v-model:visible="mobileMenuVisible" :header="t('app.name')">
      <div class="flex flex-col h-full justify-between">
        <div class="space-y-1">
          <RouterLink
            v-for="item in navItems"
            :key="item.to.name"
            :to="item.to"
            class="px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-3 no-underline"
            :class="
              route.name === item.to.name
                ? 'bg-primary/10 text-primary font-semibold'
                : 'text-surface-700 dark:text-surface-200 hover:bg-surface-100 dark:hover:bg-surface-800'
            "
            @click="mobileMenuVisible = false"
          >
            <i :class="item.icon" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </div>

        <div class="pt-4 border-t border-surface-200 dark:border-surface-800 space-y-3">
          <template v-if="authStore.isAuthenticated">
            <div class="flex items-center justify-between px-2">
              <div>
                <p class="text-sm font-semibold">
                  {{ authStore.user?.username }}
                </p>
              </div>
              <Tag :value="roleLabel" :severity="roleSeverity" />
            </div>
            <Button
              :label="t('nav.logout')"
              icon="oi oi-sign-out"
              severity="danger"
              outlined
              class="w-full"
              :loading="logoutMutation.isPending.value"
              @click="handleLogout"
            />
          </template>
          <template v-else>
            <div class="flex flex-col gap-2">
              <Button
                :label="t('nav.login')"
                icon="oi oi-sign-in"
                outlined
                class="w-full"
                @click="
                  () => {
                    mobileMenuVisible = false
                    router.push({ name: 'login' })
                  }
                "
              />
              <Button
                :label="t('nav.register')"
                icon="oi oi-user-plus"
                class="w-full"
                @click="
                  () => {
                    mobileMenuVisible = false
                    router.push({ name: 'register' })
                  }
                "
              />
            </div>
          </template>
        </div>
      </div>
    </Drawer>

    <!-- Hlavní kontejner stránky -->
    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 py-6">
      <RouterView />
    </main>
  </div>
</template>
