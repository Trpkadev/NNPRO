<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import { UserRoles, type AdminUserDetailsDto, type UserRole } from '@/types/api'

const props = defineProps<{
  userDetail: AdminUserDetailsDto
}>()

const { t } = useI18n()
const toast = useToast()

const updateRoleMutation = useUpdateUserRoleMutation()
const updateUserMutation = useUpdateUserMutation()

const selectedRole = ref<UserRole>(UserRoles.Student)
const editUsername = ref('')
const editEmail = ref('')
const editPassword = ref('')
const errorMessage = ref<string | null>(null)

const roleOptions = computed(() => [
  { label: t('roles.Admin'), value: UserRoles.Admin },
  { label: t('roles.Instructor'), value: UserRoles.Instructor },
  { label: t('roles.Student'), value: UserRoles.Student },
])

watch(
  () => props.userDetail,
  (detail) => {
    selectedRole.value = (detail.role as UserRole) || UserRoles.Student
    editUsername.value = detail.username
    editEmail.value = detail.email
    editPassword.value = ''
    errorMessage.value = null
  },
  { immediate: true },
)

async function handleRoleChange() {
  errorMessage.value = null
  try {
    await updateRoleMutation.mutateAsync({
      userId: props.userDetail.id,
      payload: { role: selectedRole.value },
    })
    toast.add({
      severity: 'success',
      summary: t('admin.roleUpdatedSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleUpdateUser() {
  errorMessage.value = null
  if (!editUsername.value.trim() || !editEmail.value.trim()) {
    errorMessage.value = t('validation.required')
    return
  }
  if (editPassword.value.length > 0 && editPassword.value.length < 8) {
    errorMessage.value = t('validation.minLength', { min: 8 })
    return
  }

  try {
    await updateUserMutation.mutateAsync({
      userId: props.userDetail.id,
      payload: {
        username: editUsername.value.trim(),
        email: editEmail.value.trim(),
        password: editPassword.value,
      },
    })
    editPassword.value = ''
    toast.add({
      severity: 'success',
      summary: t('admin.userUpdatedSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <div class="space-y-4">
    <Message v-if="errorMessage" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

    <!-- Změna role (RBAC) -->
    <div class="p-4 rounded-xl border border-surface-200 dark:border-surface-800 space-y-3">
      <label for="admin-user-role-select" class="block text-sm font-bold">
        {{ t('admin.changeRoleLabel') }}
      </label>
      <div class="flex items-center gap-2">
        <Select
          label-id="admin-user-role-select"
          v-model="selectedRole"
          :options="roleOptions"
          option-label="label"
          option-value="value"
          size="small"
          class="flex-1"
        />
        <Button
          :label="t('common.save')"
          icon="oi oi-check"
          size="small"
          :loading="updateRoleMutation.isPending.value"
          @click="handleRoleChange"
        />
      </div>
    </div>

    <!-- Úprava údajů uživatele -->
    <form
      class="p-4 rounded-xl border border-surface-200 dark:border-surface-800 space-y-3"
      @submit.prevent="handleUpdateUser"
    >
      <h4 class="text-sm font-bold">{{ t('admin.editUserTitle') }}</h4>
      <div class="space-y-1">
        <label for="admin-edit-username" class="block text-xs font-medium">
          {{ t('admin.username') }}
        </label>
        <InputText
          id="admin-edit-username"
          v-model="editUsername"
          size="small"
          class="w-full"
          required
        />
      </div>
      <div class="space-y-1">
        <label for="admin-edit-email" class="block text-xs font-medium">
          {{ t('admin.email') }}
        </label>
        <InputText
          id="admin-edit-email"
          v-model="editEmail"
          type="email"
          size="small"
          class="w-full"
          required
        />
      </div>
      <div class="space-y-1">
        <label for="admin-edit-password" class="block text-xs font-medium">
          {{ t('profile.newPasswordLabel') }}
        </label>
        <Password
          input-id="admin-edit-password"
          v-model="editPassword"
          :feedback="false"
          toggle-mask
          fluid
        />
      </div>
      <div class="flex justify-end">
        <Button
          type="submit"
          :label="t('common.save')"
          icon="oi oi-check"
          size="small"
          :loading="updateUserMutation.isPending.value"
        />
      </div>
    </form>
  </div>
</template>
