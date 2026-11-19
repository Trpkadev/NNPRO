<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()
const authStore = useAuthStore()

const updateCurrentUserMutation = useUpdateCurrentUserMutation()
const deleteCurrentUserMutation = useDeleteCurrentUserMutation()

const username = ref(authStore.user?.username ?? '')
const email = ref(authStore.user?.email ?? '')
const password = ref('')
const errorMessage = ref<string | null>(null)

watch(
  () => authStore.user,
  (u) => {
    if (u) {
      if (!username.value) username.value = u.username
      if (!email.value && u.email) email.value = u.email
    }
  },
)

async function handleUpdateProfile() {
  errorMessage.value = null
  const trimmedUser = username.value.trim()
  const trimmedEmail = email.value.trim()

  if (trimmedUser.length < 3) {
    errorMessage.value = t('validation.minLength', { min: 3 })
    return
  }
  if (trimmedUser.includes('@')) {
    errorMessage.value = t('validation.usernameNoAt')
    return
  }
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
    errorMessage.value = t('validation.email')
    return
  }
  if (password.value.length > 0 && password.value.length < 8) {
    errorMessage.value = t('validation.minLength', { min: 8 })
    return
  }

  try {
    await updateCurrentUserMutation.mutateAsync({
      username: trimmedUser,
      email: trimmedEmail,
      ...(password.value ? { password: password.value } : {}),
    })
    authStore.setKnownEmail(trimmedEmail)
    password.value = ''
    toast.add({
      severity: 'success',
      summary: t('profile.profileUpdated'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

function confirmDeleteAccount() {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('profile.deleteAccountConfirm'),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await deleteCurrentUserMutation.mutateAsync()
        await router.replace({ name: 'login' })
      } catch (err) {
        errorMessage.value = getApiErrorMessage(err)
      }
    },
  })
}
</script>

<template>
  <div class="space-y-6">
    <form
      class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
      @submit.prevent="handleUpdateProfile"
    >
      <h2 class="text-base font-bold">{{ t('profile.accountSettings') }}</h2>

      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>

      <div class="space-y-1">
        <label for="profile-username" class="block text-xs font-medium">
          {{ t('auth.usernameLabel') }}
        </label>
        <InputText id="profile-username" v-model="username" size="small" class="w-full" required />
      </div>

      <div class="space-y-1">
        <label for="profile-email" class="block text-xs font-medium">
          {{ t('auth.emailLabel') }}
        </label>
        <InputText
          id="profile-email"
          v-model="email"
          type="email"
          size="small"
          class="w-full"
          required
        />
      </div>

      <div class="space-y-1">
        <label for="profile-password" class="block text-xs font-medium">
          {{ t('profile.newPasswordLabel') }}
        </label>
        <Password
          input-id="profile-password"
          v-model="password"
          :feedback="false"
          toggle-mask
          fluid
        />
      </div>

      <Button
        type="submit"
        :label="t('profile.updateProfileBtn')"
        icon="oi oi-check"
        size="small"
        class="w-full"
        :loading="updateCurrentUserMutation.isPending.value"
      />
    </form>

    <div class="p-5 rounded-2xl border border-red-500/30 bg-red-500/5 space-y-3">
      <h3 class="text-sm font-bold text-red-600 dark:text-red-400">
        {{ t('profile.dangerZone') }}
      </h3>
      <p class="text-xs text-surface-600 dark:text-surface-400">
        {{ t('profile.deleteAccountDesc') }}
      </p>
      <Button
        :label="t('profile.deleteAccountBtn')"
        icon="oi oi-trash"
        severity="danger"
        outlined
        size="small"
        :loading="deleteCurrentUserMutation.isPending.value"
        @click="confirmDeleteAccount"
      />
    </div>
  </div>
</template>
