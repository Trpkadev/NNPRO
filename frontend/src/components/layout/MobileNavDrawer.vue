<script setup lang="ts">
const visible = defineModel<boolean>('visible', { default: false })

defineProps<{
  navItems: Array<{ label: string; icon: string; to: { name: string } }>
  isAuthenticated: boolean
  username?: string
  roleLabel: string
  roleSeverity: 'danger' | 'warn' | 'info' | 'secondary'
  logoutPending: boolean
}>()

const emit = defineEmits<{
  logout: []
}>()

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
</script>

<template>
  <Drawer v-model:visible="visible" :header="t('app.name')">
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
          @click="visible = false"
        >
          <i :class="item.icon" />
          <span>{{ item.label }}</span>
        </RouterLink>
      </div>

      <div class="pt-4 border-t border-surface-200 dark:border-surface-800 space-y-3">
        <template v-if="isAuthenticated">
          <div class="flex items-center justify-between px-2">
            <div>
              <p class="text-sm font-semibold">
                {{ username }}
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
            :loading="logoutPending"
            @click="emit('logout')"
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
                  visible = false
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
                  visible = false
                  router.push({ name: 'register' })
                }
              "
            />
          </div>
        </template>
      </div>
    </div>
  </Drawer>
</template>
